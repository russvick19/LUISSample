using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ReservationBot
{
    //[LuisModel("cc37dded-bf65-4a50-9ec9-a3b205e2b44a", "34a7147d5534422aa6ddc32f312e2f28")]
    [Serializable]
    public class ReservationLuisDialog :LuisDialog<object>
    {
        public ReservationLuisDialog() : base(new LuisService(new LuisModelAttribute("cc37dded-bf65-4a50-9ec9-a3b205e2b44a", "34a7147d5534422aa6ddc32f312e2f28")))
        {

        }
    
        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("$Sorry I can't figure out what you are looking for");
            context.Wait(MessageReceived);
        }

        [LuisIntent("reservation")]
        public async Task GetReservationInformation(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("You have reached reservation");
            context.Wait(MessageReceived);
        }
    }
}