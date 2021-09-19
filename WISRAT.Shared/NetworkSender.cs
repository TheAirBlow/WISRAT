using System;
using System.Net.Sockets;

namespace WISRAT.Shared
{
    /// <summary>
    /// Network Sender
    /// </summary>
    public class NetworkSender
    {
        /// <summary>
        /// NetworkStream instance
        /// </summary>
        private readonly NetworkStream _stream;

        /// <summary>
        /// Creates an instance of NetworkSender
        /// </summary>
        /// <param name="stream">NetworkStream instance</param>
        public NetworkSender(NetworkStream stream) => _stream = stream;

        /// <summary>
        /// Writes raw payload
        /// </summary>
        /// <param name="raw">Payload</param>
        private void SendRaw(byte[] raw) => _stream.Write(raw, 0, raw.Length);

        /// <summary>
        /// Reads raw payload to buffer
        /// </summary>
        /// <param name="buffer">Buffer</param>
        private void ReadRaw(ref byte[] buffer) => _stream.Read(buffer, 0, buffer.Length);

        /// <summary>
        /// Sends a Message object
        /// </summary>
        /// <param name="message">Message object</param>
        public void SendMessage(MessageBuilder.Message message)
        {
            byte[] payload = MessageBuilder.MessageToPayload(message);
            SendRaw(payload);
        }

        /// <summary>
        /// Reads a Message object
        /// </summary>
        /// <returns>Message object</returns>
        public MessageBuilder.Message ReadMessage()
        {
            byte[] sizeBuffer = new byte[4];
            ReadRaw(ref sizeBuffer);
            int size = BitConverter.ToInt32(sizeBuffer);
            byte[] messageBuffer = new byte[size];
            ReadRaw(ref messageBuffer);
            return MessageBuilder.PayloadToMessage(messageBuffer);
        }
    }
}