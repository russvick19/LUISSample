using JSONUtils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ConferenceRoomReservationBot
{
    public class Reservation
    {
        public string Day { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Duration { get; set; }
        public string Room { get; set; }
        public Reservation()
        {
        }

        public Reservation(LUIS luisContent)
        {               
            foreach(var item in luisContent.entities)
            {
                if (item.type == "Room")
                {
                    Room = item.entity;
                }
                if (item.type == "Day")
                {
                    if (item.entity == "tomorrow")
                    {
                        var tmr = DateTime.Now.Date.AddDays(1).ToShortDateString().ToString();
                        Day = tmr.ToString(); ;
                    }
                    else
                    {
                        Day = item.entity;
                    }
                }
                if (item.type == "Time::StartTime")
                {
                    StartTime = determineAmPm(item.entity);
                }
                if (item.type == "Time::EndTime")
                {
                    EndTime = item.entity;
                }
                if (item.type == "Duration")
                {
                    Duration = item.entity;
                }
            }
        }

        public string constructReservation()
        {
            if (String.IsNullOrEmpty(Day) &&
                String.IsNullOrEmpty(Room) && String.IsNullOrEmpty(StartTime))
            {
                return "Sorry, I need more information\n\nBe sure to include the room, day, start time and either duration or end time.";
            }
            if (String.IsNullOrEmpty(Duration) && String.IsNullOrEmpty(EndTime))
            {
                return "Sorry but I need you to tell me either the duration of your meeting or the desired end time";
            }
            else
            {
                string endtime = "";
                if (Room == "a")
                {
                    Room = findRandomRoom();
                }

                if (String.IsNullOrEmpty(Duration))
                {
                    Duration = getDuration(StartTime, EndTime);
                }
                else if (String.IsNullOrEmpty(EndTime))
                {
                    if (getEndTime(StartTime, Duration, out endtime) == true)
                    {
                        EndTime = determineAmPm(endtime);
                    }
                    else
                    {
                        return endtime;
                    }
                }
                return String.Format("How does this work\n\nRoom: {0}\n\nDay: " +
                    "{1}\n\nStart Time: {2}\n\nEnd Time: {3}\n\nDuration: {4}",Room,
                    Day, StartTime,determineAmPm(EndTime), Duration);
            }
        }

        public string getDuration(string start, string end)
        {
            try
            {
                DateTime st = DateTime.Parse(start);
                DateTime et = DateTime.Parse(end);
                return et.Subtract(st).TotalMinutes.ToString() + " minutes";
            }
            catch
            {
                return "Couldn't parse";
            }
        }

        public bool getEndTime(string start, string duration, out string nope)
        {
            int tryNParseMeBro = 0;
            DateTime st = DateTime.Parse(StartTime, CultureInfo.InvariantCulture);

            bool result = int.TryParse(Duration, out tryNParseMeBro);

            if (result)
            {
                DateTime endTime = st.AddMinutes(tryNParseMeBro);
                nope = endTime.ToString("HH:mm");
                return true;
            }
            else
            {
                nope = "Couldn't Parse duration Duration not valid";
                return false;
            }
        }

        public string determineAmPm(string time)
        {
            string cleanedStr = (Regex.Replace(time, @"\s+", "").ToString());

            int st = Convert.ToDateTime(cleanedStr).Hour;
            return cleanedStr;
            //if (cleanedStr.Contains("AM") || cleanedStr.Contains("PM"))
            //{
            //    return cleanedStr;
            //}

            //if (st > 6 && st < 12)
            //{
            //    return time + "AM";
            //}
            //else
            //{
            //    return time + "PM ";
            //}
        }

        public string findRandomRoom()
        {
            List<string> roomList = new List<string>() { "1001", "1002", "1003",
                            "1004", "1005", "1006", "2001", "2002", "2003",
                            "2004", "2005", "2006"};
            Random r = new Random();
            int ran = r.Next(0, roomList.Count);
            return roomList[ran];
        }
    }
}