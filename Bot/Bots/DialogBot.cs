// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : ActivityHandler where T : Dialog 
    {
        protected readonly IStatePropertyAccessor<DialogState> DialogState;
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly ILogger Logger;
        private readonly string _botId;

        /// <summary>
        /// Create an instance of <see cref="DialogBot{T}"/>.
        /// </summary>
        /// <param name="configuration"><see cref="IConfiguration"/> used to retrieve MicrosoftAppId
        /// which is used in ContinueConversationAsync.</param>
        /// <param name="conversationState"><see cref="ConversationState"/> used to store the DialogStack.</param>
        /// <param name="dialog">The RootDialog for this bot.</param>
        /// <param name="logger"><see cref="ILogger"/> to use.</param>
        public DialogBot(IConfiguration configuration, ConversationState conversationState, T dialog, ILogger<DialogBot<T>> logger)
        {
            _botId = configuration["MicrosoftAppId"] ?? Guid.NewGuid().ToString();
            ConversationState = conversationState;
            Dialog = dialog;
            Logger = logger;
            DialogState = ConversationState.CreateProperty<DialogState>(nameof(DialogState));
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            // The event from the Azure Function will have a name of 'LongOperationResponse' 
            if (turnContext.Activity.ChannelId == Channels.Directline && turnContext.Activity.Name == "LongOperationResponse")
            {
                // The response will have the original conversation reference activity in the .Value
                // This original activity was sent to the Azure Function via Azure.Storage.Queues in AzureQueuesService.cs.
                var continueConversationActivity = (turnContext.Activity.Value as JObject)?.ToObject<Activity>();
                await turnContext.Adapter.ContinueConversationAsync(_botId, continueConversationActivity.GetConversationReference(), async (context, cancellation) =>
                {                    
                    Logger.LogInformation("Running dialog with Activity from LongOperationResponse.");

                    // ContinueConversationAsync resets the .Value of the event being continued to Null, 
                    //so change it back before running the dialog stack. (The .Value contains the response 
                    //from the Azure Function)
                    context.Activity.Value = continueConversationActivity.Value;
                    await Dialog.RunAsync(context, DialogState, cancellationToken);

                    // Save any state changes that might have occurred during the inner turn.
                    await ConversationState.SaveChangesAsync(context, false, cancellationToken);
                }, cancellationToken);
            }
            else
            {
                await base.OnEventActivityAsync(turnContext, cancellationToken);
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await Dialog.RunAsync(turnContext, DialogState, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Hi"), cancellationToken);
                }
            }
        }
    }
}
