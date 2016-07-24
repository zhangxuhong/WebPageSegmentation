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
            this.layoutHtmlElement = new List<string> { "TABLE","TH", "TR", "TD", "LI", "OL", "DL", "DD", "DT", "DIV", "H1", "H2", "H3", "H4", "H5", "H6", "P" };
            this.leafHtmlElement = new List<string> { "A", "ADDRESS", "BLOCKQUOTE", "DEL", "PRE", "SELECT", "FIELDSET", "MAP", "EM", "H1", "H2", "H3", "H4", "H5", "H6", "OBJECT" };
            this.eventElements = new List<TreeNode>();
            this.urls = new List<string>();
            //mouseEvent1 = new HtmlElementEventHandler(MouseOver1);
            mouseEvent = new HtmlElementEventHandler(MouseOver);
            try
            {
                Connection = new MysqlDatabaseConnection();
                Connection.Connect();
            }
            catch (MySql.Data.MySqlClient.MySqlException dbe)
            {
                Console.WriteLine(dbe.Message);
            }
            this.loading = true;
            this.selectedElements = new List<HtmlElement>();
        }

        //////////////////////////////// instance variable declaration/////////////////////////////////////////////////////
        private MysqlDatabaseConnection Connection;
        private List<string> leafHtmlElement;
        private List<string> layoutHtmlElement;
        private List<TreeNode> eventElements;
        private HtmlElementEventHandler mouseEvent;
        private HtmlElement selectedElement;
        private List<HtmlElement> selectedElements;
        private bool loading;
        private List<string> urls;
        private string pageUrl;
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

        // shows the menue, when right click
        private void Document_ContextMenuShowing(object sender, HtmlElementEventArgs e)
        {
            e.ReturnValue = e.ShiftKeyPressed;
            if (!e.ReturnValue)
            {
                if (this.Notice.Text.Equals("Please mark bid details!"))
                {
                    contextMenuStrip2.Show(Cursor.Position);
                }
                else
                {
                    contextMenuStrip1.Show(Cursor.Position);
                }
            }
        }

        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.Equals(webBrowser.Document.Url) && webBrowser.ReadyState == WebBrowserReadyState.Complete && webBrowser.IsBusy == false)
            {
                //
                this.selectedElement = null;
                webBrowser.Document.ContextMenuShowing += new HtmlElementEventHandler(Document_ContextMenuShowing);
                HtmlDocument htmlDocument = webBrowser.Document;
                HtmlElement body = htmlDocument.Body;
                //body.style.borderColor = "red";
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

        private void MouseOver(object sender , HtmlElementEventArgs e)
        {          
            HtmlElement element = sender as HtmlElement;
            e.BubbleEvent = false;
            if (e.CtrlKeyPressed)
            {
                selectedElements.Add(element);
                //Console.WriteLine("***" + selectedElements.Count);
                //Console.WriteLine(selectedElements[1].InnerHtml);                
            }
            else
            {
                selectedElements.Clear();
                selectedElements.Add(element);                
            }
            this.selectedElement = element;
            this.showShadow(Method.getElementRectangle(selectedElements));   
        }

        private void MouseOver1(object sender, HtmlElementEventArgs e)// event for leaf node in the treenode list
        {
            HtmlElement element = e.ToElement;
            HtmlElement sourceElement = sender as HtmlElement;
            if (!sourceElement.Equals(element) || element.Children.Count == 0)
            {
                this.selectedElement = element;
                IHTMLElement currentNode = (IHTMLElement)element.DomElement;
                e.BubbleEvent = false;
                //this.showShadow(currentNode);
            }
        }
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode selectedNode = e.Node;
            DataNode datanode = (DataNode)e.Node.Tag;
            HtmlElement currentNode = ((DataNode)selectedNode.Tag).DomNode;                        
            this.showShadow(Method.getElementRectangle(new List<HtmlElement>(){currentNode}));
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
        private void Yes_Click(object sender, EventArgs e)
        {
            //Method.addEvent(this.eventElements, this.mouseEvent);
            if (this.Notice.Text.Equals("Locate Correctly ?"))
            {
                this.Notice.Text = "Please select sample links";
                this.showShadow(new Rectangle(0,0,0,0));
                this.button1.Visible = true;
            }
            else
            {
                //decide prefix and store into database
                int prefix = Method.commonPrefix(this.urls);
                //Connection.addPage(this.pageUrl, prefix, urls[0]);
                /*************************************************************/
                /*this is where to add new labeling process for bid details*/
                this.eventElements.Clear();
                this.eventElements.AddRange(Method.nodeExpand(mainBlock));
                Method.addEvent(this.eventElements, this.mouseEvent);
                this.Notice.Text = "Please mark bid details!";
                this.showShadow(new Rectangle(0, 0, 0, 0));
                /*************************************************************/
            }
            
        }
        private TreeNode mainBlock;
        private void textBox_URL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.pageUrl = this.textBox_URL.Text;
                this.loading = true;
                this.eventElements.Clear();
                this.webBrowser.Navigate(pageUrl);
                this.Notice.Text = "Processing";
                while (loading)
                {
                    Application.DoEvents();
                }
                
                TreeNode mainBlock = this.LocateMainBlock(this.webBrowser, this.eventElements);
                
                this.Notice.Text = "Locate Correctly ?";
                HtmlElement Block = ((DataNode)mainBlock.Tag).DomNode;
                HtmlElementCollection LinkCollection = Block.GetElementsByTagName("A");
                Method.addEvent(LinkCollection.Cast<HtmlElement>().ToList<HtmlElement>(), this.mouseEvent);
                this.selectedElements.Clear();
                this.urls.Clear();
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
                double distance = Math.Sqrt(x * x + y * y);
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

        private void CompleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            //foreach (HtmlElement link in this.selectedElements)
            //{
            //    string url = link.GetAttribute("href");
            //    if (!urls.Contains(url))
            //    {
            //        urls.Add(url);
            //        Console.WriteLine(url);
            //    }
            //}
            if (urls[0].Substring(urls[0].LastIndexOf(".")).ToUpper().Equals(".PDF") || urls[0].Substring(urls[0].LastIndexOf(".")).ToUpper().Equals(".DOC"))
            {
                //decide prefix and store into database;
                int prefix = Method.commonPrefix(this.urls);
                Connection.addPage(this.pageUrl, prefix, urls[0]);
                this.Notice.Text = "Finished !";
                return;
            }
            this.eventElements.Clear();
            this.loading = true;
            this.webBrowser.Navigate(this.urls[0]);
            while (loading)
            {
                Application.DoEvents();
            }
            this.Notice.Text = "bid information in the shadow area?";
            mainBlock = this.LocateMainBlock(this.webBrowser, this.eventElements);
        }

        private void Complete_Click(object sender, EventArgs e)
        {

        }

        private void valid1_Click(object sender, EventArgs e)
        {
            //add to bad link tables for manully check
            this.Notice.Text = "Bad Link, Try another one.";

        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string url = this.selectedElement.GetAttribute("href");
            if (! urls.Contains(url))
            {
                urls.Add(url);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Connection.addPage(this.pageUrl,0,this.pageUrl);
            this.Notice.Text = "Finished!";
            this.button1.Visible = false;
        }

        private void nEXTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode currentNode = Method.findSelectedTreeNode(this.eventElements,this.selectedElement);
            this.showShadow(new Rectangle(0, 0, 0, 0));
            Method.removeEvent(this.selectedElement, this.mouseEvent);
            this.eventElements.Remove(currentNode);
            List<TreeNode> children = Method.nodeExpand(currentNode);
            this.eventElements.AddRange(children);
            Method.addEvent(children, this.mouseEvent);
            
        }

        private void bidTittleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string itemText = item.Text;
            string path = Method.getPath(Method.findSelectedTreeNode(this.eventElements, this.selectedElement), this.mainBlock);

            Console.WriteLine(path);
            if (itemText.Equals("bid tittle"))
            {

            }
            else if (itemText.Equals("bid number"))
            {

            }
            else if (itemText.Equals("due day"))
            {

            }
            else
            {

            }
        }

    }
}
