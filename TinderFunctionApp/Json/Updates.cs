using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinderFunctionApp.Json
{
    public class Updates
    {
        // Format ISO 8601: yyyy-MM-ddTHH:mm:ssZ
        // System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        public string last_activity_date { get; set; }
    }
}
