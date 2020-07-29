# Long Operations Bot

Modified https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/05.multi-turn-prompt

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) (version 3.1)
- [Azure Account](https://azure.microsoft.com/en-us/free/)
- [Storage Accont](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-overview/)
- [Azure Function](https://docs.microsoft.com/en-us/azure/azure-functions/) (Timer Trigger)

## Getting Started

- Create a Storage Account

- Create an Azure Function App, with a [QueueTrigger function](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger)  

- Add code the function (run.csx, function.proj, function.json)

- Create an Azure [Web App Bot](https://docs.microsoft.com/en-us/azure/bot-service/abs-quickstart) Publish code from LongOperationsBot

- Ensure the `Direct Line` channel is enabled.

## Sample Projects Explained

This sample contains two projects:

1) Function
    
    This is code and project files for a dotnet core Azure Queue Trigger Function which will pause 15 seconds before sending an event to the bot using the Direct Line Client.


2) LongOperationsBot
    
    This is the bot code demonstrating a LongOperationPrompt using Azure.Storage.Queues and an Azure function.  The LongOperationPrompt waits to receive notice the long operation has completed before ending.

## App Settings

The following App Settings should be configured in the Azure Function and the Web App Bot:

- MicrosoftAppId: bot microsoft app id
- MicrosoftAppPassword: bot microsoft app password
- StorageQueueName: longprocessqueue
- QueueStorageConnection: your storage connection string

The Azure function also requires a `DirectLineSecret` app setting.
