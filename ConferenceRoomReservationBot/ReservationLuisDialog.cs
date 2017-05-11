using JSONUtils;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Services.Description;

namespace ConferenceRoomReservationBot
{
    [Serializable]
    public class ReservationLuisDialog :LuisDialog<object>
    {
        public ReservationLuisDialog() : base(new LuisService
            (new LuisModelAttribute("1942af04-a252-417f-917d-ccf281d06116",
                                    "34a7147d5534422aa6ddc32f312e2f28")))
        {
        }
        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached the none intent. ok said: {result.Query}"); //
            context.Wait(MessageReceived);
        }

        [LuisIntent("Confirmation")]
        public async Task ConfirmationIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Great you have been booked!"); //
            context.Wait(MessageReceived);
        }

        [LuisIntent("Rejection")]
        public async Task RejectionIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Ok no problem.\n\n" +
                $"Enter in your desired reservation information."); //
            context.Wait(MessageReceived);
        }

        [LuisIntent("Reservation")]
        public async Task ReservationIntent(IDialogContext context, LuisResult result)
        {
            LUIS luisJson = await QueryLUIS(result.Query);

            Reservation reservationInfo = new Reservation(luisJson);

            var message = context.MakeMessage();
            message.Attachments = new List<Attachment>();

            //await context.PostAsync(reservationInfo.constructReservation());
            //await context.PostAsync(message);

            context.Wait(this.MessageReceivedAsync);

            //context.Wait(MessageReceived);
        }

        private void ShowOptions(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() {"Reservation" , "Form Bot" }, "Bot Options", "Not a valid option", 3);
        }

        private Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            throw new NotImplementedException();
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            this.ShowOptions(context);
        }

        private static async Task<LUIS> QueryLUIS(string Query)
        {
            LUIS LUISResult = new LUIS();
            var LUISQuery = Uri.EscapeDataString(Query);
            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
            {
                // Get key values from the web.config
                string LUIS_Url = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/1942af04-a252-417f-917d-ccf281d06116";
                string LUIS_Subscription_Key = "34a7147d5534422aa6ddc32f312e2f28";
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