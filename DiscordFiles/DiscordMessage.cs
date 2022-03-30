using System.Text.Json;

namespace DiscordConnection{
    class DiscordMessage{
        
        GateWay.Opcode opCode;
        public GateWay.Opcode GetOpCode(){
            return opCode;
        }
        string url;
        public string GetUrl(){
            return url;
        }
        string plaintext;
        public string GetPlainText(){
            return plaintext;
        }

        public DiscordMessage(string _inputText){
            plaintext = _inputText;
            JsonDocument _json = JsonSerializer.Deserialize<JsonDocument>(_inputText);
            //Read the opcode;
            foreach(JsonProperty _property in _json.RootElement.EnumerateObject()){
                switch(_property.Name){
                    case "url":
                        url = _property.Value.GetString();
                        break;
                    case "op":
                        opCode = (GateWay.Opcode) _property.Value.GetInt32();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}