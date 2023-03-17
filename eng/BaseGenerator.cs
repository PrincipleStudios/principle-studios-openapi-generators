using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
    private readonly IOpenApiCodeGenerator generator;

    public BaseGenerator(string generatorTypeName)
    {
        var myAsm = this.GetType().Assembly;
        List<Assembly> loadedAssemblies = new();
        foreach (var resource in myAsm.GetManifestResourceNames().Where(r => r.EndsWith(".dll")))
        {
            using var stream = myAsm.GetManifestResourceStream(resource);
            var dllBytes = new byte[stream.Length];
            stream.Read(dllBytes, 0, (int)stream.Length);
            loadedAssemblies.Add(Assembly.Load(dllBytes));
        }

        var generatorType = loadedAssemblies.Select(asm => asm.GetType(generatorTypeName, false)).FirstOrDefault();
        if (generatorType == null)
            throw new InvalidOperationException($"Could not find generator {generatorType}");

        generator = (IOpenApiCodeGenerator)Activator.CreateInstance(generatorType);
    }

    public BaseGenerator(IOpenApiCodeGenerator generator)
    {
        this.generator = generator ?? throw new ArgumentNullException(nameof(generator), message: "Must provide a generator implementation");
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
        return new (file.GetText()?.ToString()!, opt);
    }

    protected abstract void ReportCompilationDiagnostics(Compilation compilation, CompilerApis apis);
    protected abstract bool IsRelevantFile(AdditionalTextWithOptions additionalText);
    private void GenerateSources(AdditionalTextWithOptions additionalText, CompilerApis apis)
    {
        var document = new OpenApiDocumentConfiguration(
            additionalText.TextContents,
            new ReadOnlyDictionary<string, string?>(
                generator.MetadataKeys.ToDictionary(key => key, additionalText.ConfigOptions.GetAdditionalFilesMetadata)
            )
        );
        var result = generator.Generate(document);
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
