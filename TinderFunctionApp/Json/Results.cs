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
    }

    [DataContract]
    public class Photo
    {
        [DataMember]
        public List<ProcessedFile> processedFiles { get; set; }
    }

    [DataContract]
    public class ProcessedFile
    {
        [DataMember]
        public string url { get; set; }
    }
}
