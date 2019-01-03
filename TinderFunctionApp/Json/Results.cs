using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
    }
}
