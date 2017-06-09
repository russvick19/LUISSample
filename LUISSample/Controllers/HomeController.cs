using JSONUtils;
using LUISSample.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Configuration;
using Microsoft.Bot.Connector.DirectLine;
using System.Speech.Recognition;
using System.Threading;
using System.Diagnostics;

namespace LUISSample.Controllers
{
    public class HomeController : Controller
    {
        private static string directLineSecret = ConfigurationManager.AppSettings["DirectLineSecret"];
        private static string botId = ConfigurationManager.AppSettings["BotId"];
        private static string fromUser = "DirectLineSampleClientUser";

        //Luis look up and analyze
        public ActionResult Index(string searchString)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(Question q)
        {
            return View(q);
        }

        private static void EnginedRecognizedSpeech(object sp, SpeechRecognizedEventArgs e)
        {
            
        }

        private static async Task StartBotConversation()
        {
            DirectLineClient client = new DirectLineClient(directLineSecret);

            var conversation = await client.Conversations.StartConversationAsync();

            new System.Threading.Thread(async () => await ReadBotMessagesAsync(client, conversation.ConversationId)).Start();
        }

        private static Task ReadBotMessagesAsync(DirectLineClient client, string conversationId)
        {
            var conv = conversationId;
            return null;
        }

        public string determineAmPm(string time)
        {
            string cleanedStr = (Regex.Replace(time, @"\s+", "").ToString());
            int st = Convert.ToDateTime(cleanedStr).Hour;

            if (cleanedStr.Contains("AM") || cleanedStr.Contains("PM"))
            {
                return cleanedStr;
            }

            if (st > 6 && st < 12)
            {
                return time + "AM";
            }
            else
            {
                return time + "PM ";
            }
        }

        private static async Task<LUIS> QueryLUIS(string Query)
        {
            LUIS LUISResult = new LUIS();
            var LUISQuery = Uri.EscapeDataString(Query);
            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
            {
                // Get key values from the web.config
                string LUIS_Url = WebConfigurationManager.AppSettings["LUIS_Url"];
                string LUIS_Subscription_Key = WebConfigurationManager.AppSettings["LUIS_Subscription_Key"];
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

        [ActionName("Schedule")]
        public async Task<ActionResult> IndexAsync()
        {
            var items = await DocumentDBRepository<Schedule>.GetItemsAsync(d => d.UserId == User.Identity.Name);
            return View(items);
        }

        public ActionResult Reservation()
        {
            return View();
        }

        public ActionResult Form()
        {
            return View();
        }

#pragma warning disable 1998

        [ActionName("Create")]
        public async Task<ActionResult> CreateAsync()
        {
            return View("Schedule");
        }
#pragma warning restore 1998

        //Posts to document db
        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind(Include = "Room,StartTime,EndTime,Duration,Day,Size,UserId")] Schedule schedule)
        {
            schedule.IsAvailable = "false";
            schedule.UserId = User.Identity.Name;
            //Schedule schedule = helperCopy(reserveRoom);
            //Copy over all contents from reservationRoom to schedule
            if (ModelState.IsValid)
            {
                await DocumentDBRepository<Schedule>.CreateItemAsync(schedule);
                return View("Schedule", schedule);
            }

            return View("Index");
        }


        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind(Include = "Id, RoomNumber, StartTime, EndTime, DurationInMinutes,Size, UserId")] Schedule item)
        {
            if (ModelState.IsValid)
            {
                await DocumentDBRepository<Schedule>.UpdateItemAsync(item.UserId.ToString(), item);
                return RedirectToAction("Schedule");
            }

            return View(item);
        }

        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Schedule item = await DocumentDBRepository<Schedule>.GetItemAsync(id);
            if (item == null)
            {
                return HttpNotFound();
            }

            return View(item);
        }
    }
}