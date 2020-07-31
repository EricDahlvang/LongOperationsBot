#r "Newtonsoft.Json"

using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Bot.Connector.DirectLine;
using System.Threading;

public static async Task Run(string queueItem, ILogger log)
{
    log.LogInformation($"C# Queue trigger function processing");

    JsonSerializerSettings jsonSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
    var originalActivity =  JsonConvert.DeserializeObject<Activity>(queueItem, jsonSettings);

    // Perform long operation here....
    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(15));

    // Return result within the original activity value
    if(originalActivity.Value.ToString() == "option 1") 
    {
        originalActivity.Value = "(Result for long operation one!)";
    }
    else if(originalActivity.Value.ToString() == "option 2") 
    {
        originalActivity.Value = "(A different result for operation two!)";
    }

    originalActivity.Value = "LongOperationComplete: " + originalActivity.Value;

    // Create the 'event' activity which will hold the original activity in the .Value.
    var responseActivity =  new Activity("event");
    responseActivity.Value = originalActivity;
    responseActivity.Name = "LongOperationResponse";
    responseActivity.From = new ChannelAccount("GenerateReport", "AzureFunction");

    // Lastly, send the 'event' to the bot using the DirectLineClient.
    var directLineSecret = Environment.GetEnvironmentVariable("DirectLineSecret");            
    using(DirectLineClient client = new DirectLineClient(directLineSecret)) 
    {
        var conversation = await client.Conversations.StartConversationAsync();
        await client.Conversations.PostActivityAsync(conversation.ConversationId, responseActivity);
    }

    log.LogInformation($"Done...");
}
