using System;

namespace PrincipleStudios.OpenApi.Transformations;

public class ResolveNodeException : Exception
{
	public Uri Uri { get; }

	public ResolveNodeException(Uri uri) : base(Errors.ResolveNodeException)
	{
		this.Uri = uri;
	}

	public ResolveNodeException(Uri uri, Exception innerException) : base(Errors.ResolveNodeException, innerException)
	{
		this.Uri = uri;
	}
}
