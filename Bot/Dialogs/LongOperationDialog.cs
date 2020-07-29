// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Dialogs;

namespace Microsoft.BotBuilderSamples
{
    public class LongOperationDialog : ComponentDialog
    {
        public LongOperationDialog(AzureQueuesService queueService)
            : base(nameof(LongOperationDialog))
        {
            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                OperationTimeStepAsync,
                LongOperationStepAsync,
                OperationCompleteStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new LongOperationPrompt(nameof(LongOperationPrompt), (vContext, token) =>
            {
                return Task.FromResult(vContext.Recognized.Succeeded);
            }, queueService));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> OperationTimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the user's response is received.
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please select a long operation test option."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "option 1", "option 2", "option 3" }),
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> LongOperationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var value = ((FoundChoice)stepContext.Result).Value;
            stepContext.Values["longOperationOption"] = value;

            var prompt = MessageFactory.Text("...one moment please....");
            var retryPrompt = MessageFactory.Text($"Still performing the long operation: {value} ... (is the Azure Function executing from the queue?)");
            return await stepContext.PromptAsync(nameof(LongOperationPrompt),
                                                        new LongOperationPromptOptions
                                                        {
                                                            Prompt = prompt,
                                                            RetryPrompt = retryPrompt,
                                                            LongOperationOption = value,
                                                        }, cancellationToken);
        }

        private static async Task<DialogTurnResult> OperationCompleteStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["longOperationResult"] = stepContext.Result;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks for waiting. { (stepContext.Result as Activity).Value}"), cancellationToken);

            // Start over
            return await stepContext.ReplaceDialogAsync(nameof(WaterfallDialog), null, cancellationToken);
        }
    }
}
