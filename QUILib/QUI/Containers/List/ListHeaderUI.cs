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
    public class ListHeaderUI : HorizontalLayoutUI
    {
        public ListHeaderUI()
        {

        }
        ~ListHeaderUI()
        {

        }
        public override string getClass()
        {
            return "ListHeaderUI";
        }

        public override ControlUI getInterface(string pstrName)
        {
            if (pstrName == "ListHeader") return this;
            return base.getInterface(pstrName);
        }
        public override Size estimateSize(Size szAvailable)
        {
            Size cXY = new Size(0, mXYFixed.Height);
            if (cXY.Height == 0 && mManager != null)
            {
                cXY.Height = mManager.getDefaultFont().Height + 6;
            }

            for (int it = 0; it < mItems.Count; it++)
            {
                cXY.Width += mItems[it].estimateSize(szAvailable).Width;
            }

            return cXY;
        }
        public override void eventProc(ref TEventUI newEvent)
        {
            // 设置鼠标形状
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SETCURSOR)
            {
                if (mManager.getPaintWindow().Cursor != Cursors.Arrow)
                {
                    mManager.getPaintWindow().Cursor = Cursors.Arrow;
                }
                return;
            }

            base.eventProc(ref newEvent);
        }
        public override int getControlFlags()
        {
            if (!isEnabled()) return base.getControlFlags();

            return (int)ControlFlag.UIFLAG_SETCURSOR | (int)ControlFlag.UIFLAG_TABSTOP;
        }



    }
}
