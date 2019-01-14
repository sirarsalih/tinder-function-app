using Microsoft.WindowsAzure.Storage.Table;

namespace TinderFunctionApp.TableEntities
{
    public class Message : TableEntity
    {
        public Message() { }

        public Message(string messageId, string userId)
        {
            PartitionKey = messageId;
            RowKey = userId;
        }
    }
}
