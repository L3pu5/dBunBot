using System.IO;

namespace BunDiscordInterface{
    //Credentials
    class Configuration{

        string operatingSystem;
        public string GetOperatingSystem(){
            return operatingSystem;
        }
        string deviceName;
        public string GetDeviceName(){
            return deviceName;
        }

        //Reads the subsequent lines from a .txt file and parses attributes.
        public Configuration (StreamReader _reader)
        {
            operatingSystem = _reader.ReadLine().Split(":")[1];
            deviceName =  _reader.ReadLine().Split(":")[1];

        }
    }
}