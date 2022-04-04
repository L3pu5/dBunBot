using System.Text.Json;

namespace BunDiscordInterface{
    class ApiMessage{
        public string content {get; set;}
        public bool tts {get; set;} = false;
        public object[] embeds {get; set;} = new object[0];

        public void SetContent(string _content){
            content = _content;
        }

        public void AddEmbed(object _embed){
            object[] _newEmbeds = new object[embeds.Length + 1];
            embeds.CopyTo(_newEmbeds, 0);
            _newEmbeds[embeds.Length] = _embed;
            embeds = _newEmbeds;
        }

        public string  Build(){
            return JsonSerializer.Serialize(this);
        }

        public ApiMessage (string _content){
            content = _content;
        }
    }
}