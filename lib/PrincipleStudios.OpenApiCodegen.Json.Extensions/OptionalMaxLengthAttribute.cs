using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace PrincipleStudios.OpenApiCodegen.Json.Extensions
{
	public class OptionalMaxLengthAttribute : ValidationAttribute
	{
		private readonly int _maxLength;

		public OptionalMaxLengthAttribute(int maxLength) : base(() => "The field {0} must be a string or array type with a maximum length of '{1}'.")
		{
			_maxLength = maxLength;
		}

		public override bool IsValid(object? value)
		{
			EnsureLegalLengths();

			if (value == null)
			{
				return true;
			}

			int length;
			if (value is Optional<string> optional)
			{
				if (!optional.TryGet(out string? presentValue) || string.IsNullOrEmpty(presentValue))
				{
					return true;
				}

				length = presentValue.Length;
			}
			else if (value is string stringValue)
			{
				if (string.IsNullOrEmpty(stringValue))
				{
					return true;
				}

				length = stringValue.Length;
			}
			else if (!CountPropertyHelper.TryGetCount(value, out length))
			{
				return true;
			}

			return length <= _maxLength;
		}

		public override string FormatErrorMessage(string name) => string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, _maxLength);

		private void EnsureLegalLengths()
		{
			if (_maxLength == 0 || _maxLength < -1)
			{
				throw new InvalidOperationException("OptionalMaxLengthAttribute must have a Length value that is greater than zero");
			}
		}
	}

	internal static class CountPropertyHelper
	{
		public static bool TryGetCount(object value, out int count)
		{
			Debug.Assert(value != null);

			if (value is ICollection collection)
			{
				count = collection.Count;
				return true;
			}

			PropertyInfo? property = value!.GetType().GetRuntimeProperty("Count");
			if (property != null && property.CanRead && property.PropertyType == typeof(int))
			{
				count = (int)property.GetValue(value)!;
				return true;
			}

			count = -1;
			return false;

		}
	}
}
