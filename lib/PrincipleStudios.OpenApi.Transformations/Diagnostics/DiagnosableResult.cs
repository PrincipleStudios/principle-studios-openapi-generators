using System;
using System.Collections.Generic;
using System.Linq;
using PrincipleStudios.OpenApi.Transformations.Specifications;

namespace PrincipleStudios.OpenApi.Transformations.Diagnostics;

public abstract record DiagnosableResult<T>
{
#pragma warning disable CA1000 // Do not declare static members on generic types
	public static DiagnosableResult<T> Pass(T Keyword) =>
		new DiagnosableResult<T>.Success(Keyword);

	public static DiagnosableResult<T> Fail(NodeMetadata nodeInfo, DocumentRegistry registry, params DiagnosticException.ToDiagnostic[] diagnostics) =>
		new DiagnosableResult<T>.Failure(diagnostics.Select(d => d(registry.ResolveLocation(nodeInfo))).ToArray());

	public static DiagnosableResult<T> Fail(params DiagnosticBase[] diagnostics) =>
		new DiagnosableResult<T>.Failure(diagnostics);
#pragma warning restore CA1000 // Do not declare static members on generic types

	public abstract DiagnosableResult<TResult> Select<TResult>(Func<T, TResult> selector);

	public abstract TResult Fold<TResult>(Func<T, TResult> success, Func<IReadOnlyList<DiagnosticBase>, TResult> failure);

	protected DiagnosableResult() { }
	public record Success(T Value) : DiagnosableResult<T>
	{
		public override TResult Fold<TResult>(Func<T, TResult> success, Func<IReadOnlyList<DiagnosticBase>, TResult> failure) =>
			success(Value);

		public override DiagnosableResult<TResult> Select<TResult>(Func<T, TResult> selector) =>
			new DiagnosableResult<TResult>.Success(selector(Value));
	}
	public record Failure(IReadOnlyList<DiagnosticBase> Diagnostics) : DiagnosableResult<T>
	{
		public override DiagnosableResult<TResult> Select<TResult>(Func<T, TResult> selector) =>
			new DiagnosableResult<TResult>.Failure(Diagnostics);
		public override TResult Fold<TResult>(Func<T, TResult> success, Func<IReadOnlyList<DiagnosticBase>, TResult> failure) =>
			failure(Diagnostics);
	}
}
