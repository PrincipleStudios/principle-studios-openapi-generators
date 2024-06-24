using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
	public static class EnumerableUtils
	{
		public static IEnumerable<T> ConcatOne<T>(this IEnumerable<T> enumerable, T item) =>
			enumerable.Concat(Enumerable.Repeat(item, 1));
	}
}
