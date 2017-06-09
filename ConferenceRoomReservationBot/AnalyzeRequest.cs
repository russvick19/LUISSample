using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccessibilityQABot
{
    public class AnalyzeRequest
    {

        public int MyProperty { get; set; }
        public AnalyzeRequest() {}

        public string getUserRequestType(LUIS luisContent)
        {
            return luisContent.entities.First().type;
        }
    }
}