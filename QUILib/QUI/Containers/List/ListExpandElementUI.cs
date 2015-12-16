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
    public class ListExpandElementUI : ListTextElementUI
    {
        public ListExpandElementUI()
        {
            mExpanded = false;
            mHideSelf = false;
            mSubControl = null;
        }
        ~ListExpandElementUI()
        {

        }

        public override string getClass()
        {
            return "ListExpandElementUI";
        }
        public override ControlUI getInterface(string pstrName)
        {
            if (pstrName == "ListExpandElement") return (ListExpandElementUI)this;
            return base.getInterface(pstrName);
        }
        public override bool isExpanded()
        {
            return mExpanded;
        }
        public override bool expand(bool bExpand)
        {
            if (mOwner == null) return false;
            if (bExpand == mExpanded) return true;
            mExpanded = bExpand;
            if (!mOwner.expandItem(mIndex, bExpand)) return false;
            // We need to manage the "expanding items", which are actually embedded controls
            // that we selectively display or not.
            if (bExpand)
            {
                if (mSubControl != null)
                {
                    mSubControl = null;
                }
                mSubControl = null;

                if (mManager != null)
                {
                    mManager.sendNotify(this, "itemexpand");
                }
                if (mSubControl != null)
                {
                    mManager.initControls(mSubControl, this);
                }
            }
            else
            {
                if (mManager != null)
                {
                    mManager.sendNotify(this, "itemcollapse");
                }
            }
            return true;
        }
        public bool getExpandHideSelf()
        {
            return mHideSelf;
        }
        public void setExpandHideSelf(bool bHideSelf)
        {
            mHideSelf = bHideSelf;
            needParentUpdate();
        }
        public Rectangle getExpanderRect()
        {
            return mRectExpander;
        }
        public void setExpanderRect(Rectangle rc)
        {
            mRectExpander = rc;
        }
        public void setExpandItem(ControlUI pControl)
        {
            if (mSubControl != null)
            {
                mSubControl = null;
            }
            mSubControl = pControl;
        }
        public override void setAttribute(string name, string value)
        {
            if (name == "expander")
            {
                string[] listValue = value.Split(',');
                Rectangle rcExpander = new Rectangle(int.Parse(listValue[0]),
                    int.Parse(listValue[1]),
                    int.Parse(listValue[2]) - int.Parse(listValue[0]),
                    int.Parse(listValue[3]) - int.Parse(listValue[1]));

                setExpanderRect(rcExpander);
            }
            else if (name == "hideself")
            {
                setExpandHideSelf(value == "true");
            }
            else
            {
                base.setAttribute(name, value);
            }
        }
        public override void eventProc(ref TEventUI newEvent)
        {
            if (!isMouseEnabled() && newEvent.mType > (int)EVENTTYPE_UI.UIEVENT__MOUSEBEGIN && newEvent.mType < (int)EVENTTYPE_UI.UIEVENT__MOUSEEND)
            {
                if (mOwner != null)
                {
                    mOwner.eventProc(ref newEvent);
                }
                else
                {
                    base.eventProc(ref newEvent);
                }

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
                if (mOwner == null) return;
                TListInfoUI pInfo = mOwner.getListInfo();
                Rectangle rcExpander = new Rectangle(mRectItem.Left + mRectExpander.Left,
                    mRectItem.Top + mRectExpander.Top,
                    mRectItem.Left + mRectExpander.Right,
                    mRectItem.Top + mRectExpander.Bottom);
                if (pInfo.mExpandable && rcExpander.Contains(newEvent.mMousePos))
                {
                    expand(!mExpanded);
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_KEYDOWN)
            {
                switch ((Keys)newEvent.mKey)
                {
                    case Keys.Left:
                        expand(false);
                        return;
                    case Keys.Right:
                        expand(true);
                        return;
                }
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSEENTER)
            {
                if ((mButtonState & (int)PaintFlags.UISTATE_HOT) == 0)
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
        public override Size estimateSize(Size szAvailable)
        {
            if (mExpanded && mSubControl != null)
            {
                if (mHideSelf) return mSubControl.estimateSize(szAvailable);

                Size sz = mSubControl.estimateSize(szAvailable);
                if (sz.Height < mSubControl.getMinHeight()) sz.Height = mSubControl.getMinHeight();
                if (sz.Height > mSubControl.getMaxHeight()) sz.Height = mSubControl.getMaxHeight();

                if (mXYFixed.Height == 0) sz.Height += base.estimateSize(szAvailable).Height;
                else sz.Height += mXYFixed.Height;
                return new Size(mXYFixed.Width, sz.Height);
            }
            else
            {
                return base.estimateSize(szAvailable);
            }
        }
        public override void setVisible(bool bVisible)
        {
            if (mVisible == bVisible) return;
            setVisible0(bVisible);

            if (mExpanded && mSubControl != null)
            {
                mSubControl.setInternVisible(bVisible);
            }
        }
        public override void setInternVisible(bool bVisible)
        {
            if (mExpanded && mSubControl != null)
            {
                mSubControl.setInternVisible(bVisible);
            }
            setInternVisible0(bVisible);
        }
        public override void setMouseEnabled(bool bEnabled)
        {
            mMouseEnabled = bEnabled;

            if (mExpanded && mSubControl != null)
            {
                mSubControl.setMouseEnabled(bEnabled);
            }
        }
        public override void setPos(Rectangle rc)
        {
            base.setPos(rc);
            if (mSubControl != null)
            {
                if (mHideSelf)
                {
                    Rectangle rcPadding = mSubControl.getPadding();
                    Rectangle rcSubItem = new Rectangle(rc.Left + rcPadding.Left,
                        rc.Top + rcPadding.Top,
                        rc.Right - rcPadding.Right,
                        rc.Bottom - rcPadding.Bottom);

                    mSubControl.setPos(rcSubItem);
                }
                else
                {
                    Size sz = mSubControl.estimateSize(new Size(rc.Right - rc.Left, rc.Bottom - rc.Top));
                    if (sz.Height < mSubControl.getMinHeight()) sz.Height = mSubControl.getMinHeight();
                    if (sz.Height > mSubControl.getMaxHeight()) sz.Height = mSubControl.getMaxHeight();
                    Rectangle rcPadding = mSubControl.getPadding();

                    if (sz.Height == 0 && mXYFixed.Height == 0)
                    {
                        Rectangle rcSubItem = new Rectangle(rc.Left + rcPadding.Left,
                            (rc.Bottom + rc.Top) / 2 + rcPadding.Top,
                            rc.Right - rcPadding.Right,
                            rc.Bottom - rcPadding.Bottom);
                        mSubControl.setPos(rcSubItem);
                    }
                    else if (sz.Height != 0 && mXYFixed.Height == 0)
                    {
                        Rectangle rcSubItem = new Rectangle(rc.Left + rcPadding.Left,
                            rc.Bottom - sz.Height - rcPadding.Bottom,
                            rc.Right - rcPadding.Right,
                            rc.Bottom - rcPadding.Bottom);
                        mSubControl.setPos(rcSubItem);
                    }
                    else
                    {
                        Rectangle rcSubItem = new Rectangle(rc.Left + rcPadding.Left,
                            rc.Top + mXYFixed.Height + rcPadding.Top,
                            rc.Right - rcPadding.Right,
                            rc.Bottom - rcPadding.Bottom);
                        mSubControl.setPos(rcSubItem);
                    }
                }
            }
        }
        public override void doPaint(ref Graphics graphics, ref Bitmap bitmap, Rectangle rcPaint)
        {
            base.doPaint(ref graphics, ref bitmap, rcPaint);

            if (mExpanded && mSubControl != null)
            {
                mSubControl.doPaint(ref graphics, ref bitmap, rcPaint);
            }
        }
        public override void setManager(PaintManagerUI pManager, ControlUI pParent)
        {
            if (mExpanded && mSubControl != null) mSubControl.setManager(pManager, this);
            base.setManager(pManager, pParent);
        }
        public override ControlUI findControl(FINDCONTROLPROC Proc, ref object pData, uint uFlags)
        {
            ControlUI pResult = null;
            if (mExpanded && mSubControl != null) pResult = mSubControl.findControl(Proc, ref pData, uFlags);
            if (pResult == null) pResult = base.findControl(Proc, ref pData, uFlags);
            return pResult;
        }
        public override void drawItemText(ref Graphics graphics, ref Bitmap bitmap, ref Rectangle rcItem)
        {
            drawItemBk(ref graphics, ref bitmap, ref rcItem);
            if (mExpanded && mSubControl != null && mHideSelf) return;

            if (mOwner == null) return;
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

            IListCallbackUI pCallback = mOwner.getTextCallback();
            if (pCallback == null) return;
            mNumLinks = 0;
            int nLinks = mRectLinks.Length;
            for (int i = 0; i < pInfo.mColumns; i++)
            {
                // Paint text
                Rectangle rcItem1 = new Rectangle(pInfo.mListColumn[i].Left, mRectItem.Top, pInfo.mListColumn[i].Right, mRectItem.Bottom);
                if (mExpanded && mSubControl != null)
                {
                    rcItem1.Height = rcItem1.Bottom - mSubControl.getHeight() - rcItem1.Top;
                }
                int newLeft = rcItem1.Left + pInfo.mRectTextPadding.Left;
                int newRigh = rcItem1.Right - pInfo.mRectTextPadding.Right;
                int newTop = rcItem1.Top + pInfo.mRectTextPadding.Top;
                int newBottom = rcItem1.Bottom - pInfo.mRectTextPadding.Bottom;

                rcItem1.X = newLeft;
                rcItem1.Width = newRigh - newLeft;
                rcItem1.Y = newTop;
                rcItem1.Height = newBottom - newTop;

                string pstrText = pCallback.getItemText(this, mIndex, i);
                if (pInfo.mShowHtml)
                {
                    RenderEngine.drawHtmlText(ref graphics,
                        ref bitmap,
                        ref mManager,
                        ref rcItem1,
                        pstrText,
                        iTextColor,
                        ref mRectLinks,
                        ref mLinks,
                        ref nLinks,
                        (int)FormatFlags.DT_SINGLELINE | pInfo.mTextStyle);
                }
                else
                {
                    RenderEngine.drawText(ref graphics,
                        ref bitmap,
                        ref mManager,
                        ref rcItem1,
                        pstrText,
                        iTextColor,
                        pInfo.mFontIdx,
                        (int)FormatFlags.DT_SINGLELINE | pInfo.mTextStyle);
                }

                if (nLinks > 0)
                {
                    mNumLinks = nLinks;
                    nLinks = 0;
                }
                else
                {
                    nLinks = mRectLinks.Length;
                }
            }
        }



        protected bool mExpanded;
        protected bool mHideSelf;
        protected Rectangle mRectExpander;
        protected ControlUI mSubControl;

    }
}
