using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using mshtml;
using System.Windows.Forms;
using System.Collections;
namespace bid
{
    static class Method
    {

        public static Rectangle getElementRectangle(List<HtmlElement> elements)
        {
            int left = getElementLeft((IHTMLElement)elements[0].DomElement);
            int top = getElementTop((IHTMLElement)elements[0].DomElement);
            int height = ((IHTMLElement)elements[0].DomElement).offsetHeight;
            int width = ((IHTMLElement)elements[0].DomElement).offsetWidth;
            IHTMLElement temp;
            int offsetLeft;
            int offsetTop;
            for (int i = 1; i < elements.Count; i++)
            {
                temp = (IHTMLElement)elements[i].DomElement;
                offsetLeft = getElementLeft(temp);
                offsetTop = getElementTop(temp);
                int Ptop = top;
                int Pleft = left;
                if (offsetTop < top)
                {
                    top = offsetTop;
                }
                if (offsetLeft < left)
                {
                    left = offsetLeft;
                }
                if (Ptop + height < offsetTop + temp.offsetHeight)
                {
                    height = offsetTop + temp.offsetHeight - top;
                }
                else
                {
                    height = Ptop + height - top;
                }
                if (Pleft + width < offsetLeft + temp.offsetWidth)
                {
                    width = offsetLeft + temp.offsetWidth - left;
                }
                else
                {
                    width = Pleft + width - left;
                }
            }
            return new Rectangle(left, top, width, height);
        }
        public static int getElementTop(IHTMLElement element)
        {
            int actualTop = element.offsetTop;
            IHTMLElement current = null;
            
            if (element.offsetParent != null)
            {
                current = element.offsetParent;
            }
            while (current != null)
            {
                actualTop += current.offsetTop;
                current = current.offsetParent;
            }
            return actualTop;
        }

        public static int getElementLeft(IHTMLElement element)
        {
            int actualLeft = element.offsetLeft;
            IHTMLElement current = null;
            if (element.offsetParent != null)
            {
                current = element.offsetParent;
            }
            while (current != null)
            {
                actualLeft += current.offsetLeft;
                current = current.offsetParent;
            }
            return actualLeft;
        }

