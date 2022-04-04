using System.Collections.Generic;
using System;

namespace BunDiscordInterface{
    /// <summary>
    /// The Guild class represents a single 'Guild'. This is a collection of channels and is often referred to as a 'Discord'.
    /// </summary>
    class Guild : ChatMessageHandler{
        
        public string Name{
            get;
            private set;
        }
        
        /// <summary>
        /// This function should only be called during the creation of the Guild. 
        /// Sets the 'Name' of the guild if it is null, otherwise throws an exception.
        /// </summary>
        /// <param name="_name"></param>
        public void SetName(string _name){
            if(Name != null) throw new Exception($"Illegally tried to change the name of {Name}.");
            Name = _name;
        }
        HashSet<Channel> Channels = new HashSet<Channel>();

        /// <summary>
        /// Returns the given Channel from the ID.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public Channel GetChannelById(string _id){
            Channel _output = new Channel(_id);
            Channels.TryGetValue(_output, out _output);
            return _output;
        }

        /// <summary>
        /// Returns the Channel from a given name if it is contained in the guild.
        /// Else returns null.
        /// Keep in mind that Names in discord are case and space specific.
        /// </summary>
        /// <param name="_name"></param>
        /// <returns></returns>
        public Channel GetChannelByName(string _name){
            foreach(Channel _channel in Channels){
                if(_channel.Name == _name){
                    return _channel;
                }
            }
            return null;
        }
        public void SetId(string _id){
            Id = _id;
        }
        public string GetId(){
            return Id;
        }

        string AfkChannelId = "";
        public void SetAfkChannelId(string _id){
            AfkChannelId = _id;
        }

        //Channel DefaultChannel;
        Channel AfkChannel;

        public void AddChannel(Channel _channel, bool isDefault=false){
            Console.WriteLine($"Trying to add a guild: {_channel.ToString()} with hash {_channel.GetHashCode()}");
            Channels.Add(_channel);
            if(isDefault){
                //DefaultChannel = _channel;
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

        public Guild(string _id){
            this.Id = _id;
        }

        public Guild(){

        }
    }

    enum ChannelType {Text, Voice=2, Container=4}
    class Channel : ChatMessageHandler{
        public string Name{
            get;
            private set;
        }
        public void SetName(string _name){
            Name = _name;
        }
        ChannelType Type;
        public void SetType(int _type){
            Type = (ChannelType) _type;
        }

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
        public Channel(string _id){
            this.Id = _id;
        }

        public Channel(){}
    }
}