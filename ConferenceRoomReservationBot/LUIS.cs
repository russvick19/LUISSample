using System.Collections.Generic;

namespace AccessibilityQABot
{
    public class TopScoringIntent
    {
        public string intent { get; set; }
        public double score { get; set; }
    }

    public class Intent
    {
        public string intent { get; set; }
        public double score { get; set; }
    }

    public class Resolution
    {
        public string time { get; set; }
        public string date { get; set; }
    }

    //Model for JSON response
    public class Query
    {
        public string QuestionIdentifier { get; set; }
        public string Verb { get; set; }
        public string Adjective { get; set; }
        public string Subject { get; set; }
        public string Noun { get; set; }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public double score { get; set; }
        public Resolution resolution { get; set; }
    }

    public class LUIS
    {
        public string query { get; set; }
        public TopScoringIntent topScoringIntent { get; set; }
        public IList<Intent> intents { get; set; }
        public IList<Entity> entities { get; set; }
    }
}