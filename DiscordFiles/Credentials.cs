using System.IO;
using System;

namespace BunDiscordInterface{
    //Credentials
    class Credentials{
        string AppId;
        string PublicKey;
        string Token;
        public string GetToken(){
            return Token;
        }
        double Permissions;
        public double GetPermissions(){
            return Permissions;
        }

        //Reads the subsequent lines from a .txt file and parses attributes.
        public Credentials (StreamReader _reader)
        {
            AppId = _reader.ReadLine().Substring(4);
            PublicKey = _reader.ReadLine().Substring(4);
            Token = _reader.ReadLine().Substring(4);
            Double.TryParse(_reader.ReadLine().Substring(4), out Permissions);
        }
    }
}