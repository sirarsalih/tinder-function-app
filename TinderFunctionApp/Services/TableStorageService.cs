﻿using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public async void InsertAsync(CloudTable cloudTable, TableEntity tableEntity)
        {
            var insertOperation = TableOperation.Insert(tableEntity);
            await cloudTable.ExecuteAsync(insertOperation);
        }

        public Entities.Match GetMatch(CloudTable cloudTable, string id)
        {
            var query = new TableQuery<Entities.Match>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, id));
            return cloudTable.ExecuteQuery(query).FirstOrDefault();
        }
    }
}