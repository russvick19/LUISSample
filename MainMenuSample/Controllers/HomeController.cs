using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MainMenuSample.Models;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.DirectLine;
using System.Configuration;

namespace MainMenuSample.Controllers
{
    public class HomeController : Controller
    {
        private static string directLineSecret = ConfigurationManager.AppSettings["DirectLineSecret"];
        private static string botId = ConfigurationManager.AppSettings["BotId"];

        private static string reservationLineSecret= ConfigurationManager.AppSettings["ReservationLineSecret"];
        private static string reservationBotId = ConfigurationManager.AppSettings["ReservationBotId"];

        public ActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<ActionResult> Index(DirectLine dl)
        {
            await Result(dl.UserRequest);
            return View();
        }

        [HttpPost]
        public static async Task Result(string str)
        {
            //Connect to direct line services
            DirectLineClient client = new DirectLineClient(directLineSecret);
            var conversation = await client.Conversations.StartConversationAsync();

            //Send and recieve text from client
            new System.Threading.Thread(async () => 
            await ReadBotMessagesAsync(client, conversation.ConversationId))
            .Start();

            Activity userString = new Activity()
            {
                Text = str
            };
            //use robert to log in 
            //user: 
            //pass: ,CmJ9<C6

            await client.Conversations
                .PostActivityAsync(conversation.ConversationId, userString);            
        }

        private static async Task ReadBotMessagesAsync(DirectLineClient client, string conversationId)
        {
            string watermark = null;

            var activitySet = await client.Conversations.GetActivitiesAsync(conversationId, watermark);
            watermark = activitySet?.Watermark;

            var activities = from x in activitySet.Activities
                             where x.From.Id == botId
                             select x;

            foreach (Activity activity in activities)
            {
                foreach (Attachment attachment in activity.Attachments)
                {
                }
            }
        }
    }
}