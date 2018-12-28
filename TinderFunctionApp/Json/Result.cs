using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TinderFunctionApp.Json
{
    [DataContract]
    public class Result
    {
        [DataMember]
        public string _id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string distance_mi { get; set; }
    }
}
