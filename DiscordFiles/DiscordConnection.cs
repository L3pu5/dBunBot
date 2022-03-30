using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

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
        public enum Opcode {Dispatch, Heartbeat, Identify, PresenceUpdate, VoiceStateUpdate, Resume, Reconnect, RequestGuildMembers, InvalidSession, Hello, HeartbeatAck};
        //Close Event Codes
        public enum CloseCode{UnkownError=4000, UnkownOpcode=4001, DecodeError=4002, NotAuthenticated=4003, AuthenticationFailed=4004, AlreadyAuthenticated=4005, 
        InvalidSeq=4007, RateLimited=4008, SessionTimedOut=4009, InvalidShard=4010, ShardingRequired=4011, InvalidAPIVersion=4012, InvalidIntent =4013,
        DisallowedIntent=4014};
    }


    class Connection{
        //Size of read Buffer in Bytes
        public int BufferSize = 512;
        byte[] Buffer;

        //WebSocket
        ClientWebSocket Socket;
        

        Credentials credentials;

        //This reads the Credentials into local variables. A 'Credentials.txt' is expected in the working directory.
        //This method currently requires the Credentials file to have:
        //AID:<field> (App Id)
        //PKE:<field> (Public Key)
        //TKE:<field> (Token)
        //PMI:<field> (Permissions Integer)
        void readCredentials(){
            Console.WriteLine(Directory.GetCurrentDirectory() + "\\DiscordFiles\\Credentials.txt");
            if(File.Exists(Directory.GetCurrentDirectory() + "\\DiscordFiles\\Credentials.txt")){
                using (StreamReader _reader = File.OpenText(Directory.GetCurrentDirectory() + "\\DiscordFiles\\Credentials.txt")){
                    credentials = new Credentials(_reader);
                    Console.WriteLine(credentials.GetPermissions());
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
            await Socket.ConnectAsync(new Uri(API.MakeGateWayURL(_message.GetUrl())), CancellationToken.None);
            Console.WriteLine(Socket.State);
            //Begin Read Loop
            await Read();
        }

        async Task Read()
        {
            Console.WriteLine("Staring a read.");
            Buffer = new byte[BufferSize];
            await Socket.ReceiveAsync(Buffer, CancellationToken.None);
            Console.WriteLine(System.Text.Encoding.ASCII.GetString(Buffer));
            await Read();
        }
        
        public Connection(){
            readCredentials();
            Console.WriteLine("Got Credentials");
            Task.Run(async ()=>{await MakeAsyncAPIGatewayRequest(API.BaseURL + "/gateway/bot", credentials);});
        }
    }
}