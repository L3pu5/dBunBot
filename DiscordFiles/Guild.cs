using System.Collections.Generic;

namespace DiscordConnection{
    class Guild{
        string Name;
        public void SetName(string _name){
            Name = _name;
        }
        HashSet<Channel> Channels = new HashSet<Channel>();
        string Id;
        public void SetId(string _id){
            Id = _id;
        }
        public string GetId(){
            return Id;
        }
        Channel DefaultChannel;

        string AfkChannelId = "";
        public void SetAfkChannelId(string _id){
            AfkChannelId = _id;
        }
        Channel AfkChannel;

        public void AddChannel(Channel _channel, bool isDefault=false){
            Channels.Add(_channel);
            if(isDefault){
                DefaultChannel = _channel;
            }
            if(_channel.GetId() == AfkChannelId){
                AfkChannel = _channel;
            }
        }

        public override string ToString(){
            string _channelStrings = "";
            foreach(Channel _channel in Channels){
                _channelStrings += _channel.ToString() + "\n";
            }
            return $"{this.Name}: " + _channelStrings;
        }
    }

    enum ChannelType {Text, Voice=2, Container=4}
    class Channel{
        string Name;
        public void SetName(string _name){
            Name = _name;
        }
        ChannelType Type;
        public void SetType(int _type){
            Type = (ChannelType) _type;
        }
        string Id;
        public string GetId(){
            return Id;
        }
        public void SetID(string _id){
            Id = _id;
        }

        double LastMessageId;
        public void SetLastMessageId(double _id){
            LastMessageId = _id;
        }

        public override string ToString()
        {
            return $"{this.Name} | {this.Id} ({this.Type.ToString()})";
        }
        //bool IsAfk = false;
    }
}