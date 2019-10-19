// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;

namespace Microsoft.BotBuilderSamples
{
    public class QnABot : ActivityHandler
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger<QnABot> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly QnAMakerOptions QnaMakerOptions;
        private readonly DialogSet _dialogs;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        private readonly BotServices BotServices;
        private readonly DialogHelper _dialogHelper;

        public QnABot(IConfiguration configuration, ILogger<QnABot> logger, IHttpClientFactory httpClientFactory, 
            ConversationState conversationState, UserState userState, IBotServices botServices)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            
            BotServices = (BotServices)botServices;
            if (botServices.QnAMakerService == null)
            {
                  _logger.LogError("Cannot use QNAMakerService");
                  System.Environment.Exit(-1);
            }

            ConversationState = conversationState;
            UserState = userState;

            //active learning
            // QnA Maker dialog options
            QnaMakerOptions = new QnAMakerOptions
            {
                Top = 3,
                ScoreThreshold = 0.03F,
            };

            _dialogs = new DialogSet(ConversationState.CreateProperty<DialogState>(nameof(DialogState)));

            _dialogHelper = new DialogHelper(botServices);

            _dialogs.Add(_dialogHelper.QnAMakerActiveLearningDialog);
        }
   
      
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dialogContext.ContinueDialogAsync(cancellationToken);
                switch (results.Status)
                {
                    case DialogTurnStatus.Cancelled:
                    case DialogTurnStatus.Empty:
                        await dialogContext.BeginDialogAsync(_dialogHelper.ActiveLearningDialogName, QnaMakerOptions, cancellationToken);
                        break;
                    case DialogTurnStatus.Complete:
                        break;
                    case DialogTurnStatus.Waiting:
                        // If there is an active dialog, we don't need to do anything here.
                        break;
                }

                // Save any state changes that might have occured during the turn.
                await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                
                //await SendPingMessageAsync(turnContext, cancellationToken);
                await SendIntroCardAsync(turnContext, cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected", cancellationToken: cancellationToken);
            }
        }
        
        private static async Task SendIntroCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
           var card = new HeroCard();
           card.Title = "Welcome to Robbie Shopping Bot!";
           card.Text = @"Welcome to Robbie Shopping Bot!";
           card.Images = new List<CardImage>() { new CardImage("https://aka.ms/bf-welcome-card-image") };
           card.Buttons = new List<CardAction>()
           {
            new CardAction(ActionTypes.OpenUrl, "About me", null, "About me", "About me", "https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"),
            new CardAction(ActionTypes.ImBack, "Ask a question", null, "Ask a question", "Ask a question", "What would you be interested in?"),
            new CardAction(ActionTypes.OpenUrl, "Learn how to use", null, "Learn how to use", "Learn how to use", "https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-deploy-azure?view=azure-bot-service-4.0"),
           };
    
           var response = MessageFactory.Attachment(card.ToAttachment());
           await turnContext.SendActivityAsync(response, cancellationToken);
        }
        
        
        private static async Task SendPingMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
           await turnContext.SendActivityAsync(
                        $"Should I help you with anything else?",
                        cancellationToken: cancellationToken);
        }
        
    }
}
