using PrincipleStudios.OpenApi.Transformations.Abstractions;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations.Specifications;

public static class CommonParsers
{
	private static readonly DocumentRegistry registry = new DocumentRegistry();
	public static readonly IParser<OpenApiDocument> OpenApi3_0Parser = new OpenApi3_0.OpenApi3_0Parser(registry);
	public static readonly IParser<OpenApiDocument> OpenApi3_1Parser = new OpenApi3_1.OpenApi3_1Parser(registry);

	public static readonly IReadOnlyList<IParser<OpenApiDocument>> DefaultParsers = new[]
	{
		OpenApi3_0Parser,
		// OpenApi3_1Parser,
	};

	public static ParseResult<TResult>? Parse<TResult>(this IEnumerable<IParser<TResult>> parsers, IDocumentReference document, DocumentRegistry documentRegistry)
		where TResult : class, IReferenceableDocument
	{
		return parsers
			.Where(parser => parser.CanParse(document))
			.Select(parser => parser.Parse(document, documentRegistry))
			.FirstOrDefault();
	}
}
