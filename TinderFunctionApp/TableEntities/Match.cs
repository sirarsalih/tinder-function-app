using Microsoft.WindowsAzure.Storage.Table;

namespace TinderFunctionApp.TableEntities
{
    public class Match : TableEntity
    {
        public Match() { }

        public Match(string id, string name)
        {
            PartitionKey = id;
            RowKey = name;
        }
    }
}
