// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    /// <summary>
    /// Options sent to <see cref="LongOperationPrompt"/> demonstrating how a value
    /// can be passed along with the enqueued activity.
    /// </summary>
    public class LongOperationPromptOptions : PromptOptions
    {
        public string LongOperationOption { get; set; }
    }

}
