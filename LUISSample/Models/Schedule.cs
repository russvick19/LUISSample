using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LUISSample.Models
{
    public class Schedule
    {
        //room,startTime,endTime,euration,day,size,userId
        [Required]
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [Required]
        [JsonProperty(PropertyName ="room")]
        public string Room { get; set; }

        [Required]
        [JsonProperty(PropertyName="startTime")]
        public DateTime StartTime { get; set; }

        [Required]
        [JsonProperty(PropertyName="endTime")]
        public DateTime EndTime { get; set; }

        [Required]
        [JsonProperty(PropertyName ="duration")]
        public int Duration { get; set; }
    
        [Required]
        [JsonProperty(PropertyName ="size")]
        public string Size { get; set; }

        [JsonProperty(PropertyName = "day")]
        public string Day { get; set; }

        [JsonProperty(PropertyName = "isAvailable")]
        public string IsAvailable { get; set; }
    }
}