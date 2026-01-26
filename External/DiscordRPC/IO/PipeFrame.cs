using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace DiscordRPC.IO
{
    /// <summary>
    /// A frame received and sent to the Discord client for RPC communications.
    /// </summary>
    public struct PipeFrame : IEquatable<PipeFrame>, IDisposable
    {
        /// <summary>
        /// The maximum size of a pipe frame (16kb).
        /// </summary>
        public static readonly int MAX_SIZE = 16 * 1024;

        /// <summary>
        /// The opcode of the frame
        /// </summary>
        public Opcode Opcode { get; set; }

        /// <summary>
        /// The length of the frame data
        /// </summary>
        public uint Length { get; private set; }

        /// <summary>
        /// The data in the frame
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// The data represented as a string.
        /// </summary>
        public string Message
        {
            get { return GetMessage(); }
            set { SetMessage(value); }
        }

        /// <summary>
        /// Creates a new pipe frame instance
        /// </summary>
        /// <param name="opcode">The opcode of the frame</param>
        /// <param name="data">The data of the frame that will be serialized as JSON</param>
        /// <param name="jsonTypeInfo">The JSON type info of the data</param>
        public static PipeFrame Create<T>(Opcode opcode, T data, JsonTypeInfo<T> jsonTypeInfo)
            where T : class
        {
            PipeFrame frame = new PipeFrame()
            {
                Opcode = opcode,
                Data   = null,
                Length = 0
            };
            frame.SetObject(data, jsonTypeInfo);

            return frame;
        }

        /// <summary>
        /// Gets the encoding used for the pipe frames
        /// </summary>
        public Encoding MessageEncoding { get { return Encoding.UTF8; } }

        /// <summary>
        /// Sets the data based of a string
        /// </summary>
        /// <param name="str"></param>
        private void SetMessage(string str)
        {
            int maxDataBytes = MessageEncoding.GetByteCount(str);
            Data = ArrayPool<byte>.Shared.Rent(maxDataBytes);
            Length = (uint)MessageEncoding.GetBytes(str, Data);
        }

        /// <summary>
        /// Gets a string based of the data
        /// </summary>
        /// <returns></returns>
        private string GetMessage()
        {
            Span<byte> dataSpan = Data;
            return MessageEncoding.GetString(dataSpan[..(int)Length]);
        }

        /// <summary>
        /// Sets the opcodes and serializes the object into a json string.
        /// </summary>
        /// <param name="opcode"></param>
        /// <param name="obj"></param>
        /// <param name="jsonTypeInfo"></param>
        public void SetObject(Opcode opcode, object obj, JsonTypeInfo jsonTypeInfo)
        {
            Opcode = opcode;
            SetObject(obj, jsonTypeInfo);
        }

        /// <summary>
        /// Serializes the object into json string then encodes it into <see cref="Data"/>.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="jsonTypeInfo"></param>
        public void SetObject(object obj, JsonTypeInfo jsonTypeInfo)
        {
            string json = JsonSerializer.Serialize(obj, jsonTypeInfo);
            SetMessage(json);
        }

        /// <summary>
        /// Deserializes the data into the supplied type using JSON.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into</typeparam>
        /// <returns></returns>
        public T GetObject<T>(JsonTypeInfo<T> typeInfo)
            where T : class
        {
            string json = GetMessage();
            return JsonSerializer.Deserialize(json, typeInfo);
        }

        /// <summary>
        /// Attempts to read the contents of the frame from the stream
        /// </summary>
        public bool ReadBuffer(Span<byte> frameBuffer)
        {
            if (frameBuffer.Length < 8) // sizeof(long)
            {
                return false;
            }

            Opcode = MemoryMarshal.Read<Opcode>(frameBuffer);
            Length = MemoryMarshal.Read<uint>(frameBuffer[4..]);

            scoped Span<byte> dataBuffer = frameBuffer.Slice(8, (int)Length);

            Data = ArrayPool<byte>.Shared.Rent((int)Length);
            scoped Span<byte> toCopyBufferSpan = Data.AsSpan(0, (int)Length);
            if (toCopyBufferSpan.Length < dataBuffer.Length ||
                !dataBuffer.TryCopyTo(toCopyBufferSpan))
            {
                ArrayPool<byte>.Shared.Return(Data);
                Data = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Writes the frame into the target frame as one big byte block.
        /// </summary>
        /// <param name="stream"></param>
        public void WriteStream(Stream stream)
        {
            int dataLength = 8 + (int)Length;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(dataLength);
            scoped Span<byte> bufferSpan = buffer.AsSpan(0, dataLength);
            try
            {
                MemoryMarshal.Write(bufferSpan, Opcode);
                MemoryMarshal.Write(bufferSpan[4..], Length);

                // Copy it all into a buffer
                scoped Span<byte> contentData = bufferSpan[8..];
                scoped Span<byte> sourceData = Data.AsSpan(0, (int)Length);
                sourceData.CopyTo(contentData);

                // Write it to the stream
                stream.Write(bufferSpan);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Compares if the frame equals the other frame.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(PipeFrame other)
        {
            return Opcode == other.Opcode &&
                    Length == other.Length &&
                    Data.AsSpan(0, (int)Length).SequenceEqual(other.Data.AsSpan(0, (int)other.Length));
        }

        public readonly void Dispose()
        {
            if (Data != null)
            {
                ArrayPool<byte>.Shared.Return(Data);
            }
            GC.SuppressFinalize(this);
        }
    }
}