        public static bool AndOperation(List<TreeNode> list)// list element can not be 0
        {

            for (int i = 0; i < list.Count; i++)
            {
                if (((DataNode)list[i].Tag).IsUnite == false)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool invalidNode(HtmlElement node)
        {
            IHTMLElement2 currentNode1 = (IHTMLElement2)node.DomElement;
            IHTMLElement currentNode = (IHTMLElement)node.DomElement;
            string id = "bearhehe";
            if (currentNode.id != null)
            {
                id = currentNode.id;
            }
            if (currentNode1.currentStyle.display.Contains("none")
                || currentNode1.currentStyle.visibility.Contains("hidden")
                || currentNode.tagName.Equals("STYLE")
                || currentNode.tagName.Equals("APPLET")
                || currentNode.tagName.Equals("NOSCRIPT")
                || currentNode.tagName.Equals("SCRIPT")
                || currentNode.tagName.Equals("!")
                || currentNode.tagName.Equals("BR")
                || id.Contains("ooxx")
                || Method.getElementLeft(currentNode) + currentNode.offsetWidth < 0
                || Method.getElementTop(currentNode) + currentNode.offsetHeight < 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Regroup(List<TreeNode> children, HtmlElement parent)
        {
            IHTMLElement currentNode = (IHTMLElement)parent.DomElement;
            
            if (parent.TagName.Equals("TD") || parent.TagName.Equals("TBODY") || parent.TagName.Equals("TR"))
            {
                if (!((IHTMLElement2)Method.getTableElement(parent).DomElement).currentStyle.borderStyle.Equals("none"))
                {
                    return false;
                }
            }
            for (int i = 0; i < children.Count; i++)
            {
                IHTMLElement temp = (IHTMLElement)((DataNode)children[i].Tag).DomNode.DomElement;
                int temp_left = Method.getElementLeft(temp);
                int current_left = Method.getElementLeft(currentNode);
                int temp_top = Method.getElementTop(temp);
                int current_top = Method.getElementTop(currentNode);
                if (temp_top < current_top - 10
                    || temp_left < current_left - 10
                    || temp_left + temp.offsetWidth > current_left + currentNode.offsetWidth + 10
                    || temp_top + temp.offsetHeight > currentNode.offsetHeight + current_top + 10)
                {
                    Console.Write(currentNode.tagName + ": " + current_left + " " + current_top + " " + temp.tagName + ": " + temp_left + " " + temp_top + "\n");
                    return true;
                }
            }
            if (currentNode.tagName.Equals("TR"))
            {
                if (((IHTMLElement)currentNode.children[0]).offsetLeft > 10
                    || ((IHTMLElement)currentNode.children[parent.Children.Count - 1]).offsetLeft + ((IHTMLElement)currentNode.children[parent.Children.Count - 1]).offsetWidth < currentNode.offsetWidth + currentNode.offsetLeft - 10)
                {
                    return true;
                }
            }
            return false;
        }

        public static HtmlElement getTableElement(HtmlElement node)
        {
            HtmlElement current = node.Parent;
            while (!current.TagName.Equals("TABLE"))
            {
                current = current.Parent;
            }
            return current;
        }

        public static bool checkHorizontalLayout(List<TreeNode> children)
        {
            for (int i = 0; i < children.Count; i++)
            {
                IHTMLElement temp1 = (IHTMLElement)((DataNode)children[i].Tag).DomNode.DomElement;
                IHTMLElement2 temp3 = (IHTMLElement2)((DataNode)children[i].Tag).DomNode.DomElement;
                int top1 = Method.getElementTop(temp1);
                int left1 = Method.getElementLeft(temp1);
                for (int j = i + 1; j < children.Count; j++)
                {
                    IHTMLElement temp2 = (IHTMLElement)((DataNode)children[j].Tag).DomNode.DomElement;
                    IHTMLElement2 temp4 = (IHTMLElement2)((DataNode)children[j].Tag).DomNode.DomElement;
                    if (temp1.tagName.Equals("TD") && temp2.tagName.Equals("TD"))
                    {
                        if (!((IHTMLElement2)Method.getTableElement(((DataNode)children[i].Tag).DomNode).DomElement).currentStyle.borderStyle.Equals("none"))
                        {
                            continue;
                        }
                    }
                    int top2 = Method.getElementTop(temp2);
                    int left2 = Method.getElementLeft(temp2);
                    if (top1 < top2 + 1)
                    {
                        if (top2 < top1 + temp1.offsetHeight)
                        {
                            if (left1 + temp1.offsetWidth < left2 + 5 || left2 + temp2.offsetWidth < left1 + 5)//5 to count for the unexpected design error
                            {
                                if (((DataNode)children[i].Tag).LineNumber == 2 && ((DataNode)children[j].Tag).LineNumber == 2)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (top1 < top2 + temp2.offsetHeight)
                        {
                            if (left1 + temp1.offsetWidth < left2 + 5 || left2 + temp2.offsetWidth < left1 + 5)
                            {
                                if (((DataNode)children[i].Tag).LineNumber == 2 && ((DataNode)children[j].Tag).LineNumber == 2)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool checkOverlap(TreeNode node1, TreeNode node2)
        {
            IHTMLElement element1 = (IHTMLElement)((DataNode)node1.Tag).DomNode.DomElement;
            IHTMLElement element2 = (IHTMLElement)((DataNode)node2.Tag).DomNode.DomElement;
            int element1_left = Method.getElementLeft(element1);
            int element2_left = Method.getElementLeft(element2);
            if (element1_left < element2_left)
            {
                if (element1_left + element1.offsetWidth - 10 > element2_left)
                {
                    return true;
                }
            }
            else
            {
                if (element1_left < element2_left + element2.offsetWidth - 10)
                {
                    return true;
                }
            }

            return false;
        }

        public static int ComputeLineNumber(List<TreeNode> children)
        {
            IHTMLElement temp = (IHTMLElement)((DataNode)children[0].Tag).DomNode.DomElement;
            int top = Method.getElementTop(temp);
            int height = temp.offsetHeight;
            for (int i = 0; i < children.Count; i++)
            {
                if (((DataNode)children[i].Tag).LineNumber == 2)
                {
                    return 2;
                }
                else
                {
                    IHTMLElement element = (IHTMLElement)((DataNode)children[i].Tag).DomNode.DomElement;
                    if (Method.getElementTop(element) > top + height - 1 || Method.getElementTop(element) + element.offsetHeight < top + 1)
                    {
                        return 2;
                    }
                }
            }
            return 1;
        }

        public static bool viewble(HtmlElement node)
        {
            IHTMLElement currentNode = (IHTMLElement)node.DomElement;
            if (currentNode.offsetHeight > 1 && currentNode.offsetWidth > 1 && currentNode.offsetTop > -1 && currentNode.offsetLeft > -1)// use getelementtop?
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void addEvent(List<HtmlElement> eventElements, HtmlElementEventHandler mouseEvent)
        {
            for (int i = 0; i < eventElements.Count; i++)
            {
                eventElements[i].MouseOver += mouseEvent;
            }
        }
        public static void addEvent(List<TreeNode> eventElements, HtmlElementEventHandler mouseEvent)
        {
            for (int i = 0; i < eventElements.Count; i++)
            {
                ((DataNode)eventElements[i].Tag).DomNode.MouseOver += mouseEvent;
            }
        }

        public static void removeEvent(List<TreeNode> eventElements, HtmlElementEventHandler mouseEvent)
        {
            for (int i = 0; i < eventElements.Count; i++)
            {
                ((DataNode)eventElements[i].Tag).DomNode.MouseOver -= mouseEvent;
            }
        }

        public static void removeEvent(HtmlElement element, HtmlElementEventHandler mouseEvent)
        {
            element.MouseOver -= mouseEvent;
        }

        public static TreeNode findSelectedTreeNode(List<TreeNode> eventElements, HtmlElement choosenElement)
        {
            for (int i = 0; i < eventElements.Count; i++)
            {
                if (((DataNode)eventElements[i].Tag).DomNode.Equals(choosenElement))
                {
                    return eventElements[i];
                }
            }
            return null;
        }

        private static int getLongestPrefixSize(string url1, string url2)
        {
            int length = 0;
            //url1 = url1.Substring(0, url1.LastIndexOf("/") + 1);
            //url2 = url2.Substring(0, url2.LastIndexOf("/") + 1);
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

        public static int commonPrefix(List<string> urls)
        {
            int prefix = urls[0].Length;
            for (int i = 0; i < urls.Count; i++)
            {
                for (int j = i + 1; j < urls.Count; j++)
                {
                    int length = Method.getLongestPrefixSize(urls[i], urls[j]);
                    if (length < prefix)
                    {
                        prefix = length;
                    }
                }
            }
            return prefix;
        }

        public static List<TreeNode> nodeExpand(TreeNode node)
        {
            TreeNode temp = node;
            while (temp.Nodes.Count == 1)
            {
                temp.Expand();
                temp = temp.Nodes[0];
            }
            if (temp.Nodes.Count == 0)
            {
                return new List<TreeNode>() { temp };
            }
            else
            {
                temp.Expand();
                return temp.Nodes.Cast<TreeNode>().ToList<TreeNode>();
            }
        }

        public static TreeNode RetrieveElement(TreeNode root, string path)
        {
            string[] pathArray = path.Split('/');
            TreeNode target = null;
            if (pathArray.Length == 1)
            {
                return root;
            }
            for (int i = 1; i < pathArray.Length; i++)
            {
                int index = int.Parse(pathArray[i].Split(' ')[1]);
                string nodeName = pathArray[i].Split(' ')[0];

                if (root.Nodes[index].Text.Equals(nodeName))
                {
                    root = root.Nodes[index];
                    target = root;
                }
                else
                {
                    target = null;
                    break;
                }
            }
            return target;
        }

        public static string getPath(TreeNode startNode, TreeNode root)
        {
            TreeNode currentnode = startNode;
            string path = "";
            while (!currentnode.Equals(root))
            {
                path = "/" + currentnode.Text + " " + currentnode.Index.ToString() + path;
                currentnode = currentnode.Parent;
            }
            path = currentnode.Text + " " + currentnode.Index.ToString() + path;
            return path;
        }
    }
}
