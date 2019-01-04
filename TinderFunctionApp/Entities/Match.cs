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
