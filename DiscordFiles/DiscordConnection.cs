using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace DiscordConnection
{

    //Static API Handler
    public static class API{
        public static string BaseURL = "https://discord.com/api";
        public static string UserAgent = "DiscordBot (https://github.com/L3pu5/dBunBot, 0.0)";
        public static string MakeGateWayURL(string _base){
            return _base + $"/?v={GateWay.Version}&encoding={GateWay.Encoding}";
        }
    }
    //This handles the Gateway for the Discord Messages with the bot.
    public static class GateWay{
        //Used Version
        public static int Version = 9;
        //Encoding,
        public static string Encoding = "json";
        //Opening Codes
        public enum Opcode {Dispatch, Heartbeat, Identify, PresenceUpdate, VoiceStateUpdate, NULL, Resume, Reconnect, RequestGuildMembers, InvalidSession, Hello, HeartbeatAck};
        //Close Event Codes
        public enum CloseCode{UnkownError=4000, UnkownOpcode=4001, DecodeError=4002, NotAuthenticated=4003, AuthenticationFailed=4004, AlreadyAuthenticated=4005, 
        InvalidSeq=4007, RateLimited=4008, SessionTimedOut=4009, InvalidShard=4010, ShardingRequired=4011, InvalidAPIVersion=4012, InvalidIntent =4013,
        DisallowedIntent=4014};
        public static int HeartbeatCounter = 0;
        public static int HeartbeatInterval = 0;
    }


    class Connection{
        //Size of read Buffer in Bytes
        public int BufferSize = 5000;

        //WebSocket
        public ClientWebSocket Socket;
        CancellationTokenSource _cancelToken = new CancellationTokenSource();
        
        //Session Variables
        public string SessionId;
        public HashSet<string> Guilds;
        int DispatchCount = 0;

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
        void readCredentials(){
            //Credentials
            if(File.Exists(Directory.GetCurrentDirectory() + "\\DiscordFiles\\Credentials.txt")){
                using (StreamReader _reader = File.OpenText(Directory.GetCurrentDirectory() + "\\DiscordFiles\\Credentials.txt")){
                    credentials = new Credentials(_reader);
                    Console.WriteLine(credentials.GetPermissions());
                }
            }
            //Config.txt
            if(File.Exists(Directory.GetCurrentDirectory() + "\\DiscordFiles\\BotConfig.txt")){
                using (StreamReader _reader = File.OpenText(Directory.GetCurrentDirectory() + "\\DiscordFiles\\BotConfig.txt")){
                    configuration = new Configuration(_reader);
                }
            }
        }

        public async Task MakeAsyncAPIRequest(string _request){
            HttpClient _client = new HttpClient();
            //Add Specific Headers;
            _client.DefaultRequestHeaders.Add("UserAgent", API.UserAgent);
            _client.DefaultRequestHeaders.Add("Authorization", "Bot " + credentials.GetToken());

            string _response = await _client.GetStringAsync(_request);
            Console.WriteLine(_response);
        }
        public async Task MakeAsyncAPIGatewayRequest(string _request, Credentials _credentials){
            HttpClient _client = new HttpClient();
            //Add Specific Headers;
            _client.DefaultRequestHeaders.Add("UserAgent", API.UserAgent);
            _client.DefaultRequestHeaders.Add("Authorization", "Bot " + _credentials.GetToken());

            DiscordMessage _message = new DiscordMessage(await _client.GetStringAsync(_request));
            Console.WriteLine(_message.GetPlainText());



            //Connect to the Gateway
            Socket = new ClientWebSocket();
            Console.WriteLine("Sock: " + Socket.State);
            await Socket.ConnectAsync(new Uri(API.MakeGateWayURL(_message.GetData("url"))), CancellationToken.None);
            Console.WriteLine(Socket.State);
            //Start the Reading process - we exept an op 10 hello to arrive immediately;
            await Read();
        }

        async Task Read()
        {
            Console.WriteLine("Staring a read.");
            byte[] _buffer = new byte[BufferSize];
            //Read the bytes received from the socket Async
            WebSocketReceiveResult _result = await Socket.ReceiveAsync(_buffer, CancellationToken.None);
            //Start the next read
            //Handle the data of this read
            string _jsonResponse = System.Text.Encoding.UTF8.GetString(_buffer, 0, _result.Count);
            //Make a DiscordMessage Object
            Console.WriteLine("Got a message!");
            if(Socket.CloseStatusDescription != "")
                Console.WriteLine("Socket closed: " + Socket.CloseStatusDescription);
            Console.WriteLine(_jsonResponse);
            DiscordMessage _message = new DiscordMessage(_jsonResponse);
            //Handle the message.
            await HandleReceivedMessage(_message);
            await Read();
        }

        //Creates a new, empty discord message with the credentials/configuration of the connection.
        public DiscordMessage MakeMessage(DiscordMessage.MessageTemplateType _type)
        {
            DiscordMessage _message = new DiscordMessage();
            _message.SetConfiguration (configuration);
            _message.SetCredentails (credentials);
            _message.SetType(_type);
            return _message;
        }

        public async Task SendMessage(DiscordMessage _message){
            if(Socket == null){
                //Add to a buffer.
                UnsentMessages.Add(_message);
                return;
            }
            Console.WriteLine("Sending message!");
            Console.WriteLine("message: " + _message.Build());
            byte[] _buffer = System.Text.ASCIIEncoding.UTF8.GetBytes(_message.Build());
            ArraySegment<byte> _segment = new ArraySegment<byte>(_buffer);
            await Socket.SendAsync(_segment, WebSocketMessageType.Binary, true, CancellationToken.None);
            Console.WriteLine("SENT!");
        }

        void Flush(){
            Console.WriteLine("Flush");
            foreach(DiscordMessage _message in UnsentMessages){
                byte[] _buffer = System.Text.ASCIIEncoding.UTF8.GetBytes(_message.Build());
                ArraySegment<byte> _segment = new ArraySegment<byte>(_buffer);
                Socket.SendAsync(_segment, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            UnsentMessages = null;
        }

        public async Task HandleReceivedMessage(DiscordMessage _message){
            switch(_message.GetOpCode()){
                case GateWay.Opcode.Hello:
                    //Make our Identify message
                    DiscordMessage _identify = MakeMessage(DiscordConnection.DiscordMessage.MessageTemplateType.Identify);
                    //Set the OP Code
                    _identify.SetOpCode(GateWay.Opcode.Identify);
                    //Send
                    await SendMessage(_identify);
                    break;
                case GateWay.Opcode.Dispatch:
                    if(DispatchCount > 0){
                        return;
                    }
                    DispatchCount++;
                    Console.WriteLine("Received Dispatch");
                    //read Data
                    SessionId = _message.GetData("sessionid");
                    Guilds = new HashSet<string>();
                    foreach(string _guildId in _message.GetData("guilds").Split('&')){
                        Guilds.Add(_guildId);
                    }
                    Console.WriteLine($"The dispatch was read. We are in: {Guilds.Count - 1} guilds. Our session id is {SessionId}.");
                    break;
    
            }
        }

    
        
        public Connection(){
            readCredentials();
            Console.WriteLine("Got Credentials");
            Task.Run(async ()=>{await MakeAsyncAPIGatewayRequest(API.BaseURL + "/gateway/bot", credentials);});
        }
    }
}