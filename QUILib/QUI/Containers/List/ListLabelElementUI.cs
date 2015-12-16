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
    public class ListLabelElementUI : ListElementUI
    {
        public ListLabelElementUI()
        {
        }
        ~ListLabelElementUI()
        {

        }
        public override string getClass()
        {
            return "ListLabelElementUI";
        }
        public override ControlUI getInterface(string name)
        {
            if ("ListLabelElement" == name)
            {
                return this;
            }

            return base.getInterface(name);
        }

        public override void eventProc(ref TEventUI newEvent)
        {
            if (!isMouseEnabled() && newEvent.mType > (int)EVENTTYPE_UI.UIEVENT__MOUSEBEGIN && newEvent.mType < (int)EVENTTYPE_UI.UIEVENT__MOUSEEND)
            {
                if (mOwner != null) mOwner.eventProc(ref newEvent);
                else base.eventProc(ref newEvent);
                return;
            }

            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONDOWN)
            {
                if (isEnabled())
                {
                    mManager.sendNotify(this, "itemclick");
                    select();
                    invalidate();
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSEMOVE)
            {
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONUP)
            {
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSEENTER)
            {
                if (isEnabled())
                {
                    mButtonState |= (int)PaintFlags.UISTATE_HOT;
                    invalidate();
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSELEAVE)
            {
                if ((mButtonState & (int)PaintFlags.UISTATE_HOT) != 0)
                {
                    mButtonState &= ~(int)PaintFlags.UISTATE_HOT;
                    invalidate();
                }
                return;
            }
            base.eventProc(ref newEvent);
        }

        // 渲染前会调用该函数计算列表大小
        public override Size estimateSize(Size szAvailable)
        {
            if (mOwner == null)
            {
                return new Size(0, 0);
            }

            TListInfoUI pInfo = mOwner.getListInfo();
            Size cXY = mXYFixed;
            if (cXY.Height == 0 && mManager != null)
            {
                cXY.Height = mManager.getDefaultFont().Height + 8;
                cXY.Height += pInfo.mRectTextPadding.Top + pInfo.mRectTextPadding.Bottom;
            }

            if (cXY.Width == 0 && mManager != null)
            {
                Rectangle rcText = new Rectangle(0, 0, 9999, cXY.Height);
                if (pInfo.mShowHtml)
                {
                    int nLinks = 0;
                    Rectangle[] rcs = null;
                    string[] sss = null;
                    Graphics graphics = mManager.getBufferManager().getGraphics();
                    Bitmap bitmap = mManager.getBufferManager().getBackBuffer();
                    RenderEngine.drawHtmlText(ref graphics,
                        ref bitmap,
                        ref mManager,
                        ref rcText,
                        mText,
                        0,
                        ref rcs,
                        ref sss,
                        ref nLinks,
                        (int)FormatFlags.DT_SINGLELINE | (int)FormatFlags.DT_CALCRECT | pInfo.mTextStyle & ~(int)FormatFlags.DT_RIGHT & ~(int)FormatFlags.DT_CENTER);
                }
                else
                {
                    Graphics graphics = mManager.getBufferManager().getGraphics();
                    Bitmap bitmap = mManager.getBufferManager().getBackBuffer();

                    RenderEngine.drawText(ref graphics,
                        ref bitmap,
                        ref mManager,
                        ref rcText,
                        mText,
                        0,
                        pInfo.mFontIdx,
                       (int)FormatFlags.DT_SINGLELINE | (int)FormatFlags.DT_CALCRECT | pInfo.mTextStyle & ~(int)FormatFlags.DT_RIGHT & ~(int)FormatFlags.DT_CENTER);
                }
                cXY.Width = rcText.Right - rcText.Left + pInfo.mRectTextPadding.Left + pInfo.mRectTextPadding.Right;
            }

            return cXY;
        }

        public override void doPaint(ref Graphics graphics, ref Bitmap bitmap, Rectangle rcPaint)
        {
            if (rcPaint.IntersectsWith(mRectItem) == false)
            {
                return;
            }

            drawItemBk(ref graphics, ref bitmap, ref mRectItem);
            drawItemText(ref graphics, ref bitmap, ref mRectItem);
        }

        public override void drawItemText(ref Graphics graphics, ref Bitmap bitmap, ref Rectangle rcItem)
        {
            if (mText == "")
            {
                return;
            }

            if (mOwner == null)
            {
                return;
            }
            TListInfoUI pInfo = mOwner.getListInfo();
            int iTextColor = pInfo.mTextColor;
            if ((mButtonState & (int)PaintFlags.UISTATE_HOT) != 0)
            {
                iTextColor = pInfo.mHotTextColor;
            }
            if (isSelected())
            {
                iTextColor = pInfo.mSelectedTextColor;
            }
            if (!isEnabled())
            {
                iTextColor = pInfo.mDisabledTextColor;
            }
            int nLinks = 0;
            Rectangle rcText = rcItem;
            int newLeft = rcText.Left + pInfo.mRectTextPadding.Left;
            int newRight = rcText.Right - pInfo.mRectTextPadding.Right;
            int newTop = rcText.Top + pInfo.mRectTextPadding.Top;
            int newBottom = rcText.Bottom - pInfo.mRectTextPadding.Bottom;

            rcText.X = newLeft;
            rcText.Width = newRight - newLeft;
            rcText.Y = newTop;
            rcText.Height = newBottom - newTop;

            Rectangle rcccc = rcText;

            if (pInfo.mShowHtml)
            {
                Rectangle[] rcs = null;
                string[] ss = null;
                RenderEngine.drawHtmlText(ref graphics,
                    ref bitmap,
                    ref mManager,
                    ref rcText,
                    mText,
                    iTextColor,
                   ref rcs,
                   ref ss,
                   ref nLinks,
                   (int)FormatFlags.DT_SINGLELINE | pInfo.mTextStyle);
            }
            else
            {
                RenderEngine.drawText(ref graphics,
                    ref bitmap,
                    ref mManager,
                    ref rcText,
                    mText,
                    iTextColor,
                    pInfo.mFontIdx,
                    (int)FormatFlags.DT_SINGLELINE | pInfo.mTextStyle);

            }


        }


        // 属性

    }


}
