using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.SystemTextJsonExperiments
{

	/// <summary>
	/// <see cref="JsonConverterFactory"/> to convert enums to and from strings, respecting <see cref="EnumMemberAttribute"/> decorations. Supports nullable enums.
	/// </summary>
	public class JsonStringEnumMemberConverter : JsonConverterFactory
	{
		private readonly JsonNamingPolicy? _NamingPolicy;
		private readonly bool _AllowIntegerValues;

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonStringEnumMemberConverter"/> class.
		/// </summary>
		public JsonStringEnumMemberConverter()
			: this(namingPolicy: null, allowIntegerValues: true)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonStringEnumMemberConverter"/> class.
		/// </summary>
		/// <param name="namingPolicy">
		/// Optional naming policy for writing enum values.
		/// </param>
		/// <param name="allowIntegerValues">
		/// True to allow undefined enum values. When true, if an enum value isn't
		/// defined it will output as a number rather than a string.
		/// </param>
		public JsonStringEnumMemberConverter(JsonNamingPolicy? namingPolicy = null, bool allowIntegerValues = true)
		{
			_NamingPolicy = namingPolicy;
			_AllowIntegerValues = allowIntegerValues;
		}

		/// <inheritdoc/>
		public override bool CanConvert(Type typeToConvert)
		{
			// Don't perform a typeToConvert == null check for performance. Trust our callers will be nice.
#pragma warning disable CA1062 // Validate arguments of public methods
			return typeToConvert.IsEnum
				|| (typeToConvert.IsGenericType && TestNullableEnum(typeToConvert).IsNullableEnum);
#pragma warning restore CA1062 // Validate arguments of public methods
		}

		/// <inheritdoc/>
		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			(bool IsNullableEnum, Type? UnderlyingType) = TestNullableEnum(typeToConvert);

			return IsNullableEnum
				? (JsonConverter)Activator.CreateInstance(
					typeof(NullableEnumMemberConverter<>).MakeGenericType(UnderlyingType!),
					BindingFlags.Instance | BindingFlags.Public,
					binder: null,
					args: new object?[] { _NamingPolicy, _AllowIntegerValues },
					culture: null)!
				: (JsonConverter)Activator.CreateInstance(
					typeof(EnumMemberConverter<>).MakeGenericType(typeToConvert),
					BindingFlags.Instance | BindingFlags.Public,
					binder: null,
					args: new object?[] { _NamingPolicy, _AllowIntegerValues },
					culture: null)!;
		}

		private static (bool IsNullableEnum, Type? UnderlyingType) TestNullableEnum(Type typeToConvert)
		{
			Type? UnderlyingType = Nullable.GetUnderlyingType(typeToConvert);

			return (UnderlyingType?.IsEnum ?? false, UnderlyingType);
		}

#pragma warning disable CA1812 // Remove class never instantiated
		private class EnumMemberConverter<TEnum> : JsonConverter<TEnum>
			where TEnum : struct, Enum
#pragma warning restore CA1812 // Remove class never instantiated
		{
			private readonly JsonStringEnumMemberConverterHelper<TEnum> _JsonStringEnumMemberConverterHelper;

			public EnumMemberConverter(JsonNamingPolicy? namingPolicy, bool allowIntegerValues)
			{
				_JsonStringEnumMemberConverterHelper = new JsonStringEnumMemberConverterHelper<TEnum>(namingPolicy, allowIntegerValues);
			}

			public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
				=> _JsonStringEnumMemberConverterHelper.Read(ref reader);

			public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
				=> _JsonStringEnumMemberConverterHelper.Write(writer, value);
		}

#pragma warning disable CA1812 // Remove class never instantiated
		private class NullableEnumMemberConverter<TEnum> : JsonConverter<TEnum?>
			where TEnum : struct, Enum
#pragma warning restore CA1812 // Remove class never instantiated
		{
			private readonly JsonStringEnumMemberConverterHelper<TEnum> _JsonStringEnumMemberConverterHelper;

			public NullableEnumMemberConverter(JsonNamingPolicy? namingPolicy, bool allowIntegerValues)
			{
				_JsonStringEnumMemberConverterHelper = new JsonStringEnumMemberConverterHelper<TEnum>(namingPolicy, allowIntegerValues);
			}

			public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
				=> _JsonStringEnumMemberConverterHelper.Read(ref reader);

			public override void Write(Utf8JsonWriter writer, TEnum? value, JsonSerializerOptions options)
				=> _JsonStringEnumMemberConverterHelper.Write(writer, value!.Value);
		}
	}

	internal class JsonStringEnumMemberConverterHelper<TEnum>
		where TEnum : struct, Enum
	{
		private class EnumInfo
		{
#pragma warning disable SA1401 // Fields should be private
			public string Name;
			public TEnum EnumValue;
			public ulong RawValue;
#pragma warning restore SA1401 // Fields should be private

			public EnumInfo(string name, TEnum enumValue, ulong rawValue)
			{
				Name = name;
				EnumValue = enumValue;
				RawValue = rawValue;
			}
		}

		private const BindingFlags EnumBindings = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

#if NETSTANDARD2_0
		private static readonly string[] s_Split = new string[] { ", " };
#endif

		private readonly bool _AllowIntegerValues;
		private readonly Type _EnumType;
		private readonly TypeCode _EnumTypeCode;
		private readonly bool _IsFlags;
		private readonly Dictionary<TEnum, EnumInfo> _RawToTransformed;
		private readonly Dictionary<string, EnumInfo> _TransformedToRaw;

		public JsonStringEnumMemberConverterHelper(JsonNamingPolicy? namingPolicy, bool allowIntegerValues)
		{
			_AllowIntegerValues = allowIntegerValues;
			_EnumType = typeof(TEnum);
			_EnumTypeCode = Type.GetTypeCode(_EnumType);
			_IsFlags = _EnumType.IsDefined(typeof(FlagsAttribute), true);

			string[] builtInNames = _EnumType.GetEnumNames();
			Array builtInValues = _EnumType.GetEnumValues();

			_RawToTransformed = new Dictionary<TEnum, EnumInfo>();
			_TransformedToRaw = new Dictionary<string, EnumInfo>();

			for (int i = 0; i < builtInNames.Length; i++)
			{
				Enum? enumValue = (Enum?)builtInValues.GetValue(i);
				if (enumValue == null)
					continue;
				ulong rawValue = GetEnumValue(enumValue);

				string name = builtInNames[i];
				FieldInfo field = _EnumType.GetField(name, EnumBindings)!;
				EnumMemberAttribute? enumMemberAttribute = field.GetCustomAttribute<EnumMemberAttribute>(true);
				string transformedName = enumMemberAttribute?.Value ?? namingPolicy?.ConvertName(name) ?? name;

				if (enumValue is not TEnum typedValue)
					throw new NotSupportedException();

				_RawToTransformed[typedValue] = new EnumInfo(transformedName, typedValue, rawValue);
				_TransformedToRaw[transformedName] = new EnumInfo(name, typedValue, rawValue);
			}
		}

		public TEnum Read(ref Utf8JsonReader reader)
		{
			JsonTokenType token = reader.TokenType;

			if (token == JsonTokenType.String)
			{
				string enumString = reader.GetString()!;

				// Case sensitive search attempted first.
				if (_TransformedToRaw.TryGetValue(enumString, out EnumInfo? enumInfo))
					return enumInfo.EnumValue;

				if (_IsFlags)
				{
					ulong calculatedValue = 0;

#if NETSTANDARD2_0
					string[] flagValues = enumString.Split(s_Split, StringSplitOptions.None);
#else
					string[] flagValues = enumString.Split(", ");
#endif
					foreach (string flagValue in flagValues)
					{
						// Case sensitive search attempted first.
						if (_TransformedToRaw.TryGetValue(flagValue, out enumInfo))
						{
							calculatedValue |= enumInfo.RawValue;
						}
						else
						{
							// Case insensitive search attempted second.
							bool matched = false;
							foreach (KeyValuePair<string, EnumInfo> enumItem in _TransformedToRaw)
							{
								if (string.Equals(enumItem.Key, flagValue, StringComparison.OrdinalIgnoreCase))
								{
									calculatedValue |= enumItem.Value.RawValue;
									matched = true;
									break;
								}
							}

							if (!matched)
								throw ThrowHelper.GenerateJsonException_DeserializeUnableToConvertValue(_EnumType, flagValue);
						}
					}

					TEnum enumValue = (TEnum)Enum.ToObject(_EnumType, calculatedValue);
					if (_TransformedToRaw.Count < 64)
					{
						_TransformedToRaw[enumString] = new EnumInfo(enumString, enumValue, calculatedValue);
					}
					return enumValue;
				}

				// Case insensitive search attempted second.
				foreach (KeyValuePair<string, EnumInfo> enumItem in _TransformedToRaw)
				{
					if (string.Equals(enumItem.Key, enumString, StringComparison.OrdinalIgnoreCase))
					{
						return enumItem.Value.EnumValue;
					}
				}

				throw ThrowHelper.GenerateJsonException_DeserializeUnableToConvertValue(_EnumType, enumString);
			}

			if (token != JsonTokenType.Number || !_AllowIntegerValues)
				throw ThrowHelper.GenerateJsonException_DeserializeUnableToConvertValue(_EnumType);

			switch (_EnumTypeCode)
			{
				case TypeCode.Int32:
					if (reader.TryGetInt32(out int int32))
					{
						return (TEnum)Enum.ToObject(_EnumType, int32);
					}
					break;
				case TypeCode.Int64:
					if (reader.TryGetInt64(out long int64))
					{
						return (TEnum)Enum.ToObject(_EnumType, int64);
					}
					break;
				case TypeCode.Int16:
					if (reader.TryGetInt16(out short int16))
					{
						return (TEnum)Enum.ToObject(_EnumType, int16);
					}
					break;
				case TypeCode.Byte:
					if (reader.TryGetByte(out byte ubyte8))
					{
						return (TEnum)Enum.ToObject(_EnumType, ubyte8);
					}
					break;
				case TypeCode.UInt32:
					if (reader.TryGetUInt32(out uint uint32))
					{
						return (TEnum)Enum.ToObject(_EnumType, uint32);
					}
					break;
				case TypeCode.UInt64:
					if (reader.TryGetUInt64(out ulong uint64))
					{
						return (TEnum)Enum.ToObject(_EnumType, uint64);
					}
					break;
				case TypeCode.UInt16:
					if (reader.TryGetUInt16(out ushort uint16))
					{
						return (TEnum)Enum.ToObject(_EnumType, uint16);
					}
					break;
				case TypeCode.SByte:
					if (reader.TryGetSByte(out sbyte byte8))
					{
						return (TEnum)Enum.ToObject(_EnumType, byte8);
					}
					break;
			}

			throw ThrowHelper.GenerateJsonException_DeserializeUnableToConvertValue(_EnumType);
		}

		public void Write(Utf8JsonWriter writer, TEnum value)
		{
			if (_RawToTransformed.TryGetValue(value, out EnumInfo? enumInfo))
			{
				writer.WriteStringValue(enumInfo.Name);
				return;
			}

			ulong rawValue = GetEnumValue(value);

			if (_IsFlags)
			{
				ulong calculatedValue = 0;

				StringBuilder Builder = new StringBuilder();
				foreach (KeyValuePair<TEnum, EnumInfo> enumItem in _RawToTransformed)
				{
					enumInfo = enumItem.Value;
					if (!value.HasFlag(enumInfo.EnumValue)
						|| enumInfo.RawValue == 0) // Definitions with 'None' should hit the cache case.
					{
						continue;
					}

					// Track the value to make sure all bits are represented.
					calculatedValue |= enumInfo.RawValue;

					if (Builder.Length > 0)
						Builder.Append(", ");
					Builder.Append(enumInfo.Name);
				}
				if (calculatedValue == rawValue)
				{
					string finalName = Builder.ToString();
					if (_RawToTransformed.Count < 64)
					{
						_RawToTransformed[value] = new EnumInfo(finalName, value, rawValue);
					}
					writer.WriteStringValue(finalName);
					return;
				}
			}

			if (!_AllowIntegerValues)
				throw new JsonException($"Enum type {_EnumType} does not have a mapping for integer value '{rawValue.ToString(CultureInfo.CurrentCulture)}'.");

			switch (_EnumTypeCode)
			{
				case TypeCode.Int32:
					writer.WriteNumberValue((int)rawValue);
					break;
				case TypeCode.Int64:
					writer.WriteNumberValue((long)rawValue);
					break;
				case TypeCode.Int16:
					writer.WriteNumberValue((short)rawValue);
					break;
				case TypeCode.Byte:
					writer.WriteNumberValue((byte)rawValue);
					break;
				case TypeCode.UInt32:
					writer.WriteNumberValue((uint)rawValue);
					break;
				case TypeCode.UInt64:
					writer.WriteNumberValue(rawValue);
					break;
				case TypeCode.UInt16:
					writer.WriteNumberValue((ushort)rawValue);
					break;
				case TypeCode.SByte:
					writer.WriteNumberValue((sbyte)rawValue);
					break;
				default:
					throw new JsonException(); // GetEnumValue should have already thrown.
			}
		}

		private ulong GetEnumValue(object value)
		{
			return _EnumTypeCode switch
			{
				TypeCode.Int32 => (ulong)(int)value,
				TypeCode.Int64 => (ulong)(long)value,
				TypeCode.Int16 => (ulong)(short)value,
				TypeCode.Byte => (byte)value,
				TypeCode.UInt32 => (uint)value,
				TypeCode.UInt64 => (ulong)value,
				TypeCode.UInt16 => (ushort)value,
				TypeCode.SByte => (ulong)(sbyte)value,
				_ => throw new NotSupportedException($"Enum '{value}' of {_EnumTypeCode} type is not supported."),
			};
		}
	}

	internal static class ThrowHelper
	{
		private static readonly PropertyInfo? s_JsonException_AppendPathInformation
			= typeof(JsonException).GetProperty("AppendPathInformation", BindingFlags.NonPublic | BindingFlags.Instance);

		/// <summary>
		/// Generate a <see cref="JsonException"/> using the internal
		/// <c>JsonException.AppendPathInformation</c> property that will
		/// eventually include the JSON path, line number, and byte position in
		/// line.
		/// <para>
		/// The final message of the exception looks like: The JSON value could
		/// not be converted to {0}. Path: $.{JSONPath} | LineNumber:
		/// {LineNumber} | BytePositionInLine: {BytePositionInLine}.
		/// </para>
		/// </summary>
		/// <param name="propertyType">Property type.</param>
		/// <returns><see cref="JsonException"/>.</returns>
		public static JsonException GenerateJsonException_DeserializeUnableToConvertValue(Type propertyType)
		{
			Debug.Assert(s_JsonException_AppendPathInformation != null);

			JsonException jsonException = new JsonException($"The JSON value could not be converted to {propertyType}.");
			s_JsonException_AppendPathInformation?.SetValue(jsonException, true);
			return jsonException;
		}


		/// <summary>
		/// Generate a <see cref="JsonException"/> using the internal
		/// <c>JsonException.AppendPathInformation</c> property that will
		/// eventually include the JSON path, line number, and byte position in
		/// line.
		/// <para>
		/// The final message of the exception looks like: The JSON value '{1}'
		/// could not be converted to {0}. Path: $.{JSONPath} | LineNumber:
		/// {LineNumber} | BytePositionInLine: {BytePositionInLine}.
		/// </para>
		/// </summary>
		/// <param name="propertyType">Property type.</param>
		/// <param name="propertyValue">Value that could not be parsed into
		/// property type.</param>
		/// <param name="innerException">Optional inner <see cref="Exception"/>.</param>
		/// <returns><see cref="JsonException"/>.</returns>
		public static JsonException GenerateJsonException_DeserializeUnableToConvertValue(
			Type propertyType,
			string propertyValue,
			Exception? innerException = null)
		{
			Debug.Assert(s_JsonException_AppendPathInformation != null);

			JsonException jsonException = new JsonException(
				$"The JSON value '{propertyValue}' could not be converted to {propertyType}.",
				innerException);
			s_JsonException_AppendPathInformation?.SetValue(jsonException, true);
			return jsonException;
		}
	}
}
