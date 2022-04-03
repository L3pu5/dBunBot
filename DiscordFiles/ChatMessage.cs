using System;
using System.Collections.Generic;

namespace DiscordConnection{
    class ChatMessage {
        User Author;
        string Content;

        public ChatMessage(User _author, string _content){
            Author = _author;
            Content = _content;
        }
    }
}