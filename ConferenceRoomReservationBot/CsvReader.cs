using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Linq;
using System.Web;

namespace AccessibilityQABot
{
    public class CsvReader
    {
        public List<string> MASNumber { get; set; }
        public List<string> Title { get; set; }
        public List<string> Description { get; set; }
        public List<string> Tools { get; set; }


        public CsvReader()
        {
            MASNumber = new List<string>();
            Title = new List<string>();
            Description = new List<string>();
            Tools = new List<string>();
        }

        public void FindMASfromCsv()
        {
            //MAS	Title	Requirement Description	Tools Used (for both manual and automated validations)
            var currentFilePath = Path.GetFullPath("masStandard.txt");
            //var lines = File.ReadAllLines(currentFilePath).Select(a => a.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
            using (var fs = File.OpenRead(currentFilePath))
            using (var reader = new StreamReader(fs))
            {
                var titleLine = reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    //Filter out all non tabed items
                    if (line.Contains('\t'))
                    {
                        
                        var values = line.Split('\t');
                        if (!String.IsNullOrEmpty(values[0]) && values.Length >=8)
                        {
                            MASNumber.Add(values[0]);
                            Title.Add(values[1]);
                            Description.Add(values[2]);
                            if(values.Length >= 7)
                            {
                                Tools.Add(values[7]);
                            }
                        }
                    } 
                }
            }
        }
    }
}