// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Storage.Queues;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// Service used to add messages to an Azure.Storage.Queues.
    /// </summary>
    public class AzureQueuesService
    {
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings() 
            { 
                Formatting = Formatting.Indented, 
                NullValueHandling = NullValueHandling.Ignore 
            };

        private bool _createQueuIfNotExists = true;
        private readonly QueueClient _queueClient;

        /// <summary>
        /// Creates a new instance of <see cref="AzureQueuesService"/>.
        /// </summary>
        /// <param name="config"><see cref="IConfiguration"/> used to retrieve 
        /// StorageQueueName and QueueStorageConnection from appsettings.json.</param>
        public AzureQueuesService(IConfiguration config)
        {
            var queueName = config["StorageQueueName"];
            var connectionString = config["QueueStorageConnection"];

            _queueClient = new QueueClient(connectionString, queueName);
        }

        /// <summary>
        /// Queue and Activity, with option in the Activity.Value to Azure.Storage.Queues
        /// 
        /// <seealso cref="https://github.com/microsoft/botbuilder-dotnet/blob/master/libraries/Microsoft.Bot.Builder.Azure/Queues/ContinueConversationLater.cs"/>
        /// </summary>
        /// <param name="referenceActivity">Activity to queue after a call to GetContinuationActivity.</param>
        /// <param name="option">The option the user chose, which will be passed within the .Value of the activity queued.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Queued <see cref="Azure.Storage.Queues.Models.SendReceipt.MessageId"/>.</returns>
        public async Task<string> QueueActivityToProcess(Activity referenceActivity, string option, CancellationToken cancellationToken)
        {
            if (_createQueuIfNotExists)
            {
                _createQueuIfNotExists = false;
                await _queueClient.CreateIfNotExistsAsync().ConfigureAwait(false);
            }

            // create ContinuationActivity from the conversation reference.
            var activity = referenceActivity.GetConversationReference().GetContinuationActivity();
            // Pass the user's choice in the .Value
            activity.Value = option;

            var message = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(activity, jsonSettings)));
             
            // Aend ResumeConversation event, it will get posted back to us with a specific value, giving us 
            // the ability to process it and do the right thing.
            var reciept = await _queueClient.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
            return reciept.Value.MessageId;
        }
    }
}
