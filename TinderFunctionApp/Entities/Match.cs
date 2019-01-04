using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace TinderFunctionApp.Entities
{
    public class Match : TableEntity
    {
        public Match() { }

        public Match(string id, string createdDate)
        {
            PartitionKey = id;
            RowKey = createdDate;
        }
    }
}
