using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using mshtml;
using System.Text.RegularExpressions;
using System.Collections;
using DatabaseConnection;
using System.Net;
/*weakness: for similar elements which forms unseprated blocks, need method to detect this bloks
 * table is sloved, other forms not sovled
 * ul ol list
 * may miss some text, like <font> tag
 * 
 * detect linenumber.
 */
namespace bid
{
    public partial class brownser : Form
    {
        public brownser()
        {     
            InitializeComponent();
            ConsoleWindow2.CreateConsole();
            loading = true;
            this.layoutHtmlElement = new List<string> { "TABLE","TH", "TR", "TD", "LI", "OL", "DL", "DD", "DT", "DIV", "H1", "H2", "H3", "H4", "H5", "H6", "P" };
            this.leafHtmlElement = new List<string> { "A", "ADDRESS", "BLOCKQUOTE", "DEL", "PRE", "SELECT", "FIELDSET", "MAP", "EM", "H1", "H2", "H3", "H4", "H5", "H6", "OBJECT" };
            this.eventElements = new List<TreeNode>();
            try
            {
                Connection = new MysqlDatabaseConnection();
                Connection.Connect();
            }
            catch (MySql.Data.MySqlClient.MySqlException dbe)
            {
                Console.WriteLine(dbe.Message);
            }
        }

