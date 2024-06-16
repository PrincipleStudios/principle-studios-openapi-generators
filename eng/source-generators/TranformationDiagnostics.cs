
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;

namespace PrincipleStudios.OpenApiCodegen;

public partial class TransformationDiagnostics
{
	public static readonly IReadOnlyDictionary<string, DiagnosticDescriptor> DiagnosticBy;

	static TransformationDiagnostics()
	{
		DiagnosticBy = (
			from field in typeof(TransformationDiagnostics).GetFields(BindingFlags.Static | BindingFlags.Public)
			let attr = field.GetCustomAttribute<TransformationDiagnosticAttribute>()
			where attr != null
			select new { attr.FullTypeName, Descriptor = (DiagnosticDescriptor)field.GetValue(null)! }
		)
			.ToDictionary(kvp => kvp.FullTypeName, kvp => kvp.Descriptor);
	}

	// TODO: Split across multiple files for maintainability
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.InvalidRetrievalUri")]
	public static readonly DiagnosticDescriptor InvalidRetrievalUriDiagnostic =
		new DiagnosticDescriptor(id: "PSAPIPARSE002",
								title: "An invalid URI was provided to retrieve a document",
								messageFormat: "An invalid URI was provided to retrieve a document: {0}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.InvalidDocumentBaseUri")]
	public static readonly DiagnosticDescriptor InvalidDocumentBaseUriDiagostic =
		new DiagnosticDescriptor(id: "PSAPIPARSE004",
								title: "An invalid base URI was provided by a document",
								messageFormat: "An invalid base URI was provided by a document: {0}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.InvalidFragmentDiagnostic")]
	public static readonly DiagnosticDescriptor InvalidFragmentDiagostic =
		new DiagnosticDescriptor(id: "PSAPIPARSE005",
								title: "The fragment provided was not a valid JSON pointer",
								messageFormat: "The fragment provided was not a valid JSON pointer: {0}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);

	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.ResolveDocumentDiagnostic")]
	public static readonly DiagnosticDescriptor ResolveDocumentDiagnosticDiagnostic =
		new DiagnosticDescriptor(id: "PSAPIPARSE009",
								title: "Could not retrieve the specified document",
								messageFormat: "Could not resolve the document with the URI {0}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Validation.UnableToParseRequiredKeyword")]
	public static readonly DiagnosticDescriptor UnableToParseRequiredKeyword =
		new DiagnosticDescriptor(id: "PSAPIPARSE010",
								title: "Could not parse the 'required' property; is it a string?",
								messageFormat: "Could not parse the 'required' property; is it a string?",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0.UnhandledExceptionDiagnostic")]
	public static readonly DiagnosticDescriptor OpenApi30UnhandledExceptionDiagnostic =
		new DiagnosticDescriptor(id: "PSAPIPARSE011",
								title: "Unhandled exception during parsing of an OpenAPI 3.0 document",
								messageFormat: "Unhandled exception during parsing of an OpenAPI 3.0 document: [{0}] {1}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);

	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.CouldNotFindTargetNodeDiagnostic")]
	public static readonly DiagnosticDescriptor CouldNotFindTargetNodeDiagnostic =
		new DiagnosticDescriptor(id: "PSAPIPARSE012",
								title: "Target node did not exist in given document",
								messageFormat: "Unable to locate node: {0}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
}
