using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AccessibilityQABot
{
    public class AnalyzeQuestion
    {
        public string QIdentifier { get; private set; }
        public string Verb { get; private set; }
        public string Adjective { get; private set; }
        public string Subject { get; private set; }
        public string MASNumber { get; private set; }
        public string Noun { get; private set; }

        public AnalyzeQuestion() {}

        //Run through the Json returned by luis and fill in appropriate fields.
        public AnalyzeQuestion(LUIS response)
        {
            foreach(var item in response.entities)
            {
                if(item.type == "Subject")
                {
                    Subject = item.entity;
                }
                if(item.type.Contains("QuestionIdentifier"))
                {
                    string[] typeOfQuestion = item.type.Split(new string[] { "::" }, StringSplitOptions.None);
                    QIdentifier = typeOfQuestion[1];
                }
                if(item.type == "masNumber")
                {
                    MASNumber = item.entity;
                }
                if(item.type == "Adjective")
                {
                    Adjective = item.entity;
                }
                if(item.type == "Noun")
                {
                    Noun = item.entity;
                }
                if (item.type == "Verb")
                {
                    Verb = item.entity;
                }
            }
        }

        public string GeneralQuestionAccessibility()
        { 
            //General Accessibility Question - "Importance" intent
            if(!String.IsNullOrEmpty(Adjective))
            {
                if (QIdentifier.ToLower().Contains("is") || QIdentifier.ToLower().Contains("why") || QIdentifier.ToLower().Contains("how"))
                {
                    if (Adjective.Contains("important") || Adjective.Contains("require"))
                    {
                        return "Accessibility is important because it allows for programs and applications to be available to all people regardless of their disability";
                    }
                }
                else if (Adjective.Contains("importan") || Adjective.Contains("require"))
                {
                    return "Accessibility is important because it allows for programs and applications to be available to all " 
                        +"people regardless of their disability";
                }
            }
            //Verb such as "start" occurs in the question string
            else if(!String.IsNullOrEmpty(Verb))
            {
                if (!String.IsNullOrEmpty(QIdentifier))
                {
                    if (QIdentifier.ToLower().Contains("why"))
                    {
                        if (!String.IsNullOrEmpty(Verb) && Verb.ToLower().Contains("start"))
                        {
                            return "You should start using accessibility to ensure that you application or program is available to all kinds of people\n\n" +
                                "regardless of a dissability\n\nYou can get started by visiting:\n\nhttps://developer.microsoft.com/en-us/windows/accessible-apps";
                        }
                    }
                    else if (QIdentifier.ToLower().Contains("how"))
                    {
                        if (!String.IsNullOrEmpty(Verb) && Verb.ToLower().Contains("start"))
                        {
                            return "You can get started by visiting:\n\nhttps://microsoft.sharepoint.com/teams/msenable/Pages/AccessibilityStandard.aspx ";
                        }
                        return "There are 3 crucial steps to ensure to follow to ensure that your application\n\n" +
                            "is accessible to anyone despite any type of disability\n\n1. Plan for accessibility up front\n\n2. Integrate accessibility throughout the development lifecyle" +
                            "\n\n3. Incorporate both automated and manual accessibility assurance\n\nTo learn more please visit:" +
                            "\n\nhttps://developer.microsoft.com/en-us/windows/accessible-apps";
                    }
                    else if (QIdentifier.ToLower().Contains("where"))
                    {
                        if (!String.IsNullOrEmpty(Verb) && Verb.ToLower().Contains("start"))
                        {
                            return "You can get started by visiting:\n\nhttps://microsoft.sharepoint.com/teams/msenable/Pages/AccessibilityStandard.aspx ";
                        }
                    }
                }
            }
            //If someone asks a question along the lines of define accessibility or explain accessibility
            else if (!String.IsNullOrEmpty(Subject))
            {
                return "Accessibility focuses on people with auditory, cognitive, neurological, physical, speech, and visual impairment disabilities. "+
                    "To learn more please visit:\n\nhttps://microsoft.sharepoint.com/teams/msenable/Pages/AccessibilityStandard.aspx";
            }
            return "Sorry but can you rephrase your question.\n\n";
        }

        #region Mas Specific Questions
        public Activity MASSpecifics(IDialogContext context)
        {
            CsvReader reader = new CsvReader();
            reader.FindMASfromCsv();

            string tool = "";

            Activity replyToConversation = (Activity)context.MakeMessage();
            replyToConversation.Recipient = replyToConversation.Recipient;
            replyToConversation.Type = "message";

            replyToConversation.CreateReply("Tool: " + tool);

            //If our user asks for a specific number in regards to the MAS specifications
            if (!String.IsNullOrEmpty(MASNumber))
            {
                HeroCard MASSpecificCard = constructCardHelper(reader, MASNumber, out tool);
                if(!String.IsNullOrEmpty(tool))
                {
                    Attachment masCard = MASSpecificCard.ToAttachment();
                    replyToConversation.Attachments.Add(masCard);
                    return replyToConversation;
                }
                else
                {
                    replyToConversation.Text = "Sorry but I can't find any mas standard for MAS: " + MASNumber;
                    return replyToConversation;
                }
            }
            else
            {
                replyToConversation.Text = "Microsoft accessibility standards is a list of several features that " +
                    "100 different MAS standards ranging from software to hardware\n\nTo see a comprehensive list " +
                    "please visit:\n\n https://www.microsoft.com/en-us/accessibility";
                return replyToConversation;
            }
        }

        private HeroCard constructCardHelper(CsvReader csv, string num, out string tool)
        {
            int indexOfMasNum = getIndexOfMasNumber(csv, num);
            if(indexOfMasNum == -1)
            {
                tool = "";
                return null;
            }
            CardImage test = new CardImage(determineImageUrl(csv, indexOfMasNum, out tool));

            return new HeroCard()
            {
                Title = "MAS " + csv.MASNumber[indexOfMasNum],
                Subtitle = csv.Title[indexOfMasNum],
                Text = csv.Description[indexOfMasNum],
                Images = new List<CardImage>()
                {
                    test
                }
            };
        }

        private int getIndexOfMasNumber(CsvReader csv, string mas)
        {
            StringBuilder sb = new StringBuilder();
            char[] digits = mas.Where(Char.IsDigit).ToArray();
            char[] letters = mas.Where(Char.IsLetter).ToArray();
            string letter = "";
            if (letters.Length == 1)
            {
                letter = letters[0].ToString().ToUpper();
            }
            else if (letters.Length < 0 || letters.Length > 1)
            {
                return -1;
            }

            if (!String.IsNullOrEmpty(letter))
            {
                foreach (var d in digits)
                {
                    sb.Append(d.ToString());
                }
                sb.Append(" " + letter);
            }
            else
            {
                sb.Append(mas);
            }

            for (int i = 0; i < csv.MASNumber.Count; i++)
            {
                if (csv.MASNumber[i].ToUpper() == sb.ToString())
                {
                    return i;
                }
            }
            return -1;
        }
        private string determineImageUrl(CsvReader item, int num, out string tool)
        {
            //Magnifier
            //new CardImage("http://img.informer.com/icons/png/128/3823/3823636.png")
            //Color contrast Analyzer (CCA
            //new CardImage("https://is5-ssl.mzstatic.com/image/thumb/Purple18/v4/89/25/09/89250991-bb7a-12d5-f16c-fb33cd340459/source/256x256bb.jpg")
            //Web Accessibility Toolbar (WAT)"
            //new CardImage("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS_rK56TNh2Tk3p9VXZLy8oORcG3dTnpWUgxO4cR4ULqoB4xSYiYg")
            //Jaws
            //new CardImage("http://www.aph.org/wp-content/uploads/2016/04/JAWS-for-Windows-logo.png")
            //Keros
            //new CardImage("https://www.materialui.co/materialIcons/action/extension_black_192x192.png")


            if (item.Tools[num].ToLower().Contains("magnifer"))
            {
                tool = "Magnifier";
                return "http://img.informer.com/icons/png/128/3823/3823636.png";
            }
            else if (item.Tools[num].ToLower().Contains("color contrast"))
            {
                tool = "Color Contrast Analyzer";
                return "https://is5-ssl.mzstatic.com/image/thumb/Purple18/v4/89/25/09/89250991-bb7a-12d5-f16c-fb33cd340459/source/256x256bb.jpg";
            }
            else if (item.Tools[num].ToLower().Contains("toolbar"))
            {
                tool = "Web Access Toolbar";
                return "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS_rK56TNh2Tk3p9VXZLy8oORcG3dTnpWUgxO4cR4ULqoB4xSYiYg";
            }
            else if (item.Tools[num].ToLower().Contains("jaw"))
            {
                tool = "JAWS";
                return "http://www.aph.org/wp-content/uploads/2016/04/JAWS-for-Windows-logo.png";
            }
            else if (item.Tools[num].ToLower().Contains("kero"))
            {
                tool = "Keros";
                return "https://www.materialui.co/materialIcons/action/extension_black_192x192.png";
            }
            else
            {
                tool = "Manual-Keyboard";
                return "";
            }

        }
        #endregion

        public Activity CodeSnippet(IDialogContext context)
        {
            Activity replyToConversation = (Activity)context.MakeMessage();
            replyToConversation.Recipient = replyToConversation.Recipient;
            replyToConversation.Type = "message";
            string res = "";
            if (!String.IsNullOrEmpty(MASNumber))
            {
                if (MASNumber == "6" || !String.IsNullOrEmpty(Noun) && Noun.ToLower().Contains("focus"))
                {
                    replyToConversation.Text = "```<input id=\"target\" type=\"button\" value=\"Field1\">\n"
                    + "\n< div id =\"divData\"  style =\"display: none\"/>\n"
                    + "\n< input id =”other” type =\"button\" value =\"Field3\">\n"
                    + "\n\t$(document).ready(function(){<input id=\"target\" type=\"button\" value=\"Field1\" >\n"
                    + "\n\t$( \"#target\" ).focus(function() {\n\n( \"#divData\" ).show(); alert( \"Handler for .focus() called.\"\n\n});\n"
                    + "\n$( \"#other\" ).click(function(){\n"
                    + "\n\t\t$( \"#divData\" ).show(); alert( \"Handler for .click() called.\" );\n"
                    + "\n});\n```\n";
                }
                else if (MASNumber == "7" || !String.IsNullOrEmpty(Noun) && Noun.ToLower().Contains("keyboard"))
                {
                    replyToConversation.Text = "```$(‘#xxx').keypress(function(e){\n" +
                        "if (e.which == 27)\n" +
                        "{\n" +
                        "\t// Close my modal window\n"
                        + "\t$(\"#popwindowid\").hide();\n" +
                        "\t// Or set focus to element that activated the modal orcombobox or to the next element\n" +
                        "$\t(“#yourcontrolid”).focus();\n" +
                        "}\n});\n```\n";
                }
                else if(MASNumber == "26" || !String.IsNullOrEmpty(Noun) && Noun.ToLower().Contains("multiple"))
                {
                    replyToConversation.Text = "```\n$(‘#xxx').keypress(function(e){\n"
                    + "\tif (e.which == 13)\n"
                    + "\t{\n"
                    + "\t// perform the desired click action\n"
                    + "\t}\n"
                    + "});\n```\n";
                }
                else if (MASNumber == "27" || !String.IsNullOrEmpty(Noun) && (Noun.ToLower().Contains("bypass") || (Noun.ToLower().Contains("blocks"))))
                {
                    replyToConversation.Text = "```\n< div >\n" +
                        "\t< a href = \"#MainContent\" id = \"SkipToContent\" >\n"
                        + "\tSkip to main content</ a >\n"
                        + "</ div >\n```\n";
                }
            }
            else if(!String.IsNullOrWhiteSpace(Noun))
            {
                if (!String.IsNullOrEmpty(Noun) && Noun.ToLower().Contains("focus"))
                {
                    replyToConversation.Text = "```<input id=\"target\" type=\"button\" value=\"Field1\">"
                    + "\n< div id =\"divData\"  style =\"display: none\"/>"
                    + "\n< input id =”other” type =\"button\" value =\"Field3\">"
                    + "\n\t$(document).ready(function(){<input id=\"target\" type=\"button\" value=\"Field1\" >"
                    + "\n\t$( \"#target\" ).focus(function() {\n\n( \"#divData\" ).show(); alert( \"Handler for .focus() called.\"\n});"
                    + "\n$( \"#other\" ).click(function(){"
                    + "\n\t\t$( \"#divData\" ).show(); alert( \"Handler for .click() called.\" );"
                    + "\n});\n```\n";
                }
                else if (!String.IsNullOrEmpty(Noun) && Noun.ToLower().Contains("keyboard"))
                {
                    replyToConversation.Text = "```$(‘#xxx').keypress(function(e){\n" +
                        "if (e.which == 27)\n" +
                        "{\n" +
                        "\t// Close my modal window\n"
                        + "\t$(\"#popwindowid\").hide();\n" +
                        "\t// Or set focus to element that activated the modal orcombobox or to the next element\n" +
                        "$\t(“#yourcontrolid”).focus();\n" +
                        "}\n});\n```\n";
                }
                else if (!String.IsNullOrEmpty(Noun) && Noun.ToLower().Contains("multiple"))
                {
                    replyToConversation.Text = "```\n$(‘#xxx').keypress(function(e){\n"
                    + "\tif (e.which == 13)\n"
                    + "\t{\n"
                    + "\t// perform the desired click action\n"
                    + "\t}\n"
                    + "});\n```\n";
                }
                else if (!String.IsNullOrEmpty(Noun) && (Noun.ToLower().Contains("bypass") || (Noun.ToLower().Contains("blocks"))))
                {
                    replyToConversation.Text = "```\n< div >\n" +
                        "\t< a href = \"#MainContent\" id = \"SkipToContent\" >\n"
                        + "\tSkip to main content</ a >\n"
                        + "</ div >\n```\n";
                }
            }
            else
            {
                res = "Sorry but can you rephrase your request?\n\n";
            }

            HeroCard MASSpecificCard = new HeroCard()
            {
                Title = "MAS " + MASNumber + ": ",
                Text = res
            };
            Attachment masCard = MASSpecificCard.ToAttachment();

            if(String.IsNullOrWhiteSpace(replyToConversation.Text))
            {
                replyToConversation.Text = "I'm sorry but I currently do not have a code snippet for that MAS";
            }
            //replyToConversation.Attachments.Add(masCard);
            return replyToConversation;

            //return "Sorry but I couldn't process your request. Try again.";
        }
    }
}