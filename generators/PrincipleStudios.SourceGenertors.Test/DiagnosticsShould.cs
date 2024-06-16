using System;
using System.Collections.Generic;
using System.Linq;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApiCodegen;

namespace PrincipleStudios.OpenApiCodegen;

public class DiagnosticsShould
{
	[MemberData(nameof(DiagnosticBaseNames))]
	[Theory]
	public void CoverAllDiagnosticBases(string typeName)
	{
		Assert.True(BaseGenerator.DiagnosticBy.ContainsKey(typeName), $"Missing diagnostic for '{typeName}'");
	}

	public static IEnumerable<object[]> DiagnosticBaseNames()
	{
		return from type in ChildTypesOf(typeof(DiagnosticBase))
			   select new object[] { type.FullName! };
	}

	private static IEnumerable<Type> ChildTypesOf(Type target)
	{
		return from asm in AppDomain.CurrentDomain.GetAssemblies()
			   from type in asm.GetTypes()
			   where type.IsAssignableTo(target)
			   where !type.IsAbstract
			   select type;
	}
}