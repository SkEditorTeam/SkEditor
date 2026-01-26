using DiscordRPC.Helper;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DiscordRPC.RPC.Payload
{
    /// <summary>
    /// The payload that is sent by the client to discord for events such as setting the rich presence.
    /// <para>
    /// SetPresence
    /// </para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ArgumentPayload<T> : IPayload
        where T : class
    {
        /// <summary>
        /// The data the server sent to us
        /// </summary>
        [JsonPropertyName("args")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonDocument Arguments { get; set; }

        public ArgumentPayload() { Arguments                         = null; }
        public ArgumentPayload(long nonce) : base(nonce) { Arguments = null; }
        public ArgumentPayload(object args, JsonTypeInfo<T> jsonTypeInfo, long nonce) : base(nonce)
        {
            SetObject((T)args, jsonTypeInfo);
        }

        /// <summary>
        /// Sets the object stored within the data.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="typeInfo"></param>
        public void SetObject(T obj, JsonTypeInfo<T> typeInfo)
        {
            Arguments = JsonSerializer.SerializeToDocument(obj, typeInfo);
        }

        /// <summary>
        /// Gets the object stored within the Data
        /// </summary>
        /// <returns></returns>
        public T GetObject(JsonTypeInfo<T> typeInfo) => Arguments.Deserialize(typeInfo);

        public override string ToString()
        {
            return "Argument " + base.ToString();
        }
    }
}
