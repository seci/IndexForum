using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace IndexForumCrawler
{
    public partial class Form1 : Form
    {
        List<Article> Forum;
        int TopicId;
        string TopicName = "";
        string Filename;
        Dictionary<string, int> PreloadedTopics = new Dictionary<string, int>();
        string TopicsFilename = "topics.xml";
#if false
// File format:
<Topics>
  <Topic Id="9119377" Name="Dominika" />
  <Topic Id="9049026" Name="Maldives" />
  <Topic Id="9004994" Name="Tenerife" />
  <Topic Id="9039352" Name="Seychelles" />
  <Topic Id="9059424" Name="Mauritius" />
  <Topic Id="9077345" Name="SharmElSheik" />
  <Topic Id="9073668" Name="Jardania" />
  <Topic Id="9015458" Name="Izland" />
</Topics>
#endif

        public Form1(int id, string file)
        {
            InitializeComponent();
            LoadTopics();
            foreach (string fn in PreloadedTopics.Keys)
            {
                comboBoxPreloaded.Items.Add(fn);
            }
            textBoxTopicId.Text = TopicId.ToString();
            textBoxFilename.Text = Filename;
            dateTimePickerFrom.Value = new DateTime(DateTime.Now.Year, 1, 1);
            Forum = new List<Article>();
            // Article.LoadFromXML(Filename + ".xml", Forum);
            // listBoxMsgs.Items.AddRange(Forum.ToArray());
        }

        public void LoadTopics()
        {
            if (File.Exists(TopicsFilename))
            {
                try
                {
                    XDocument xdoc = XDocument.Load(TopicsFilename);
                    if (xdoc != null)
                    {
                        foreach (XElement xe in xdoc.Root.Elements("Topic"))
                        {
                            PreloadedTopics.Add(xe.Attribute("Name").Value, int.Parse(xe.Attribute("Id").Value));
                        }
                    }
                }
                catch { }
            }
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            bool foundNew = false;
            Forum.Clear();
            listBoxMsgs.Items.Clear();
            buttonSize.Text = "0";
            richTextBoxMessage.Text = "";
            Refresh();
            TopicId = int.Parse(textBoxTopicId.Text);
            Filename = textBoxFilename.Text;
            int i = 0;
            do
            {
                foundNew = HtmlMagic.ProcessChunk(TopicId, i, Forum, dateTimePickerFrom.Value, ref TopicName);
                i++;
            } while (foundNew);
            Article.SaveToXML(Filename + ".xml", Forum);
            listBoxMsgs.Items.AddRange(Forum.ToArray());
            buttonSize.Text = Forum.Count().ToString();
        }

        private void listBoxMsgs_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (lb.SelectedItem != null)
            {
                int index = int.Parse(lb.SelectedItem.ToString().Substring(0, 5));
                Article a = Forum.Find(x => x.Id == index);
                richTextBoxMessage.Text = a.Message;
            }
            else
            {
                richTextBoxMessage.Text = "";
            }
        }

        private void buttonHTML_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<head><style>p { margin: 0px 0px 0px 20px;} </style></head>");
            sb.AppendLine("<body>");
            string fref = @"<A href=" + '"' + @"http://forum.index.hu/Article/showArticle?t=" + TopicId + '"' + ">" + Filename + "</A>";
            if (TopicName == "")
            {
                sb.AppendLine("<H1>Index " + fref + " fóruma </H1>");
            }
            else
            {
                sb.AppendLine("<H1> " + TopicName + " </H1>");
            }
            foreach (Article a in Forum.Reverse<Article>())
            {
                string rep = "";
                if (a.ReplyToId != 0)
                {
                    Article re = Forum.Find(x => x.Id == a.ReplyToId);
                    if (re != null)
                    {
                        rep = " - RE: " + re.UserName + " (" + re.Id + ") ";
                    }
                }
                string refe = @"<A href=" + '"' + @"http://forum.index.hu/Article/viewArticle?a=" + a.RefId + "&t=" + TopicId + '"' + ">" + a.UserName + " (" + a.Id + ")" + "</A>";
                sb.AppendLine(refe + " - " + a.Date + rep);
                sb.AppendLine("<p>" + a.Message.Replace("\r\n", "<BR />") + "</p>");
                if (a.Imgs.Count() > 0)
                {
                    sb.AppendLine("<p>");
                    foreach (string img in a.Imgs)
                    {
                        // if (img.Contains("/BIG"))
                        if (img.Contains("/MED") || img.Contains("/THM"))
                        {
                            // sb.AppendLine("<p><img width= " + '"' + "500px" + '"' + " src=" + '"' + img + '"' + "/></p><BR />");
                            sb.AppendLine("<img width= " + '"' + "300px" + '"' + " src=" + '"' + img + '"' + "/>");
                        }
                    }
                    sb.AppendLine("</p>");
                }
                sb.AppendLine("<HR />");
            }
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            try
            {
                using (StreamWriter sw = new StreamWriter(Filename + ".html", false))
                {
                    sw.Write(sb.ToString());
                }
            }
            catch (Exception)
            {
                ; // do nothing, directory renamed or disk full
            }
        }

        private void dateTimePickerFrom_ValueChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxPreloaded_SelectedIndexChanged(object sender, EventArgs e)
        {
            Filename = comboBoxPreloaded.Items[comboBoxPreloaded.SelectedIndex].ToString();
            TopicId = PreloadedTopics[Filename];
            textBoxTopicId.Text = TopicId.ToString();
            textBoxFilename.Text = Filename;
            richTextBoxMessage.Text = "";
        }
    }
}

