using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Diagnostics
{
	public static class AnalyzerConfigOptionsExtensions
	{
		public static string? GetAdditionalFilesMetadata(this AnalyzerConfigOptions opt, string fieldName) =>
			opt.TryGetValue($"build_metadata.additionalfiles.{fieldName}", out var value) ? value : null;
	}
}
