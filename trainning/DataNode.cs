using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using mshtml;
namespace bid
{
    class DataNode
    {
        private HtmlElement domNode;

        public HtmlElement DomNode
        {
            get { return domNode; }
            set { domNode = value; }
        }
        private bool isUnite;

        public bool IsUnite
        {
            get { return isUnite; }
            set { isUnite = value; }
        }
        private bool isHorizontalAlignmentExist;

        public bool IsHorizontalAlignmentExist
        {
            get { return isHorizontalAlignmentExist; }
            set { isHorizontalAlignmentExist = value; }
        }
        private int lineNumber;

        public int LineNumber
        {
            get { return lineNumber; }
            set { lineNumber = value; }
        }


        public DataNode(HtmlElement htmlElement)
        {
            this.domNode = htmlElement;
            this.isUnite = false;
            this.isHorizontalAlignmentExist = false;
        }
        public DataNode()
        {
            this.domNode = null;
            this.isUnite = false;
            this.isHorizontalAlignmentExist = false;
            this.lineNumber = 0;
        }
    }
}
