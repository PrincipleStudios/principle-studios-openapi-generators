﻿using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers.Interface;
using Microsoft.OpenApi.Readers;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApiCodegen;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;

namespace PrincipleStudios.OpenApi.CSharp;

public class MvcServerGenerator : IOpenApiCodeGenerator
{
	const string propNamespace = "Namespace";
	const string propConfig = "Configuration";
	const string propIdentity = "identity";
	const string propLink = "link";
	private readonly IEnumerable<string> metadataKeys = new[]
	{
		propNamespace,
		propConfig,
		propIdentity,
		propLink,
	};

	public IEnumerable<string> MetadataKeys => metadataKeys;

	public GenerationResult Generate(string documentContents, IReadOnlyDictionary<string, string?> additionalTextMetadata)
	{
		if (!TryParseFile(documentContents, out var document, out var diagnostic))
		{
			return new GenerationResult(Array.Empty<OpenApiCodegen.SourceEntry>(), diagnostic);
		}
		var sourceProvider = CreateSourceProvider(document, additionalTextMetadata);
		var openApiDiagnostic = new OpenApiTransformDiagnostic();

		var sources = (from entry in sourceProvider.GetSources(openApiDiagnostic)
					   select new OpenApiCodegen.SourceEntry(entry.Key, entry.SourceText)).ToArray();

		return new GenerationResult(
			sources,
			// TODO - do something with the errors in openApiDiagnostic.Errors!
			Array.Empty<DiagnosticInfo>()
		);
	}

	private static bool TryParseFile(string openapiTextContent, [NotNullWhen(true)] out OpenApiDocument? document, out IReadOnlyList<DiagnosticInfo> diagnostic)
	{
		document = null;
		diagnostic = Array.Empty<DiagnosticInfo>();
		try
		{
			var reader = new OpenApiStringReader();
			document = reader.Read(openapiTextContent, out var openApiDiagnostic);
			if (openApiDiagnostic.Errors.Any())
			{
				// TODO - report issues
				// diagnostic = Diagnostic.Create();

				return false;
			}

			return true;
		}
#pragma warning disable CA1031 // Do not catch general exception types
		catch
#pragma warning restore CA1031 // Do not catch general exception types
		{
			// TODO - report invalid files
			// diagnostic = Diagnostic.Create();
			return false;
		}
	}

	private static ISourceProvider CreateSourceProvider(OpenApiDocument document, IReadOnlyDictionary<string, string?> opt)
	{
		var options = LoadOptions(opt[propConfig]);
		var documentNamespace = opt[propNamespace];
		if (string.IsNullOrEmpty(documentNamespace))
			documentNamespace = GetStandardNamespace(opt, options);

		return document.BuildCSharpPathControllerSourceProvider(GetVersionInfo(), documentNamespace, options);
	}

	private static CSharpServerSchemaOptions LoadOptions(string? optionsFiles)
	{
		using var defaultJsonStream = CSharpSchemaOptions.GetDefaultOptionsJson();
		var builder = new ConfigurationBuilder();
		builder.AddYamlStream(defaultJsonStream);
		if (optionsFiles is { Length: > 0 })
		{
			foreach (var file in optionsFiles.Split(';'))
			{
				if (System.IO.File.Exists(file))
				{
					builder.AddYamlFile(file);
				}
			}
		}
		var result = builder.Build().Get<CSharpServerSchemaOptions>();
		// TODO - generate diagnostic instead of throwing exception
		if (result == null) throw new InvalidOperationException("Could not build schema options");
		return result;
	}

	private static string GetVersionInfo()
	{
		return $"{typeof(CSharpControllerTransformer).FullName} v{typeof(CSharpControllerTransformer).Assembly.GetName().Version}";
	}

	private static string? GetStandardNamespace(IReadOnlyDictionary<string, string?> opt, CSharpSchemaOptions options)
	{
		var identity = opt[propIdentity];
		var link = opt[propLink];
		opt.TryGetValue("build_property.projectdir", out var projectDir);
		opt.TryGetValue("build_property.rootnamespace", out var rootNamespace);

		return CSharpNaming.ToNamespace(rootNamespace, projectDir, identity, link, options.ReservedIdentifiers());
	}
}
