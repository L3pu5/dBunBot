namespace BunDiscordInterface{
    /// <summary>
    /// This is a internal class used for the comparison of objects assigned an ID by discord.
    /// </summary>
    class IdBearer{
        protected string Id;

        /// <summary>
        /// Provides a Hash of the String ID provided by Discord.
        /// This is NOT session safe. It will be different between instances
        /// but should remain static for the lifetime of a program.
        /// </summary>
        /// <returns>Int32: Hash</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        
        public override bool Equals(object other)
        {
            if(other == null) return false;
            
            return (this.GetHashCode() == other.GetHashCode());
        }
        public static bool operator == (IdBearer a, IdBearer b){
            return a.Equals(b);
        }

        public static bool operator != (IdBearer a, IdBearer b){
            return !a.Equals(b);
        }   
    }
}