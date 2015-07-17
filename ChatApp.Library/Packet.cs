using System;
using System.Collections.Generic;
using System.Text;
using Catel.Data;

namespace ChatApp.Library
{
    public class Packet : ModelBase
    {

        #region Properties
        /// <summary>
        /// Gets or sets the ChatDataIdentifier.
        /// </summary>
        public DataIdentifier ChatDataIdentifier
        {
            get { return GetValue<DataIdentifier>(ChatDataIdentifierProperty); }
            set { SetValue(ChatDataIdentifierProperty, value); }
        }

        /// <summary>
        /// Register the ChatDataIdentifier property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ChatDataIdentifierProperty = RegisterProperty("ChatDataIdentifier", typeof(DataIdentifier), null);


        /// <summary>
        /// Gets or sets the ChatName.
        /// </summary>
        public string ChatName
        {
            get { return GetValue<string>(ChatNameProperty); }
            set { SetValue(ChatNameProperty, value); }
        }

        /// <summary>
        /// Register the FamilyName property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ChatNameProperty = RegisterProperty("ChatName", typeof(string), null);


        /// <summary>
        /// Gets or sets the Chat Message.
        /// </summary> 
        public string ChatMessage
        {
            get { return GetValue<string>(ChatMessageProperty); }
            set { SetValue(ChatMessageProperty, value); }
        }

        /// <summary>
        /// Register the FamilyName property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ChatMessageProperty = RegisterProperty("ChatMessage", typeof(string), null); 
        #endregion

        #region Constructor
        public Packet()
        {
            ChatDataIdentifier = DataIdentifier.Null;
            ChatMessage = null;
            ChatName = null;
        }

        public Packet(byte[] dataStream)
        {
            // Read the data identifier from the beginning of the stream (4 bytes)
            ChatDataIdentifier = (DataIdentifier)BitConverter.ToInt32(dataStream, 0);

            // Read the length of the name (4 bytes)
            int nameLength = BitConverter.ToInt32(dataStream, 4);

            // Read the length of the message (4 bytes)
            int msgLength = BitConverter.ToInt32(dataStream, 8);

            // Read the name field
            ChatName = nameLength > 0 ? Encoding.UTF8.GetString(dataStream, 12, nameLength) : null;

            // Read the message field
            ChatMessage = msgLength > 0 ? Encoding.UTF8.GetString(dataStream, 12 + nameLength, msgLength) : null;
        }

        #endregion

        #region Method
        // Converts the packet into a byte array for sending/receiving 
        public byte[] GetDataStream()
        {
            var dataStream = new List<byte>();

            // Add the dataIdentifier
            dataStream.AddRange(BitConverter.GetBytes((int)ChatDataIdentifier));

            // Add the name length
            dataStream.AddRange(ChatName != null ? BitConverter.GetBytes(ChatName.Length) : BitConverter.GetBytes(0));

            // Add the message length
            dataStream.AddRange(ChatMessage != null
                ? BitConverter.GetBytes(ChatMessage.Length)
                : BitConverter.GetBytes(0));

            // Add the name
            if (ChatName != null)
                dataStream.AddRange(Encoding.UTF8.GetBytes(ChatName));

            // Add the message
            if (ChatMessage != null)
                dataStream.AddRange(Encoding.UTF8.GetBytes(ChatMessage));

            return dataStream.ToArray();
        }
        #endregion
    }


    public enum DataIdentifier
    {
        Message,
        LogIn,
        LogOut,
        Null
    }
}
