using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples
{
    public static class  Cards
    {
        public static HeroCard GetHeroCard(string[] qnaAnswerData)
        {
            var heroCard = new HeroCard
            {
                Title = qnaAnswerData[0],
                Subtitle = qnaAnswerData[1],
                Text = qnaAnswerData[2],
                Images = new List<CardImage> { new CardImage(qnaAnswerData[3]) },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: qnaAnswerData[4]) },
            };

            return heroCard;
        }
        public static VideoCard GetVideoCard(string[] qnaAnswerData)
        {
            var videoCard = new VideoCard
            {
                Title = qnaAnswerData[0],
                Subtitle = qnaAnswerData[1],
                Text = qnaAnswerData[2],
                Image = new ThumbnailUrl
                {
                    Url = qnaAnswerData[3],
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = qnaAnswerData[4],
                    },
                },
                Buttons = new List<CardAction>
                {
                    new CardAction()
                    {
                        Title = "Learn More",
                        Type = ActionTypes.OpenUrl,
                        Value =  qnaAnswerData[5],
                    },
                },
            };

            return videoCard;
        }
    }
}
