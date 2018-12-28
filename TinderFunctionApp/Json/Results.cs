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
}
