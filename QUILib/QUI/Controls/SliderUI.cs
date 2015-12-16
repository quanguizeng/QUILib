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
    public class SliderUI : ProgressUI
    {
        public SliderUI()
        {
            mTextStyle = (int)FormatFlags.DT_SINGLELINE | (int)FormatFlags.DT_CENTER;
            mThumb.Width = mThumb.Height = 10;
            mButtonState = 0;

            mThumbImage = "";
            mThumbHotImage = "";
            mThumbPushedImage = "";

            mImageModify = "";

        }
        public override string getClass()
        {
            return "SliderUI";
        }
        public override int getControlFlags()
        {
            if (isEnabled()) return (int)ControlFlag.UIFLAG_SETCURSOR;
            else return 0;
        }
        public override ControlUI getInterface(string name)
        {
            if (name == "Slider") return (SliderUI)(this);
            return base.getInterface(name);
        }
        public override void setEnabled(bool bEnable)
        {
            base.setEnabled(bEnable);
            if (!isEnabled())
            {
                mButtonState = 0;
            }
        }
        public void setThumbSize(Size szXY)
        {
            mThumb = szXY;
        }
        public Rectangle getThumbRect()
        {
            if (mHorizontal)
            {
                int left = mRectItem.Left + (mRectItem.Right - mRectItem.Left - mThumb.Width) * (mValue - mMin) / (mMax - mMin);
                int top = (mRectItem.Bottom + mRectItem.Top - mThumb.Height) / 2;
                return new Rectangle(left, top, mThumb.Width, mThumb.Height);
            }
            else
            {
                int left = (mRectItem.Right + mRectItem.Left - mThumb.Width) / 2;
                int top = mRectItem.Bottom - mThumb.Height - (mRectItem.Bottom - mRectItem.Top - mThumb.Height) * (mValue - mMin) / (mMax - mMin);
                return new Rectangle(left, top, mThumb.Width, mThumb.Height);
            }
        }
        public string getThumbImage()
        {
            return mThumbImage;
        }
        public void setThumbImage(string pStrImage)
        {
            mThumbImage = pStrImage;
            invalidate();
        }
        public string getThumbHotImage()
        {
            return mThumbHotImage;
        }
        public void setThumbHotImage(string pStrImage)
        {
            mThumbHotImage = pStrImage;
            invalidate();
        }
        public string getThumbPushedImage()
        {
            return mThumbPushedImage;
        }

        public void setThumbPushedImage(string pStrImage)
        {
            mThumbPushedImage = pStrImage;
            invalidate();
        }
        public override void eventProc(ref TEventUI newEvent)
        {
            if (!isMouseEnabled() && newEvent.mType > (int)EVENTTYPE_UI.UIEVENT__MOUSEBEGIN && newEvent.mType < (int)EVENTTYPE_UI.UIEVENT__MOUSEEND)
            {
                if (mParent != null) mParent.eventProc(ref newEvent);
                else base.eventProc(ref newEvent);
                return;
            }

            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONDOWN || newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_DBLCLICK)
            {
                if (isEnabled())
                {
                    Rectangle rcThumb = getThumbRect();
                    if (rcThumb.Contains(newEvent.mMousePos))
                    {
                        mButtonState |= (int)PaintFlags.UISTATE_CAPTURED;
                    }
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONUP)
            {
                if ((mButtonState & (int)PaintFlags.UISTATE_CAPTURED) != 0)
                {
                    mButtonState &= ~(int)PaintFlags.UISTATE_CAPTURED;
                    mManager.sendNotify(this, "valuechanged");
                    invalidate();
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSEMOVE)
            {
                if ((mButtonState & (int)PaintFlags.UISTATE_CAPTURED) != 0)
                {
                    if (mHorizontal)
                    {
                        if (newEvent.mMousePos.X >= mRectItem.Right - mThumb.Width / 2)
                        {
                            mValue = mMax;
                        }
                        else if (newEvent.mMousePos.X <= mRectItem.Left + mThumb.Width / 2)
                        {
                            mValue = mMin;
                        }
                        else
                        {
                            mValue = mMin + (mMax - mMin) * (newEvent.mMousePos.X - mRectItem.Left - mThumb.Width / 2) / (mRectItem.Right - mRectItem.Left - mThumb.Width);
                        }
                    }
                    else
                    {
                        if (newEvent.mMousePos.Y >= mRectItem.Bottom - mThumb.Height / 2)
                        {
                            mValue = mMin;
                        }
                        else if (newEvent.mMousePos.Y <= mRectItem.Top + mThumb.Height / 2)
                        {
                            mValue = mMax;
                        }
                        else
                        {
                            mValue = mMin + (mMax - mMin) * (mRectItem.Bottom - newEvent.mMousePos.Y - mThumb.Height / 2) / (mRectItem.Bottom - mRectItem.Top - mThumb.Height);
                        }
                    }

                    invalidate();
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SETCURSOR)
            {
                Rectangle rcThumb = getThumbRect();
                if (isEnabled() && rcThumb.Contains(newEvent.mMousePos))
                {
                    if (mManager.getPaintWindow().Cursor != Cursors.Hand)
                    {
                        mManager.getPaintWindow().Cursor = Cursors.Hand;
                    }
                    return;
                }
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
                if (isEnabled())
                {
                    mButtonState &= ~(int)PaintFlags.UISTATE_HOT;
                    invalidate();
                }
                return;
            }
            eventProc0(ref newEvent);
        }
        public override void setAttribute(string pstrName, string pstrValue)
        {
            if (pstrName == "thumbimage") setThumbImage(pstrValue);
            else if (pstrName == "thumbhotimage") setThumbHotImage(pstrValue);
            else if (pstrName == "thumbpushedimage") setThumbPushedImage(pstrValue);
            else if (pstrName == "thumbsize")
            {
                Size szXY = new Size();
                string[] listValue = pstrValue.Split(',');
                szXY.Width = int.Parse(listValue[0]);
                szXY.Height = int.Parse(listValue[1]);
                setThumbSize(szXY);
            }
            else base.setAttribute(pstrName, pstrValue);
        }

        public override void paintStatusImage(ref Graphics hDC, ref Bitmap bitmap)
        {
            base.paintStatusImage(ref hDC, ref bitmap);

            Rectangle rcThumb = getThumbRect();
            int newLeft = rcThumb.Left - mRectItem.Left;
            int newRight = rcThumb.Right - mRectItem.Left;
            int newTop = rcThumb.Top - mRectItem.Top;
            int newBottom = rcThumb.Bottom - mRectItem.Top;
            rcThumb.X = newLeft;
            rcThumb.Width = newRight-newLeft;
            rcThumb.Y = newTop;
            rcThumb.Height = newBottom-newTop;

            if ((mButtonState & (int)PaintFlags.UISTATE_CAPTURED) != 0)
            {
                if (mThumbPushedImage != "")
                {
                    mImageModify = "";
                    mImageModify = string.Format("dest='{0},{1},{2},{3}'", rcThumb.Left, rcThumb.Top, rcThumb.Right, rcThumb.Bottom);
                    if (!drawImage(ref hDC, ref bitmap, mThumbPushedImage, mImageModify)) mThumbPushedImage = "";
                    else return;
                }
            }
            else if ((mButtonState & (int)PaintFlags.UISTATE_HOT) != 0)
            {
                if (mThumbHotImage != "")
                {
                    mImageModify = "";
                    mImageModify = string.Format("dest='{0},{1},{2},{3}'", rcThumb.Left, rcThumb.Top, rcThumb.Right, rcThumb.Bottom);
                    if (!drawImage(ref hDC, ref bitmap, mThumbHotImage, mImageModify)) mThumbHotImage = "";
                    else return;
                }
            }

            if (mThumbImage != "")
            {
                mImageModify = "";
                mImageModify = string.Format("dest='{0},{1},{2},{3}'", rcThumb.Left, rcThumb.Top, rcThumb.Right, rcThumb.Bottom);
                if (!drawImage(ref hDC, ref bitmap, mThumbImage, mImageModify)) mThumbImage = "";
                else return;
            }
        }

        protected Size mThumb;
        protected int mButtonState;

        protected string mThumbImage;
        protected string mThumbHotImage;
        protected string mThumbPushedImage;

        protected string mImageModify;
    }
}
