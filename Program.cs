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

            Task.Run( async () => {             HttpClient _client = new HttpClient();
            HttpResponseMessage _response = await _client.GetAsync("https://discord.com/api/gateway/bot");
            string _data = await _response.Content.ReadAsStringAsync();
            Console.WriteLine(_data); });
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
