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
    public class ListHeaderItemUI : ControlUI
    {
        public ListHeaderItemUI()
        {
            mDragable = true;
            mButtonState = 0;
            mSepWidth = 4;
            mTextStyle = (int)FormatFlags.DT_VCENTER | (int)FormatFlags.DT_CENTER | (int)FormatFlags.DT_SINGLELINE;
            mTextColor = 0;
            mFont = -1;
            mShowHtml = false;
            mLastMouse.X = mLastMouse.Y = 0;
            setMinWidth(16);

            mNormalImage = "";
            mHotImage = "";
            mPushedImage = "";
            mFocusedImage = "";
            mSepImage = "";
            mSepImageModify = "";
        }
        public override string getClass()
        {
            return "ListHeaderItemUI";
        }
        public override ControlUI getInterface(string pstrName)
        {
            if (pstrName == "ListHeaderItem")
            {
                return this;
            }
            return base.getInterface(pstrName);
        }
        public override int getControlFlags()
        {
            if (isEnabled() && mSepWidth != 0)
            {
                return (int)ControlFlag.UIFLAG_SETCURSOR;
            }
            else
            {
                return 0;
            }
        }
        public override void setEnabled(bool bEnable)
        {
            base.setEnabled(bEnable);
            if (!isEnabled())
            {
                mButtonState = 0;
            }
        }
        public void setDragable(bool bDragable)
        {
            mDragable = bDragable;
            if (!mDragable)
            {
                mButtonState &= ~(int)ControlFlag.UISTATE_CAPTURED;
            }
        }
        public void setSepWidth(int iWidth)
        {
            mSepWidth = iWidth;
        }
        public void setTextStyle(int uStyle)
        {
            mTextStyle = uStyle;
            invalidate();
        }
        public void setTextColor(int dwTextColor)
        {
            mTextColor = dwTextColor;
        }
        public void setFont(int index)
        {
            mFont = index;
        }
        public bool isShowHtml()
        {
            return mShowHtml;
        }
        public void setShowHtml(bool bShowHtml)
        {
            if (mShowHtml == bShowHtml)
            {
                return;
            }

            mShowHtml = bShowHtml;
            invalidate();
        }
        public void setNormalImage(string pStrImage)
        {
            mNormalImage = pStrImage;
            invalidate();
        }
        public string getHotImage()
        {
            return mHotImage;
        }
        public void setHotImage(string pStrImage)
        {
            mHotImage = pStrImage;
            invalidate();
        }
        public string getPushedImage()
        {
            return mPushedImage;
        }
        public void setPushedImage(string pStrImage)
        {
            mPushedImage = pStrImage;
            invalidate();
        }
        public string getFocusedImage()
        {
            return mFocusedImage;
        }
        public void setFocusedImage(string pStrImage)
        {
            mFocusedImage = pStrImage;
            invalidate();
        }
        public string getSepImage()
        {
            return mSepImage;
        }
        public void setSepImage(string pStrImage)
        {
            mSepImage = pStrImage;
            invalidate();
        }
        public override void setAttribute(string pstrName, string pstrValue)
        {
            if (pstrName == "dragable") setDragable(pstrValue == "true");
            else if (pstrName == "sepwidth") setSepWidth(int.Parse(pstrValue));
            else if (pstrName == "align")
            {
                if (pstrValue.IndexOf("left") >= 0)
                {
                    mTextStyle &= ~((int)FormatFlags.DT_CENTER | (int)FormatFlags.DT_RIGHT);
                    mTextStyle |= (int)FormatFlags.DT_LEFT;
                }
                if (pstrValue.IndexOf("center") >= 0)
                {
                    mTextStyle &= ~((int)FormatFlags.DT_LEFT | (int)FormatFlags.DT_RIGHT);
                    mTextStyle |= (int)FormatFlags.DT_CENTER;
                }
                if (pstrValue.IndexOf("right") >= 0)
                {
                    mTextStyle &= ~((int)FormatFlags.DT_LEFT | (int)FormatFlags.DT_CENTER);
                    mTextStyle |= (int)FormatFlags.DT_RIGHT;
                }
            }
            else if (pstrName == "font") setFont(int.Parse(pstrValue));
            else if (pstrName == "textcolor")
            {
                pstrValue = pstrValue.TrimStart('#');
                int clrColor = Convert.ToInt32(pstrValue, 16);
                setTextColor(clrColor);
            }
            else if (pstrName == "showhtml") setShowHtml(pstrValue == "true");
            else if (pstrName == "normalimage") setNormalImage(pstrValue);
            else if (pstrName == "hotimage") setHotImage(pstrValue);
            else if (pstrName == "pushedimage") setPushedImage(pstrValue);
            else if (pstrName == "focusedimage") setFocusedImage(pstrValue);
            else if (pstrName == "sepimage") setSepImage(pstrValue);
            else base.setAttribute(pstrName, pstrValue);
        }

        public override void eventProc(ref TEventUI newEvent)
        {
            if (!isMouseEnabled() && newEvent.mType > (int)EVENTTYPE_UI.UIEVENT__MOUSEBEGIN && newEvent.mType < (int)EVENTTYPE_UI.UIEVENT__MOUSEEND)
            {
                if (mParent != null) mParent.eventProc(ref newEvent);
                else base.eventProc(ref newEvent);
                return;
            }

            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SETFOCUS)
            {
                invalidate();
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_KILLFOCUS)
            {
                invalidate();
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONDOWN || newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_DBLCLICK)
            {
                if (!isEnabled()) return;
                Rectangle rcSeparator = getThumbRect();
                if (rcSeparator.Contains(newEvent.mMousePos))
                {
                    if (mDragable)
                    {
                        mButtonState |= (int)PaintFlags.UISTATE_CAPTURED;
                        mLastMouse = newEvent.mMousePos;
                    }
                }
                else
                {
                    mButtonState |= (int)PaintFlags.UISTATE_PUSHED;
                    mManager.sendNotify(this, "headerclick");
                    invalidate();
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONUP)
            {
                if ((mButtonState & (int)PaintFlags.UISTATE_CAPTURED) != 0)
                {
                    mButtonState &= ~(int)PaintFlags.UISTATE_CAPTURED;
                    if (getParent() != null)
                    {
                        getParent().needParentUpdate();
                    }
                }
                else if ((mButtonState & (int)PaintFlags.UISTATE_PUSHED) != 0)
                {
                    mButtonState &= ~(int)PaintFlags.UISTATE_PUSHED;
                    invalidate();
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSEMOVE)
            {
                if ((mButtonState & (int)PaintFlags.UISTATE_CAPTURED) != 0)
                {
                    Rectangle rc = mRectItem;
                    if (mSepWidth >= 0)
                    {
                        int newRight = rc.Right - (mLastMouse.X - newEvent.mMousePos.X);
                        rc.Width = newRight - rc.Left;
                    }
                    else
                    {
                        int newLeft = rc.Left - (mLastMouse.X - newEvent.mMousePos.X);
                        rc.X = newLeft;
                    }

                    if (rc.Right - rc.Left > getMinWidth())
                    {
                        mXYFixed.Width = rc.Right - rc.Left;
                        mLastMouse = newEvent.mMousePos;
                        if (getParent() != null)
                        {
                            getParent().needParentUpdate();
                        }
                    }
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SETCURSOR)
            {
                Rectangle rcSeparator = getThumbRect();
                if (isEnabled() && mDragable && rcSeparator.Contains(newEvent.mMousePos))
                {
                    if (mManager.getPaintWindow().Cursor != Cursors.SizeWE)
                    {
                        mManager.getPaintWindow().Cursor = Cursors.SizeWE;
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
            base.eventProc(ref newEvent);
        }
        public override Size estimateSize(Size szAvailable)
        {
            if (mXYFixed.Height == 0) return new Size(mXYFixed.Width, mManager.getDefaultFont().Height + 14);
            return base.estimateSize(szAvailable);
        }
        public Rectangle getThumbRect()
        {
            if (mSepWidth >= 0)
            {
                int newLeft = mRectItem.Right - mSepWidth;
                int newRight = mRectItem.Right;
                int newTop = mRectItem.Top;
                int newBottom = mRectItem.Bottom;

                return new Rectangle(newLeft, newTop, newRight - newLeft, newBottom - newTop);
            }
            else
            {
                int newLeft = mRectItem.Left;
                int newRight = mRectItem.Left - mSepWidth;
                int newTop = mRectItem.Top;
                int newBottom = mRectItem.Bottom;

                return new Rectangle(newLeft, newTop, newRight - newLeft, newBottom - newTop);
            }
        }
        public override void paintStatusImage(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (isFocused()) mButtonState |= (int)PaintFlags.UISTATE_FOCUSED;
            else mButtonState &= ~(int)PaintFlags.UISTATE_FOCUSED;

            if ((mButtonState & (int)PaintFlags.UISTATE_PUSHED) != 0)
            {
                if (mPushedImage == "" && mNormalImage != "") drawImage(ref graphics, ref bitmap, mNormalImage);
                if (!drawImage(ref graphics, ref bitmap, mPushedImage)) mPushedImage = "";
            }
            else if ((mButtonState & (int)PaintFlags.UISTATE_HOT) != 0)
            {
                if (mHotImage == "" && mNormalImage != "") drawImage(ref graphics, ref bitmap, mNormalImage);
                if (!drawImage(ref graphics, ref bitmap, mHotImage)) mHotImage = "";
            }
            else if ((mButtonState & (int)PaintFlags.UISTATE_FOCUSED) != 0)
            {
                if (mFocusedImage == "" && mNormalImage != "") drawImage(ref graphics, ref bitmap, mNormalImage);
                if (!drawImage(ref graphics, ref bitmap, mFocusedImage)) mFocusedImage = "";
            }
            else
            {
                if (mNormalImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mNormalImage)) mNormalImage = "";
                }
            }

            if (mSepImage != "")
            {
                Rectangle rcThumb = getThumbRect();

                int newLeft = rcThumb.Left - mRectItem.Left;
                int newTop = rcThumb.Top - mRectItem.Top;
                int newRight = rcThumb.Right - mRectItem.Left;
                int newBottom = rcThumb.Bottom - mRectItem.Top;
                rcThumb.X = newLeft;
                rcThumb.Width = newRight - newLeft;
                rcThumb.Y = newTop;
                rcThumb.Height = newBottom - newTop;

                mSepImageModify = "";
                mSepImageModify = string.Format("dest='{0},{1},{2},{3}'", newLeft, newTop, newRight, newBottom);
                if (!drawImage(ref graphics, ref bitmap, mSepImage, mSepImageModify)) mSepImage = "";
            }
        }

        public override void paintText(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (mTextColor == 0) mTextColor = mManager.getDefaultFontColor().ToArgb();

            if (mText == "") return;
            int nLinks = 0;
            if (mShowHtml)
            {
                Rectangle[] rcs = null;
                string[] ss = null; ;
                RenderEngine.drawHtmlText(ref graphics,
                    ref bitmap,
                    ref mManager,
                    ref mRectItem,
                    mText,
                    mTextColor,
                    ref rcs,
                    ref ss,
                    ref nLinks,
                    (int)FormatFlags.DT_SINGLELINE | mTextStyle);
            }
            else
            {
                RenderEngine.drawText(ref graphics,
                    ref bitmap,
                    ref mManager,
                    ref mRectItem,
                    mText,
                    mTextColor,
                    mFont,
                    (int)FormatFlags.DT_SINGLELINE | mTextStyle);
            }
        }

        protected Point mLastMouse;
        protected bool mDragable;
        protected int mButtonState;
        protected int mSepWidth;
        protected int mTextColor;
        protected int mFont;
        protected int mTextStyle;
        protected bool mShowHtml;
        protected string mNormalImage;
        protected string mHotImage;
        protected string mPushedImage;
        protected string mFocusedImage;
        protected string mSepImage;
        protected string mSepImageModify;
    }
}
