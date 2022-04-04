namespace BunDiscordInterface{
    /// <summary>
    /// This class represents a discord user.
    /// </summary>
    class User : IdBearer{
        /// <summary>
        /// The Username provided by the user.
        /// </summary>
        /// <value>String: 'UserName' in 'UserName.XXXX'</value>
        public string UserName{
            get;
            private set;
        }

        /// <summary>
        /// The Discriminator attached to the user.
        /// </summary>
        /// <value>String: 'XXXX' in 'UserName.XXXX'</value>
        public string Discriminator{
            get;
            private set;
        }

        /// <summary>
        /// The combination of the UserName and Discriminator.
        /// </summary>
        /// <value>String: 'Username.XXXX'</value>
        public string FullName{
            get{
                return $"{UserName}.{Discriminator}";
            }
        }

        public override string ToString()
        {
            return $"{FullName} ({this.Id})";
        }

        public User(string _username, string _discriminator, string _id){
            this.UserName = _username;
            this.Discriminator = _discriminator;
            this.Id = _id;
        }
    }
}