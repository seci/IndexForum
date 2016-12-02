using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Xml;

namespace IndexForumCrawler
{
    public class Article
    {
        public int Id;
        public int RefId; // amit az index hasznal
        public int UserId;
        public string UserName = "";
        public string Message = "";
        public int ReplyToId;
        public string Date = "";
        public List<string> Imgs = new List<string>();
        public Article()
        {
        }
        public Article(XElement xe)
        {
            Id = int.Parse(xe.Attribute("Id").Value);
            RefId = int.Parse(xe.Attribute("RefId").Value);
            UserId = int.Parse(xe.Attribute("UserId").Value);
            UserName = xe.Attribute("UserName").Value;
            Message = xe.Attribute("Message").Value;
            ReplyToId = int.Parse(xe.Attribute("ReplyToId").Value);
            Date = xe.Attribute("Date").Value;
//            Imgs = xe.Attribute("Image").Value;
        }
        XElement ToXML()
        {
            return new XElement("Article",
                new XAttribute("Id", Id),
                new XAttribute("RefId", RefId),
                new XAttribute("UserId", UserId),
                new XAttribute("UserName", UserName),
                new XAttribute("Message", Message),
                new XAttribute("ReplyToId", ReplyToId),
                new XAttribute("Date", Date)
//                new XAttribute("Image", Imgs)
                );
        }
        public static void SaveToXML(string filename, List<Article> forum)
        {
            XDocument xdoc = new XDocument();
            XElement xtop = new XElement("Forum");
            XElement x;

            x = new XElement("Articles");
            foreach (Article a in forum)
            {
                x.Add(a.ToXML());
            }
            xtop.Add(x);
            xdoc.Add(xtop);

            try
            {
                using (StreamWriter sw = new StreamWriter(filename, false))
                {
                    sw.Write(xdoc);
                }
            }
            catch (Exception)
            {
                ; // do nothing, directory renamed or disk full
            }
        }

        public static void LoadFromXML(string filename, List<Article> forum)
        {
            forum.Clear();
            if (File.Exists(filename))
            {
                try
                {
                    XDocument xdoc = XDocument.Load(filename);
                    if (xdoc != null)
                    {
                        foreach (XElement xe in xdoc.Root.Elements("Articles").Elements("Article"))
                        {
                            forum.Add(new Article(xe));
                        }
                    }
                }
                catch { }
            }
        }

        public override string ToString()
        {
            //return Id.ToString() + " " + UserName + " (" + Date + ")";
            return String.Format("{0:D4} {1} ({2})", Id, UserName.PadRight(18).Substring(0, 18), Date);
        }
    }
}
