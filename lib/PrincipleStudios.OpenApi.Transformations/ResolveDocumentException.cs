using System;

namespace PrincipleStudios.OpenApi.Transformations;

public class ResolveDocumentException : Exception
{
	public Uri Uri { get; }

	public ResolveDocumentException(Uri uri) : base(Errors.ResolveDocumentException)
	{
		this.Uri = uri;
	}

	public ResolveDocumentException(Uri uri, Exception innerException) : base(Errors.ResolveDocumentException, innerException)
	{
		this.Uri = uri;
	}
}
