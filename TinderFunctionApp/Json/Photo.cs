using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TinderFunctionApp.Json
{
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
