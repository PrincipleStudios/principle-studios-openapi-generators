using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using static System.Linq.Expressions.Expression;
using System.Reflection;
using System.Text;

namespace PrincipleStudios.OpenApiCodegen;

using ReportDiagnostic = Action<Diagnostic>;
using AddSourceText = Action<string, SourceText>;

public abstract class BaseGenerator :
#if ROSLYN4_0_OR_GREATER
	IIncrementalGenerator
#else
	ISourceGenerator
#endif
{
	private static readonly DiagnosticDescriptor OpenApiConversionError = new DiagnosticDescriptor(id: "PSAPIPARSE001",
																								title: "A conversion error was encountered",
																								messageFormat: "A conversion error was encountered: {0}",
																								category: "PrincipleStudios.OpenApiCodegen",
																								DiagnosticSeverity.Error,
																								isEnabledByDefault: true);

	private static readonly object lockHandle = new object();

	private readonly Func<IEnumerable<string>> getMetadataKeys;
	private readonly Func<string, string, IReadOnlyDictionary<string, string?>, object> generate;

	protected BaseGenerator(string generatorTypeName, string assemblyName)
	{
		var myAsm = this.GetType().Assembly;

		var references = myAsm.GetReferencedAssemblies();

		List<Assembly> loadedAssemblies = new() { myAsm };
		AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ResolveAssembly!;
		AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly!;

		// When using Type.GetType, the `RequestingAssembly` ends up null. If a generic is passed to Assembly.GetType, it also comes through as null.
		// See https://github.com/dotnet/runtime/issues/11895, https://github.com/dotnet/runtime/issues/12668
		var generatorType = GetEmbeddedAssemblyByName(assemblyName)?.GetType(generatorTypeName, throwOnError: false)
			?? throw new InvalidOperationException($"Could not find generator {generatorTypeName}");

		var generator = Activator.CreateInstance(generatorType);

		var generateMethod = generatorType.GetMethod("Generate")!;
		var generatorExpression = Constant(generator);
		var compilerApisParameter = Parameter(typeof(CompilerApis));
		var pathParameter = Parameter(typeof(string));
		var textParameter = Parameter(typeof(string));
		var dictionaryParameter = Parameter(typeof(IReadOnlyDictionary<string, string?>));
		getMetadataKeys = Lambda<Func<IEnumerable<string>>>(Property(generatorExpression, "MetadataKeys")).Compile();
		generate = Lambda<Func<string, string, IReadOnlyDictionary<string, string?>, object>>(
			Convert(Call(generatorExpression, generateMethod, pathParameter, textParameter, dictionaryParameter), typeof(object))
			, pathParameter, textParameter, dictionaryParameter).Compile();

		Assembly? ResolveAssembly(object sender, ResolveEventArgs ev)
		{
			// I'm not sure why this lock makes a difference; maybe by preventing multiple loads of the same assembly.
			// As a result, this maybe can be moved.
			lock (lockHandle)
			{
				if (ev.RequestingAssembly == null)
					// Someone loaded something through Type.GetType or a generic in Assembly.GetType.
					// This project shouldn't do that, so we can safely ignore it.
					return null;
				if (!loadedAssemblies.Contains(ev.RequestingAssembly))
					// If it wasn't one of our assemblies requesting the DLL, do not respond, let something else handle it
					return null;
				if (references.Any(asm => asm.FullName == ev.Name) && AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.FullName == ev.Name) is Assembly currentDomainAsm)
					// If it's something this assembly references (which is only core Roslyn files), return it from the app domain.
					return currentDomainAsm;
				return GetEmbeddedAssemblyByName(ev.Name);
			}
		}

		Assembly? GetEmbeddedAssemblyByName(string name)
		{
			if (loadedAssemblies.FirstOrDefault(asm => asm.FullName == name) is Assembly preloaded)
				return preloaded;

			using var stream = myAsm.GetManifestResourceStream(name.Split(',')[0] + ".dll");
			if (stream != null)
			{
				var dllBytes = new byte[stream.Length];
				stream.Read(dllBytes, 0, (int)stream.Length);
				var resultAsm = Assembly.Load(dllBytes);
				loadedAssemblies.Add(resultAsm);
				return resultAsm;
			}
			return null;
		}
	}

