namespace DiscordConnection{
    //This class represents a Discord User. 
    class User{
        public string UserName;
        public string Id;

        public override int GetHashCode()
        {
            int _hashCode = 0;
            int.TryParse(Id, out _hashCode);
            return _hashCode;
        }

        public override bool Equals(object other)
        {
            return (this.GetHashCode() == other.GetHashCode());
        }
        public static bool operator == (User a, User b){
            return a.Equals(b);
        }

        public static bool operator != (User a, User b){
            return !a.Equals(b);
        }

        public override string ToString()
        {
            return $"{this.UserName} ({this.Id})";
        }
    }
}