using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TinderFunctionApp.Json
{
    [DataContract]
    public class Results
    {
        [DataMember]
        public List<Result> results { get; set; }
    }

    [DataContract]
    public class Result
    {
        [DataMember]
        public string _id { get; set; }

        [DataMember]
        public string birth_date { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public int gender { get; set; }

        [DataMember]
        public string distance_mi { get; set; }

        [DataMember]
        public List<Photo> photos { get; set; }

        [DataMember]
        public List<Job> jobs { get; set; }

        [DataMember]
        public List<School> schools { get; set; }
    }

    [DataContract]
    public class Job
    {
        [DataMember]
        public Company company { get; set; }

        [DataMember]
        public Title title { get; set; }
    }

    [DataContract]
    public class Company
    {
        [DataMember]
        public string name { get; set; }
    }

    [DataContract]
    public class Title
    {
        [DataMember]
        public string name { get; set; }
    }

    [DataContract]
    public class School
    {
        [DataMember]
        public string id { get; set; }

        [DataMember]
        public string name { get; set; }
    }
}
