using System.Collections.Generic;

namespace DiscordConnection{
    class Guild{
        string Name;
        HashSet<Channel> Channels;
        string OwnerId;
    }

    enum ChannelType {Text, Voice=2, Container=4}
    class Channel{
        string Name;
        ChannelType Type;
        string Id;
        string LastMessageId;
        bool IsAfk = false;
    }
}