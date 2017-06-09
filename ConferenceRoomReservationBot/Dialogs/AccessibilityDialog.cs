using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Bot.Connector;
using System.Collections.Generic;

namespace AccessibilityQABot
{
    [Serializable]
    public class AccessibilityDialog : LuisDialog<object>
    {
        private string Response { get; set; }
        public AccessibilityDialog() : base(new LuisService
            (new LuisModelAttribute("8b396766-831a-4032-aa00-6c7472a16cd6",
                                    "decb94ad3ccb4cfc8f20671f15dd6428")))
        {
        }

        #region Greeting and None intent
        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"I'm sorry I didn't catch that. Could you rephrase your question. {result.Query}"); //
            context.Wait(MessageReceived);
        }

        [LuisIntent("Accessibility Question Greeting")]
        public async Task GreetingUserIntent(IDialogContext context, LuisResult result)
        {
            LUIS luisJson = await QueryLUIS(result.Query);
            string requestedQuestionCategory = "";

            if (luisJson.entities != null && luisJson.entities.Count != 0)
            {
                requestedQuestionCategory = luisJson.entities.FirstOrDefault().type;
            }            

            if(requestedQuestionCategory == "General Greeting")
            {
                await context.PostAsync($"To learn more about general accessibility ask...\n\n\"What is accessibility\" or \"How can I get started\""); 

            }
            else if (requestedQuestionCategory == "Details Greeting")
            {
                await context.PostAsync($"To learn more about MAS Standards ask...\n\n\"What is MAS 52\" or \"What are the MAS standards Available\"");

            }
            else if (requestedQuestionCategory == "Code Snippet Greeting")
            {
                await context.PostAsync($"To see a code snippet ask...\n\n\"Can I see a code snippet that is accessibility compliant\"");
            }
            else
            {
                await context.PostAsync($"Sorry I can't tell what you are looking for\n\nPlease rephrase your request.\n\n"); //
            }

            context.Wait(MessageReceived);
        }
        #endregion

        #region General/Specific/Code Snippet questions
        [LuisIntent("General Questions")]
        public async Task GeneralQuestionIntent(IDialogContext context, LuisResult result)
        {
            LUIS luisJson = await QueryLUIS(result.Query);

            AnalyzeQuestion aQuestion = new AnalyzeQuestion(luisJson);
             
            await context.PostAsync(aQuestion.GeneralQuestionAccessibility()); //
            context.Wait(MessageReceived);
        }

        [LuisIntent("Tools")]
        public async Task Tools(IDialogContext context, LuisResult result)
        {
            Activity replyToConversation = (Activity)context.MakeMessage();
            replyToConversation.Recipient = replyToConversation.Recipient;
            replyToConversation.Type = "message";

            HeroCard magnifierCard = new HeroCard()
            {
                Title = "Magnifier",
                Text = "A tool that enlarges part or all-of the screen so that we can see the images and words better which people with low vision" +
                "to enlarge the screen and have a better viewing experience",
                Images = new List<CardImage>()
                {
                    new CardImage("http://img.informer.com/icons/png/128/3823/3823636.png")
                },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Learn More", value: "https://support.microsoft.com/en-us/help/11542/windows-use-magnifier") }

            };
            HeroCard CCACard = new HeroCard()
            {
                Title = "Color contrast Analyzer (CCA)",
                Text = "Helps to determine the legibility of text and the  contrast of visual elements, such as graphical controls and visual indicators",
                Images = new List<CardImage>()
                {
                    new CardImage("https://is5-ssl.mzstatic.com/image/thumb/Purple18/v4/89/25/09/89250991-bb7a-12d5-f16c-fb33cd340459/source/256x256bb.jpg")
                },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Learn More", value: "https://www.paciellogroup.com/resources/contrastanalyser/") }

            };
            HeroCard WATCard = new HeroCard()
            {
                Title = "Web Accessibility Toolbar (WAT)",
                Text = "Has been developed to aid manual examination of web pages for a variety of aspects of accessibility",
                Images = new List<CardImage>()
                {
                    new CardImage("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS_rK56TNh2Tk3p9VXZLy8oORcG3dTnpWUgxO4cR4ULqoB4xSYiYg")
                },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Learn More", value: "https://www.paciellogroup.com/resources/wat/") }

            };
            HeroCard JawsCard = new HeroCard()
            {
                Title = "Jaws (Job Access With Speech)",
                Text = "A computer screen reader program for Microsoft Windows that allows blind and visually impaired users to read the screen either with a text-to-speech output",
                Images = new List<CardImage>()
                {
                    new CardImage("http://www.aph.org/wp-content/uploads/2016/04/JAWS-for-Windows-logo.png")
                },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Learn More", value: "https://www.freedomscientific.com/support/TechnicalSupport/Windows10Upgrade") }

            };
            HeroCard KerosCard = new HeroCard()
            {
                Title = "Keros",
                Text = "Keros is an extension which will add accessibility audit and an accessibility sidebar pane in elements tab to developer tools which results the list of violations made in the page",
                Images = new List<CardImage>()
                {
                    new CardImage("https://www.materialui.co/materialIcons/action/extension_black_192x192.png")
                },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Learn More", value: "https://chrome.google.com/webstore/detail/accessibility-developer-t/fpkknkljclfencbdbgkenhalefipecmb?hl=en") }
            };

            Attachment magnifierAttachment = magnifierCard.ToAttachment();
            Attachment CCACardAttachment = CCACard.ToAttachment();
            Attachment WATCardAttachment = WATCard.ToAttachment();
            Attachment JawsAttachment = JawsCard.ToAttachment();
            Attachment KerosAttachment = KerosCard.ToAttachment();

            replyToConversation.Attachments.Add(magnifierAttachment);
            replyToConversation.Attachments.Add(CCACardAttachment);
            replyToConversation.Attachments.Add(WATCardAttachment);
            replyToConversation.Attachments.Add(JawsAttachment);
            replyToConversation.Attachments.Add(KerosAttachment);

            replyToConversation.AttachmentLayout = "list";
            replyToConversation.Text = "Here are a list of tools that concern Accessibility:\n\n";

            await context.PostAsync(replyToConversation);
            context.Wait(MessageReceived);
        }

        [LuisIntent("MAS standard details")]
        public async Task SpecificQuestionIntent(IDialogContext context, LuisResult result)
        {
            LUIS luisJson = await QueryLUIS(result.Query);

            AnalyzeQuestion aQuestion = new AnalyzeQuestion(luisJson);

            await context.PostAsync(aQuestion.MASSpecifics(context));
            context.Wait(MessageReceived);
        }

        [LuisIntent("Code Snippet")]
        public async Task CodeSnippetIntent(IDialogContext context, LuisResult result)
        {
            LUIS luisJson = await QueryLUIS(result.Query);

            AnalyzeQuestion aQuestion = new AnalyzeQuestion(luisJson);

            await context.PostAsync(aQuestion.CodeSnippet(context)); 
            context.Wait(MessageReceived);
        }
        #endregion

        private static async Task<LUIS> QueryLUIS(string Query)
        {
            LUIS LUISResult = new LUIS();
            var LUISQuery = Uri.EscapeDataString(Query);
            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
            {
                // Get key values from the web.config
                string LUIS_Url = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/8b396766-831a-4032-aa00-6c7472a16cd6";
                string LUIS_Subscription_Key = "decb94ad3ccb4cfc8f20671f15dd6428";
                string RequestURI = String.Format("{0}?subscription-key={1}&q={2}",
                    LUIS_Url, LUIS_Subscription_Key, LUISQuery);
                System.Net.Http.HttpResponseMessage msg = await client.GetAsync(RequestURI);
                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    LUISResult = JsonConvert.DeserializeObject<LUIS>(JsonDataResponse);
                }
            }
            return LUISResult;
        }
    }
}
