using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OmniSharp.Extensions.DebugAdapter.Protocol.Models;

namespace OmniSharp.Extensions.DebugAdapter.Protocol.Serialization
{
    internal sealed class ContainerJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Container<>);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            // Register the protocol's closed collection types at compile time for NativeAOT.
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.Breakpoint>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.Breakpoint>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.Checksum>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.Checksum>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.ChecksumAlgorithm>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.ChecksumAlgorithm>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.ColumnDescriptor>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.ColumnDescriptor>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.DataBreakpoint>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.DataBreakpoint>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.DataBreakpointAccessType>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.DataBreakpointAccessType>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.ExceptionBreakpointsFilter>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.ExceptionBreakpointsFilter>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.ExceptionDetails>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.ExceptionDetails>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.ExceptionFilterOptions>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.ExceptionFilterOptions>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.InstructionBreakpoint>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.InstructionBreakpoint>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.InvalidatedAreas>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.InvalidatedAreas>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.Module>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.Module>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.Source>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.Source>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.SourceBreakpoint>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.SourceBreakpoint>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.StackFrame>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.StackFrame>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.Thread>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.Thread>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.Variable>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.Variable>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.VariableAttributes>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.VariableAttributes>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.BreakpointLocation>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.BreakpointLocation>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.CompletionItem>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.CompletionItem>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.DisassembledInstruction>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.DisassembledInstruction>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.ExceptionOptions>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.ExceptionOptions>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.ExceptionPathSegment>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.ExceptionPathSegment>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.FunctionBreakpoint>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.FunctionBreakpoint>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.GotoTarget>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.GotoTarget>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.Scope>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.Scope>();
            if (typeToConvert == typeof(Container<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.StepInTarget>)) return new ContainerJsonConverter<global::OmniSharp.Extensions.DebugAdapter.Protocol.Models.StepInTarget>();
            if (typeToConvert == typeof(Container<long>)) return new ContainerJsonConverter<long>();
            if (typeToConvert == typeof(Container<string>)) return new ContainerJsonConverter<string>();
            return (JsonConverter)Activator.CreateInstance(typeof(ContainerJsonConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0]))!;
        }
    }

    internal sealed class ContainerJsonConverter<T> : JsonConverter<Container<T>>
    {
        public override Container<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException("Expected an array.");
            var items = new List<T>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray) return new Container<T>(items);
                items.Add(JsonSerializer.Deserialize<T>(ref reader, options)!);
            }
            throw new JsonException("Incomplete array.");
        }

        public override void Write(Utf8JsonWriter writer, Container<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var item in value) JsonSerializer.Serialize(writer, item, options);
            writer.WriteEndArray();
        }
    }
}
