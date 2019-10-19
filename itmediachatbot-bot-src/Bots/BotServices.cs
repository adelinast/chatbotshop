// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
namespace Microsoft.BotBuilderSamples
{
    public class BotServices : IBotServices
    {
         private string GetHostname(IConfiguration Configuration)
        {
            var hostname = Configuration["QnAEndpointHostName"];
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
        public BotServices(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            var httpClient = httpClientFactory.CreateClient();
            QnAMakerService =         
            new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = configuration["QnAKnowledgebaseId"],
                EndpointKey = configuration["QnAAuthKey"],
                Host = GetHostname(configuration)
            },
            null,
            httpClient);
            
        }

        public QnAMaker QnAMakerService { get; private set; }
    }
}