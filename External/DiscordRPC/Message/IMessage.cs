using System;

namespace DiscordRPC.Message
{
    /// <summary>
    /// Messages received from discord.
    /// </summary>
    public abstract class IMessage
    {
        /// <summary>
        /// The type of message received from discord
        /// </summary>
        public abstract MessageType Type { get; }

        /// <summary>
        /// The time the message was created
        /// </summary>
        public DateTime TimeCreated { get; }

        /// <summary>
        /// Creates a new instance of the message
        /// </summary>
        public IMessage()
        {
            TimeCreated = DateTime.Now;
        }
    }
}
