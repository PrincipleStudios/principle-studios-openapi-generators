using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations;

public abstract record Either<TLeft, TRight>
{
	private Either() { }

	[DebuggerDisplay("Left = {Value}")]
	public record Left(TLeft Value) : Either<TLeft, TRight>
	{
		public override T Reduce<T>(Func<TLeft, T> handleLeft, Func<TRight, T> handleRight)
		{
			return handleLeft(Value);
		}
	}
	[DebuggerDisplay("Right = {Value}")]
	public record Right(TRight Value) : Either<TLeft, TRight>
	{
		public override T Reduce<T>(Func<TLeft, T> handleLeft, Func<TRight, T> handleRight)
		{
			return handleRight(Value);
		}
	}


	public abstract T Reduce<T>(Func<TLeft, T> handleLeft, Func<TRight, T> handleRight);
}
