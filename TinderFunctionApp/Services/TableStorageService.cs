using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

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
    }
}
