using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StudentCommentCounter
{
    public class Program
    {
        static void Main(string[] args)
        {
            FileStream stream = File.OpenRead("output.csv");
            CsvParser parser = new CsvParser(stream);
            Dictionary<string, int> studentCommentCount = new Dictionary<string, int>();
            List<string> row = new List<string>();
            
            //build student comment counts
            while(row != null)
            {
                row = parser.getNextRow();
                if (row == null)
                {
                    continue;
                }
                //first two columns are the reviwer and total comment count.  Skip these.
                for (int i = 2; i < row.Count; i++)
                {
                    //split the row
                    string[] pieces = row[i].Split(':');
                    if (pieces.Length == 2)
                    {
                        int count = 0;
                        if (Int32.TryParse(pieces[1], out count) == true)
                        {
                            if (studentCommentCount.ContainsKey(pieces[0]) == false)
                            {
                                studentCommentCount.Add(pieces[0], count);
                            }
                            else
                            {
                                studentCommentCount[pieces[0]] += count;
                            }
                        }                        
                    }
                }
            };

            //output to our own csv file
            TextWriter textFile = File.CreateText("studentCount.csv");
            foreach (string key in studentCommentCount.Keys)
            {
                string output = string.Format("{0},{1}", key, studentCommentCount[key]);
                textFile.WriteLine(output);
            }
            textFile.Close();
        }
    }
}
