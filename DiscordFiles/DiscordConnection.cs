using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace BunDiscordInterface
{

    //Static API Handler
    public static class API
    {
        public static string BaseURL = "https://discord.com/api";
        public static string UserAgent = "DiscordBot (https://github.com/L3pu5/dBunBot, 0.0)"; 
        public static string MakeGateWayURL(string _base)
        {
            return _base + $"/?v={GateWay.Version}&encoding={GateWay.Encoding}";
        }
    }
    //This handles the Gateway for the Discord Messages with the bot.
    public static class GateWay
    {
        //Used Version
        public static int Version = 9;
        //Encoding,
        public static string Encoding = "json";
        //Opening Codes
        public enum Opcode { Dispatch, Heartbeat, Identify, PresenceUpdate, VoiceStateUpdate, NULL, Resume, Reconnect, RequestGuildMembers, InvalidSession, Hello, HeartbeatAck };
        //Close Event Codes
        public enum CloseCode
        {
            UnkownError = 4000, UnkownOpcode = 4001, DecodeError = 4002, NotAuthenticated = 4003, AuthenticationFailed = 4004, AlreadyAuthenticated = 4005,
            InvalidSeq = 4007, RateLimited = 4008, SessionTimedOut = 4009, InvalidShard = 4010, ShardingRequired = 4011, InvalidAPIVersion = 4012, InvalidIntent = 4013,
            DisallowedIntent = 4014
        };
        public static int HeartbeatCounter = 0;
        public static int HeartbeatInterval = 0;
    }


    ///<Summary>The Connection class represents a single Websocket enabled connection.
    ///Since each WSS requires identification with a single bot or service,
    ///The Connection can be thought of as the instance of a single bot.
    ///</Summary>
    class Connection : ChatMessageHandler
    {
        //Size of read Buffer in Bytes
        public int BufferSize = 5000;

        //WebSocket
        public ClientWebSocket Socket;
        public Timer HeartBeat;
        //Session Variables
        public HashSet<Guild> Guilds = new HashSet<Guild>();
        /// <summary>
        /// Gets a guild by name. Note. Name is case sensitive and specific.
        /// </summary>
        /// <param name="_name"></param>
        /// <returns></returns>
        public Guild GetGuildByName(string _name){
            foreach(Guild _guild in Guilds){
                if (_guild.Name == _name){
                    return _guild;
                }
            }
            return null;
        }
        /// <summary>
        /// Gets a guild by id.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public Guild GetGuildById(string _id){
            foreach(Guild _guild in Guilds){
                if (_guild.GetId() == _id){
                    return _guild;
                }
            }
            return null;
        }
        int DispatchCount = 0;

        /// <summary>
        /// Represents the number of connections between the server and connection.
        /// This is sent as the data for the heartbeat.
        /// </summary>
        int SequenceCounter = 0;

        //Bot Credentials
        Credentials credentials;
        Configuration configuration;


        //Buffered messages not sent
        HashSet<DiscordMessage> UnsentMessages = new HashSet<DiscordMessage>();



        //This reads the Credentials into local variables. A 'Credentials.txt' is expected in the working directory.
        //This method currently requires the Credentials file to have:
        //AID:<field> (App Id)
        //PKE:<field> (Public Key)
        //TKE:<field> (Token)
        //PMI:<field> (Permissions Integer)
        void readCredentials()
        {
            //Credentials
            if (File.Exists(Directory.GetCurrentDirectory() + "\\DiscordFiles\\Credentials.txt"))
            {
                using (StreamReader _reader = File.OpenText(Directory.GetCurrentDirectory() + "\\DiscordFiles\\Credentials.txt"))
                {
                    credentials = new Credentials(_reader);
                    Console.WriteLine(credentials.GetPermissions());
                }
            }
            //Config.txt
            if (File.Exists(Directory.GetCurrentDirectory() + "\\DiscordFiles\\BotConfig.txt"))
            {
                using (StreamReader _reader = File.OpenText(Directory.GetCurrentDirectory() + "\\DiscordFiles\\BotConfig.txt"))
                {
                    configuration = new Configuration(_reader);
                }
            }
        }

        public async Task MakeAsyncAPIRequest(string _request)
        {
            HttpClient _client = new HttpClient();
            //Add Specific Headers;
            _client.DefaultRequestHeaders.Add("UserAgent", API.UserAgent);
            _client.DefaultRequestHeaders.Add("Authorization", "Bot " + credentials.GetToken());
            string _response = await _client.GetStringAsync(_request);
        }

        public async Task MakeAsyncAPIPOST(string _request, ApiMessage _message){
            HttpClient _client = new HttpClient();
            //Add Specific Headers;
            _client.DefaultRequestHeaders.Add("UserAgent", API.UserAgent);
            _client.DefaultRequestHeaders.Add("Authorization", "Bot " + credentials.GetToken());
            StringContent _messageData = new StringContent(_message.Build(), System.Text.Encoding.UTF8, "application/json");
            //Console.WriteLine(_request);
            //Console.WriteLine(_messageData.ToString());
            HttpResponseMessage _response = await _client.PostAsync(_request, _messageData);
            //Console.WriteLine(_response.StatusCode);
            string _content = await _response.Content.ReadAsStringAsync();
            //Console.WriteLine(_content);
        }

        public async Task MakeAsyncApiMessage(Channel _channel, ApiMessage _message){
            await MakeAsyncAPIPOST(API.BaseURL + "/channels/" + _channel.GetId() + "/messages", _message);
        }

        public async Task MakeAsyncApiMessage(string _channel, ApiMessage _message){
            await MakeAsyncAPIPOST(API.BaseURL + "/channels/" + _channel +  "/messages", _message);
        }

        public async Task MakeAsyncAPIGatewayRequest(string _request, Credentials _credentials)
        {
            HttpClient _client = new HttpClient();
            //Add Specific Headers;
            _client.DefaultRequestHeaders.Add("UserAgent", API.UserAgent);
            _client.DefaultRequestHeaders.Add("Authorization", "Bot " + _credentials.GetToken());

            DiscordMessage _message = new DiscordMessage(await _client.GetStringAsync(_request));



            //Connect to the Gateway
            Socket = new ClientWebSocket();
            await Socket.ConnectAsync(new Uri(API.MakeGateWayURL((string)_message.GetData("url"))), CancellationToken.None);
            await Read();
        }

        async Task Read()
        {
            byte[] _buffer = new byte[BufferSize];
            //Read the bytes received from the socket Async
            WebSocketReceiveResult _result = await Socket.ReceiveAsync(_buffer, CancellationToken.None);
            //Start the next read
            //Handle the data of this read
            string _jsonResponse = System.Text.Encoding.UTF8.GetString(_buffer, 0, _result.Count);
            //Make a DiscordMessage Object
            if (Socket.CloseStatusDescription != "")
                Console.WriteLine("Socket closed: " + Socket.CloseStatusDescription);
                Console.WriteLine(_jsonResponse);
            try
            {
                DiscordMessage _message = new DiscordMessage(_jsonResponse);
                await HandleReceivedMessage(_message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString() + "\n");
            }
            //Handle the message.
            await Read();
        }

        //Creates a new, empty discord message with the credentials/configuration of the connection.
        public DiscordMessage MakeMessage(DiscordMessage.MessageTemplateType _type)
        {
            DiscordMessage _message = new DiscordMessage();
            _message.SetConfiguration(configuration);
            _message.SetCredentails(credentials);
            _message.SetType(_type);
            _message.SetSequence(SequenceCounter);
            return _message;
        }

        public async Task SendMessage(DiscordMessage _message)
        {
            if (Socket == null){
                //Add to a buffer.
                UnsentMessages.Add(_message);
                return;
            }
            byte[] _buffer = System.Text.ASCIIEncoding.UTF8.GetBytes(_message.Build());
            ArraySegment<byte> _segment = new ArraySegment<byte>(_buffer);
            await Socket.SendAsync(_segment, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        void Flush()
        {
            foreach (DiscordMessage _message in UnsentMessages)
            {
                byte[] _buffer = System.Text.ASCIIEncoding.UTF8.GetBytes(_message.Build());
                ArraySegment<byte> _segment = new ArraySegment<byte>(_buffer);
                Socket.SendAsync(_segment, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            UnsentMessages = null;
        }

        public async Task HandleReceivedMessage(DiscordMessage _message)
        {
            //Update Sequence
            if(_message.GetSequence() != -1)
                SequenceCounter = _message.GetSequence();

            switch (_message.GetOpCode())
            {
                case GateWay.Opcode.Hello:
                    //Handle the HeartBeat.
                    HeartBeat = new Timer( async (a) => { DiscordMessage _message = MakeMessage(BunDiscordInterface.DiscordMessage.MessageTemplateType.HeartBeat); _message.SetOpCode(GateWay.Opcode.Heartbeat); await SendMessage(_message);}, this, (int) _message.GetData("HeartBeatInterval"), (int) _message.GetData("HeartBeatInterval"));
                    //Make our Identify message
                    DiscordMessage _identify = MakeMessage(BunDiscordInterface.DiscordMessage.MessageTemplateType.Identify);
                    //Set the OP Code
                    _identify.SetOpCode(GateWay.Opcode.Identify);
                    //Send
                    await SendMessage(_identify);
                    break;
                case GateWay.Opcode.Dispatch:
                    switch (_message.GetTypeCode())
                    {
                        case DiscordMessage.MessageType.GuildCreate:
                            Guilds.Add((Guild)_message.GetData("Guild"));
                            break;
                        case DiscordMessage.MessageType.READY:
                            Id = (string)_message.GetData("sessionid");
                            break;
                        case DiscordMessage.MessageType.Message:
                            ChatMessage _chatMessage = (ChatMessage) _message.GetData("ChatMessage");
                            //Resolve the Guild and Channel.
                            Guild _guild = new Guild(_chatMessage.GuildId);
                            Guilds.TryGetValue(_guild, out _guild);
                            Channel _channel = _guild.GetChannelById(_chatMessage.ChannelId);
                            //Update the internal data of the Chatmessage
                            _chatMessage.SetContext(_guild, _channel);
                            //Invoke the Handlers
                            this.OnChatMessageReceived(_chatMessage);
                            _guild.OnChatMessageReceived(_chatMessage);
                            _channel.OnChatMessageReceived(_chatMessage);
                            break;
                        default:

                            break;
                    }
                    DispatchCount++;
                    //Console.WriteLine("Received Dispatch");
                    //read Data
                    break;

            }
        }

        //Returns a String containing the .ToString() method of each guild: 
        // 1 Guild per line.
        public string PrintGuilds()
        {
            if (Guilds.Count == 0)
            {
                return "No guilds are in this connection.";
            }

            string _output = "";
            foreach (Guild _guild in Guilds)
            {
                _output += _guild.ToString() + "\n";
            }
            return _output;
        }

        public Connection()
        {
            readCredentials();
            Task.Run(async () => { await MakeAsyncAPIGatewayRequest(API.BaseURL + "/gateway/bot", credentials); });
        }
    }
}