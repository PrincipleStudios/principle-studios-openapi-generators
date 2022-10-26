using FluentAssertions.Json;
using PrincipleStudios.OpenApiCodegen.Json.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{

    public class OptionalShould
    {
        public class HasOptional
        {
            [System.Text.Json.Serialization.JsonPropertyName("optionalNullableInteger")]
            [global::System.Text.Json.Serialization.JsonIgnore(Condition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
            public Optional<int?>? OptionalNullableInteger { get; set; }
        }

        [Fact]
        public void DeserializeOptionalMissing()
        {
            var json = @"{}";
            HasOptional target = JsonSerializer.Deserialize<HasOptional>(json)!;
            Assert.Null(target.OptionalNullableInteger);
        }

        [Fact]
        public void DeserializeOptionalAsNull()
        {
            var json = @"{ ""optionalNullableInteger"": null }";
            HasOptional target = JsonSerializer.Deserialize<HasOptional>(json)!;
            Assert.True(target.OptionalNullableInteger is Optional<int?>.Present { Value: null });
        }

        [Fact]
        public void DeserializeOptionalPresent()
        {
            var json = @"{ ""optionalNullableInteger"": 15 }";
            HasOptional target = JsonSerializer.Deserialize<HasOptional>(json)!;
            Assert.True(target.OptionalNullableInteger is Optional<int?>.Present { Value: 15 });
        }

        [Fact]
        public void SerializeOptionalMissing()
        {
            var expectedJson = @"{}";
            var original = new HasOptional { };
            var actual = JsonSerializer.Serialize<HasOptional>(original);
            Assert.Equal(expectedJson, actual);
        }

        [Fact]
        public void SerializeOptionalAsNull()
        {
            var expectedJson = @"{""optionalNullableInteger"":null}";
            var original = new HasOptional { OptionalNullableInteger = new Optional<int?>.Present(null) };
            var actual = JsonSerializer.Serialize<HasOptional>(original);
            Assert.Equal(expectedJson, actual);
        }

        [Fact]
        public void SerializeOptionalPresent()
        {
            var expectedJson = @"{""optionalNullableInteger"":15}";
            var original = new HasOptional { OptionalNullableInteger = new Optional<int?>.Present(15) };
            var actual = JsonSerializer.Serialize<HasOptional>(original);
            Assert.Equal(expectedJson, actual);
        }

        public record RecordHasOptional(
            [property: System.Text.Json.Serialization.JsonPropertyName("optionalNullableInteger")]
            [property: global::System.Text.Json.Serialization.JsonIgnore(Condition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
            global::PrincipleStudios.OpenApiCodegen.Json.Extensions.Optional<int?>? OptionalNullableInteger
        );

        [Fact]
        public void DeserializeRecordOptionalMissing()
        {
            var json = @"{}";
            RecordHasOptional target = JsonSerializer.Deserialize<RecordHasOptional>(json)!;
            Assert.Null(target.OptionalNullableInteger);
        }

        [Fact]
        public void DeserializeRecordOptionalAsNull()
        {
            var json = @"{ ""optionalNullableInteger"": null }";
            RecordHasOptional target = JsonSerializer.Deserialize<RecordHasOptional>(json)!;
            Assert.True(target.OptionalNullableInteger is Optional<int?>.Present { Value: null });
        }

        [Fact]
        public void DeserializeRecordOptionalPresent()
        {
            var json = @"{ ""optionalNullableInteger"": 15 }";
            RecordHasOptional target = JsonSerializer.Deserialize<RecordHasOptional>(json)!;
            Assert.True(target.OptionalNullableInteger is Optional<int?>.Present { Value: 15 });
        }

        [Fact]
        public void SerializeRecordOptionalMissing()
        {
            var expectedJson = @"{}";
            var original = new RecordHasOptional(Optional<int?>.None);
            var actual = JsonSerializer.Serialize<RecordHasOptional>(original);
            Assert.Equal(expectedJson, actual);
        }

        [Fact]
        public void SerializeRecordOptionalAsNull()
        {
            var expectedJson = @"{""optionalNullableInteger"":null}";
            var original = new RecordHasOptional(new Optional<int?>.Present(null));
            var actual = JsonSerializer.Serialize<RecordHasOptional>(original);
            Assert.Equal(expectedJson, actual);
        }

        [Fact]
        public void SerializeRecordOptionalPresent()
        {
            var expectedJson = @"{""optionalNullableInteger"":15}";
            var original = new RecordHasOptional(new Optional<int?>.Present(15));
            var actual = JsonSerializer.Serialize<RecordHasOptional>(original);
            Assert.Equal(expectedJson, actual);
        }



    }
}