#if ROSLYN4_0_OR_GREATER
	public virtual void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterImplementationSourceOutput(context.CompilationProvider, (context, compilation) =>
		{
			ReportCompilationDiagnostics(compilation, context);
		});

		var additionalTexts = context.AdditionalTextsProvider.Combine(context.AnalyzerConfigOptionsProvider)
			.Select(static (tuple, _) => GetOptions(tuple.Left, tuple.Right))
			.Where(static (tuple) => tuple.TextContents != null)
			.Where(IsRelevantFile);
		context.RegisterSourceOutput(additionalTexts, (context, tuple) =>
		{
			GenerateSources(tuple, context);
		});
	}
#else
	public virtual void Execute(GeneratorExecutionContext context)
	{
		ReportCompilationDiagnostics(context.Compilation, context);

		var additionalTexts = context.AdditionalFiles.Select(file => GetOptions(file, context.AnalyzerConfigOptions))
			.Where(static (tuple) => tuple.TextContents != null)
			.Where(IsRelevantFile);
		foreach (var additionalText in additionalTexts)
		{
			GenerateSources(additionalText, context);
		}
	}

	public void Initialize(GeneratorInitializationContext context)
	{
	}
#endif


	protected record AdditionalTextWithOptions(string Path, string TextContents, AnalyzerConfigOptions ConfigOptions);
	protected record CompilerApis(AddSourceText AddSource, ReportDiagnostic ReportDiagnostic)
	{
#pragma warning disable CA2225 // Operator overloads have named alternates
#if ROSLYN4_0_OR_GREATER
		public static implicit operator CompilerApis(SourceProductionContext context) =>
			new(context.AddSource, context.ReportDiagnostic);
#else
		public static implicit operator CompilerApis(GeneratorExecutionContext context) =>
			new(context.AddSource, context.ReportDiagnostic);
#endif
#pragma warning restore CA2225 // Operator overloads have named alternates
	}
	private static AdditionalTextWithOptions GetOptions(AdditionalText file, AnalyzerConfigOptionsProvider analyzerConfigOptions)
	{
		var opt = analyzerConfigOptions.GetOptions(file);
		return new(file.Path, file.GetText()?.ToString()!, opt);
	}

	protected abstract void ReportCompilationDiagnostics(Compilation compilation, CompilerApis apis);
	protected abstract bool IsRelevantFile(AdditionalTextWithOptions additionalText);
	private void GenerateSources(AdditionalTextWithOptions additionalText, CompilerApis apis)
	{
		IEnumerable<string> metadataKeys = getMetadataKeys();
		// result is of type PrincipleStudios.OpenApiCodegen.GenerationResult
		dynamic result = generate(
			additionalText.Path,
			additionalText.TextContents,
			new ReadOnlyDictionary<string, string?>(
				metadataKeys.ToDictionary(key => key, additionalText.ConfigOptions.GetAdditionalFilesMetadata)
			)
		);
		foreach (var entry in result.Sources)
		{
			apis.AddSource($"PS_{entry.Key}", SourceText.From(entry.SourceText, Encoding.UTF8));
		}
		foreach (var diagnostic in result.Diagnostics)
		{
			if (TransformationDiagnostics.DiagnosticBy.TryGetValue((string)diagnostic.Id, out var descriptor))
				apis.ReportDiagnostic(
					Diagnostic.Create(
						descriptor,
						Location.Create(
							diagnostic.Location.FilePath,
							default(TextSpan),
							diagnostic.Location.Range == null ? default : new LinePositionSpan(
								new LinePosition(diagnostic.Location.Range.Start.Line - 1, diagnostic.Location.Range.Start.Column - 1),
								new LinePosition(diagnostic.Location.Range.End.Line - 1, diagnostic.Location.Range.End.Column - 1)
							)
						),
						((IReadOnlyList<object>)diagnostic.Metadata).ToArray()
					)
				);
			else
				apis.ReportDiagnostic(
					Diagnostic.Create(
						OpenApiConversionError,
						Location.Create(
							diagnostic.Location.FilePath,
							default(TextSpan),
							diagnostic.Location.Range == null ? default : new LinePositionSpan(
								new LinePosition(diagnostic.Location.Range.Start.Line - 1, diagnostic.Location.Range.Start.Column - 1),
								new LinePosition(diagnostic.Location.Range.End.Line - 1, diagnostic.Location.Range.End.Column - 1)
							)
						),
						(string)diagnostic.Id
					)
				);
		}
	}


}
