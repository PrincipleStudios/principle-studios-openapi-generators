using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations.Specifications;

public static class UriUtils
{
	public static Uri AppendPointer(this Uri uri, string pointerParts)
	{
		var result = new UriBuilder(uri);
		result.Fragment = result.Fragment + "/" + pointerParts;
		return result.Uri;
	}
}
