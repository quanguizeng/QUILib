using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Collections;
using System.Drawing.Drawing2D;

namespace QUI
{
    public class DialogLayoutUI : ContainerUI
    {
        public DialogLayoutUI()
        {
        }
        ~DialogLayoutUI()
        {
        }

        public override string getClass()
        {
            return "DialogLayoutUI";
        }

        public override ControlUI getInterface(string name)
        {
            if (name == "DialogLayout")
            {
                return this;
            }

            return base.getInterface(name);
        }

        public void setStretchMode(ControlUI control, UInt32 mode)
        {
        }

        public override void setPos(Rectangle rect)
        {
        }
        public override Size estimateSize(Size available)
        {
            return base.estimateSize(available);
        }

        public Rectangle recalcArea()
        {
            Rectangle rect = new Rectangle();

            return rect;
        }


        protected class STRETCHMODE
        {
            ControlUI mControl;
            UInt32 mMode;
            Rectangle mRectItem;
        }

        protected List<STRETCHMODE> mListMode;
    }
}