        //////////////////////////////// instance variable declaration/////////////////////////////////////////////////////
        private MysqlDatabaseConnection Connection;
        private List<string> leafHtmlElement;
        private List<string> layoutHtmlElement;
        private List<TreeNode> eventElements;// store the big blocks of current page.
        private bool loading;
        private Page page;
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            
        private bool checkLeafHtmlElement(string tagName)
        {
            if (this.leafHtmlElement.Contains(tagName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.Equals(webBrowser.Document.Url) && webBrowser.ReadyState == WebBrowserReadyState.Complete && webBrowser.IsBusy == false)
            {
                this.eventElements.Clear();
                HtmlDocument htmlDocument = webBrowser.Document;
                HtmlElement body = htmlDocument.Body;
                this.treeView1.Nodes.Clear();
                TreeNode root = new TreeNode(body.TagName);
                DataNode temp = new DataNode(body);
                root.Tag = temp;
                this.treeView1.Nodes.Add(root);
                root.Nodes.AddRange(this.TreeCleanandMark(body).ToArray());
                root.Expand();
                if (root.Nodes.Count > 1)
                {
                    for (int i = 0; i < root.Nodes.Count; i++)
                    {
                        expandTree(root.Nodes[i], 0, true);
                    }
                }
                else
                {
                    for (int i = 0; i < root.Nodes.Count; i++)
                    {
                        expandTree(root.Nodes[i], 0, false);
                    }
                }
                HtmlElement div = body.Document.CreateElement("DIV");             
                div.Id = "coverxx";
                IHTMLElement domDIV = (IHTMLElement)div.DomElement;
                domDIV.style.setAttribute("position", "absolute");
                body.AppendChild(div);
                this.loading = false;
            }
        }

        private List<TreeNode> TreeCleanandMark(HtmlElement parent)
        {
            if (parent.Children.Count == 0 || this.checkLeafHtmlElement(parent.TagName))
            {
                return null;
            }
            List<TreeNode> children = new List<TreeNode>();
            List<TreeNode> groupList = new List<TreeNode>();
            for (int i = 0; i < parent.Children.Count; i++)
            {
                if (!Method.invalidNode(parent.Children[i]))
                {
                    List<TreeNode> temp = this.TreeCleanandMark(parent.Children[i]);
                    if (temp == null)
                    {
                        if (parent.Children[i].TagName.Equals("A") || (parent.Children[i].InnerText != null && !Regex.IsMatch(parent.Children[i].InnerText, "^\\s+$") && Method.viewble(parent.Children[i])))
                        {
                            DataNode newNode = new DataNode();
                            newNode.DomNode = parent.Children[i];
                            newNode.IsHorizontalAlignmentExist = false;
                            newNode.IsUnite = true;
                            newNode.LineNumber = 1;
                            TreeNode node = new TreeNode();
                            node.Tag = newNode;
                            node.Text = parent.Children[i].TagName;
                            children.Add(node);
                        }
                    }
                    else
                    {
                        List<TreeNode> temp1 = new List<TreeNode>();// use only layout node to decide whether to regroup.
                        for (int k = 0; k < temp.Count; k++)
                        {
                            if (this.layoutHtmlElement.Contains(temp[k].Text))
                            {
                                temp1.Add(temp[k]);
                            }
                        }
                        if (!Method.viewble(parent.Children[i]))
                        {
                            children.AddRange(temp);
                        }
                        else if (Method.Regroup(temp1, parent.Children[i]))
                        {
                            children.AddRange(temp);
                        }
                        else
                        {

                            //normal process
                            DataNode newNode = new DataNode();
                            newNode.DomNode = parent.Children[i];
                            newNode.LineNumber = Method.ComputeLineNumber(temp);
                            newNode.IsHorizontalAlignmentExist = Method.checkHorizontalLayout(temp1);
                            if (newNode.IsHorizontalAlignmentExist == true)
                            {
                                newNode.IsUnite = false;
                            }
                            else
                            {
                                newNode.IsUnite = Method.AndOperation(temp);
                            }
                            // analyze at here
                            //add this newnode to children
                            TreeNode node = new TreeNode();
                            node.Tag = newNode;
                            node.Text = parent.Children[i].TagName;
                            node.Nodes.AddRange(temp.ToArray());
                            children.Add(node);
                        }
                    }
                }
            }
            if (children.Count == 0)
            {
                return null;
            }
            else
            {
                return children;
            }
        }

        private TreeNode LocateMainBlock(WebBrowser Browser, List<TreeNode> blockElements)
        {
            Console.WriteLine(Browser.Document.Body.ScrollRectangle.Width);
            Console.WriteLine(Browser.Document.Body.ScrollRectangle.Height);
            int centerX = Browser.Document.Body.ScrollRectangle.Width / 2 + Browser.Document.Body.ScrollLeft;
            int centerY = Browser.Document.Body.ScrollRectangle.Height / 2 + Browser.Document.Body.ScrollTop;
            double shortest = 0;
            TreeNode mainBlock = blockElements[0];
            for (int i = 0; i < blockElements.Count; i++)
            {
                IHTMLElement domElement = (IHTMLElement)(((DataNode)blockElements[i].Tag).DomNode.DomElement);
                int blockX = Method.getElementLeft(domElement) + domElement.offsetWidth / 2;
                int blockY = Method.getElementTop(domElement) + domElement.offsetHeight / 2;
                int x = Math.Abs(centerX - blockX);
                int y = Math.Abs(centerY - blockY);
                double distance = Math.Sqrt(x*x + y*y);
                if (i == 0)
                {
                    shortest = distance;
                    mainBlock = blockElements[i];
                }
                else
                {
                    if (shortest > distance)
                    {
                        shortest = distance;
                        mainBlock = blockElements[i];
                    }
                }
            }

            this.showShadow(Method.getElementRectangle(new List<HtmlElement> { ((DataNode)mainBlock.Tag).DomNode }));
            return mainBlock;
        }

        private List<string> extractLinks(TreeNode Block, string seedLink, string prefix)
        {
            List<string> urls = new List<string>();
            HtmlElement mainBlock = ((DataNode)Block.Tag).DomNode;
            HtmlElementCollection LinkCollection = mainBlock.GetElementsByTagName("A");
            string linkPath = null;
            foreach (HtmlElement link in LinkCollection)
            {
                string url = link.GetAttribute("href");
                Console.WriteLine(url);
                if (url.Equals(seedLink))
                {
                    linkPath = Method.getHtmlPath(link, this.webBrowser.Document.Body);
                    Console.WriteLine(linkPath);
                    break;
                }
            }
            if (linkPath == null)
            {
                Console.WriteLine("sample is invalid now!!!!!");
                Connection.markPageForRetraining(this.page);
                return urls;
                //alert in database
            }         
            foreach (HtmlElement link in LinkCollection)
            {
                string url1 = link.GetAttribute("href");
                string text = link.InnerText;
                if (text == null)
                {
                    text = "abc";
                }
                if ((!text.ToUpper().Contains("APPENDIX") && !text.ToUpper().Contains("ATTACHMENT")  && !text.ToUpper().Contains("ADDENDUM") && url1.IndexOf(prefix) == 0 
                    && Method.getHtmlPath(link, this.webBrowser.Document.Body).Equals(linkPath))
                    || url1.Equals(seedLink))
                {
                    Console.WriteLine(linkPath);
                    Console.WriteLine(url1);
                    urls.Add(url1);
                }
            }
            //this.processLinks(urls);
            if (urls.Count == 0)
            {
                Console.WriteLine("sample link maybe invalid now");
                Connection.markPageForRetraining(this.page);
                // alert in database
            }
            else
            {
                // update sample link, make it the newleast
                Connection.setSampleLink(this.page, this.page.prefixOffset, urls[0]);
            }
            return urls;
        }

        private void processLinks(List<string> urls)
        {
            computeMD5andSHA1 MD5 = new computeMD5andSHA1();
            string hashValue;
            
            foreach (string url in urls)
            {
                HttpWebRequest HttpWReq = (HttpWebRequest)WebRequest.Create(url);
                HttpWReq.Method = "HEAD";
                HttpWebResponse HttpWResp = (HttpWebResponse)HttpWReq.GetResponse();
                
                if (! HttpWResp.ContentType.Contains("text/html"))
                {
                    HttpWResp.Close();
                    Console.WriteLine(url);
                    hashValue = MD5.MD5File(url);
                    Console.WriteLine(hashValue);
                    Connection.checkIfNewHash(this.page, url, hashValue);
                    Connection.markPageAsVisited(this.page);
                }
                else
                {
                    HttpWResp.Close();
                    loading = true;
                    this.webBrowser.Navigate(url);
                    while (loading)
                    {
                        Application.DoEvents();
                    }
                    TreeNode mainblock = this.LocateMainBlock(this.webBrowser, this.eventElements);
                    string text = ((DataNode)mainblock.Tag).DomNode.InnerText;
                    Console.WriteLine(text.Trim());
                    hashValue = MD5.MD5Text(text.Trim());
                    Console.WriteLine(hashValue);
                    if (Connection.checkIfNewHash(this.page, url, hashValue))
                    {
                        //extract all the text previous marked
                        //Method.RetrieveElement(mainblock, "path");
                    }
                    Connection.markPageAsVisited(this.page);
                }
            }
        }

        private int getLongestPrefixSize(string url1, string url2)
        {
            int length = 0;
            url1 = url1.Substring(0, url1.LastIndexOf("/") + 1);
            url2 = url2.Substring(0, url2.LastIndexOf("/") + 1);
            if (url1.Length > url2.Length)
            {
                length = url2.Length;
            }
            else
            {
                length = url1.Length;
            }
            int size = 0;
            for (int i = 0; i < length; i++)
            {
                if (url1[i] == url2[i])
                {
                    size++;
                }
                else
                {
                    break;
                }
            }
            return size;
        }

        public class nodeComparer : IComparer<TreeNode>
        {
            public int Compare(TreeNode x, TreeNode y)
            {
                IHTMLElement x1 = (IHTMLElement)((DataNode)x.Tag).DomNode.DomElement;
                IHTMLElement y1 = (IHTMLElement)((DataNode)x.Tag).DomNode.DomElement;
                int x_left = Method.getElementLeft(x1);
                int y_left = Method.getElementLeft(y1);
                return x_left.CompareTo(y_left);
            }
        }
        
        private List<TreeNode> group(List<TreeNode> grouplist1)
        {
            List<List<int>> overlapIndex = new List<List<int>>();
            for (int i = 0; i < grouplist1.Count; i++)
            {
                overlapIndex.Add(new List<int>());
            }
            List<TreeNode> newChildren = new List<TreeNode>();
            grouplist1.Sort(new nodeComparer());
            for (int i = 0; i < grouplist1.Count; i++)
            {
                for (int j = i + 1; j < grouplist1.Count; j++)
                {
                    if (Method.checkOverlap(grouplist1[i], grouplist1[j]))
                    {
                        overlapIndex[i].Add(j);
                        overlapIndex[j].Add(i);
                    }
                }
            }
            List<TreeNode> grouplist = new List<TreeNode>(grouplist1);
            for (int i = 0; i < overlapIndex.Count; i++)
            {
                if (overlapIndex[i].Count > 1)
                {
                    for (int j = 0; j < overlapIndex[i].Count; j++)
                    {
                        for (int k = j + 1; k < overlapIndex[i].Count; k++)
                        {
                            if (! overlapIndex[overlapIndex[i][j]].Contains(overlapIndex[i][k]))
                            {
                                
                                if (! newChildren.Contains(grouplist1[i]))
                                { 
                                    newChildren.Add(grouplist1[i]);
                                }
                                grouplist.Remove(grouplist1[i]);
                            }
                        }

                    }
                }
            }
            IHTMLElement element1 = (IHTMLElement)((DataNode)grouplist[0].Tag).DomNode.DomElement;
            int maxleft = Method.getElementLeft(element1);
            int maxright = maxleft + element1.offsetWidth;
            int minleft = maxleft;
            int minright = minleft;
            int minDeviation = (int)(element1.offsetWidth * 0.3);
            int maxDeviation = minDeviation;
            List<TreeNode> bucket = new List<TreeNode>();
            for (int i = 0; i < grouplist.Count; i++)
            {
                IHTMLElement element = (IHTMLElement)((DataNode)grouplist[i].Tag).DomNode.DomElement;
                int element_left = Method.getElementLeft(element);
                if (element_left > maxleft - maxDeviation && element_left < minleft + minDeviation && element_left + element.offsetWidth > minright - minDeviation && element_left + element.offsetWidth < maxright + maxDeviation)
                {
                    bucket.Add(grouplist[i]);
                    if (element.offsetWidth > maxright - maxleft)
                    {
                        maxleft = element_left;
                        maxright = element_left + element.offsetWidth;
                    }
                    if (element.offsetWidth > minright - minleft)
                    {
                        minleft = element_left;
                        minright = element_left + element.offsetWidth;
                    }
                    maxDeviation = (int)((maxright - maxleft) * 0.3);
                    minDeviation = (int)((minright - minleft) * 0.3);
                }
                else
                {
                    if (bucket.Count > 1)
                    {
                        newChildren.Add(this.formDIV(bucket));
                    }
                    else
                    {
                        newChildren.AddRange(bucket);
                    }
                    bucket.Clear();
                    bucket.Add(grouplist[i]);
                    maxleft = element_left;
                    maxright = element_left + element.offsetWidth;
                    minleft = element_left;
                    minright = element_left + element.offsetWidth;
                    minDeviation = (int)(element.offsetWidth * 0.3);
                    maxDeviation = minDeviation;
                }

            }
            if (bucket.Count > 1)
            {
                newChildren.Add(this.formDIV(bucket));
            }
            else
            {
                newChildren.AddRange(bucket);
            }
            bucket.Clear();
            return newChildren;
        }

        private TreeNode formDIV(List<TreeNode> nodeList)
        {
            HtmlElement div = webBrowser.Document.CreateElement("DIV");
            webBrowser.Document.Body.AppendChild(div);
            IHTMLElement temp = (IHTMLElement)((DataNode)nodeList[0].Tag).DomNode.DomElement;
            int left = Method.getElementLeft(temp);
            int right = left + temp.offsetWidth;
            int top = Method.getElementTop(temp);
            int bottom = top + temp.offsetHeight;
            TreeNode node = new TreeNode();
            DataNode newNode = new DataNode();
            newNode.DomNode = div;
            newNode.IsUnite = Method.AndOperation(nodeList);
            newNode.IsHorizontalAlignmentExist = false;
            newNode.LineNumber = Method.ComputeLineNumber(nodeList);
            node.Tag = newNode;
            node.Text = "DIV";
            for (int i = 0; i < nodeList.Count; i++)
            {
                node.Nodes.Add(nodeList[i]);
                //if (((DataNode)nodeList[i].Tag).IsUnite == true)
                //{
                //    newNode.IsUnite = true;
                //}
                IHTMLElement element = (IHTMLElement)((DataNode)nodeList[i].Tag).DomNode.DomElement;
                int element_left = Method.getElementLeft(element);
                int element_top = Method.getElementTop(element);
                if (left > element_left)
                {
                    left = element_left;
                }
                if(right < element_left + element.offsetWidth)
                {
                    right = element_left + element.offsetWidth;
                }
                if(top > element_top)
                {
                    top = element_top;
                }
                if(bottom < element_top + element.offsetHeight)
                {
                    bottom = element_top + element.offsetHeight;
                }
            }
            IHTMLElement domdiv = (IHTMLElement)div.DomElement;
            //domdiv.style.display = "none";
            domdiv.id = "ooxx" + DateTime.Now.ToString();
            domdiv.style.setAttribute("position", "absolute");
            domdiv.style.top = top.ToString() + "px";
            domdiv.style.left = left.ToString() + "px";
            domdiv.style.width = (right - left).ToString() + "px";
            domdiv.style.height = (bottom - top).ToString() + "px";
            domdiv.style.zIndex = 10000;
            return node;
        }

        private void showShadow(Rectangle Rec)
        {
            
            HtmlElement div = webBrowser.Document.GetElementById("coverxx");
            IHTMLElement domdiv = (IHTMLElement)div.DomElement;
            domdiv.style.display = "block";
            domdiv.style.top = Rec.Top.ToString() + "px";
            domdiv.style.left = Rec.Left.ToString() + "px";
            domdiv.style.width = Rec.Width.ToString() + "px";
            domdiv.style.height = Rec.Height.ToString() + "px";
            domdiv.style.zIndex = "10000";
            domdiv.style.backgroundColor = "blue";
            domdiv.style.filter = "alpha(opacity = 50)";
            //timer1.Start();
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode selectedNode = e.Node;
            DataNode datanode = (DataNode)e.Node.Tag;
            HtmlElement currentNode = ((DataNode)selectedNode.Tag).DomNode;                        
            this.showShadow(Method.getElementRectangle(new List<HtmlElement>(){currentNode}));
            this.showNodeInfo(datanode);
        }
        private void showNodeInfo(DataNode dataNode)
        {
            HtmlElement Node = dataNode.DomNode;
            IHTMLElement temp = (IHTMLElement)Node.DomElement;
            IHTMLElement2 temp1 = (IHTMLElement2)Node.DomElement;
            this.listView1.Items.Clear();
            ListViewItem item = null;

            item = new ListViewItem("TagName", 0);
            item.SubItems.Add(temp.tagName);
            listView1.Items.Add(item);

            item = new ListViewItem("InnerText", 0);
            if (temp.innerText != null)
            {
                item.SubItems.Add(temp.innerText);
                listView1.Items.Add(item);
            }
            else
            {
                item.SubItems.Add("null");
                listView1.Items.Add(item);
            }

            item = new ListViewItem("InnerHtml", 0);
            if (temp.innerHTML != null)
            {
                item.SubItems.Add(temp.innerHTML);
                listView1.Items.Add(item);
            }
            else
            {
                item.SubItems.Add("null");
                listView1.Items.Add(item);
            }
            item = new ListViewItem("BackgroudColor", 0);
            item.SubItems.Add((string)temp1.currentStyle.backgroundColor);
            listView1.Items.Add(item);

            item = new ListViewItem("Color", 0);
            item.SubItems.Add((string)temp1.currentStyle.color);
            listView1.Items.Add(item);

            item = new ListViewItem("FontSize", 0);
            item.SubItems.Add(Convert.ToString(temp1.currentStyle.fontSize));
            listView1.Items.Add(item);

            item = new ListViewItem("FontWeight", 0);
            item.SubItems.Add(Convert.ToString(temp1.currentStyle.fontWeight));
            listView1.Items.Add(item);

            item = new ListViewItem("ClientHeight", 0);
            item.SubItems.Add(Convert.ToString(temp1.clientHeight));
            listView1.Items.Add(item);

            item = new ListViewItem("OffsetHeight", 0);
            item.SubItems.Add(Convert.ToString(temp.offsetHeight));
            listView1.Items.Add(item);

            item = new ListViewItem("Offsetwidth", 0);
            item.SubItems.Add(Convert.ToString(temp.offsetWidth));
            listView1.Items.Add(item);

            item = new ListViewItem("OffsetTop", 0);
            item.SubItems.Add(Convert.ToString(temp.offsetTop));
            listView1.Items.Add(item);

            item = new ListViewItem("OffsetLeft", 0);
            item.SubItems.Add(Convert.ToString(temp.offsetLeft));
            listView1.Items.Add(item);

            item = new ListViewItem("Left", 0);
            item.SubItems.Add(Convert.ToString(Method.getElementLeft(temp)));
            listView1.Items.Add(item);
            item = new ListViewItem("Top", 0);
            item.SubItems.Add(Convert.ToString(Method.getElementTop(temp)));
            listView1.Items.Add(item);

            item = new ListViewItem("Float", 0);
            item.SubItems.Add(Convert.ToString(temp1.currentStyle.getAttribute("float")));
            listView1.Items.Add(item);

            item = new ListViewItem("LineNumber", 0);
            item.SubItems.Add(Convert.ToString(dataNode.LineNumber));
            listView1.Items.Add(item);

            item = new ListViewItem("Isunit", 0);
            item.SubItems.Add(Convert.ToString(dataNode.IsUnite));
            listView1.Items.Add(item);

            item = new ListViewItem("HorizontalLayout", 0);
            item.SubItems.Add(Convert.ToString(dataNode.IsHorizontalAlignmentExist));
            listView1.Items.Add(item);

            item = new ListViewItem("borderWidth", 0);
            item.SubItems.Add(temp1.currentStyle.borderWidth);
            listView1.Items.Add(item);

            item = new ListViewItem("borderColor", 0);
            item.SubItems.Add(temp1.currentStyle.borderStyle);
            listView1.Items.Add(item);

            //item = new ListViewItem("Style", 0);
            //item.SubItems.Add(temp.style.);
            //listView1.Items.Add(item);
        }

        private void expandTree(TreeNode root, int horizonSplitTime, bool tag)// tag true when root has more than one children
        {
            DataNode node = (DataNode)root.Tag;
            if (node.IsUnite == true && root.Nodes.Count > 1)
            {
                if (tag == false)
                {
                    root.Expand();
                    eventElements.AddRange(root.Nodes.Cast<TreeNode>().ToArray());
                }
                else
                {
                    eventElements.Add(root);
                }               
                return;
            }
            else
            {
                if (horizonSplitTime == 1)
                {
                    eventElements.Add(root);
                    return;
                }
                root.Expand();
                if (node.IsHorizontalAlignmentExist)
                {
                    
                    if (tag == true || root.Nodes.Count > 1)
                    {
                        for (int i = 0; i < root.Nodes.Count; i++)
                        {
                            expandTree(root.Nodes[i], horizonSplitTime + 1, true);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < root.Nodes.Count; i++)
                        {
                            expandTree(root.Nodes[i], horizonSplitTime + 1, false);
                        }
                    }                     
                }
                else
                {
                    if (tag == true || root.Nodes.Count > 1)
                    {
                        for (int i = 0; i < root.Nodes.Count; i++)
                        {
                            expandTree(root.Nodes[i], horizonSplitTime, true);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < root.Nodes.Count; i++)
                        {
                            expandTree(root.Nodes[i], horizonSplitTime, false);
                        }
                    }
                }
            }
            if (root.Nodes.Count == 0)
            {
                eventElements.Add(root);
            }
        }

        /**********************variables used to store labeling result************************************************/

        /***************************************************************************************************/

        private void textBox_URL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string url = this.textBox_URL.Text;
                loading = true;
                this.webBrowser.Navigate(url);
                while (loading)
                {
                    Application.DoEvents();
                }
                TreeNode mainBlock = this.LocateMainBlock(this.webBrowser, this.eventElements);
                List<string> links =  this.extractLinks(mainBlock, this.textBox1.Text, this.textBox2.Text);
                this.processLinks(links);
            }
        }

        private void start_Click(object sender, EventArgs e)
        {
           // while (this.Continue)
            //{
                this.page = Connection.getOldestPage();
                string url = page.url;
                loading = true;
                this.webBrowser.Navigate(url);
                while (loading)
                {
                    Application.DoEvents();
                }
                TreeNode mainBlock = this.LocateMainBlock(this.webBrowser, this.eventElements);
                if (this.page.url.Equals(this.page.sampleLink))
                {
                    List<string> links = new List<string> { this.page.url };
                    this.processLinks(links);
                }
                else
                {
                    List<string> links = this.extractLinks(mainBlock, this.page.sampleLink, this.page.sampleLink.Substring(0, this.page.prefixOffset));
                    this.processLinks(links);                
                }
                
            //}
        }

        private void stop_Click(object sender, EventArgs e)
        {
            this.Continue = false;

        }
        private bool Continue = true;
    }
}
