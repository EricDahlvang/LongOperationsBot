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
    /// <summary>
    /// <see cref="ActivityPrompt"/> implementation which will queue an activity,
    /// along with the <see cref="LongOperationPromptOptions.LongOperationOption"/>,
    /// and wait for an <see cref="ActivityTypes.Event"/> with name of "ContinueConversation"
    /// and Value containing the text: "LongOperationComplete".
    /// 
    /// The result of this prompt will be the received Event Activity, which is sent by 
    /// the Azure Function after it finishes the long operation.
    /// </summary>
    public class LongOperationPrompt : ActivityPrompt
    {
        private readonly AzureQueuesService _queueService;
        
        /// <summary>
        /// Create a new instance of <see cref="LongOperationPrompt"/>.
        /// </summary>
        /// <param name="dialogId">Id of this <see cref="LongOperationPrompt"/>.</param>
        /// <param name="validator">Validator to use for this prompt.</param>
        /// <param name="queueService"><see cref="AzureQueuesService"/> to use for Enqueuing the activity to process.</param>
        public LongOperationPrompt(string dialogId, PromptValidator<Activity> validator, AzureQueuesService queueService) 
            : base(dialogId, validator)
        {
            _queueService = queueService;
        }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options, CancellationToken cancellationToken = default)
        {
            // When the dialog begins, queue the option chosen within the Activity queued.
            await _queueService.QueueActivityToProcess(dc.Context.Activity, (options as LongOperationPromptOptions).LongOperationOption, cancellationToken);

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
