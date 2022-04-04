using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using System.IO;

namespace BunDiscordInterface{
    class Log : IEnumerable{
        List<ChatMessage> Messages = new List<ChatMessage>();

        public string Name {get; private set;} = "";
        public void SetName(string _name){
            Name = _name;
        }
        public string Description {get; private set;}= "";
        public void SetDescription(string _description){
            Description = _description;
        }

        public int Count {get{
            return Messages.Count();
        }}

        public void AddMessage(ChatMessage _message)
        {
            Console.WriteLine("Adding the message " + _message.ToString());
            Messages.Add(_message);
            Console.WriteLine("Added! We have .." + Messages.Count + " messages!");
        }

        public ChatMessage GetMessage(int _index){
            if(Messages.Count < _index)
                return Messages[_index];
            return null;
        }

        public List<ChatMessage> GetMessagesBy(User _user){
            return Messages.Where(x => x.Author == _user).ToList<ChatMessage>();
        }
        
        public List<ChatMessage> GetMessagesBy(string _FullUserName){
            return Messages.Where(x => x.Author.FullName == _FullUserName).ToList<ChatMessage>();
        }

        /// <summary>
        /// This function exports the log in its buffer by writing it in a generic format to a user defined path.
        /// </summary>
        /// <param name="_path">String: The full path, including file name and extention to save the file as.</param>
        public void WriteToFile(string _path){
            using (StreamWriter _iostream =  new StreamWriter (File.Create(_path)) )
            {
                _iostream.WriteLine($"Log file of {Name} created at {DateTime.Now.Date}, {DateTime.Now.TimeOfDay}.");
                _iostream.WriteLine($"{Description}");
                _iostream.WriteLine("----------------------------------------------------------------------------");
                foreach(ChatMessage _message in Messages){
                    _iostream.WriteLine(_message.ToString());
                }
            }
        }

        //Enumeration
        IEnumerator IEnumerable.GetEnumerator(){
            return (IEnumerator) GetEnumerator();
        }

        public LogEnum GetEnumerator(){
            return new LogEnum(Messages);
        }

        public Log(string _name, string _description){
            Name = _name;
            Description = _description;
        }

        public Log(){

        }
    }

    class LogEnum : IEnumerator{
        public List<ChatMessage> messages;
        int _index = -1;
        
        public LogEnum (List<ChatMessage> _list){
            messages = _list;
        }

        public bool MoveNext(){
            _index++;
            return (_index < messages.Count);
        }

        public void Reset(){
            _index = -1;
        }

        object IEnumerator.Current{
            get{
                return Current;
            }
        }

        public ChatMessage Current {
            get{
                try{
                    return messages[_index];
                }
                catch(IndexOutOfRangeException){
                    throw new InvalidOperationException();
                }
            }
        }
    }
}