#if NOT_USED
<!-- hozzaszolas start -->
<table class="art">
  <tr class="art_h">
    <td class="art_h_l hasBadge specAge14">
        <a name="130752498"></a>
        <a href="/User/UserDescription?u=377113" class="art_owner" title="Veterán"><strong>Blende</strong></a>
        <span> <a rel="license" href="http://forum.index.hu/felhasznalasiFeltetelek" target="license"><img alt="Creative Commons License" title="&copy; Index.hu Zrt." src="/img/licence_index.png" /></a> <a href="/Article/viewArticle?a=130752498&amp;t=9039352" target="_blank" rel="bookmark" title="2014.06.22 23:58:45">2014.06.22</a></span>
    </td>
    <td class="art_h_m"></td>
    <td class="art_h_r">
        <a href="/EditArticle/ReplayEditArticle?a=130752498&amp;t=9039352" rel="nofollow" class="art_cnt art_rpl" title="válasz" onclick="logReply(this)"></a>
        <a href="/Article/viewArticle?a=130752498&amp;t=9039352" rel="nofollow" class="art_vw_shd" onclick="toggleLink(this); return false;" title="a hozzászólás közvetlen linkje"><span class="art_cnt art_vw"></span><span class="art_lnk"><input value="http://forum.index.hu/Article/viewArticle?a=130752498&amp;t=9039352" readonly="readonly"/></span></a> <a href="/Article/addBookmark?a=130752498&amp;t=9039352" rel="nofollow" class="art_cnt art_bmk" onclick="logBookmark(this)" title="könyvjelző hozzáadása"></a>  <a href="/User/nickIgnore?u=377113&amp;t=9039352&amp;d=1" rel="nofollow" class="art_cnt art_ud" title="Blende hozzászólásainak elrejtése" onclick="return userDisable(this)"></a>  <span class="art_rat"><span class="art_rat_lft">0</span><a href="/Article/addRating?a=130752498&amp;r=-1" rel="nofollow" class="art_cnt art_rat_ng" title="negatív értékelés leadása" onclick="return addRating(this)"></a> <a href="/Article/addRating?a=130752498&amp;r=1" rel="nofollow" class="art_cnt art_rat_pl" title="pozitív értékelés leadása" onclick="return addRating(this)"></a>0</span>
        <span class="art_nr">5705</span>
    </td>
  </tr>
  <tr class="art_b"><td colspan="3"><div class="art_t"><p>Hát ez afféle hobbi nálam. :) Vadászom az error fare jegyeket. Európán kívül csak így utazunk. Megéri.</p>
<p>&nbsp;</p>
<p>Önmagában a 600 EUR nem rossz mivel BUD ról cca 220-240k HUF az átlagos ár.</p>
<p>Viszont szerintem most kicsit bizonytalan ez az ethiopian. Ha nem fix időpontra kell menni hanem spontán vágyódás a dolog én a helyedben rámennék egy error fare jegyre. Sokat tudsz vele spórolni..</p>
<p>&nbsp;</p>
<p>Indulási helynek talán FCO és MXP jó még mindkettő olcsón és gyorsan elérhető BUD ról és mindkettő Etihad uticél. Ők ugye otthon vannak SEZ en és gyakran hibáznak.. :)</p>
<p>&nbsp;</p>
<p>Béccsel az a gond még, hogyha nem busszal hanem kocsival akarsz menni az útiköltségen kívül is a legolcsóbb parkolás 100 EUR / hét. 2-3 hetes út esetén ez már elég jelentős árdrágító tényező.</p>
<p>&nbsp;</p>
<p>Így már 2 főre 2 hétre az útiköltséggel együtt a 600 EUR jegyből hirtelen 750 EUR lesz. Ennyiért meg már BUD ról is akad jegy.&nbsp;Nyilván ez attól is függ Mo. mely része a kiindulás helye. Egy soproninak nem tétel, de mondjuk Bp ről vagy Nyíregyről szintén más kérdés..</p></div></td></tr>
    <tr class="art_f"><td colspan="3">Előzmény: <a href="/Article/jumpTree?a=130702595&amp;t=9039352" rel="nofollow">k125k125 (5704)</a><br></td></tr>
</table>
<!-- hozzaszolas  end -->
#endif