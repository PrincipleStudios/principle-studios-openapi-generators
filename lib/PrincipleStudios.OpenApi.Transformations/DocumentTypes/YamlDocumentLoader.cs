using Json.Pointer;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using Yaml2JsonNode;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace PrincipleStudios.OpenApi.Transformations.DocumentTypes;

public class YamlDocumentLoader : IDocumentTypeLoader
{
	public IDocumentReference LoadDocument(Uri retrievalUri, Stream stream)
	{
		var yamlStream = new YamlStream();
		using var sr = new StreamReader(stream);
		try
		{
			yamlStream.Load(sr);
		}
		catch (YamlException ex)
		{
			throw new DocumentException(YamlLoadDiagnostic.Builder(ex), Errors.UnableToLoadYaml, ex);
		}

		return new YamlDocument(retrievalUri, yamlStream);
	}

	public static FileLocationRange FromException(YamlException ex) =>
		new FileLocationRange(
			FromMark(ex.Start),
			FromMark(ex.End)
		);

	public static FileLocationMark FromMark(Mark mark) => new FileLocationMark(mark.Line, mark.Column);

	private class YamlDocument : IDocumentReference
	{
		private YamlStream yamlStream;

		public YamlDocument(Uri retrievalUri, YamlStream yamlStream)
		{
			this.RetrievalUri = retrievalUri;
			this.yamlStream = yamlStream;
			this.RootNode = yamlStream.Documents[0].ToJsonNode();
		}

		public Uri RetrievalUri { get; }

		public JsonNode? RootNode { get; }

		public string OriginalPath => throw new NotImplementedException();

		public FileLocationRange? GetLocation(JsonPointer path)
		{
			throw new NotImplementedException();
		}
	}
}

public record YamlLoadDiagnostic(Location Location, string Message) : DiagnosticBase(Location)
{
	public static DocumentException.ToDiagnostic Builder(YamlException ex)
	{
		return (retrievalUri) =>
		{
			return new YamlLoadDiagnostic(new Location(retrievalUri, YamlDocumentLoader.FromMark(ex.Start), YamlDocumentLoader.FromMark(ex.End)), ex.Message);
		};
	}
}
