using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace CommentCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());
            TextWriter textFile = File.CreateText("output.csv");
            foreach (string fileName in files)
            {
                //only look at cpml files
                if (Path.GetExtension(fileName) != ".cpml")
                {
                    continue;
                }
                XDocument doc = XDocument.Load(fileName);
                Dictionary<string, List<string>> userComments = new Dictionary<string, List<string>>();

                //all but equation comments and sticky notes should be pulled by the "Comment" element
                foreach (XElement comment in doc.Descendants("Comment"))
                {
                    //user name can either be an attribute or a sub element
                    XAttribute userNameAttribute = comment.Attribute("UserName");
                    string userName = "Unknown";
                    if (userNameAttribute != null)
                    {
                        userName = userNameAttribute.Value;
                    }
                    else
                    {
                        //check for sub element
                        XElement userNameElement = comment.Descendants("UserName").FirstOrDefault();
                        if (userNameElement != null)
                        {
                            userName = userNameElement.Value;
                        }
                    }

                    //add entry for user if necessary
                    if (userComments.ContainsKey(userName) == false)
                    {
                        userComments.Add(userName, new List<string>());
                    }

                    //comment can either be in a "content" block or just inline
                    XElement contentElement = comment.Descendants("Content").FirstOrDefault();
                    string content = "";
                    if (contentElement != null)
                    {
                        content = contentElement.Value;
                    }
                    else
                    {
                        content = comment.Value;
                    }

                    //add comment to the dictionary
                    if (content.Length > 0)
                    {
                        userComments[userName].Add(content);
                    }
                }

                //now find all equation annotations
                foreach (XElement element in doc.Descendants("Annotation"))
                {
                    string userName = "Unknown";
                    XAttribute userNameAttribute = element.Attribute("UserName");
                    if (userNameAttribute != null)
                    {
                        userName = userNameAttribute.Value;
                    }

                    //add entry for user if necessary
                    if (userComments.ContainsKey(userName) == false)
                    {
                        userComments.Add(userName, new List<string>());
                    }

                    //add comment
                    if (element.Value.Length > 0)
                    {
                        userComments[userName].Add(element.Value);
                    }
                }

                //finally, do sticky notes
                foreach (XElement element in doc.Descendants("StickyNote"))
                {
                    string userName = "Unknown";
                    XElement userNameElement = element.Descendants("UserName").FirstOrDefault();
                    if (userNameElement != null)
                    {
                        userName = userNameElement.Value;
                    }

                    //add entry for user if necessary
                    if (userComments.ContainsKey(userName) == false)
                    {
                        userComments.Add(userName, new List<string>());
                    }

                    //add comment
                    string comment = element.Descendants("Content").FirstOrDefault().Value;
                    if (comment.Length > 0)
                    {
                        userComments[userName].Add(comment);
                    }
                }

                //figure out statistics
                List<string> pieces = new List<string>();
                pieces.Add(Path.GetFileName(fileName));
                int counter = 0;
                
                //determine author if possible
                string[] authorPieces = Path.GetFileNameWithoutExtension(fileName).Split('_');
                string author = authorPieces[0];
                if (userComments.ContainsKey(author) == true)
                {
                    pieces.Add(string.Format("\"{0}:{1}\"", author, userComments[author].Count));
                    userComments.Remove(author);
                }
                else
                {
                    //add blank spot where author should be
                    pieces.Add("");
                }

                TextWriter commentsFile = File.CreateText(string.Format("{0}_comments.txt", Path.GetFileNameWithoutExtension(fileName)));

                foreach (string key in userComments.Keys)
                {
                    counter += userComments[key].Count;
                    pieces.Add(string.Format("\"{0}:{1}\"", key, userComments[key].Count));
                    foreach (string comment in userComments[key])
                    {
                        string[] commentPieces = comment.Split(new string[] { "\n", Environment.NewLine, "." }, StringSplitOptions.None);
                        foreach (string piece in commentPieces)
                        {
                            commentsFile.WriteLine(string.Format("{0}\t{1}", key, piece));
                        }
                    }
                }
                commentsFile.Close();
                pieces.Insert(1, counter.ToString());
                textFile.WriteLine(string.Join(",", pieces));
                textFile.Flush();
                
            }
            textFile.Close();
        }
    }
}
