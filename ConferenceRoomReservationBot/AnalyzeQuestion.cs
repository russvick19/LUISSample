using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private string[] masSpecificNameArray = {"Platform-provided Accessibility User Settings", "Accessible Content Creation", "Preservation of accessibility information in transformations",
            "Authoring and Development Tool Text Search", "Focus Order", "","On Focus", "No Keyboard Trap", "Three Flashes or Below Threshold"};

        private string[] masSpecificRequirementArray = { "The platform (OS) must provide the user with accessibility settings such as: High contrast, alternative alerts, system level volume, customized input, increased text sizes, custom fonts, custom focus appearances, and navigation alternatives.",
            "An authoring tool must guide authors to create accessible content and check for accessibility problems.", "A content generating product must be able to preserve the platform-level accessibility settings without impacting the output of the product.",
            "An authoring/development tool must provide text search capabilities for design and code surfaces.", "There must be a logical flow to the focus in an interface. And the view follows the focus.",
            "", "All focus changes must result from user action and the user should expect the change.","A user must be able to move focus out of a control by using a method consistent with the method used to move focus into the control.",
            "Product doesn't cause risk of seizures. The UI doesn't flash at a rate greater than three times per second."};

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
                if(item.type == "builtin.number")
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

        public Activity MASSpecifics(IDialogContext context)
        {
            Activity replyToConversation = (Activity)context.MakeMessage();
            replyToConversation.Recipient = replyToConversation.Recipient;
            replyToConversation.Type = "message";
     
            //If our user asks for a specific number in regards to the MAS specifications
            if (!String.IsNullOrEmpty(MASNumber))
            {
                int arrayPosition = Int32.Parse(MASNumber.TrimStart('0')) - 1;
                if(arrayPosition == 5)
                {
                    replyToConversation.Text = "Sorry but I not have mas: " + MASNumber.TrimStart('0') + " implemented yet";
                    return replyToConversation;
                }
                HeroCard MASSpecificCard = new HeroCard()
                {
                    Title = "MAS " + MASNumber + ": " + masSpecificNameArray[arrayPosition],
                    Text = masSpecificRequirementArray[arrayPosition]
                };
                Attachment masCard = MASSpecificCard.ToAttachment();
                replyToConversation.Attachments.Add(masCard);
                return replyToConversation; 
            }
            else
            {
                replyToConversation.Text = "Microsoft accessibility standards is a list of several features that "+
                    "100 different MAS standards ranging from software to hardware\n\nTo see a comprehensive list " +
                    "please visit:\n\n https://www.microsoft.com/en-us/accessibility";
                return replyToConversation;
            }
        }

        public string GeneralQuestionAccessibility()
        { 
            if(String.IsNullOrEmpty(QIdentifier) && !String.IsNullOrEmpty(Subject))
            {
                return "Accessibility focuses on people with enabling indivduals with auditory, cognitive, neurological, physical, speech, and visual impairment disabilities to interact with technology\n\n";
            }
            //General Questions about importance
            if(!String.IsNullOrEmpty(Adjective) && (Adjective.ToLower().Contains("important") || Adjective.Contains("signifi")) && (QIdentifier.ToLower() == "why" || QIdentifier.ToLower() == "is" || QIdentifier.ToLower() == "how" || QIdentifier == "what"))
            {
                return "Accessibility is important because it allows for programs and applications to be available to all people regardless of their disability";
            }

            if (QIdentifier.ToLower().Contains("is") || QIdentifier.ToLower().Contains("why"))
            {
                if(QIdentifier.ToLower().Contains("why"))
                {
                    if(!String.IsNullOrEmpty(Subject) && !String.IsNullOrEmpty(Adjective))
                    {
                        if (Adjective.Contains("important") || Adjective.Contains("require"))
                        {
                            return "Accessibility is important because it allows for programs and applications to be available to all people regardless of their disability";
                        }
                    }
                    else if(!String.IsNullOrEmpty(Subject))
                    {
                        return "You should use accessibility to ensure that your application is avaiable to everyone regardless of their disability";
                    }
                }
                else
                {
                    if (Adjective.Contains("important") || Adjective.Contains("require"))
                    {
                        return "Yes it is important to follow Microsoft's accessibility rules in efforts to make your project available to all people\n\n" +
                            "Please review the web page for more information:\n\nhttps://www.microsoft.com/en-us/accessibility";
                    }
                }
            }
            else if(QIdentifier.ToLower().Contains("what") || String.IsNullOrEmpty(QIdentifier))
            {
                return "Accessibility focuses on people with enabling indivduals with auditory, cognitive, neurological, physical, speech, and visual impairment disabilities to interact with technology\n\n";
            }
            else if (QIdentifier.ToLower().Contains("where"))
            {
                if(!String.IsNullOrEmpty(Verb) && Verb.ToLower().Contains("start"))
                {
                    return "You can get started by visiting:\n\nhttps://developer.microsoft.com/en-us/windows/accessible-apps";
                }
                return "Accessibility should be taken into consideration in any product you expect to be client-facing. If you believe your product may be used by the general public or even people"
                    + "outside of your team you should take accessibility into consideration when designing your application";
            }
            //helloasdfsa
            else if (QIdentifier.ToLower().Contains("how"))
            {
                if (!String.IsNullOrEmpty(Verb) && Verb.ToLower().Contains("start"))
                {
                    return "You can get started by visiting:\n\nhttps://developer.microsoft.com/en-us/windows/accessible-apps";
                }
                return "There are 3 crucial steps to ensure to follow to ensure that your application\n\n"+
                    "is accessible to anyone despite any type of disability\n\n1. Plan for accessibility up front\n\n2. Integrate accessibility throughout the development lifecyle"+
                    "\n\n3. Incorporate both automated and manual accessibility assurance\n\nTo learn more please visit:"+
                    "\n\nhttps://developer.microsoft.com/en-us/windows/accessible-apps";
            }
            else if (QIdentifier.ToLower().Contains("when"))
            {
                return "Accessibility should be used anytime you are looking to design a program or application which will potentially be used by a client.";
            }

            return "Sorry but can you rephrase your question.\n\n";
        }

        public Activity CodeSnippet(IDialogContext context)
        {
            Activity replyToConversation = (Activity)context.MakeMessage();
            replyToConversation.Recipient = replyToConversation.Recipient;
            replyToConversation.Type = "message";
            string res = "";
            if (!String.IsNullOrEmpty(MASNumber))
            {
                int masNum = Int32.Parse(MASNumber.TrimStart('0'));
                if (masNum == 6 || !String.IsNullOrEmpty(Noun) && Noun.ToLower().Contains("focus"))
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
                else if (masNum == 7 || !String.IsNullOrEmpty(Noun) && Noun.ToLower().Contains("keyboard"))
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
                else if(masNum == 26 || !String.IsNullOrEmpty(Noun) && Noun.ToLower().Contains("multiple"))
                {
                    replyToConversation.Text = "```\n$(‘#xxx').keypress(function(e){\n"
                    + "\tif (e.which == 13)\n"
                    + "\t{\n"
                    + "\t// perform the desired click action\n"
                    + "\t}\n"
                    + "});\n```\n";
                }
                else if (masNum == 27 || !String.IsNullOrEmpty(Noun) && (Noun.ToLower().Contains("bypass") || (Noun.ToLower().Contains("blocks"))))
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