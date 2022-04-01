using System;
using System.Net.Http;
using System.Threading.Tasks;
using DiscordConnection;


namespace dBunBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Connection _connection = null;
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

                if (s == "c")
                {
                    //DiscordMessage _test = _connection.MakeMessage();
                }
            }
        }
    }
}
