using System.Runtime.Serialization;

namespace TinderFunctionApp.Json
{
    [DataContract]
    public class Profile
    {
        [DataMember]
        public Result results { get; set; }
    }
}