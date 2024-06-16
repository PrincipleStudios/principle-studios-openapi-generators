using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
}
