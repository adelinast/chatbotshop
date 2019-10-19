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
using System.Data.SqlClient;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;

namespace Microsoft.BotBuilderSamples
{
    public class QnABot : ActivityHandler
    {
        static int index = 0;
        static int userid = 1;
        static int employeeid = 2;
        private readonly IConfiguration _configuration;
        private readonly ILogger<QnABot> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        string IdClient = "";
        string IdComanda = "";
        static bool welcomed = false;
        string sum = "";
        SqlDataReader reader = null;
        string[] input_smartphones = new string[8];
        string[] input_laptops = new string[14];
        private readonly QnAMakerOptions QnaMakerOptions;
        private readonly DialogSet _dialogs;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        private readonly BotServices BotServices;


        private readonly DialogHelper _dialogHelper;

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
       
        public QnABot(IConfiguration configuration, ILogger<QnABot> logger, IHttpClientFactory httpClientFactory, ConversationState conversationState, UserState userState, IBotServices botServices)
        //public QnABot(ConversationState conversationState, UserState userState, IBotServices botServices)
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

            for (int i = 0; i < 14; i++)
            {
                input_laptops[i] = "0";
            }
            for (int i = 0; i < 8; i++)
            {
                input_smartphones[i] = "0";
            }

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




        /*protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var qnaMaker = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
                EndpointKey = _configuration["QnAAuthKey"],
                Host = GetHostname()
            },
            null,
            httpClient);

            _logger.LogInformation("Calling QnA Maker");


            // The actual call to the QnA Maker service.
            var response = await qnaMaker.GetAnswersAsync(turnContext);

            //added

            if (response != null && response.Length > 0)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(response[0].Answer), cancellationToken);

                if (response[0].Answer.Contains("I will open an order request"))
                {

                    //  Method to update the order id
                    // Create an entry in Comanda Table
                    DBService.Instance.PostQueryAction("insert into Client (IdUser) values (" + userid + ")");

                    // Get the Id of the new ClientID
                    reader = DBService.Instance.GetQueryResult("select IdClient from Client where IdUser=" + userid);
                    IdClient = reader.GetString(0);
                    // Create entry in Comanda Table
                    DBService.Instance.PostQueryAction("insert into Comanda (DataComanda, IdClient) values (select getdate(), " + IdClient + ")");
                    reader = DBService.Instance.GetQueryResult("select IdComanda from Comanda where IdClient=" + IdClient);
                    IdComanda = reader.GetString(0);
                }

                else if (response[0].Answer.Contains("Sure. I will show the shopping cart"))
                {
                    // Display cart command -> TODO
                }

                else if (response[0].Answer.Contains("finish the order"))
                {
                    // Finish order command - create entry in Chitanta table
                    reader = DBService.Instance.GetQueryResult("select Pret from ContinutComanda where IdComanda=" + IdComanda);
                    sum = reader.GetString(0);
                    DBService.Instance.PostQueryAction("insert into Chitanta (DataChitanta, SumaPlatita, IdComanda, IdAngajat) values (select getdate(), " + sum + ", " + IdComanda + ", " + employeeid + ")");
                }

                else if (response[0].Answer.Contains("processor"))
                {

                    // Process the requests from ML Studio
                    input_laptops[0] = "1";
                }

                else if (response[0].Answer.Contains("GPU"))
                {
                    input_laptops[2] = "1";
                    input_laptops[9] = "Gaming";
                    input_laptops[3] = "1";
                    input_laptops[4] = "1";
                }
                else if (response[0].Answer.Contains("Cheap"))
                {
                    input_laptops[10] = "0.2";
                }

                else if (response[0].Answer.Contains("IT"))
                {
                    for (int i = 0; i < 14; i++)
                    {
                        input_laptops[i] = "1";
                    }
                }

                else if (response[0].Answer.Contains("Business"))
                {
                    input_laptops[10] = "Bussines";
                }

                else if (response[0].Answer.Contains("request"))
                {

                    // Send Azure ML
                    await AzureMLService.Instance.InvokeRequestForLaptopsData(input_laptops);
                    string s = "I would recommend" + AzureMLService.Instance.result;
                    await turnContext.SendActivityAsync(MessageFactory.Text(s), cancellationToken);
                    //await turnContext.SendActivityAsync(MessageFactory.Text($("), cancellationToken); 
                }

            }

            else
            {
                // Give the command

                if (index == 0)
                {
                    index++;
                    await turnContext.SendActivityAsync(MessageFactory.Text("Sorry. I didn't get that. Can you rephrase it for me?"), cancellationToken);
                }

                else if (index == 1)
                {
                    index++;
                    await turnContext.SendActivityAsync(MessageFactory.Text("Can you say it again. I couldn't understand."), cancellationToken);
                }

                else
                {
                    index = 0;
                    await turnContext.SendActivityAsync(MessageFactory.Text("Sorry. I don't know the answer."), cancellationToken);
                }
            }
        }*/

        private string GetHostname()
        {
            var hostname = _configuration["QnAEndpointHostName"];
            if (!hostname.StartsWith("https://"))
            {
                hostname = string.Concat("https://", hostname);
            }

            if (!hostname.EndsWith("/qnamaker"))
            {
                hostname = string.Concat(hostname, "/qnamaker");
            }

            return hostname;
        }
        //added
        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            //foreach (var member in turnContext.Activity.MembersAdded)
            {
                //if (member.Id != turnContext.Activity.Recipient.Id) 
                {
                    await turnContext.SendActivityAsync(
                        $"Welcome to Robbie Shopping Bot",
                        cancellationToken: cancellationToken);// {member.Name}
                }
            }
        }
      
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
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
                {
                    // Send a welcome message to the user and tell them what actions they may perform to use this bot
                    //await SendWelcomeMessageAsync(turnContext, cancellationToken);
                    await SendIntroCardAsync(turnContext, cancellationToken);
                }
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected", cancellationToken: cancellationToken);
            }
        }
        
    }
}
