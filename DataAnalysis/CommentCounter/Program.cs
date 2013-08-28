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
                Dictionary<string, List<DocumentComment>> userComments = new Dictionary<string, List<DocumentComment>>();

                //all but equation comments and sticky notes should be pulled by the "Comment" element
                foreach (XElement comment in doc.Descendants("Comment"))
                {
                    //user name can either be an attribute or a sub element
                    XAttribute userNameAttribute = comment.Attribute("UserName");
                    string userName = "Unknown";
                    string referent = "Unknown";
                    XElement top = comment.Parent.Parent;
                    if (top.Attribute("ProcessUnitType") != null)
                    {
                        referent = top.Attribute("ProcessUnitType").Value;
                    }
                    if (top.Attribute("StreamType") != null)
                    {
                        referent = top.Attribute("StreamType").Value;
                    }
                    if (userNameAttribute != null)
                    {
                        userName = userNameAttribute.Value + " stream";
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
                        userComments.Add(userName, new List<DocumentComment>());
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
                        userComments[userName].Add(new DocumentComment() { Comment = content, Referent = referent });
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
                        userComments.Add(userName, new List<DocumentComment>());
                    }

                    //add comment
                    if (element.Value.Length > 0)
                    {
                        userComments[userName].Add(new DocumentComment() { Comment = element.Value, Referent = "Equation" });
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
                        userComments.Add(userName, new List<DocumentComment>());
                    }

                    //add comment
                    string comment = element.Descendants("Content").FirstOrDefault().Value;
                    if (comment.Length > 0)
                    {
                        userComments[userName].Add(new DocumentComment() { Comment = comment, Referent = "Sticky Note" });
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
                TextWriter expertsFile = File.CreateText(string.Format("{0}_expert.xls", Path.GetFileNameWithoutExtension(fileName)));
                TextWriter commentsFile = File.CreateText(string.Format("{0}_comments.xls", Path.GetFileNameWithoutExtension(fileName)));
                commentsFile.WriteLine("<table>");
                expertsFile.WriteLine("<table>");
                string expertsHeader = "<tr><th>Author</th><th>Comment</th><th>#Actual Errors</th><th># correct stylistic</th><th># incorrect stylistic</th><th># correct non-stylistic</th><th>#incorrect non-stylistic</th></tr>";
                string commentsHeader = "<tr><th>Author</th><th>Referent</th><th>Comment</th></tr>";
                expertsFile.WriteLine(expertsHeader);
                commentsFile.WriteLine(commentsHeader);
                foreach (string key in userComments.Keys)
                {
                    counter += userComments[key].Count;
                    pieces.Add(string.Format("\"{0}:{1}\"", key, userComments[key].Count));
                    foreach (DocumentComment comment in userComments[key])
                    {
                        expertsFile.WriteLine("<tr><td>{0}</td><td>{1}</td></tr>", key, comment.Comment);
                        string[] commentPieces = comment.Comment.Split(new string[] { "\n", Environment.NewLine, "." }, StringSplitOptions.None);
                        foreach (string piece in commentPieces)
                        {
                            if (piece.Length == 0)
                            {
                                continue;
                            }
                            commentsFile.WriteLine("<tr>");
                            commentsFile.WriteLine(string.Format("<td>{0}</td><td>{1}</td><td>{2}</td>", key, comment.Referent, piece));
                            commentsFile.WriteLine("</tr>");
                        }
                    }
                }
                commentsFile.WriteLine("</table>");
                expertsFile.WriteLine("</table>");
                commentsFile.Close();
                expertsFile.Close();
                pieces.Insert(1, counter.ToString());
                textFile.WriteLine(string.Join(",", pieces));
                textFile.Flush();
                
            }
            textFile.Close();
        }
    }
}
