using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace PrincipleStudios.OpenApiCodegen.Json.Extensions
{
	public class OptionalMinLengthAttribute : ValidationAttribute
	{
		private readonly int _maxLength;

		public OptionalMinLengthAttribute(int maxLength) : base(() => "The field {0} must be a string or array type with a minimum length of '{1}'.")
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
			else if (!OptionalCountPropertyHelper.TryGetCount(value, out length))
			{
				return true;
			}

			return length >= _maxLength;
		}

		public override string FormatErrorMessage(string name) => string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, _maxLength);

		private void EnsureLegalLengths()
		{
			if (_maxLength == 0 || _maxLength < -1)
			{
				throw new InvalidOperationException("OptionalMinLengthAttribute must have a Length value that is zero or greater.");
			}
		}
	}
}
