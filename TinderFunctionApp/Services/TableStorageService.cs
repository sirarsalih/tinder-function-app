using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Match = TinderFunctionApp.TableEntities.Match;

namespace TinderFunctionApp.Services
{
    public class TableStorageService
    {
        private readonly CloudTableClient _cloudTableClient;
        public TableStorageService(string storageAccountName, string storageAccountKey)
        {
            var connectionString = $"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={storageAccountKey};EndpointSuffix=core.windows.net";
            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            _cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
        }

        public CloudTable GetCloudTable(string tableName)
        {
            return _cloudTableClient.GetTableReference(tableName);
        }

        public async void InsertAsync(CloudTable cloudTable, TableEntity tableEntity)
        {
            var insertOperation = TableOperation.Insert(tableEntity);
            await cloudTable.ExecuteAsync(insertOperation);
        }

        public Match GetMatch(CloudTable cloudTable, string partitionKey)
        {
            var query = new TableQuery<Match>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            return cloudTable.ExecuteQuery(query).FirstOrDefault();
        }

        public Match GetMatch(CloudTable cloudTable, string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<Match>(partitionKey, rowKey);
            return (Match)cloudTable.Execute(retrieveOperation).Result;
        }

        public async void DeleteAsync(CloudTable cloudTable, TableEntity tableEntity)
        {
            var deleteOperation = TableOperation.Delete(tableEntity);
            await cloudTable.ExecuteAsync(deleteOperation);
        }

        public async void InsertOrReplaceAsync(CloudTable cloudTable, TableEntity tableEntity, string partitionKey, string rowKey)
        {
            tableEntity.PartitionKey = partitionKey;
            tableEntity.RowKey = rowKey;
            //"last-write-wins" strategy
            tableEntity.ETag = "*";
            var insertOrReplaceOperation = TableOperation.InsertOrReplace(tableEntity);
            await cloudTable.ExecuteAsync(insertOrReplaceOperation);
        }
    }
}
