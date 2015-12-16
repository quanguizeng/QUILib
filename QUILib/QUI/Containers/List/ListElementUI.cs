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
    public class ListElementUI : ControlUI, IListItemUI
    {
        public ListElementUI()
        {
            mIndex = -1;
            mOwner = null;
            mSelected = false;
            mButtonState = 0;
        }

        ~ListElementUI()
        {

        }
        public override string getClass()
        {
            return "ListElementUI";
        }
        public override int getControlFlags()
        {
            return (int)ControlFlag.UIFLAG_WANTRETURN;
        }
        public override ControlUI getInterface(string pstrName)
        {
            if (pstrName == "ListItem") return this;
            return base.getInterface(pstrName);
        }
        public virtual IListOwnerUI getOwner()
        {
            return mOwner;
        }
        public virtual void setOwner(ControlUI pOwner)
        {
            mOwner = (IListOwnerUI)pOwner.getInterface("ListOwner");
        }
        public override void setVisible(bool bVisible)
        {
            base.setVisible(bVisible);
            if (!isVisible() && mSelected)
            {
                mSelected = false;
                if (mOwner != null) mOwner.selectItem(-1);
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
        public virtual int getIndex()
        {
            return mIndex;
        }
        public virtual void setIndex(int iIndex)
        {
            mIndex = iIndex;
        }
        public override  void invalidate()
        {
            if (!isVisible()) return;

            if (getParent() != null)
            {
                ContainerUI pParentContainer = (ContainerUI)getParent().getInterface("Container");
                if (pParentContainer != null)
                {
                    Rectangle rc = pParentContainer.getPos();
                    Rectangle rcInset = pParentContainer.getInset();

                    int newLeft = rc.Left + rcInset.Left;
                    int newRight = rc.Right - rcInset.Right;
                    int newTop = rc.Top + rcInset.Top;
                    int newBottom = rc.Bottom - rcInset.Bottom;
                    rc.X = newLeft;
                    rc.Width = newRight - newLeft;
                    rc.Y = newTop;
                    rc.Height = newBottom - newTop;

                    ScrollbarUI pVerticalScrollbar = pParentContainer.getVerticalScrollbar();
                    if (pVerticalScrollbar != null && pVerticalScrollbar.isVisible())
                    {
                        rc.Width = rc.Right - pVerticalScrollbar.getFixedWidth() - rc.Left;
                    }
                    ScrollbarUI pHorizontalScrollbar = pParentContainer.getHorizontalScrollbar();
                    if (pHorizontalScrollbar != null && pHorizontalScrollbar.isVisible())
                    {
                        rc.Height = rc.Bottom - pHorizontalScrollbar.getFixedHeight() - rc.Top;
                    }

                    Rectangle invalidateRc = mRectItem;
                    if (!invalidateRc.IntersectsWith(mRectItem))
                    {
                        return;
                    }
                    invalidateRc.Intersect(mRectItem);

                    ControlUI pParent = getParent();
                    Rectangle rcTemp;
                    Rectangle rcParent;
                    while ((pParent = pParent.getParent()) != null)
                    {
                        rcTemp = invalidateRc;
                        rcParent = pParent.getPos();
                        if (!rcTemp.IntersectsWith(rcParent))
                        {
                            return;
                        }
                        invalidateRc.Intersect(rcParent);
                    }

                    if (mManager != null) mManager.invalidate(ref invalidateRc);
                }
                else
                {
                    base.invalidate();
                }
            }
            else
            {
                base.invalidate();
            }
        }
        public override bool activate()
        {
            if (!base.activate()) return false;
            if (mManager != null) mManager.sendNotify(this, "itemactivate");
            return true;
        }
        public virtual bool isSelected()
        {
            return mSelected;
        }
        public virtual bool select(bool bSelect = true)
        {
            if (!isEnabled()) return false;
            if (bSelect == mSelected) return true;
            mSelected = bSelect;
            if (bSelect && mOwner != null) mOwner.selectItem(mIndex);
            invalidate();

            return true;
        }
        public virtual bool isExpanded()
        {
            return false;
        }
       public  virtual bool expand(bool bExpand = true)
        {
            return false;
        }

        public override void eventProc(ref TEventUI newEvent)
        {
            if (!isMouseEnabled() && newEvent.mType > (int)EVENTTYPE_UI.UIEVENT__MOUSEBEGIN && newEvent.mType < (int)EVENTTYPE_UI.UIEVENT__MOUSEEND)
            {
                if (mOwner != null) mOwner.eventProc(ref newEvent);
                else base.eventProc(ref newEvent);
                return;
            }

            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_DBLCLICK)
            {
                if (isEnabled())
                {
                    activate();
                    invalidate();
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_KEYDOWN && isEnabled())
            {
                if (newEvent.mKey == (char)Keys.Return)
                {
                    activate();
                    invalidate();
                    return;
                }
            }
            // An important twist: The list-item will send the event not to its immediate
            // parent but to the "attached" list. A list may actually embed several components
            // in its path to the item, but key-presses etc. needs to go to the actual list.
            if (mOwner != null) mOwner.eventProc(ref newEvent); else base.eventProc(ref newEvent);
        }
        public override void setAttribute(string pstrName, string pstrValue)
        {
            if (pstrName == "selected") select();
            else base.setAttribute(pstrName, pstrValue);
        }
        public void drawItemBk(ref Graphics graphics, ref Bitmap bitmap, ref Rectangle rcItem)
        {
            if (mOwner == null) return;
            TListInfoUI pInfo = mOwner.getListInfo();
            int iBackColor = pInfo.mBkColor;
            if ((mButtonState & (int)PaintFlags.UISTATE_HOT) != 0)
            {
                iBackColor = pInfo.mHotBkColor;
            }
            if (isSelected())
            {
                iBackColor = pInfo.mSelectedBkColor;
            }
            if (!isEnabled())
            {
                iBackColor = pInfo.mDisabledBkColor;
            }

            if (iBackColor != 0)
            {
                RenderEngine.drawColor(ref graphics, ref bitmap, ref mRectItem, iBackColor);
            }

            if (!isEnabled())
            {
                if (pInfo.mDisabledImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, pInfo.mDisabledImage)) pInfo.mDisabledImage = "";
                    else return;
                }
            }
            if (isSelected())
            {
                if (pInfo.mSelectedImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, pInfo.mSelectedImage)) pInfo.mSelectedImage = "";
                    else return;
                }
            }
            if ((mButtonState & (int)PaintFlags.UISTATE_HOT) != 0)
            {
                if (pInfo.mHotImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, pInfo.mHotImage)) pInfo.mHotImage = "";
                    else return;
                }
            }

            if (mBkImage != "")
            {
                if (!drawImage(ref graphics, ref bitmap, mBkImage)) mBkImage = "";
            }

            if (mBkImage == "")
            {
                if (pInfo.mImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, pInfo.mImage)) pInfo.mImage = "";
                    else return;
                }
            }

            if (pInfo.mLineColor != 0)
            {
                Rectangle rcLine = new Rectangle(mRectItem.Left, mRectItem.Bottom - 1, mRectItem.Right, mRectItem.Bottom - 1);
                RenderEngine.drawLine(ref graphics, ref bitmap, ref rcLine, 1, pInfo.mLineColor);
            }

        }

        public virtual void drawItemText(ref Graphics graphics, ref Bitmap bitmap, ref Rectangle rcItem)
        {

        }

        protected int mIndex;
        protected bool mSelected;
        protected int mButtonState;
        protected IListOwnerUI mOwner;

    }
}
