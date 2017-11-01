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
    public class ListUI : VerticalLayoutUI, IListUI
    {
        public ListUI()
        {
            mCallback = null;
            mCurSel = -1;
            mExpandedItem = -1;
            mList = new ListBodyUI(this);
            mHeader = new ListHeaderUI();
            mListInfo = new TListInfoUI();
            mListInfo.mListColumn = new Dictionary<int, Rectangle>();

            add(mHeader);
            base.add(mList);

            mListInfo.mColumns = 0;
            mListInfo.mFontIdx = -1;
            mListInfo.mTextStyle = (int)FormatFlags.DT_VCENTER; // m_uTextStyle(DT_VCENTER | DT_END_ELLIPSIS)
            mListInfo.mTextColor = Color.FromArgb(0xFF, 00, 00, 00).ToArgb();
            mListInfo.mBkColor = 0;
            mListInfo.mSelectedTextColor = Color.FromArgb(0xFF, 00, 00, 00).ToArgb();
            mListInfo.mSelectedBkColor = Color.FromArgb(0xFF, 0xC1, 0xE3, 0xFF).ToArgb();
            mListInfo.mHotTextColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00).ToArgb();
            mListInfo.mHotBkColor = Color.FromArgb(0xFF, 0xE9, 0xF5, 0xFF).ToArgb();
            mListInfo.mDisabledTextColor = Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC).ToArgb();
            mListInfo.mDisabledBkColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF).ToArgb();
            mListInfo.mLineColor = 0;
            mListInfo.mShowHtml = false;
            mListInfo.mExpandable = false;
            mListInfo.mMultiExpandable = false;

            mScrollSelect = false;
        }
        ~ListUI()
        {

        }
        public override string getClass()
        {
            return "ListUI";
        }
        public override int getControlFlags()
        {
            return (int)ControlFlag.UIFLAG_TABSTOP;
        }

        public override ControlUI getInterface(string name)
        {
            if (name == "List") return this;
            if (name == "ListOwner") return this;
            return base.getInterface(name);
        }

        public override ControlUI getItemAt(int iIndex)
        {
            return mList.getItemAt(iIndex);
        }
        public override int getItemIndex(ControlUI pControl)
        {
            if (pControl.getInterface("ListHeader") != null) return base.getItemIndex(pControl);
            // We also need to recognize header sub-items
            if (pControl.getClass() == "ListHeaderItemUI") return mHeader.getItemIndex(pControl);

            return mList.getItemIndex(pControl);
        }
        public override bool setItemIndex(ControlUI pControl, int iIndex)
        {
            if (pControl.getInterface("ListHeader") != null) return base.setItemIndex(pControl, iIndex);
            // We also need to recognize header sub-items
            if (pControl.getClass() == "ListHeaderItemUI") return mHeader.setItemIndex(pControl, iIndex);

            int iOrginIndex = mList.getItemIndex(pControl);
            if (iOrginIndex == -1) return false;

            if (!mList.setItemIndex(pControl, iIndex)) return false;

            // The list items should know about us
            IListItemUI pListItem = (IListItemUI)pControl.getInterface("ListItem");
            if (pListItem != null)
            {
                pListItem.setIndex(getCount());
            }

            for (int i = iOrginIndex; i < mList.getCount(); ++i)
            {
                ControlUI p = mList.getItemAt(i);
                pListItem = (IListItemUI)(p.getInterface("ListItem"));
                if (pListItem != null)
                {
                    pListItem.setIndex(pListItem.getIndex() - 1);
                }
            }

            selectItem(findSelectable(mCurSel, false));
            ensureVisible(mCurSel);

            return true;
        }
        public override int getCount()
        {
            return mList.getCount();
        }
        public override bool add(ControlUI pControl)
        {
            // Override the Add() method so we can add items specifically to
            // the intended widgets. Headers are assumed to be
            // answer the correct interface so we can add multiple list headers.
            if (pControl.getInterface("ListHeader") != null)
            {
                if (mHeader != pControl && mHeader.getCount() == 0)
                {
                    {
                        // 把旧的表头控件属性赋予到新的表头控件
                        pControl.setBackImage(mHeader.getBackImage());
                        pControl.setVisible(mHeader.isVisible());
                    }
                    base.remove(mHeader);
                    mHeader = null;
                    mHeader = (ListHeaderUI)(pControl);
                }

                return base.addAt(pControl,0);
            }
            // We also need to recognize header sub-items
            if (pControl.getClass() == "ListHeaderItemUI") return mHeader.add(pControl);
            // The list items should know about us
            IListItemUI pListItem = (IListItemUI)pControl.getInterface("ListItem");
            if (pListItem != null)
            {
                pListItem.setOwner(this);
                pListItem.setIndex(getCount());
            }
            return mList.add(pControl);
        }

        public override bool addAt(ControlUI pControl, int iIndex)
        {
            // Override the AddAt() method so we can add items specifically to
            // the intended widgets. Headers and are assumed to be
            // answer the correct interface so we can add multiple list headers.
            if (pControl.getInterface("ListHeader") != null) return base.addAt(pControl, iIndex);
            // We also need to recognize header sub-items
            if (pControl.getClass() == "ListHeaderItemUI") return mHeader.addAt(pControl, iIndex);

            if (!mList.addAt(pControl, iIndex)) return false;

            // The list items should know about us
            IListItemUI pListItem = (IListItemUI)pControl.getInterface("ListItem");
            if (pListItem != null)
            {
                pListItem.setOwner(this);
                pListItem.setIndex(iIndex);
            }

            for (int i = iIndex + 1; i < mList.getCount(); ++i)
            {
                ControlUI p = mList.getItemAt(i);
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
            if (pControl.getInterface("ListHeader") != null)
            {
                return base.remove(pControl);
            }
            // We also need to recognize header sub-items
            if (pControl.getClass() == "ListHeaderItemUI")
            {
                return mHeader.remove(pControl);
            }

            int iIndex = mList.getItemIndex(pControl);
            if (iIndex == -1)
            {
                return false;
            }

            if (!mList.removeAt(iIndex))
            {
                return false;
            }

            for (int i = iIndex; i < mList.getCount(); ++i)
            {
                ControlUI p = mList.getItemAt(i);
                IListItemUI pListItem = (IListItemUI)p.getInterface("ListItem");
                if (pListItem != null)
                {
                    pListItem.setIndex(pListItem.getIndex() - 1);
                }
            }
            mCurSel = mCurSel > iIndex ? mCurSel - 1 : mCurSel;
            selectItem(findSelectable(mCurSel, false));
            ensureVisible(mCurSel);

            return true;
        }
        public override bool removeAt(int iIndex)
        {
            if (!mList.removeAt(iIndex))
            {
                return false;
            }

            for (int i = iIndex; i < mList.getCount(); ++i)
            {
                ControlUI p = mList.getItemAt(i);
                IListItemUI pListItem = (IListItemUI)p.getInterface("ListItem");
                if (pListItem != null)
                {
                    pListItem.setIndex(pListItem.getIndex() - 1);
                }
            }

            mCurSel = mCurSel > iIndex ? mCurSel - 1 : mCurSel;
            selectItem(findSelectable(mCurSel, false));
            ensureVisible(mCurSel);
            needParentUpdate();

            return true;
        }
        public override void removeAll()
        {
            mCurSel = -1;
            mExpandedItem = -1;
            mList.removeAll();
        }
        public override void setPos(Rectangle rc)
        {
            base.setPos(rc);

            if (mHeader == null) return;
            // Determine general list information and the size of header columns
            mListInfo.mColumns = Math.Min(mHeader.getCount(), UILIST_MAX_COLUMNS);
            // The header/columns may or may not be visible at runtime. In either case
            // we should determine the correct dimensions...

            if (!mHeader.isVisible())
            {
                mHeader.setInternVisible(true);
                mHeader.setPos(new Rectangle(rc.Left, 0, rc.Width, 0));
            }
            int iOffset = mList.getScrollPos().Width;
            for (int i = 0; i < mListInfo.mColumns; i++)
            {
                ControlUI pControl = (ControlUI)mHeader.getItemAt(i);
                if (!pControl.isVisible())
                {
                    continue;
                }
                if (pControl.isFloat())
                {
                    continue;
                }

                Rectangle rcPos = pControl.getPos();
                if (iOffset > 0)
                {
                    int newLeft = rcPos.X - iOffset;
                    int newRight = rcPos.Right - iOffset;
                    rcPos.X = newLeft;
                    rcPos.Width = newRight-newLeft;
                    pControl.setPos(rcPos);
                }
                mListInfo.mListColumn[i] = pControl.getPos();
            }
            if (!mHeader.isVisible())
            {
                mHeader.setInternVisible(false);
            }
        }
        public override void eventProc(ref TEventUI newEvent)
        {
            if (!isMouseEnabled() && newEvent.mType > (int)EVENTTYPE_UI.UIEVENT__MOUSEBEGIN && newEvent.mType < (int)EVENTTYPE_UI.UIEVENT__MOUSEEND)
            {
                if (mParent != null)
                {
                    mParent.eventProc(ref newEvent);
                }
                else
                {
                    base.eventProc(ref newEvent);
                }
                return;
            }

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

            switch (newEvent.mType)
            {
                case (int)EVENTTYPE_UI.UIEVENT_KEYDOWN:
                    switch ((Keys)newEvent.mKey)
                    {
                        case Keys.Up:
                            selectItem(findSelectable(mCurSel - 1, false));
                            ensureVisible(mCurSel);
                            return;
                        case Keys.Down:
                            selectItem(findSelectable(mCurSel + 1, true));
                            ensureVisible(mCurSel);
                            return;
                        case Keys.Prior:
                            pageUp();
                            return;
                        case Keys.Next:
                            pageDown();
                            return;
                        case Keys.Home:
                            selectItem(findSelectable(0, false));
                            ensureVisible(mCurSel);
                            return;
                        case Keys.End:
                            selectItem(findSelectable(getCount() - 1, true));
                            ensureVisible(mCurSel);
                            return;
                        case Keys.Return:
                            if (mCurSel != -1) getItemAt(mCurSel).activate();
                            return;
                    }
                    break;
                case (int)EVENTTYPE_UI.UIEVENT_SCROLLWHEEL:
                    {
                        switch ((ScrollBarCommands)LOWORD((int)newEvent.mWParam))
                        {
                            case ScrollBarCommands.SB_LINEUP:
                                {
                                    if (mScrollSelect)
                                    {
                                        selectItem(findSelectable(mCurSel - 1, false));
                                        ensureVisible(mCurSel);
                                    }
                                    else
                                    {
                                        lineUp();
                                    }
                                    return;
                                }
                            case ScrollBarCommands.SB_LINEDOWN:
                                {
                                    if (mScrollSelect)
                                    {
                                        selectItem(findSelectable(mCurSel + 1, true));
                                        ensureVisible(mCurSel);
                                    }
                                    else
                                    {
                                        lineDown();
                                    }
                                    return;
                                }
                        }
                    }
                    break;
            }

            eventProc0(ref newEvent);
        }

        public virtual ListHeaderUI getHeader()
        {
            return mHeader;
        }
        public virtual ContainerUI getList()
        {
            return mList;
        }
        public virtual int getCurSel()
        {
            return mCurSel;
        }
        public virtual bool selectItem(int iIndex)
        {
            if (iIndex == mCurSel) return true;
            ControlUI pControl;
            IListItemUI pListItem;
            // We should first unselect the currently selected item
            if (mCurSel >= 0)
            {
                pControl = getItemAt(mCurSel);
                if (pControl != null)
                {
                    pListItem = (IListItemUI)pControl.getInterface("ListItem");
                    if (pListItem != null) pListItem.select(false);
                }

                mCurSel = -1;
            }

            pControl = getItemAt(iIndex);
            if (pControl == null) return false;
            if (!pControl.isVisible()) return false;
            if (!pControl.isEnabled()) return false;

            pListItem = (IListItemUI)pControl.getInterface("ListItem");
            if (pListItem == null) return false;
            mCurSel = iIndex;
            if (!pListItem.select(true))
            {
                mCurSel = -1;
                return false;
            }
            pControl.setFocus();
            if (mManager != null)
            {
                mManager.sendNotify(this, "itemselect");
            }

            return true;
        }
        public virtual TListInfoUI getListInfo()
        {
            return mListInfo;
        }
        public override int getChildPadding()
        {
            return mList.getChildPadding();
        }
        public override void setChildPadding(int iPadding)
        {
            mList.setChildPadding(iPadding);
        }
        public void setItemFont(int index)
        {
            mListInfo.mFontIdx = index;
            needUpdate();
        }
        public void setItemTextStyle(int uStyle)
        {
            mListInfo.mTextStyle = uStyle;
            needUpdate();
        }
        public void setItemTextPadding(Rectangle rc)
        {
            mListInfo.mRectTextPadding = rc;
            needUpdate();
        }
        public void setItemTextColor(int dwTextColor)
        {
            mListInfo.mTextColor = dwTextColor;
            needUpdate();
        }
        public void setItemBkColor(int dwBkColor)
        {
            mListInfo.mBkColor = dwBkColor;
            invalidate();
        }
        public void setItemImage(string strImage)
        {
            mListInfo.mImage = strImage;
            invalidate();
        }
        public void setSelectedItemTextColor(int dwTextColor)
        {
            mListInfo.mSelectedTextColor = dwTextColor;
            invalidate();
        }
        public void setSelectedItemBkColor(int dwBkColor)
        {
            mListInfo.mSelectedBkColor = dwBkColor;
            invalidate();
        }
        public void setSelectedItemImage(string pStrImage)
        {
            mListInfo.mSelectedImage = pStrImage;
            invalidate();
        }
        public void setHotItemTextColor(int dwTextColor)
        {
            mListInfo.mHotTextColor = dwTextColor;
            invalidate();
        }
        public void setHotItemBkColor(int dwBkColor)
        {
            mListInfo.mHotBkColor = dwBkColor;
            invalidate();
        }
        public void setHotItemImage(string pStrImage)
        {
            mListInfo.mHotImage = pStrImage;
            invalidate();
        }
        public void setDisabledItemTextColor(int dwTextColor)
        {
            mListInfo.mDisabledTextColor = dwTextColor;
            invalidate();
        }
        public void setDisabledItemBkColor(int dwBkColor)
        {
            mListInfo.mDisabledBkColor = dwBkColor;
            invalidate();
        }
        public void setDisabledItemImage(string pStrImage)
        {
            mListInfo.mDisabledImage = pStrImage;
            invalidate();
        }
        public void setItemLineColor(int dwLineColor)
        {
            mListInfo.mLineColor = dwLineColor;
            invalidate();
        }
        public bool isItemShowHtml()
        {
            return mListInfo.mShowHtml;
        }
        public void setItemShowHtml(bool bShowHtml)
        {
            if (mListInfo.mShowHtml == bShowHtml) return;

            mListInfo.mShowHtml = bShowHtml;
            needUpdate();
        }
        public void setExpanding(bool bExpandable)
        {
            mListInfo.mExpandable = bExpandable;
        }
        public void setMultiExpanding(bool bMultiExpandable)
        {
            mListInfo.mMultiExpandable = bMultiExpandable;
        }
        public virtual bool expandItem(int iIndex, bool bExpand = true)
        {
            if (!mListInfo.mExpandable) return false;
            if (mExpandedItem >= 0 && !mListInfo.mMultiExpandable)
            {
                ControlUI pControl = getItemAt(mExpandedItem);
                if (pControl != null)
                {
                    IListItemUI pItem = (IListItemUI)pControl.getInterface("ListItem");
                    if (pItem != null) pItem.expand(false);
                }
                mExpandedItem = -1;
            }
            if (bExpand)
            {
                ControlUI pControl = getItemAt(iIndex);
                if (pControl == null) return false;
                if (!pControl.isVisible()) return false;
                IListItemUI pItem = (IListItemUI)pControl.getInterface("ListItem");
                if (pItem == null) return false;
                mExpandedItem = iIndex;
                if (!pItem.expand(true))
                {
                    mExpandedItem = -1;
                    return false;
                }
            }
            needUpdate();

            return true;
        }
        public virtual int getExpandedItem()
        {
            return mExpandedItem;
        }
        public void ensureVisible(int iIndex)
        {
            if (mCurSel < 0) return;
            Rectangle rcItem = mList.getItemAt(iIndex).getPos();
            Rectangle rcList = mList.getPos();
            Rectangle rcListInset = mList.getInset();

            rcList.X += rcListInset.X;
            rcList.Width = rcList.Right - rcListInset.Right - rcList.X;
            rcList.Y += rcListInset.Y;
            rcList.Height = rcList.Bottom - rcListInset.Bottom - rcList.Y;

            ScrollbarUI pHorizontalScrollbar = mList.getHorizontalScrollbar();
            if (pHorizontalScrollbar != null && pHorizontalScrollbar.isVisible())
            {
                rcList.Height = rcList.Bottom - pHorizontalScrollbar.getFixedHeight() - rcList.Top;
            }

            int iPos = mList.getScrollPos().Height;
            if (rcItem.Top >= rcList.Top && rcItem.Bottom < rcList.Bottom) return;
            int dx = 0;
            if (rcItem.Top < rcList.Top) dx = rcItem.Top - rcList.Top;
            if (rcItem.Bottom > rcList.Bottom) dx = rcItem.Bottom - rcList.Bottom;

            scroll(0, dx);
        }
        public void scroll(int dx, int dy)
        {
            if (dx == 0 && dy == 0) return;
            Size sz = mList.getScrollPos();
            mList.setScrollPos(new Size(sz.Width + dx, sz.Height + dy));
        }
        public override void setAttribute(string pstrName, string pstrValue)
        {
            if (pstrName == "header") getHeader().setVisible(pstrValue != "hidden");
            else if (pstrName == "headerbkimage") getHeader().setBkImage(pstrValue);
            else if (pstrName == "scrollselect") setScrollSelect(pstrValue == "true");
            else if (pstrName == "expanding") setExpanding(pstrValue == "true");
            else if (pstrName == "multiexpanding") setMultiExpanding(pstrValue == "true");
            else if (pstrName == "itemfont") mListInfo.mFontIdx = int.Parse(pstrValue);
            else if (pstrName == "itemalign")
            {
                if (pstrValue.IndexOf("left") >= 0)
                {
                    mListInfo.mTextStyle &= ~((int)FormatFlags.DT_CENTER | (int)FormatFlags.DT_RIGHT);
                    mListInfo.mTextStyle |= (int)FormatFlags.DT_LEFT;
                }
                if (pstrValue.IndexOf("center") >= 0)
                {
                    mListInfo.mTextStyle &= ~((int)FormatFlags.DT_LEFT | (int)FormatFlags.DT_RIGHT);
                    mListInfo.mTextStyle |= (int)FormatFlags.DT_CENTER;
                }
                if (pstrValue.IndexOf("right") >= 0)
                {
                    mListInfo.mTextStyle &= ~((int)FormatFlags.DT_LEFT | (int)FormatFlags.DT_CENTER);
                    mListInfo.mTextStyle |= (int)FormatFlags.DT_RIGHT;
                }
            }
            if (pstrName == "itemtextpadding")
            {
                string[] listValue = pstrValue.Split(',');
                Rectangle rcTextPadding = new Rectangle(int.Parse(listValue[0]),
                    int.Parse(listValue[1]),
                    int.Parse(listValue[2]) - int.Parse(listValue[0]),
                    int.Parse(listValue[3]) - int.Parse(listValue[1]));

                setItemTextPadding(rcTextPadding);
            }
            else if (pstrName == "itemtextcolor")
            {
                pstrValue = pstrValue.TrimStart('#');

                int clrColor = Convert.ToInt32(pstrValue, 16);
                setItemTextColor(clrColor);
            }
            else if (pstrName == "itembkcolor")
            {
                pstrValue = pstrValue.TrimStart('#');

                int clrColor = Convert.ToInt32(pstrValue, 16);
                setItemBkColor(clrColor);
            }
            else if (pstrName == "itemimage") setItemImage(pstrValue);
            else if (pstrName == "itemselectedtextcolor")
            {
                pstrValue = pstrValue.TrimStart('#');

                int clrColor = Convert.ToInt32(pstrValue, 16);
                setSelectedItemTextColor(clrColor);
            }
            else if (pstrName == "itemselectedbkcolor")
            {
                pstrValue = pstrValue.TrimStart('#');

                int clrColor = Convert.ToInt32(pstrValue, 16);
                setSelectedItemBkColor(clrColor);
            }
            else if (pstrName == "itemselectedimage") setSelectedItemImage(pstrValue);
            else if (pstrName == "itemhottextcolor")
            {
                pstrValue = pstrValue.TrimStart('#');

                int clrColor = Convert.ToInt32(pstrValue, 16);
                setHotItemTextColor(clrColor);
            }
            else if (pstrName == "itemhotbkcolor")
            {
                pstrValue = pstrValue.TrimStart('#');

                int clrColor = Convert.ToInt32(pstrValue, 16);
                setHotItemBkColor(clrColor);
            }
            else if (pstrName == "itemhotimage") setHotItemImage(pstrValue);
            else if (pstrName == "itemdisabledtextcolor")
            {
                pstrValue = pstrValue.TrimStart('#');

                int clrColor = Convert.ToInt32(pstrValue, 16);
                setDisabledItemTextColor(clrColor);
            }
            else if (pstrName == "itemdisabledbkcolor")
            {
                pstrValue = pstrValue.TrimStart('#');

                int clrColor = Convert.ToInt32(pstrValue, 16);
                setDisabledItemBkColor(clrColor);
            }
            else if (pstrName == "itemdisabledimage") setDisabledItemImage(pstrValue);
            else if (pstrName == "itemlinecolor")
            {
                pstrValue = pstrValue.TrimStart('#');

                int clrColor = Convert.ToInt32(pstrValue, 16);
                setItemLineColor(clrColor);
            }
            else if (pstrName == "itemshowhtml") setItemShowHtml(pstrValue == "true");
            else base.setAttribute(pstrName, pstrValue);
        }
        public virtual IListCallbackUI getTextCallback()
        {
            return mCallback;
        }
        public virtual void setTextCallback(IListCallbackUI pCallback)
        {
            mCallback = pCallback;
        }
        public override Size getScrollPos()
        {
            return mList.getScrollPos();
        }
        public override Size getScrollRange()
        {
            return mList.getScrollRange();
        }
        public override void setScrollPos(Size szPos)
        {
            mList.setScrollPos(szPos);
        }
        public override void lineUp()
        {
            mList.lineUp();
        }
        public override void lineDown()
        {
            mList.lineDown();
        }
        public override void pageUp()
        {
            mList.pageUp();
        }
        public override void pageDown()
        {
            mList.pageDown();
        }
        public override void homeUp()
        {
            mList.homeUp();
        }
        public override void endDown()
        {
            mList.endDown();
        }
        public override void lineLeft()
        {
            mList.lineLeft();
        }
        public override void lineRight()
        {
            mList.lineRight();
        }
        public override void pageLeft()
        {
            mList.pageLeft();
        }
        public override void pageRight()
        {
            mList.pageRight();
        }
        public override void homeLeft()
        {
            mList.homeLeft();
        }
        public override void endRight()
        {
            mList.endRight();
        }
        public override void enableScrollBar(bool bEnableVertical, bool bEnableHorizontal)
        {
            mList.enableScrollBar(bEnableVertical, bEnableHorizontal);
        }
        public override ScrollbarUI getVerticalScrollbar()
        {
            return mList.getVerticalScrollbar();
        }
        public override ScrollbarUI getHorizontalScrollbar()
        {
            return mList.getHorizontalScrollbar();
        }
        public void setScrollSelect(bool bScrollSelect)
        {
            mScrollSelect = bScrollSelect;
        }

        public const int UILIST_MAX_COLUMNS = 32;
        protected bool mScrollSelect;
        protected int mCurSel;
        protected int mExpandedItem;
        protected IListCallbackUI mCallback;
        protected ListBodyUI mList;
        protected ListHeaderUI mHeader;
        protected TListInfoUI mListInfo;
    }
}
