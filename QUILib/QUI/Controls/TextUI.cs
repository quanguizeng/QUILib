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
    public class TextUI : LabelUI
    {
        public TextUI()
        {
            mRectLinks = new Rectangle[MAX_LINK];
            mLinks = new string[MAX_LINK];
            mNumLinks = 0;
            mHoverLink = -1;

            mTextStyle = (int)FormatFlags.DT_WORDBREAK;
            mRectTextPadding.X = 2;
            mRectTextPadding.Width = 0;
        }

        public override string getClass()
        {
            return "TextUI";
        }

        public override ControlUI getInterface(string pstrName)
        {
            if (pstrName == "Text") return (TextUI)(this);
            return base.getInterface(pstrName);
        }

        public override int getControlFlags()
        {
            if (isEnabled() && mNumLinks > 0) return (int)ControlFlag.UIFLAG_SETCURSOR;
            else return 0;
        }
        public string getLinkContent(int iIndex)
        {
            if (iIndex >= 0 && iIndex < mNumLinks) return mLinks[iIndex];
            return "";
        }
        public override void eventProc(ref TEventUI newEvent)
        {
            if (!isMouseEnabled() && newEvent.mType > (int)EVENTTYPE_UI.UIEVENT__MOUSEBEGIN && newEvent.mType < (int)EVENTTYPE_UI.UIEVENT__MOUSEEND)
            {
                if (mParent != null) mParent.eventProc(ref newEvent);
                else base.eventProc(ref newEvent);
                return;
            }

            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SETCURSOR)
            {
                for (int i = 0; i < mNumLinks; i++)
                {
                    if (mRectLinks[i].Contains(newEvent.mMousePos))
                    {
                        if (mManager.getPaintWindow().Cursor != Cursors.Hand)
                        {
                            mManager.getPaintWindow().Cursor = Cursors.Hand;
                        }
                        return;
                    }
                }
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONDOWN || newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_DBLCLICK && isEnabled())
            {
                for (int i = 0; i < mNumLinks; i++)
                {
                    if (mRectLinks[i].Contains(newEvent.mMousePos))
                    {
                        invalidate();
                        return;
                    }
                }
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONUP && isEnabled())
            {
                for (int i = 0; i < mNumLinks; i++)
                {
                    if (mRectLinks[i].Contains(newEvent.mMousePos))
                    {
                        mManager.sendNotify(this, "link", i);
                        return;
                    }
                }
            }
            // 在文本控件上盘旋时触发
            if (mNumLinks > 0 && newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSEMOVE && isEnabled())
            {
                int nHoverLink = -1;
                for (int i = 0; i < mNumLinks; i++)
                {
                    if (mRectLinks[i].Contains(newEvent.mMousePos))
                    {
                        nHoverLink = i;
                        break;
                    }
                }

                if (mHoverLink != nHoverLink)
                {
                    mHoverLink = nHoverLink;
                    invalidate();
                    return;
                }
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSELEAVE)
            {
                if (mNumLinks > 0 && isEnabled())
                {
                    if (mHoverLink != -1)
                    {
                        mHoverLink = -1;
                        invalidate();
                        return;
                    }
                }
            }

            base.eventProc(ref newEvent);
        }
        public override Size estimateSize(Size szAvailable)
        {
            Rectangle rcText = new Rectangle(0, 0, Math.Max(szAvailable.Width, mXYFixed.Width), 9999);

            rcText.X += mRectTextPadding.X;
            rcText.Width = rcText.Right - mRectTextPadding.Right - rcText.X;

            Graphics buffer = mManager.getBufferManager().getGraphics();
            Bitmap bitmap = mManager.getBufferManager().getBackBuffer();
            Rectangle[] rcS = null;
            string[] ss = null;

            if (mShowHtml)
            {
                int nLinks = 0;
                RenderEngine.drawHtmlText(ref buffer, ref bitmap, ref mManager, ref rcText, mText, mTextColor.ToArgb(), ref rcS, ref ss, ref nLinks, (int)FormatFlags.DT_CALCRECT | mTextStyle);
            }
            else
            {
                RenderEngine.drawText(ref buffer, ref bitmap, ref mManager, ref rcText, mText, mTextColor.ToArgb(), mFont, (int)FormatFlags.DT_CALCRECT | mTextStyle);
            }
            Size cXY = new Size(rcText.Right - rcText.Left + mRectTextPadding.Left + mRectTextPadding.Right,
                rcText.Bottom - rcText.Top + mRectTextPadding.Top + mRectTextPadding.Bottom);

            if (mXYFixed.Height != 0)
            {
                cXY.Height = mXYFixed.Height;
            }

            return cXY;
        }
        public override void paintText(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (mText == "")
            {
                mNumLinks = 0;
                return;
            }

            if (mTextColor.ToArgb() == 0) mTextColor = mManager.getDefaultFontColor();
            if (mDisabledTextColor.ToArgb() == 0) mDisabledTextColor = mManager.getDefaultDisabledColor();

            if (mText == "") return;

            mNumLinks = mRectLinks.Length;
            Rectangle rc = mRectItem;
            rc.X += mRectTextPadding.X;
            rc.Width = rc.Right - mRectTextPadding.Right - rc.X;
            rc.Y += mRectTextPadding.Top;
            rc.Height = rc.Bottom - mRectTextPadding.Bottom - rc.Y;

            if (isEnabled())
            {
                if (mShowHtml)
                {
                    RenderEngine.drawHtmlText(ref graphics, ref bitmap, ref mManager, ref rc, mText, mTextColor.ToArgb(), ref mRectLinks, ref mLinks, ref mNumLinks, mTextStyle);
                }
                else
                {
                    RenderEngine.drawText(ref graphics, ref bitmap, ref mManager, ref rc, mText, mTextColor.ToArgb(), mFont, mTextStyle);
                }
            }
            else
            {
                if (mShowHtml)
                {
                    RenderEngine.drawHtmlText(ref graphics, ref bitmap, ref mManager, ref rc, mText, mDisabledTextColor.ToArgb(), ref mRectLinks, ref mLinks, ref mNumLinks, mTextStyle);
                }
                else
                {
                    RenderEngine.drawText(ref graphics, ref bitmap, ref mManager, ref rc, mText, mDisabledTextColor.ToArgb(), mFont, mTextStyle);
                }
            }
        }

        protected const int MAX_LINK = 8;
        protected int mNumLinks;
        protected Rectangle[] mRectLinks;
        protected string[] mLinks;
        protected int mHoverLink;
    }
}
