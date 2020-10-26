@ Documentation reference:
https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-long-operations-guidance

# Long Operations Bot

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) (version 3.1)
- [Azure Account](https://azure.microsoft.com/en-us/free/)
- [Storage Accont](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-overview/)
- [Azure Function](https://docs.microsoft.com/en-us/azure/azure-functions/) (Timer Trigger)

## Getting Started

- Create a Storage Account

- Create an Azure Function App, with a [QueueTrigger function](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger)  

- Add code for the function (run.csx, function.proj, function.json)

- Create an Azure [Web App Bot](https://docs.microsoft.com/en-us/azure/bot-service/abs-quickstart) 

- Enable the Direct Line Channel.

- Setup ngrok, and change the Messaging Endpoint for local development and testing.  [see: Debugging with Ngrok](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-debug-channel-ngrok)

- Publish code from LongOperationsBot (if not locally debugging).

- Ensure the `Direct Line` channel is enabled.

- Add app settings for Web App Bot and the Azure Function.

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

## Testing the bot using Bot Framework Emulator

- Note: an external ngrok connection is required, because the Azure Function needs to be able to call back to the bot.

[Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to the bot using Bot Framework Emulator

- Launch Bot Framework Emulator
- File -> Open Bot
- Enter a Bot URL of `http://localhost:3978/api/messages`
