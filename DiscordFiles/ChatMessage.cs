using System;
using System.Collections.Generic;

namespace BunDiscordInterface{
    /// <summary>
    /// The ChatMessageHandler exists to be implemented as a base class.
    /// It contains definitions for adding Action<ChatMessage> to be invoked when a message is received.
    /// </summary>
    class ChatMessageHandler : IdBearer{
        Action<ChatMessage> OnChatMessageReceive;

        /// <summary>
        /// Adds an Action to be invoked against a ChatMessage
        /// </summary>
        /// <param name="_action">The Action to be completed on receiving a message</param>
        public void RegisterOnChannelMessageReceived(Action<ChatMessage> _action){
            OnChatMessageReceive += _action;
        }

        /// <summary>
        /// Removes an Action from the invoke list.
        /// </summary>
        /// <param name="_action">The Action to be removed</param>
        public void DeregisterOnMessageReceived(Action<ChatMessage> _action){
            OnChatMessageReceive -= _action;
        }

        public void OnChatMessageReceived(ChatMessage _message){
            Console.WriteLine("This is a function callback being ran.");
            OnChatMessageReceive?.Invoke(_message);
        }
    }

    //A ChatMessage represents a single chat message from a user in a discord channel.
    //A ChatMessage received on a Connection will fire Connection.OnChatMessageReceived,
    //  Guild.OnChatMessageReceived, and Channel.OnChatMessageReceived.
    //Ensure that rules do NOT overlap or else the logic will be run multiple times.
    class ChatMessage {
        /// <summary>
        /// The Author of the message as a 'User'
        /// </summary>
        /// <value>User: Discord User</value>
        public User Author{
            get;
            private set;
        }
        /// <summary>
        /// The string literal content inside a discord message.
        /// </summary>
        /// <value>String: The chat message text.</value>
        public string Content{
            get;
            private set;
        }

        /// <summary>
        /// The Time that the message was posted.
        /// </summary>
        /// <value>DateTime: Time of posting</value>
        public DateTime TimeStamp{
            get;
            private set;
        }


        public Channel Channel{
            get;
            private set;
        }
        public string ChannelId{
            get;
            set;
            //private set;
        }
        

        public Guild Guild{
            get;
            private set;
        }
        public string GuildId{
            get;
            set;
            //private set;
        }

        public void SetContext(string _guildId, string _channelId){
            ChannelId = _channelId;
            GuildId = _guildId;
        }

        public void SetContext(Guild _guild, Channel _channel){
            Guild = _guild;
            Channel = _channel;
        }

        public ChatMessage(User _author, string _content, DateTime _timeStamp){
            Author = _author;
            Content = _content;
            TimeStamp = _timeStamp;
        }

        public override string ToString()
        {
            string _Guild = (Guild != null) ? Guild.Name : GuildId;
            string _Channel = (Channel != null) ? Channel.Name : ChannelId;
            return $"{Author.FullName, 30} -> {_Guild,20}:{_Channel,20} |{Content}";
        }
    }
}