using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TinderFunctionApp.Json
{
    [DataContract]
    public class Updates
    {
        [DataMember]
        public List<Match> matches { get; set; }
    }

    [DataContract]
    public class Match
    {
        [DataMember]
        public string _id { get; set; }

        [DataMember]
        public Seen seen { get; set; }

        [DataMember]
        public bool closed { get; set; }

        [DataMember]
        public int common_friend_count { get; set; }

        [DataMember]
        public string created_date { get; set; }

        [DataMember]
        public bool dead { get; set; }

        [DataMember]
        public string last_activity_date { get; set; }

        [DataMember]
        public int message_count { get; set; }

        [DataMember]
        public List<Message> messages { get; set; }

        [DataMember]
        public Person person { get; set; }
    }

    [DataContract]
    public class Seen
    {
        [DataMember]
        public bool match_seen { get; set; }
    }

    [DataContract]
    public class Person
    {
        [DataMember]
        public string _id { get; set; }

        [DataMember]
        public string birth_date { get; set; }

        [DataMember]
        public int gender { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public List<Photo> photos { get; set; }
    }

    [DataContract]
    public class Message
    {
        [DataMember]
        public string message { get; set; }

        [DataMember]
        public string from { get; set; }

        [DataMember]
        public string to { get; set; }

        [DataMember]
        public string created_date { get; set; }
    }
}
