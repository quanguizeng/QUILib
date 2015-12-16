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
    public class LabelUI : ControlUI
    {
        public LabelUI()
        {
            mTextStyle = (int)FormatFlags.DT_VCENTER;
            mTextColor = Color.FromArgb(0);
            mDisabledTextColor = Color.FromArgb(0);
            mFont = -1;
            mShowHtml = false;
        }
        ~LabelUI()
        {

        }

        public override string getClass()
        {
            return "LabelUI";
        }
        public override ControlUI getInterface(string name)
        {
            if ("Label" == name)
            {
                return this;
            }

            return base.getInterface(name);
        }

        public void setTextStyle(int style)
        {
            mTextStyle = style;
            invalidate();
        }
        public void setTextColor(Color textColor)
        {
            mTextColor = textColor;
        }
        public void setDisabledTextColor(Color textColor)
        {
            mDisabledTextColor = textColor;
        }
        public void setFont(int idx)
        {
            mFont = idx;
        }
        public Rectangle getTextPadding()
        {
            return mRectTextPadding;
        }
        public void setTextPadding(Rectangle rc)
        {
            mRectPadding = rc;
            invalidate();
        }
        public bool isShowHtml()
        {
            return mShowHtml;
        }
        public void setShowHtml(bool showHtml)
        {
            if (showHtml == mShowHtml)
            {
                return;
            }

            mShowHtml = showHtml;
            invalidate();
        }
        public override Size estimateSize(Size available)
        {
            if (available.Height == 0)
            {
                Size sz = new Size(mXYFixed.Width, mManager.getDefaultFont().Height + 4);
                return sz;
            }

            return base.estimateSize(available);
        }
        public override void eventProc(ref TEventUI newEvent)
        {
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SETFOCUS)
            {
                mFocused = true;
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_KILLFOCUS)
            {
                mFocused = false;
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSEENTER)
            {
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSELEAVE)
            {
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONDOWN)
            {
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONUP)
            {
                return;
            }

            base.eventProc(ref newEvent);
        }
        public override void setAttribute(string name, string value)
        {
            if (name == "align")
            {
                if (value == "left")
                {
                    mTextStyle &= ~((int)FormatFlags.DT_CENTER | (int)FormatFlags.DT_RIGHT);
                    mTextStyle |= (int)FormatFlags.DT_LEFT;
                }
                if (value == "center")
                {
                    mTextStyle &= ~((int)FormatFlags.DT_LEFT | (int)FormatFlags.DT_RIGHT);
                    mTextStyle |= (int)FormatFlags.DT_CENTER;
                }
                if (value == "right")
                {
                    mTextStyle &= ~((int)FormatFlags.DT_LEFT | (int)FormatFlags.DT_CENTER);
                    mTextStyle |= (int)FormatFlags.DT_RIGHT;
                }
                if (value =="top")
                {
                    mTextStyle &= ~((int)FormatFlags.DT_BOTTOM | (int)FormatFlags.DT_VCENTER | (int)FormatFlags.DT_LEFT | (int)FormatFlags.DT_RIGHT);
                    mTextStyle |= (int)FormatFlags.DT_TOP;
                }
                if (value =="bottom")
                {
                    mTextStyle &= ~((int)FormatFlags.DT_TOP | (int)FormatFlags.DT_VCENTER | (int)FormatFlags.DT_LEFT | (int)FormatFlags.DT_RIGHT);
                    mTextStyle |= (int)FormatFlags.DT_BOTTOM;
                }
            }
            else if (name == "font") setFont(int.Parse(value));
            else if (name == "textcolor")
            {
                value = value.TrimStart('#');
                if(value.Length == 6)
                {
                    value =  "FF"+value;
                }
                if(value[0] == '0' && value[1] == '0')
                {
                    value = "FF" + value.Substring(2);
                }
                setTextColor(Color.FromArgb(Convert.ToInt32(value, 16)));
            }
            else if (name == "disabledtextcolor")
            {
                value = value.TrimStart('#');
                setDisabledTextColor(Color.FromArgb(Convert.ToInt32(value, 16)));
            }
            else if (name == "textpadding")
            {
                string[] listValue = value.Split(',');

                if (listValue.Length != 4)
                {
                    throw new Exception("textpadding 的参数个数为4个");
                }
                setTextPadding(new Rectangle(int.Parse(listValue[0]), int.Parse(listValue[1]), int.Parse(listValue[2]), int.Parse(listValue[3])));
            }
            else if (name == "showhtml") setShowHtml(value == "true");
            else base.setAttribute(name, value);
        }
        public override void paintText(ref Graphics graphics, ref Bitmap bitmap)
        {
            base.paintText(ref graphics,ref bitmap);
            if (mTextColor.ToArgb() == 0) mTextColor = mManager.getDefaultFontColor();
            if (mDisabledTextColor.ToArgb() == 0) mDisabledTextColor = mManager.getDefaultDisabledColor();

            if (mText == "") return;
            int nLinks = 0;
            Rectangle rc = new Rectangle(mRectItem.Left + mRectTextPadding.Left,
                mRectItem.Top + mRectTextPadding.Top,
                mRectItem.Right - mRectTextPadding.Right - mRectItem.Left,
                mRectItem.Bottom - mRectTextPadding.Bottom - mRectItem.Top);

            Rectangle[] rc1 = null;
            string[] s1 = null;
            if (isEnabled())
            {
                if (mShowHtml)
                {
                    RenderEngine.drawHtmlText(ref graphics, ref bitmap, ref mManager, ref rc, mText, mTextColor.ToArgb(), ref rc1, ref s1, ref nLinks, (int)((int)FormatFlags.DT_SINGLELINE | mTextStyle));
                }
                else
                {
                    RenderEngine.drawText(ref graphics,ref bitmap, ref mManager,ref rc, mText, mTextColor.ToArgb(), mFont, (int)((int)FormatFlags.DT_SINGLELINE | mTextStyle));
                }
            }
            else
            {
                if (mShowHtml)
                {
                    RenderEngine.drawHtmlText(ref graphics,ref bitmap, ref mManager, ref rc, mText, mDisabledTextColor.ToArgb(), ref rc1, ref s1, ref nLinks, (int)((int)FormatFlags.DT_SINGLELINE | mTextStyle));

                }
                else
                {
                    RenderEngine.drawText(ref graphics,ref bitmap, ref mManager, ref rc, mText, mDisabledTextColor.ToArgb(), mFont, (int)((int)FormatFlags.DT_SINGLELINE | mTextStyle));
                }
            }
        }

        protected Color mTextColor;
        protected Color mDisabledTextColor;
        protected int mFont;
        protected int mTextStyle;
        protected Rectangle mRectTextPadding;
        protected bool mShowHtml;
    }

}
