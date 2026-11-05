using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OmniSharp.Extensions.JsonRpc;

namespace OmniSharp.Extensions.DebugAdapter.Protocol.Serialization
{
    internal sealed class AotStringEnumConverter<T> : JsonConverter<T> where T : struct, IEnumLikeString
    {
        private readonly Func<string, T> _create;
        public AotStringEnumConverter(Func<string, T> create) => _create = create;
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            _create(reader.GetString()!);
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString());
    }
}
