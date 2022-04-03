using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

namespace DiscordConnection{
    //This class represents an object of a message sent from or to a discord endpoint.
    //This class does not necessarily represent a chat message and should not be used
    // for chat messages.
    //For chat messages (which will be encapsulated within a DiscordMessage)
    //Please use the ChatMessage class.
    class DiscordMessage{
        Dictionary<string, object> Data;


        //This is defined by the "t" label in a discord message wss message;

        public enum MessageType {Default, READY, GuildCreate, Message}
        MessageType TypeCode = MessageType.Default;
        public MessageType GetTypeCode(){
            return TypeCode;
        }
        //Returns the value for a given _key from a parsed data.
        //Returns null if the DiscordMessage doesn't contain the key, -> todo: throw exception.
        public object GetData(string _key)
        {
            if(Data != null){
                if(Data.ContainsKey(_key))
                    return Data[_key];
            }
            Console.WriteLine($"No such key: {_key} in Data for DiscordMessage.");
            return null;
        }

        //Adds _value to the Data.
        public void AddData(string _key, object _value){
            if(Data == null)
                Data = new Dictionary<string, object>();
            //Check later.
            Data[_key] = _value;
        }

        MessageTemplateType type;
        public void SetType(MessageTemplateType _type){
            type = _type;
        }
        GateWay.Opcode opCode;

        Configuration configuration;
        public void SetConfiguration(Configuration _config){
            configuration = _config;
        }
        Credentials credentials;
        public void SetCredentails(Credentials _credentials){
            credentials = _credentials;
        }

        public GateWay.Opcode GetOpCode(){
            return opCode;
        }
        public void SetOpCode(GateWay.Opcode _opcode){
            opCode = _opcode;
        }

        string plaintext;
        public string GetPlainText(){
            return plaintext;
        }

        public string Build(){
            MessageTemplate _message = new MessageTemplate(this);
            switch (this.type)
            {
                case MessageTemplateType.Identify:
                    MessageTemplate.PropertiesData _properties = new MessageTemplate.PropertiesData();
                    _properties.browser = configuration.GetDeviceName();
                    _properties.device = _properties.browser;
                    _properties.os = configuration.GetOperatingSystem();

                    _message.d = new MessageTemplate.IdentifyData() {token = credentials.GetToken(), intents = 519, properties = _properties};
                    break;
                case MessageTemplateType.HeartBeat:
                    _message.d = null;
                    break;
                default:
                break;
            }

            //The TypeNamehAndling must be auto in 
            //Console.WriteLine(JsonSerializer.Serialize(_message)); 
            return JsonSerializer.Serialize(_message);
        }

        public enum MessageTemplateType {Default, Identify, HeartBeat};
        class MessageTemplate{
            DiscordMessage Parent;
            public int op {set; get;}

            //This is of type DATA but requires to be of type object for serialization purposes.
            public object? d {get; set;}



            public class Data{
            }

            public class PropertiesData : Data {
                [JsonPropertyName("$os")]
                public string os {get; set;}
                [JsonPropertyName("$browser")]
                public string browser {get; set;}
                [JsonPropertyName("$device")]
                public string device {get; set;}                
            }

            public class HelloData : Data{
                public int heartbeat_interval {get;set;}
            }

            public class IdentifyData : Data{
                public string token {get; set;}
                public double intents {get; set;}
                public PropertiesData properties {get; set;}
            }

            public class HeartbeatData : Data
            {

            }

            public MessageTemplate(DiscordMessage _parent)
            {
                Parent = _parent;
                op = (int) _parent.opCode;
            }
        }
        
        public DiscordMessage(string _inputText){
            plaintext = _inputText;
            JsonDocument _json = JsonSerializer.Deserialize<JsonDocument>(_inputText);
            //Read the opcode;
            foreach(JsonProperty _property in _json.RootElement.EnumerateObject()){
                switch(_property.Name){
                    case "url":
                        AddData("url", _property.Value.GetString());
                        break;
                    case "op":
                        opCode = (GateWay.Opcode) _property.Value.GetInt32();
                        break;
                    case "t":
                        switch(_property.Value.GetString())
                        {
                            case "GUILD_CREATE":
                                this.TypeCode = MessageType.GuildCreate;
                                break;
                            case "READY":
                                this.TypeCode = MessageType.READY;
                                break;
                            case "MESSAGE_CREATE":
                                this.TypeCode = MessageType.Message;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "d":
                        //If this is a GuildCreateMessage
                        if(this.TypeCode == MessageType.GuildCreate){
                            Guild _guild = new Guild();
                            this.AddData("Guild", _guild);
                            foreach(JsonProperty __property in _property.Value.EnumerateObject()){
                                switch(__property.Name){
                                    case "name":
                                        _guild.SetName(__property.Value.GetString());
                                        break;
                                    case "afk_channel_id":
                                        _guild.SetAfkChannelId(__property.Value.GetString());
                                        break;
                                    case "id":
                                        _guild.SetId(__property.Value.GetString());
                                        break;
                                    case "channels":
                                        foreach(JsonElement _channel in __property.Value.EnumerateArray()){
                                            Channel _thisChannel = new Channel();
                                            foreach(JsonProperty _channelProperty in _channel.EnumerateObject()){
                                                switch(_channelProperty.Name){
                                                    case "type":
                                                        _thisChannel.SetType(_channelProperty.Value.GetInt32());
                                                        break;
                                                    case "name":
                                                        _thisChannel.SetName(_channelProperty.Value.GetString());
                                                        break;
                                                    case "id":
                                                        _thisChannel.SetID(_channelProperty.Value.GetString());
                                                        break;
                                                }
                                            }
                                            _guild.AddChannel(_thisChannel);
                                        }
                                        break;
                                }
                            }
                            return;
                        }

                        foreach(JsonProperty __property in _property.Value.EnumerateObject()){
                            switch (__property.Name){
                                case "heartbeat_interval":
                                    GateWay.HeartbeatInterval = __property.Value.GetInt32();
                                    break;
                                case "session_id":
                                    AddData("sessionid", __property.Value.GetString());
                                    break;
                                case "guilds":
                                    HashSet<string> _guilds = new HashSet<string>();
                                    foreach(JsonElement _object in __property.Value.EnumerateArray()){
                                        foreach(JsonProperty _guildProperty in _object.EnumerateObject()){
                                            if(_guildProperty.Name == "id"){
                                                _guilds.Add(_guildProperty.Value.GetString());
                                            }
                                        }
                                    }
                                    AddData("guilds", _guilds);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }


        public DiscordMessage(){

        }
    }
}