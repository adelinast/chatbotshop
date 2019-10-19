using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Microsoft.Bot.Builder;
namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// QnAMaker Active Learning Dialog helper class
    /// </summary>
    public class DialogHelper
    {
        /// <summary>
        /// QnA Maker Active Learning dialog name
        /// </summary>
        public string ActiveLearningDialogName = "active-learning-dialog";

        /// <summary>
        /// QnA Maker Active Learning Dialog
        /// </summary>
        public WaterfallDialog QnAMakerActiveLearningDialog;

        private QnAMakerOptions qnaMakerOptions;
        private readonly IBotServices _services;

        // Define value names for values tracked inside the dialogs.
        private const string CurrentQuery = "value-current-query";
        private const string QnAData = "value-qnaData";

        // Dialog Options parameters
        private const float DefaultThreshold = 0.03F;
        private const int DefaultTopN = 3;

        // Card parameters
        private const string cardTitle = "Did you mean:";
        private const string cardNoMatchText = "None of the above.";
        private const string cardNoMatchResponse = "Thanks for the feedback.";
        
        private const string OrderReqMsg = "I will open an order request";
        private const string RecommendMsg = "I would recommend";
        
        private const string RephraseMsg1 ="Sorry. I didn't get that. Can you rephrase it for me?";
        private const string RephraseMsg2 ="Can you say it again. I couldn't understand.";
        private const string RephraseMsg3 ="Sorry. I don't know the answer.";
        private const int NrFeaturesLaptop = 14;
        private const int NrFeaturesSmartphone = 8;
        //input to ML Studio
        string[] inputSmartphones = new string[8];
        string[] inputLaptops = new string[14];
        //DB Connection
        static int index = 0;
        static int userid = 1;
        static int employeeid = 2;
        string IdClient = "";
        string IdCommand = "";
        string sum = "";
        SqlDataReader reader = null;
        
        /// <summary>
        /// Dialog helper to generate dialogs
        /// </summary>
        /// <param name="services">Bot Services</param>
        public DialogHelper(IBotServices services)
        {
            QnAMakerActiveLearningDialog = new WaterfallDialog(ActiveLearningDialogName)
                .AddStep(CallGenerateAnswer)
                .AddStep(FilterLowVariationScoreList)
                .AddStep(CallTrain)
                .AddStep(DisplayQnAResult);
            _services = services;
            
            for (int i = 0; i < NrFeaturesLaptop; i++)
            {
                inputLaptops[i] = "0";
            }
            for (int i = 0; i < NrFeaturesSmartphone; i++)
            {
                inputSmartphones[i] = "0";
            }
        }

        private async Task<DialogTurnResult> CallGenerateAnswer(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var scoreThreshold = DefaultThreshold;
            var top = DefaultTopN;

            QnAMakerOptions qnaMakerOptions = null;

            // Getting options
            if (stepContext.ActiveDialog.State["options"] != null)
            {
                qnaMakerOptions = stepContext.ActiveDialog.State["options"] as QnAMakerOptions;
                scoreThreshold = qnaMakerOptions?.ScoreThreshold != null ? qnaMakerOptions.ScoreThreshold : DefaultThreshold;
                top = qnaMakerOptions?.Top != null ? qnaMakerOptions.Top : DefaultTopN;
            }

            var response = await _services.QnAMakerService.GetAnswersAsync(stepContext.Context, qnaMakerOptions);

            var filteredResponse = response.Where(answer => answer.Score > scoreThreshold).ToList();

            stepContext.Values[QnAData] = new List<QueryResult>(filteredResponse);
            stepContext.Values[CurrentQuery] = stepContext.Context.Activity.Text;
            return await stepContext.NextAsync(cancellationToken);
        }

        private async Task<DialogTurnResult> FilterLowVariationScoreList(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var response = stepContext.Values[QnAData] as List<QueryResult>;

            var filteredResponse = _services.QnAMakerService.GetLowScoreVariation(response.ToArray()).ToList();

            stepContext.Values[QnAData] = filteredResponse;

            if (filteredResponse.Count > 1)
            {
                var suggestedQuestions = new List<string>();
                foreach (var qna in filteredResponse)
                {
                    suggestedQuestions.Add(qna.Questions[0]);
                }

                // Get hero card activity
                var message = CardHelper.GetHeroCard(suggestedQuestions, cardTitle, cardNoMatchText);

                await stepContext.Context.SendActivityAsync(message);

                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }
            else
            {
                return await stepContext.NextAsync(new List<QueryResult>(response), cancellationToken);
            }
        }

        private async Task<DialogTurnResult> CallTrain(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var trainResponses = stepContext.Values[QnAData] as List<QueryResult>;
            var currentQuery = stepContext.Values[CurrentQuery] as string;

            var reply = stepContext.Context.Activity.Text;

            if (trainResponses.Count() > 1)
            {
                var qnaResult = trainResponses.Where(kvp => kvp.Questions[0] == reply).FirstOrDefault();

                if (qnaResult != null)
                {
                    stepContext.Values[QnAData] = new List<QueryResult>() { qnaResult };

                    var records = new FeedbackRecord[]
                    {
                        new FeedbackRecord
                        {
                            UserId = stepContext.Context.Activity.Id,
                            UserQuestion = currentQuery,
                            QnaId = qnaResult.Id,
                        }
                    };

                    var feedbackRecords = new FeedbackRecords { Records = records };

                    // Call Active Learning Train API
                    await _services.QnAMakerService.CallTrainAsync(feedbackRecords);

                    return await stepContext.NextAsync(new List<QueryResult>(){ qnaResult }, cancellationToken);
                }
                else if (reply.Equals(cardNoMatchText))
                {
                    await stepContext.Context.SendActivityAsync(cardNoMatchResponse, cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync();
                }
                else
                {
                    return await stepContext.ReplaceDialogAsync(ActiveLearningDialogName, stepContext.ActiveDialog.State["options"], cancellationToken);
                }
            }

            return await stepContext.NextAsync(stepContext.Result, cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayQnAResult(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await DBSendResults(stepContext, cancellationToken);
        }
        
        private async Task<DialogTurnResult> DBSendResults(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is List<QueryResult> response && response.Count > 0)
            {
                await stepContext.Context.SendActivityAsync(response[0].Answer, cancellationToken: cancellationToken);
                if (response[0].Answer.Contains(OrderReqMsg))
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
                    IdCommand = reader.GetString(0);
                }
                else if (response[0].Answer.Contains("Sure. I will show the shopping cart"))
                {
                    // Display cart command -> TODO
                }
                else if (response[0].Answer.Contains("finish the order"))
                {
                    // Finish order command - create entry in Chitanta table
                    reader = DBService.Instance.GetQueryResult("select Pret from ContinutComanda where IdComanda=" + IdCommand);
                    sum = reader.GetString(0);
                    DBService.Instance.PostQueryAction("insert into Chitanta (DataChitanta, SumaPlatita, IdComanda, IdAngajat) values (select getdate(), " + sum + ", " + IdCommand + ", " + employeeid + ")");
                }
                else if (response[0].Answer.Contains("processor"))
                {
                    // Process the requests from ML Studio
                    inputLaptops[0] = "1";
                }
                else if (response[0].Answer.Contains("GPU"))
                {
                    inputLaptops[2] = "1";
                    inputLaptops[9] = "Gaming";
                    inputLaptops[3] = "1";
                    inputLaptops[4] = "1";
                }
                else if (response[0].Answer.Contains("Cheap"))
                {
                    inputLaptops[10] = "0.2";
                }
                else if (response[0].Answer.Contains("IT"))
                {
                    for (int i = 0; i < 14; i++)
                    {
                        inputLaptops[i] = "1";
                    }
                }
                else if (response[0].Answer.Contains("Business"))
                {
                    inputLaptops[10] = "Business";
                }
                else if (response[0].Answer.Contains("request"))
                {
                    // Send Azure ML
                    await AzureMLService.Instance.InvokeRequestForLaptopsData(inputLaptops);
                    string s = RecommendMsg + AzureMLService.Instance.result;
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(s), cancellationToken);
                }
            }
            else
            {
                // Give the command
                if (index == 0)
                {
                    index++;
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(RephraseMsg1), cancellationToken);
                }
                else if (index == 1)
                {
                    index++;
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(RephraseMsg2), cancellationToken);
                }
                else
                {
                    index = 0;
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(RephraseMsg3), cancellationToken);
                }
            }
            
            return await stepContext.EndDialogAsync();
       }
    }
}