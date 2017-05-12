using System;
using Microsoft.Bot.Connector.DirectLine;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Configuration;
using Newtonsoft.Json;
using MainMenuSample.Models;

namespace MainMenuSample.Controllers
{
    public class HomeController : Controller
    {
        private static string directLineSecret = ConfigurationManager.AppSettings["DirectLineSecret"];
        private static string botId = ConfigurationManager.AppSettings["BotId"];
        private static string fromUser = "DirectLineSampleClientUser";
        private static string DiretlineUrl = @"https://directline.botframework.com";

        private static string reservationLineSecret= ConfigurationManager.AppSettings["ReservationLineSecret"];
        private static string reservationBotId = ConfigurationManager.AppSettings["ReservationBotId"];

        public ActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<ActionResult> Index(DirectLine dl)
        {
            await SendToBot(dl.UserRequest);
            return View();
        }

        private async Task SendToBot(string str)
        {
            //Connect to direct line services
            DirectLineClient client = new DirectLineClient(directLineSecret);
            var conversation = await client.Conversations.StartConversationWithHttpMessagesAsync();
            var convId = conversation.Body.ConversationId;

            var postMessage = await client.Conversations.PostActivityWithHttpMessagesAsync(convId,
             new Activity()
             {
                 Type = "message",
                 From = new ChannelAccount()
                 {
                     Id = fromUser
                 },
                 Text = str
             });

            var result = await client.Conversations.GetActivitiesAsync(convId);
            if(result.Activities.Count > 0)
            {
                //var listBots = result.Activities.Last(a => a.From)
            }
            
            //use robert to log in 
            //user: 
            //pass: ,CmJ9<C6
        }

        private static async Task ReadBotMessagesAsync(DirectLineClient client, string conversationId)
        {
            string watermark = null;

            var activitySet = await client.Conversations.GetActivitiesAsync(conversationId, watermark);
            watermark = activitySet?.Watermark;

            var activities = from x in activitySet.Activities
                             where x.From.Id == botId
                             select x;           
       }
     
    }
}