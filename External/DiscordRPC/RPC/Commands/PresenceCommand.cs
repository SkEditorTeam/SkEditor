using DiscordRPC.Helper;
using DiscordRPC.RPC.Payload;
using System.Text.Json.Serialization;

namespace DiscordRPC.RPC.Commands
{
    internal class PresenceCommand : ICommand
    {
        /// <summary>
        /// The process ID
        /// </summary>
        [JsonPropertyName("pid")]
        public int PID { get; set; }

        /// <summary>
        /// The rich presence to be set. Can be null.
        /// </summary>
        [JsonPropertyName("activity")]
        public RichPresence Presence { get; set; }

        public IPayload PreparePayload(long nonce)
        {
            return new ArgumentPayload<PresenceCommand>(this, JsonSerializationContext.Default.PresenceCommand, nonce)
            {
                Command = Command.SetActivity
            };
        }
    }
}
