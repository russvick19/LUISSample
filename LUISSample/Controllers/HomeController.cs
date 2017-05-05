using JSONUtils;
using LUISSample.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using LUISSample.ViewModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LUISSample.Controllers
{
    public class HomeController : Controller
    {
        //Luis look up and analyze
        public async Task<ActionResult> Index(string searchString)
        {
            //Query Return = new Query();
            ReservationInformation Return = new ReservationInformation();
            try
            {
                if (searchString != null)
                {
                    LUIS objLUISResult = await QueryLUIS(searchString);
                    foreach (var item in objLUISResult.entities)
                    { 
                        if(item.type == "day")
                        {
                            if(item.entity == "tomorrow")
                            {
                                var tmr = DateTime.Now.Date.AddDays(1).ToShortDateString().ToString();
                                Return.Day = tmr.ToString(); ;
                            }
                            else
                            {
                                Return.Day = item.entity;
                            }
                        }
                        if (item.type == "Time::Start")
                        {
                            Return.StartTime = determineAmPm(item.entity);
                        }
                        if (item.type == "Time::End")
                        {
                            Return.EndTime = determineAmPm(item.entity);
                        }
                        if (item.type == "duration")
                        {
                            Return.Duration = item.entity;
                        }                      
                        if (item.type == "size")
                        {
                            Return.Size = item.entity;
                        }

                        if (item.type == "room")
                        {
                            if(item.entity == "a")
                            {
                                List<string> roomList = new List<string>() { "1001", "1002", "1003",
                                "1004", "1005", "1006", "2001", "2002", "2003",
                                "2004", "2005", "2006"};
                                Random r = new Random();
                                int ran = r.Next(0, roomList.Count);
                                Return.Room = roomList[ran];
                            }
                            else
                            {
                                Return.Room = item.entity;
                            }
                        }
                    }
                }

                //Caclculate duration from end time
                if(Return.Duration == null)
                {
                    DateTime et = Convert.ToDateTime(Return.EndTime);
                    DateTime st = Convert.ToDateTime(Return.StartTime);
                    Return.Duration = et.Subtract(st).TotalMinutes.ToString();
                }
                //Calculate end time from duration
                else if(Return.EndTime == null)
                {
                    int tryNParseMeBro = 0;
                    DateTime st = Convert.ToDateTime(Return.StartTime);

                    bool result = int.TryParse(Return.Duration, out tryNParseMeBro);

                    if (result)
                    {
                        Return.EndTime = determineAmPm(
                            st.AddMinutes(tryNParseMeBro).ToString("HH:mm"));
                    }
                    else
                    {
                        Return.Duration = "Couldn't Parse duration";
                        Return.EndTime = "Duration not valid";
                    }
                }

                if (Return.Room == "a")
                {
                    if (Return.Size == null)
                    {
                        List<string> roomList = new List<string>() { "1001", "1002", "1003",
                        "1004", "1005", "1006", "2001", "2002", "2003",
                        "2004", "2005", "2006"};
                        List<ReservationInformation> resvList = new 
                            List<ReservationInformation>();
                        //Need to fetch database for available rooms and pick one
                        //fetch all rooms available
                    }
                }
                Return.UserId = User.Identity.Name;

                return View(Return);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                return View(Return);
            }
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