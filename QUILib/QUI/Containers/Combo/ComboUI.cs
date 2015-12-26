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
    public class ComboUI : ContainerUI, IListOwnerUI
    {
        public ComboUI()
        {
            mWindow = null;

            mCurSel = -1;
            mTextPadding = new Rectangle(0, 0, 0, 0);
            mDropBoxAttributes = "";
            mDropBox = new Size(0, 150);
            mButtonState = 0;

            mNormalImage = "";
            mHotImage = "";
            mPushedImage = "";
            mFocusedImage = "";
            mDisabledImage = "";

            mListInfo = new TListInfoUI();
            mListInfo.mColumns = 0;
            mListInfo.mFontIdx = -1;
            mListInfo.mTextStyle = (int)FormatFlags.DT_VCENTER;
            mListInfo.mTextColor = Color.FromArgb(0xFF, 00, 00, 00).ToArgb();
            mListInfo.mBkColor = 0;
            mListInfo.mSelectedTextColor = Color.FromArgb(0xFF, 00, 00, 00).ToArgb();
            mListInfo.mSelectedBkColor = Color.FromArgb(0xFF, 0xC1, 0xE3, 0xFF).ToArgb();
            mListInfo.mHotTextColor = Color.FromArgb(0xFF, 00, 00, 00).ToArgb();
            mListInfo.mHotBkColor = Color.FromArgb(0xFF, 0xE9, 0xF5, 0xFF).ToArgb();
            mListInfo.mDisabledTextColor = Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC).ToArgb();
            mListInfo.mDisabledBkColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF).ToArgb();
            mListInfo.mLineColor = 0;
            mListInfo.mShowHtml = false;
            mListInfo.mExpandable = false;
            mListInfo.mMultiExpandable = false;

            mListInfo.mRectTextPadding = new Rectangle(0, 0, 0, 0);
            mListInfo.mListColumn = new Dictionary<int, Rectangle>();
        }
        public override string getClass()
        {
            return "ComboUI";
        }
        public override ControlUI getInterface(string name)
        {
            if (name == "ListOwner")
            {
                return this;
            }
            return base.getInterface(name);
        }
        public void init()
        {
            if (mCurSel < 0) selectItem(0);
        }
        public virtual int getCurSel()
        {
            return mCurSel;
        }
        public virtual bool selectItem(int iIndex)
        {
            ControlUI pControl;
            IListItemUI pListItem;
            if (iIndex == mCurSel)
            {
                return true;
            }
            if (mCurSel >= 0)
            {
                pControl = (ControlUI)mItems[mCurSel];
                if (pControl == null)
                {
                    return false;
                }
                pListItem = (IListItemUI)pControl.getInterface("ListItem");
                if (pListItem != null)
                {
                    pListItem.select(false);
                }
                mCurSel = -1;
            }
            if (mItems.Count == 0)
            {
                return false;
            }
            if (iIndex < 0)
            {
                iIndex = 0;
            }
            if (iIndex >= mItems.Count)
            {
                iIndex = mItems.Count - 1;
            }
            pControl = (ControlUI)mItems[iIndex];
            if (pControl == null || !pControl.isVisible() || !pControl.isEnabled())
            {
                return false;
            }
            pListItem = (IListItemUI)pControl.getInterface("ListItem");
            if (pListItem == null)
            {
                return false;
            }
            mCurSel = iIndex;
            pControl.setFocus();
            pListItem.select(true);
            if (mManager != null)
            {
                mManager.sendNotify(this, "itemselect");
            }
            invalidate();

            return true;
        }
        public override bool setItemIndex(ControlUI pControl, int iIndex)
        {
            int iOrginIndex = getItemIndex(pControl);
            if (iOrginIndex == -1) return false;

            if (!base.setItemIndex(pControl, iIndex)) return false;

            IListItemUI pListItem = (IListItemUI)pControl.getInterface("ListItem");
            if (pListItem != null)
            {
                pListItem.setIndex(getCount()); 
            }

            for (int i = iOrginIndex; i < getCount(); ++i)
            {
                ControlUI p = getItemAt(i);
                pListItem = (IListItemUI)p.getInterface("ListItem");
                if (pListItem != null)
                {
                    pListItem.setIndex(pListItem.getIndex() - 1);
                }
            }

            selectItem(findSelectable(mCurSel, false));
            return true;
        }
        public override bool add(ControlUI pControl)
        {
            IListItemUI pListItem = (IListItemUI)pControl.getInterface("ListItem");
            if (pListItem != null)
            {
                pListItem.setOwner(this);
                pListItem.setIndex(mItems.Count);
            }
            return base.add(pControl);
        }
        public override bool addAt(ControlUI pControl, int iIndex)
        {
            if (!base.addAt(pControl, iIndex))
            {
                return false;
            }

            // 所有的列表项目都是我们指定的
            IListItemUI pListItem = (IListItemUI)pControl.getInterface("ListItem");
            if (pListItem != null)
            {
                pListItem.setOwner(this);
                pListItem.setIndex(iIndex);
            }

            for (int i = iIndex + 1; i < getCount(); ++i)
            {
                ControlUI p = getItemAt(i);
                pListItem = (IListItemUI)p.getInterface("ListItem");
                if (pListItem != null)
                {
                    pListItem.setIndex(pListItem.getIndex() + 1);
                }
            }
            return true;
        }
        public override bool remove(ControlUI pControl)
        {
            int iIndex = getItemIndex(pControl);
            if (iIndex == -1)
            {
                return false;
            }

            if (!base.removeAt(iIndex))
            {
                return false;
            }

            for (int i = iIndex; i < getCount(); ++i)
            {
                ControlUI p = getItemAt(i);
                IListItemUI pListItem = (IListItemUI)p.getInterface("ListItem");
                if (pListItem != null)
                {
                    pListItem.setIndex(pListItem.getIndex() - 1);
                }
            }

            selectItem(findSelectable(mCurSel, false));

            return true;
        }
        public override bool removeAt(int iIndex)
        {
            if (!base.removeAt(iIndex))
            {
                return false;
            }

            for (int i = iIndex; i < getCount(); ++i)
            {
                ControlUI p = getItemAt(i);
                IListItemUI pListItem = (IListItemUI)p.getInterface("ListItem");
                if (pListItem != null)
                {
                    pListItem.setIndex(pListItem.getIndex() - 1);
                }
            }

            selectItem(findSelectable(mCurSel, false));

            return true;
        }
        public override void removeAll()
        {
            mCurSel = -1;
            base.removeAll();
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
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONDOWN)
            {
                if (isEnabled())
                {
                    activate();
                    mButtonState |= (int)PaintFlags.UISTATE_PUSHED | (int)PaintFlags.UISTATE_CAPTURED;
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONUP)
            {
                if ((mButtonState & (int)PaintFlags.UISTATE_CAPTURED) != 0)
                {
                    mButtonState &= ~(int)PaintFlags.UISTATE_CAPTURED;
                    invalidate();
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSEMOVE)
            {
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_KEYDOWN)
            {
                switch ((Keys)newEvent.mKey)
                {
                    case Keys.F4:
                        activate();
                        return;
                    case Keys.Up:
                        selectItem(findSelectable(mCurSel - 1, false));
                        return;
                    case Keys.Down:
                        selectItem(findSelectable(mCurSel + 1, true));
                        return;
                    case Keys.Prior:
                        selectItem(findSelectable(mCurSel - 1, false));
                        return;
                    case Keys.Next:
                        selectItem(findSelectable(mCurSel + 1, true));
                        return;
                    case Keys.Home:
                        selectItem(findSelectable(0, false));
                        return;
                    case Keys.End:
                        selectItem(findSelectable(getCount() - 1, true));
                        return;
                }
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SCROLLWHEEL)
            {
                bool bDownward = LOWORD((int)newEvent.mWParam) == (int)ScrollBarCommands.SB_LINEDOWN;
                selectItem(findSelectable(mCurSel + (bDownward ? 1 : -1), bDownward));
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSEENTER)
            {
                if (mRectItem.Contains(newEvent.mMousePos))
                {
                    if ((mButtonState & (int)PaintFlags.UISTATE_HOT) == 0)
                        mButtonState |= (int)PaintFlags.UISTATE_HOT;
                    invalidate();
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSELEAVE)
            {
                if (!mRectItem.Contains(newEvent.mMousePos))
                {
                    if ((mButtonState & (int)PaintFlags.UISTATE_HOT) != 0)
                    {
                        mButtonState &= ~(int)PaintFlags.UISTATE_HOT;
                        invalidate();
                    }
                }
                return;
            }
            eventProc0(ref newEvent);
        }
        public override Size estimateSize(Size szAvailable)
        {
            if (mXYFixed.Height == 0) return new Size(mXYFixed.Width, mManager.getDefaultFont().Height + 12);
            return base.estimateSize(szAvailable);
        }
        public override bool activate()
        {
            if (!base.activate())
            {
                return false;
            }
            if (mWindow != null)
            {
                return true;
            }
            mWindow = new ComboWnd();
            mWindow.init(this);
            if (mManager != null)
            {
                mManager.sendNotify(this, "dropdown");
            }
            invalidate();
            return true;
        }

        public override string getText()
        {
            if (mCurSel < 0) return "";
            ControlUI pControl = (ControlUI)mItems[mCurSel];
            return pControl.getText();
        }
        public override void setEnabled(bool bEnable = true)
        {
            base.setEnabled(bEnable);
            if (!isEnabled())
            {
                mButtonState = 0;
            }
        }
        public string getDropBoxAttributeList()
        {
            return mDropBoxAttributes;
        }
        public void setDropBoxAttributeList(string pstrList)
        {
            mDropBoxAttributes = pstrList;
        }
        public Size getDropBoxSize()
        {
            return mDropBox;
        }
        public void setDropBoxSize(Size szDropBox)
        {
            mDropBox = szDropBox;
        }
        public Rectangle getTextPadding()
        {
            return mTextPadding;
        }
        public void setTextPadding(Rectangle rc)
        {
            mTextPadding = rc;
            invalidate();
        }
        public string getNormalImage()
        {
            return mNormalImage;
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
        public string getDisabledImage()
        {
            return mDisabledImage;

        }
        public void setDisabledImage(string pStrImage)
        {
            mDisabledImage = pStrImage;
            invalidate();

        }

        public virtual TListInfoUI getListInfo()
        {
            return mListInfo;
        }
        public void setItemFont(int index)
        {
            mListInfo.mFontIdx = index;
            invalidate();

        }
        public void setItemTextStyle(int uStyle)
        {
            mListInfo.mTextStyle = uStyle;
            invalidate();

        }
        public void setItemTextPadding(Rectangle rc)
        {
            mListInfo.mRectTextPadding = rc;
            invalidate();
        }
        public void setItemTextColor(int dwTextColor)
        {
            mListInfo.mTextColor = dwTextColor;
            invalidate();
        }
        public void setItemBkColor(int dwBkColor)
        {
            mListInfo.mBkColor = dwBkColor;
        }
        public void setItemImage(string pStrImage)
        {
            mListInfo.mImage = pStrImage;
        }
        public void setSelectedItemTextColor(int dwTextColor)
        {
            mListInfo.mSelectedTextColor = dwTextColor;
        }
        public void setSelectedItemBkColor(int dwBkColor)
        {
            mListInfo.mSelectedBkColor = dwBkColor;
        }
        public void setSelectedItemImage(string pStrImage)
        {
            mListInfo.mSelectedImage = pStrImage;
        }
        public void setHotItemTextColor(int dwTextColor)
        {
            mListInfo.mHotTextColor = dwTextColor;
        }
        public void setHotItemBkColor(int dwBkColor)
        {
            mListInfo.mHotBkColor = dwBkColor;
        }
        public void setHotItemImage(string pStrImage)
        {
            mListInfo.mHotImage = pStrImage;
        }
        public void setDisabledItemTextColor(int dwTextColor)
        {
            mListInfo.mDisabledTextColor = dwTextColor;
        }
        public void setDisabledItemBkColor(int dwBkColor)
        {
            mListInfo.mDisabledBkColor = dwBkColor;
        }
        public void setDisabledItemImage(string pStrImage)
        {
            mListInfo.mDisabledImage = pStrImage;
        }
        public void setItemLineColor(int dwLineColor)
        {
            mListInfo.mLineColor = dwLineColor;
        }
        public bool isItemShowHtml()
        {
            return mListInfo.mShowHtml;
        }
        public void setItemShowHtml(bool bShowHtml = true)
        {
            if (mListInfo.mShowHtml == bShowHtml) return;

            mListInfo.mShowHtml = bShowHtml;
            invalidate();
        }
        public override void setPos(Rectangle rc)
        {
            // 隐藏所有的子控件
            Rectangle rcNull = new Rectangle(0, 0, 0, 0);
            for (int i = 0; i < mItems.Count; i++)
            {
                mItems[i].setPos(rcNull);
            }
            // 重新定位子控件大小和位置
            setPos0(rc);
        }
        public override void setAttribute(string name, string value)
        {
            if (name == "textpadding")
            {
                string[] listValue = value.Split(',');
                Rectangle rc = new Rectangle(int.Parse(listValue[0]), int.Parse(listValue[1]), int.Parse(listValue[2]), int.Parse(listValue[3]));

                setTextPadding(rc);
            }
            else if (name == "normalimage") setNormalImage(value);
            else if (name == "hotimage") setHotImage(value);
            else if (name == "pushedimage") setPushedImage(value);
            else if (name == "focusedimage") setFocusedImage(value);
            else if (name == "disabledimage") setDisabledImage(value);
            else if (name == "itemfont") mListInfo.mFontIdx = int.Parse(value);
            else if (name == "itemalign")
            {
                if (value == "left")
                {
                    mListInfo.mTextStyle &= ~((int)FormatFlags.DT_CENTER | (int)FormatFlags.DT_RIGHT);
                    mListInfo.mTextStyle |= (int)FormatFlags.DT_LEFT;
                }
                if (value == "center")
                {
                    mListInfo.mTextStyle &= ~((int)FormatFlags.DT_LEFT | (int)FormatFlags.DT_RIGHT);
                    mListInfo.mTextStyle |= (int)FormatFlags.DT_CENTER;
                }
                if (value == "right")
                {
                    mListInfo.mTextStyle &= ~((int)FormatFlags.DT_LEFT | (int)FormatFlags.DT_CENTER);
                    mListInfo.mTextStyle |= (int)FormatFlags.DT_RIGHT;
                }
            }
            if (name == "itemtextpadding")
            {
                string[] listValue = value.Split(',');
                Rectangle rc = new Rectangle(int.Parse(listValue[0]), int.Parse(listValue[1]), int.Parse(listValue[2]), int.Parse(listValue[3]));

                setItemTextPadding(rc);
            }
            else if (name == "itemtextcolor")
            {
                value = value.TrimStart('#');
                setItemTextColor(Convert.ToInt32(value, 16));
            }
            else if (name == "itembkcolor")
            {
                value = value.TrimStart('#');
                setItemBkColor(Convert.ToInt32(value, 16));
            }
            else if (name == "itemimage") setItemImage(value);
            else if (name == "itemselectedtextcolor")
            {
                value = value.TrimStart('#');
                setSelectedItemTextColor(Convert.ToInt32(value, 16));
            }
            else if (name == "itemselectedbkcolor")
            {
                value = value.TrimStart('#');
                setSelectedItemBkColor(Convert.ToInt32(value, 16));
            }
            else if (name == "itemselectedimage") setSelectedItemImage(value);
            else if (name == "itemhottextcolor")
            {
                value = value.TrimStart('#');
                setHotItemTextColor(Convert.ToInt32(value, 16));
            }
            else if (name == "itemhotbkcolor")
            {
                value = value.TrimStart('#');
                setHotItemBkColor(Convert.ToInt32(value, 16));
            }
            else if (name == "itemhotimage") setHotItemImage(value);
            else if (name == "itemdisabledtextcolor")
            {
                value = value.TrimStart('#');
                setDisabledItemTextColor(Convert.ToInt32(value, 16));
            }
            else if (name == "itemdisabledbkcolor")
            {
                value = value.TrimStart('#');
                setDisabledItemBkColor(Convert.ToInt32(value, 16));
            }
            else if (name == "itemdisabledimage") setDisabledItemImage(value);
            else if (name == "itemlinecolor")
            {
                value = value.TrimStart('#');
                setItemLineColor(Convert.ToInt32(value, 16));
            }
            else if (name == "itemshowhtml") setItemShowHtml(value == "true");
            else base.setAttribute(name, value);
        }
        public override void doPaint(ref Graphics graphics, ref Bitmap bitmap, Rectangle rcPaint)
        {
            doPaint0(ref graphics, ref bitmap, rcPaint);
        }
        public override void paintStatusImage(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (isFocused()) mButtonState |= (int)PaintFlags.UISTATE_FOCUSED;
            else mButtonState &= ~(int)PaintFlags.UISTATE_FOCUSED;
            if (!isEnabled()) mButtonState |= (int)PaintFlags.UISTATE_DISABLED;
            else mButtonState &= ~(int)PaintFlags.UISTATE_DISABLED;

            if ((mButtonState & (int)PaintFlags.UISTATE_DISABLED) != 0)
            {
                if (mDisabledImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mDisabledImage)) mDisabledImage = "";
                    else return;
                }
            }
            else if ((mButtonState & (int)PaintFlags.UISTATE_PUSHED) != 0)
            {
                if (mPushedImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mPushedImage)) mPushedImage = "";
                    else return;
                }
            }
            else if ((mButtonState & (int)PaintFlags.UISTATE_HOT) != 0)
            {
                if (mHotImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mHotImage)) mHotImage = "";
                    else return;
                }
            }
            else if ((mButtonState & (int)PaintFlags.UISTATE_FOCUSED) != 0)
            {
                if (mFocusedImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mFocusedImage)) mFocusedImage = "";
                    else return;
                }
            }

            if (mNormalImage != "")
            {
                if (!drawImage(ref graphics, ref bitmap, mNormalImage)) mNormalImage = "";
                else return;
            }
        }
        public override void paintText(ref Graphics graphics, ref Bitmap bitmap)
        {
            Rectangle rcText = mRectItem;
            int newLeft = rcText.Left + mTextPadding.Left;
            int newRight = rcText.Right - mTextPadding.Right;
            int newTop = rcText.Top + mTextPadding.Top;
            int newBottom = rcText.Bottom - mTextPadding.Bottom;

            rcText.X = newLeft;
            rcText.Width = newRight - newLeft;
            rcText.Y = newTop;
            rcText.Height = newBottom - newTop;

            if (mCurSel >= 0)
            {
                ControlUI pControl = (ControlUI)mItems[mCurSel];
                IListItemUI pElement = (IListItemUI)pControl.getInterface("ListItem");
                if (pElement != null)
                {
                    pElement.drawItemText(ref graphics, ref bitmap, ref rcText);
                }
                else
                {
                    Rectangle rcOldPos = pControl.getPos();
                    pControl.setPos(rcText);
                    pControl.doPaint(ref graphics, ref bitmap, rcText);
                    pControl.setPos(rcOldPos);
                }
            }
        }

        public ComboWnd mWindow;

        protected int mCurSel;
        protected Rectangle mTextPadding;
        protected string mDropBoxAttributes;
        protected Size mDropBox;
        public int mButtonState;

        protected string mNormalImage;
        protected string mHotImage;
        protected string mPushedImage;
        protected string mFocusedImage;
        protected string mDisabledImage;

        protected TListInfoUI mListInfo;
    }
}
