using FluentAssertions.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
    public class SystemTextJsonExperimentsShould
    {
        public record NewPet(
            [property: global::System.Text.Json.Serialization.JsonPropertyName("name"), global::Newtonsoft.Json.JsonProperty("name")] string Name,
            [property: global::System.Text.Json.Serialization.JsonPropertyName("tag"), global::Newtonsoft.Json.JsonProperty("tag")] string? Tag
        );

        public record Pet(
            [property: global::System.Text.Json.Serialization.JsonPropertyName("id"), global::Newtonsoft.Json.JsonProperty("id")] long? Id,
            string Name,
            string? Tag
        ) : NewPet(Name, Tag);

        [global::Newtonsoft.Json.JsonConverter(typeof(global::Newtonsoft.Json.Converters.StringEnumConverter))]
        [global::System.Text.Json.Serialization.JsonConverter(typeof(global::PrincipleStudios.OpenApiCodegen.Json.Extensions.JsonStringEnumPropertyNameConverter))]
        public enum OrderStatus
        {
            /// <summary>
            /// Enum Placed for placed
            /// </summary>
            [global::System.Runtime.Serialization.EnumMember(Value = "placed")]
            [global::System.Text.Json.Serialization.JsonPropertyName("placed")]
            Placed = 0,

            /// <summary>
            /// Enum Approved for approved
            /// </summary>
            [global::System.Runtime.Serialization.EnumMember(Value = "approved")]
            [global::System.Text.Json.Serialization.JsonPropertyName("approved")]
            Approved = 1,

            /// <summary>
            /// Enum Delivered for delivered
            /// </summary>
            [global::System.Runtime.Serialization.EnumMember(Value = "delivered")]
            [global::System.Text.Json.Serialization.JsonPropertyName("delivered")]
            Delivered = 2,

        }

        [Fact]
        public void SerializeRecords()
        {
            var original = new NewPet("Fido", null);

            var actual = JsonSerializer.Serialize(original);

            Newtonsoft.Json.Linq.JToken.Parse(actual).Should().BeEquivalentTo(
                Newtonsoft.Json.Linq.JToken.FromObject(original)
            );
        }

        [Fact]
        public void DeserializeRecords()
        {
            var json = @"{ ""name"": ""Fido"", ""tag"": null }";
            var original = new NewPet("Fido", null);

            var actual = JsonSerializer.Deserialize<NewPet>(json)!;

            Newtonsoft.Json.Linq.JToken.FromObject(actual).Should().BeEquivalentTo(
                Newtonsoft.Json.Linq.JToken.FromObject(original)
            );
        }

        [Fact]
        public void SerializeInheritedRecords()
        {
            var original = new Pet(1007, "Fido", null);

            var actual = JsonSerializer.Serialize(original);

            Newtonsoft.Json.Linq.JToken.Parse(actual).Should().BeEquivalentTo(
                Newtonsoft.Json.Linq.JToken.FromObject(original)
            );
        }

        [Fact]
        public void DeserializeInheritedRecords()
        {
            var json = @"{ ""id"": 1007, ""name"": ""Fido"", ""tag"": null }";
            var original = new Pet(1007, "Fido", null);

            var actual = JsonSerializer.Deserialize<Pet>(json)!;

            Newtonsoft.Json.Linq.JToken.FromObject(actual).Should().BeEquivalentTo(
                Newtonsoft.Json.Linq.JToken.FromObject(original)
            );
        }

        [Fact]
        public void SerializeEnums()
        {
            var original = OrderStatus.Approved;

            var actual = JsonSerializer.Serialize(original);

            Newtonsoft.Json.Linq.JToken.Parse(actual).Should().BeEquivalentTo(
                Newtonsoft.Json.Linq.JToken.FromObject(original)
            );
        }

        [Fact]
        public void DeserializeEnums()
        {
            var json = @"""approved""";
            var original = OrderStatus.Approved;

            var actual = JsonSerializer.Deserialize<OrderStatus>(json);

            Newtonsoft.Json.Linq.JToken.FromObject(actual).Should().BeEquivalentTo(
                Newtonsoft.Json.Linq.JToken.FromObject(original)
            );
        }

        [Fact]
        public void SerializeNullableEnums()
        {
            var original = new OrderStatus?[] { null, OrderStatus.Approved, OrderStatus.Delivered };

            var actual = JsonSerializer.Serialize(original);

            Newtonsoft.Json.Linq.JToken.Parse(actual).Should().BeEquivalentTo(
                Newtonsoft.Json.Linq.JToken.FromObject(original)
            );
        }

        [Fact]
        public void DeserializeNullableEnums()
        {
            var json = @"[null, ""approved"", ""delivered""]";
            var original = new OrderStatus?[] { null, OrderStatus.Approved, OrderStatus.Delivered };

            var actual = JsonSerializer.Deserialize<OrderStatus?[]>(json)!;

            Newtonsoft.Json.Linq.JToken.FromObject(actual).Should().BeEquivalentTo(
                Newtonsoft.Json.Linq.JToken.FromObject(original)
            );
        }

        [Fact]
        public void DeserializeAny()
        {
            var json = @"[null, ""approved"", { ""id"": 1007, ""name"": ""Fido"", ""tag"": null } ]";

            var actual = JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonNode>(json)!;
            var actualJson = actual.ToJsonString();

            Newtonsoft.Json.Linq.JToken.Parse(json).Should().BeEquivalentTo(
                Newtonsoft.Json.Linq.JToken.Parse(actualJson)
            );
        }

    }
}
