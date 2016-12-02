using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mshtml;
using System.Net;
using System.IO;

namespace IndexForumCrawler
{
    public static class HtmlMagic
    {
        static string GetHtml(string url)
        {
            string html = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.AllowAutoRedirect = true;
                request.Timeout = 30000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader sr = new StreamReader(stream, Encoding.GetEncoding(1250));
                html = sr.ReadToEnd();
            }
            catch
            {
            }
            return html;
        }

        public static bool ProcessChunk(int TopicId, int n, List<Article> Forum, DateTime from, ref string TopicName)
        {
            bool foundNew = false;
            string fromStr = from.ToString("yyyy.MM.dd");
            // STEP SHOULD BE 100!!!
            string page = GetHtml("http://forum.index.hu/Article/showArticle?na_start=" + n * 100 + "&na_step=100&t=" + TopicId.ToString());
            IHTMLDocument2 doc = (IHTMLDocument2)new HTMLDocument();
            doc.write(page);
            foreach (IHTMLElement el in doc.all)
            {
                if (el.tagName == "TITLE" && TopicName == "")     // <table class="art">
                {
                    TopicName = el.innerText;
                }
                if (el.tagName == "TABLE" && el.className == "art")     // <table class="art">
                {
                    Article art = new Article();
                    foreach (IHTMLElement ell in el.all)
                    {
                        if (ell.tagName == "SPAN" && ell.className == "art_nr") // ez a hozzaszolas sorszama
                        {
                            var id = ell.innerText;
                            try
                            {
                                art.Id = int.Parse(id);
                            }
                            catch { art.Id = 0; }
                        }
                        else if (ell.tagName == "TD" && ell.className != null && ell.className.StartsWith("art_h_l")) // ez a user adatai
                        {
                            foreach (IHTMLElement inn in ell.all)
                            {
                                HTMLAnchorElement a = inn as HTMLAnchorElement;
                                if (a != null)
                                {
                                    string href = a.href;
                                    if (href != null && href.StartsWith("about:"))
                                    {
                                        if (href.StartsWith("about:/User/UserDescription?u="))
                                        {
                                            art.UserName = a.innerText;
                                            art.UserId = int.Parse(href.Substring("about:/User/UserDescription?u=".Length));
                                        }
                                        else if (href.StartsWith("about:/Article/viewArticle?a="))
                                        {
                                            string s = href.Substring("about:/Article/viewArticle?a=".Length);
                                            art.RefId = int.Parse(s.Split('&')[0]);
                                        }
                                    }
                                }
                            }
                            var msg = ell.innerText; // ez a user neve + datum egyben, ami jo is nekunk!
                            string[] parts = msg.Split(' ');
                            if (msg.Contains("napja") || msg.Contains("órája") || msg.Contains("perce"))
                            {
                                art.Date = parts[parts.Length - 3] + " " + parts[parts.Length - 2];
                            }
                            else
                            {
                                art.Date = parts[parts.Length - 2];
                            }
                        }
                        else if (ell.tagName == "TR" && ell.className == "art_b") // ez a hozzaszolas maga
                        {
                            var msg = ell.innerText;
                            if (msg != null)
                            {
                                art.Message = msg;
                            }
                            var mmm = ell.innerHTML;
                            // "http://imgfrm.index.hu/imgfrm/5/8/6/2/MED_0014095862.png\"></P></DIV></TD>"	string
                            // "http://imgfrm.index.hu/imgfrm/5/8/6/2/BIG_0014095862.png')\" title=\"\" class=\"tn_img tn_img14095862 tn_img_10\" border=0 alt=\"\" 
                            // "http://imgfrm.index.hu/imgfrm/5/8/6/2/MED_0014095862.png\"></P></DIV></TD>"	string
                            int ix = ell.innerHTML.IndexOf("http://imgfrm.index.hu/");
                            while (ix != -1)
                            {
                                int iyPNG = ell.innerHTML.IndexOf(".png", ix + 1);
                                int iyJPG = ell.innerHTML.IndexOf(".jpg", ix + 1);
                                int iy = -1;
                                if (iyPNG == -1)
                                {
                                    iy = iyJPG;
                                }
                                else if (iyJPG == -1)
                                {
                                    iy = iyPNG;
                                }
                                else if (iyJPG > iyPNG)
                                {
                                    iy = iyPNG;
                                }
                                else
                                {
                                    iy = iyJPG;
                                }
                                if (iy != -1)
                                {
                                    string img = ell.innerHTML.Substring(ix, iy - ix + 4);
                                    if (!art.Imgs.Contains(img))
                                    {
                                        art.Imgs.Add(img);
                                    }
                                    ix = ell.innerHTML.IndexOf("http://imgfrm.index.hu/", iy);
                                }
                                else
                                {
                                    ix = -1;
                                }
                            }
                        }
                        else if (ell.tagName == "TR" && ell.className == "art_f") // ez az elozmeny
                        {
                            var msg = ell.innerText;
                            int pos = msg.IndexOf('(');
                            if (pos > 1)
                            {
                                int posEnd = msg.IndexOf(')', pos);
                                art.ReplyToId = int.Parse(msg.Substring(pos + 1, posEnd - pos - 1));
                            }
                        }
                    }
                    if (!art.Date.Contains('.') || (art.Date.CompareTo(fromStr) >= 0))
                    {
                        Forum.Add(art);
                        foundNew = true;
                    }
                }
            }
            return foundNew;
        }
    }
}
