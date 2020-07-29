// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class LongOperationPrompt : ActivityPrompt
    {
        private readonly AzureQueuesService _queueService;
        
        public LongOperationPrompt(string dialogId, PromptValidator<Activity> validator, AzureQueuesService queueService) 
            : base(dialogId, validator)
        {
            _queueService = queueService;
        }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options, CancellationToken cancellationToken = default)
        {
            // When the dialog begins, enqueue the option chosen within the Activity queued.
            await _queueService.EnqueueActivityToProcess(dc.Context.Activity, (options as LongOperationPromptOptions).LongOperationOption, cancellationToken);

            return await base.BeginDialogAsync(dc, options, cancellationToken);
        }

        protected override Task<PromptRecognizerResult<Activity>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = default)
        {
            var result = new PromptRecognizerResult<Activity>() { Succeeded = false };
            if(turnContext.Activity.Type == ActivityTypes.Event 
                && turnContext.Activity.Name == "ContinueConversation" 
                && turnContext.Activity.Value != null
                // Custom validation within LongOperationPrompt.  
                // 'LongOperationComplete' is added to the Activity.Value in the Queue consumer (See: Azure Function)
                && turnContext.Activity.Value.ToString().Contains("LongOperationComplete", System.StringComparison.InvariantCultureIgnoreCase))
            {
                result.Succeeded = true;
                result.Value = turnContext.Activity;
            }

            return Task.FromResult(result);
        }
    }
}
