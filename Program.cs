using System;
using System.Net.Http;
using System.Threading.Tasks;
using BunDiscordInterface;
using System.IO;


namespace dBunBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Connection _connection = null;
            Log generalLog = new Log("General Log", "This is a log of the general channel.");
            while(true){
                string s = Console.ReadLine();
                Console.WriteLine(s);

                if (s== "start"){
                    Task.Run ( () => { _connection =  new Connection();});
                }

                if (s == "a"){
                    Console.WriteLine("heartbeat");
                    DiscordMessage _test = _connection.MakeMessage(DiscordMessage.MessageTemplateType.HeartBeat);
                    _test.SetOpCode(GateWay.Opcode.Heartbeat);
                    Task.Run(() => _connection.SendMessage(_test));
                }

                if( s=="b"){
                    Console.WriteLine(_connection.Socket.State);
                }

                if (s == "guilds")
                {
                    Console.WriteLine(_connection.PrintGuilds());
                }

                if(s=="makelog"){
                    Guild BotTest2 = _connection.GetGuildByName("BotTest2");
                    Channel general = BotTest2.GetChannelByName("general");
                    general.RegisterOnChannelMessageReceived( (_message) => {generalLog.AddMessage(_message); Console.WriteLine("Logging this message!"); Console.WriteLine(_message.ToString());});
                }

                if(s=="savelog")
                {
                    generalLog.WriteToFile(Directory.GetCurrentDirectory() + "/LogFile.txt");
                }

                if(s.Split(' ')[0] == "msg"){
                    ApiMessage _message = new ApiMessage("This is a test message.");
                    Guild BotTest2 = _connection.GetGuildByName("BotTest2");
                    if(BotTest2 != null){
                        Channel general = BotTest2.GetChannelByName("general");
                        if(general != null){
                            Task.Run(() => _connection.MakeAsyncApiMessage(general, _message));
                        }
                    }

                }
            }
        }
    }
}
