using System.Text.Json.Serialization;

namespace OmniSharp.Extensions.DebugAdapter.Protocol.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<SteppingGranularity>))]
    public enum SteppingGranularity
    {
        Statement,
        Line,
        Instruction,
    }
}
