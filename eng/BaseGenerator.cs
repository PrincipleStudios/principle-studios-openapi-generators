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

    private static readonly object lockHandle = new object();

    private const string sharedAssemblyName = "PrincipleStudios.OpenApiCodegen";
    private readonly System.Text.RegularExpressions.Regex regex = new(@"^(\.\d+)*\.dll$");
    private readonly Func<IEnumerable<string>> getMetadataKeys;
    private readonly Func<string, IReadOnlyDictionary<string, string?>, object> generate;

    public BaseGenerator(string generatorTypeName)
    {
        var myAsm = this.GetType().Assembly;

        var references = myAsm.GetReferencedAssemblies();

        List<Assembly> loadedAssemblies = new() { myAsm };
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ResolveAssembly;
        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

        var generatorType = Type.GetType(generatorTypeName, throwOnError: false)
            ?? throw new InvalidOperationException($"Could not find generator {generatorTypeName}");

        var generator = Activator.CreateInstance(generatorType);

        var generateMethod = generatorType.GetMethod("Generate");
        var generatorExpression = Constant(generator);
        var compilerApisParameter = Parameter(typeof(CompilerApis));
        var textParameter = Parameter(typeof(string));
        var dictionaryParameter = Parameter(typeof(IReadOnlyDictionary<string, string?>));
        getMetadataKeys = Lambda<Func<IEnumerable<string>>>(Property(generatorExpression, "MetadataKeys")).Compile();
        generate = Lambda<Func<string, IReadOnlyDictionary<string, string?>, object>>(
            Convert(Call(generatorExpression, generateMethod, textParameter, dictionaryParameter), typeof(object))
            , textParameter, dictionaryParameter).Compile();

        Assembly? ResolveAssembly(object sender, ResolveEventArgs ev)
        {
            // I'm not sure why this lock makes a difference; maybe by preventing multiple loads of the same assembly.
            // As a result, this maybe can be moved.
            lock (lockHandle)
            {
                if (references.Any(asm => asm.FullName == ev.Name) && AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.FullName == ev.Name) is Assembly currentDomainAsm)
                {
                    return currentDomainAsm;
                }
                if (ev.RequestingAssembly != null && !loadedAssemblies.Contains(ev.RequestingAssembly))
                {
                    return null;
                }
                if (loadedAssemblies.FirstOrDefault(asm => asm.FullName == ev.Name) is Assembly preloaded)
                    return preloaded;

                using var stream = myAsm.GetManifestResourceStream(ev.Name.Split(',')[0] + ".dll");
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
    }

#if ROSLYN4_0_OR_GREATER
    public virtual void Initialize(IncrementalGeneratorInitializationContext incremental)
    {
        incremental.RegisterImplementationSourceOutput(incremental.CompilationProvider, (context, compilation) =>
        {
            ReportCompilationDiagnostics(compilation, context);
        });

        var additionalTexts = incremental.AdditionalTextsProvider.Combine(incremental.AnalyzerConfigOptionsProvider)
            .Select(static (tuple, _) => GetOptions(tuple.Left, tuple.Right))
            .Where(static (tuple) => tuple.TextContents != null)
            .Where(IsRelevantFile);
        incremental.RegisterSourceOutput(additionalTexts, (context, tuple) =>
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


    protected record AdditionalTextWithOptions(string TextContents, AnalyzerConfigOptions ConfigOptions);
    protected record CompilerApis(AddSourceText AddSource, ReportDiagnostic ReportDiagnostic)
    {
#if ROSLYN4_0_OR_GREATER
        public static implicit operator CompilerApis(SourceProductionContext context) =>
            new(context.AddSource, context.ReportDiagnostic);
#else
        public static implicit operator CompilerApis(GeneratorExecutionContext context) =>
            new(context.AddSource, context.ReportDiagnostic);
#endif
    }
    private static AdditionalTextWithOptions GetOptions(AdditionalText file, AnalyzerConfigOptionsProvider analyzerConfigOptions)
    {
        var opt = analyzerConfigOptions.GetOptions(file);
        return new(file.GetText()?.ToString()!, opt);
    }

    protected abstract void ReportCompilationDiagnostics(Compilation compilation, CompilerApis apis);
    protected abstract bool IsRelevantFile(AdditionalTextWithOptions additionalText);
    private void GenerateSources(AdditionalTextWithOptions additionalText, CompilerApis apis)
    {
        IEnumerable<string> metadataKeys = getMetadataKeys();
        dynamic result = generate(
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
            // TODO: diagnostics
        }
    }


}
