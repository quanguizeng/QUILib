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
    public interface IContainerUI
    {
        ControlUI getItemAt(int idx);
        int getItemIndex(ControlUI control);
        bool setItemIndex(ControlUI control, int idx);
        int getCount();
        bool add(ControlUI control);
        bool addAt(ControlUI control, int idx);
        bool remove(ControlUI control);
        bool removeAt(int idx);
        void removeAll();
    }
    public class ContainerUI : ControlUI, IContainerUI
    {
        public ContainerUI()
        {
            mChildPadding = 0;
            mAutoDestroy = true;
            mMouseChildEnabled = true;
            mVerticalScrollbar = null;
            mHorizontalScrollbar = null;
            mScrollProcess = false;

            mItems = new List<ControlUI>();
        }
        ~ContainerUI()
        {
        }

        public override string getClass()
        {
            return "ContainerUI";
        }
        public override ControlUI getInterface(string name)
        {
            if (name == "IContainer")
            {
                return this;
            }
            if (name == "Container")
            {
                return this;
            }

            return base.getInterface(name);
        }
        public virtual ControlUI getItemAt(int idx)
        {
            if (idx < 0 || idx >= mItems.Count)
            {
                return null;
            }
            return (ControlUI)mItems[idx];
        }
        public virtual int getItemIndex(ControlUI control)
        {
            for (int i = 0; i < mItems.Count; i++)
            {
                if ((ContainerUI)mItems[i] == control)
                {
                    return i;
                }
            }

            return -1;
        }
        public virtual bool setItemIndex(ControlUI control, int idx)
        {
            for (int i = 0; i < mItems.Count; i++)
            {
                if ((ControlUI)mItems[i] == control)
                {
                    mItems.Remove(mItems[i]);
                    mItems.Insert(idx, control);

                    return true;
                }
            }

            return false;
        }
        public virtual int getCount()
        {
            return mItems.Count;

        }
        public virtual bool add(ControlUI control)
        {
            if (control == null)
            {
                return false;
            }

            if (mManager != null)
            {
                mManager.initControls(control, this);
            }
            needUpdate();
            mItems.Add(control);

            return true;
        }
        public virtual bool addAt(ControlUI control, int idx)
        {
            if (control == null || idx < 0 || idx > mItems.Count)
            {
                return false;
            }

            if (mManager != null)
            {
                mManager.initControls(control, this);
            }
            needUpdate();

            mItems.Insert(idx, control);

            return true;
        }
        public virtual bool remove(ControlUI control)
        {

            if (control == null)
            {
                return false;
            }

            return mItems.Remove(control);
        }
        public virtual bool removeAt(int idx)
        {
            if (idx < 0 || idx >= mItems.Count)
            {
                return false;
            }

            return mItems.Remove(mItems[idx]);
        }
        public virtual void removeAll()
        {
            // 需要遍历子树，释放子节点内存
        }
        public override void eventProc(ref TEventUI newEvent)
        {
            if (isMouseEnabled() == false &&
                newEvent.mType > (int)EVENTTYPE_UI.UIEVENT__MOUSEBEGIN &&
                newEvent.mType < (int)EVENTTYPE_UI.UIEVENT__MOUSEEND)
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

            if (mVerticalScrollbar != null &&
                mVerticalScrollbar.isVisible() &&
                mVerticalScrollbar.isEnabled())
            {
                if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_KEYDOWN)
                {
                    switch ((Keys)newEvent.mKey)
                    {
                        case Keys.Down:
                            {
                                lineDown();

                                return;
                            }
                        case Keys.Up:
                            {
                                lineUp();

                                return;
                            }
                        case Keys.Next:
                            {
                                pageDown();

                                return;
                            }
                        case Keys.Prior:
                            {
                                pageUp();
                                return;
                            }
                        case Keys.Home:
                            {
                                homeUp();
                                return;
                            }
                        case Keys.End:
                            {
                                endDown();
                                return;
                            }
                    }
                }
                else if (newEvent.mKey == (int)EVENTTYPE_UI.UIEVENT_SCROLLWHEEL)
                {
                    switch ((ScrollBarCommands)LOWORD((int)newEvent.mWParam))
                    {
                        case ScrollBarCommands.SB_LINEUP:
                            lineUp();
                            return;
                        case ScrollBarCommands.SB_LINEDOWN:
                            lineDown();
                            return;
                    }
                }
            }
            else if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible() && mHorizontalScrollbar.isEnabled())
            {
                if (newEvent.mType == (int)EventTypeUI.UIEVENT_KEYDOWN)
                {
                    switch ((Keys)newEvent.mKey)
                    {
                        case Keys.Down:
                            lineRight();
                            return;
                        case Keys.Up:
                            lineLeft();
                            return;
                        case Keys.Next:
                            pageRight();
                            return;
                        case Keys.Prior:
                            pageLeft();
                            return;
                        case Keys.Home:
                            homeLeft();
                            return;
                        case Keys.End:
                            endRight();
                            return;
                    }
                }
                else if (newEvent.mType == (int)EventTypeUI.UIEVENT_SCROLLWHEEL)
                {
                    switch ((ScrollBarCommands)LOWORD((int)newEvent.mWParam))
                    {
                        case ScrollBarCommands.SB_LINEUP:
                            lineLeft();
                            return;
                        case ScrollBarCommands.SB_LINEDOWN:
                            lineRight();
                            return;
                    }
                }
            }
            base.eventProc(ref newEvent);
        }
        public static Int16 LOWORD(int x)
        {
            Int16 result = (Int16)((short)x & (int)0xffff);

            return result;
        }

        public override void setVisible(bool visible = true)
        {
            if (visible == mVisible)
            {
                return;
            }

            foreach (var item in mItems)
            {
                item.setInternVisible(visible);
            }

            base.setVisible(visible);
        }

        public override void setInternVisible(bool visible = true)
        {
            foreach (var item in mItems)
            {
                item.setInternVisible(visible);
            }

            base.setVisible(visible);
        }

        public override void setMouseEnabled(bool enable = true)
        {
            if (mVerticalScrollbar != null)
            {
                mVerticalScrollbar.setMouseEnabled(enable);
            }
            if (mHorizontalScrollbar != null)
            {
                mHorizontalScrollbar.setMouseEnabled(enable);
            }

            base.setMouseEnabled(enable);
        }


        public virtual Rectangle getInset()
        {
            return mRectInset;
        }
        public virtual void setInset(Rectangle rectInset)
        {
            mRectInset = rectInset;
            needUpdate();
        }
        public virtual int getChildPadding()
        {
            return mChildPadding;
        }
        public virtual void setChildPadding(int padding)
        {
            mChildPadding = padding;
            needUpdate();
        }
        public virtual bool isAutoDestroy()
        {
            return mAutoDestroy;
        }
        public virtual void setAutoDestroy(bool auto)
        {
            mAutoDestroy = auto;
        }
        public virtual bool isMouseChildEnabled()
        {
            return mMouseChildEnabled;
        }
        public virtual void setMouseChildEnabled(bool enable)
        {
            mMouseChildEnabled = enable;
        }


        public virtual int findSelectable(int idx, bool forward = true)
        {
            if (getCount() == 0)
            {
                return -1;
            }
            idx = idx > 0 ? idx : 0;
            idx = idx > getCount() - 1 ? getCount() - 1 : idx;
            if (forward)
            {
                for (int i = idx; i < getCount(); i++)
                {
                    if (getItemAt(i).getInterface("ListItem") != null &&
                        getItemAt(i).isVisible() &&
                        getItemAt(i).isEnabled())
                    {
                        return i;
                    }
                }
                return -1;
            }
            else
            {
                for (int i = idx; i >= 0; i--)
                {
                    if (getItemAt(i).getInterface("ListItem") != null &&
                        getItemAt(i).isVisible() &&
                        getItemAt(i).isEnabled())
                    {
                        return i;
                    }
                }

                return findSelectable(0, true);
            }
        }
        public virtual void setFloatPos(int iIndex)
        {
            // 因为CControlUI::SetPos对float的操作影响，这里不能对float组件添加滚动条的影响
            if (iIndex < 0 || iIndex >= mItems.Count) return;

            ControlUI pControl = mItems[iIndex];

            if (!pControl.isVisible()) return;
            if (!pControl.isFloat()) return;

            Size szXY = pControl.getFixedXY();
            Size sz = new Size(pControl.getFixedWidth(), pControl.getFixedHeight());
            Rectangle rcCtrl = new Rectangle();
            int right = 0;
            int bottom = 0;
            if (szXY.Width >= 0)
            {
                rcCtrl.X = mRectItem.Left + szXY.Width;
                right = mRectItem.Left + szXY.Width + sz.Width;
            }
            else
            {
                rcCtrl.X = mRectItem.Right + szXY.Width - sz.Width;
                right = mRectItem.Right + szXY.Width;
            }
            if (szXY.Height >= 0)
            {
                rcCtrl.Y = mRectItem.Top + szXY.Height;
                bottom = mRectItem.Top + szXY.Height + sz.Height;
            }
            else
            {
                rcCtrl.Y = mRectItem.Bottom + szXY.Height - sz.Height;
                bottom = mRectItem.Bottom + szXY.Height;
            }
            rcCtrl = new Rectangle(rcCtrl.X, rcCtrl.Y, right - rcCtrl.X, bottom - rcCtrl.Y);
            pControl.setPos(rcCtrl);
        }
        public override void setPos(Rectangle rect)
        {
            base.setPos(rect);

            if (mRectItem.IsEmpty)
            {
                return;
            }
            int newLeft = rect.Left + mRectInset.Left;
            int newTop = rect.Top + mRectInset.Top;
            int newRight = rect.Right - mRectInset.Width;
            int newBottom = rect.Bottom - mRectInset.Height;

            rect.X = newLeft;
            rect.Width = newRight - newLeft;
            rect.Y = newTop;
            rect.Height = newBottom - newTop;

            for (int i = 0; i < mItems.Count; i++)
            {
                if (mItems[i].isVisible() == false)
                {
                    continue;
                }
                if (mItems[i].isFloat())
                {
                    setFloatPos(i);
                }
                else
                {
                    mItems[i].setPos(rect);
                }
            }
        }
        public override void doPaint(ref Graphics graphics, ref Bitmap bitmap, Rectangle rectPaint)
        {
            Rectangle rcTemp;
            if (rectPaint.IntersectsWith(mRectItem) == false)
            {
                return;
            }
            rcTemp = rectPaint;
            rcTemp.Intersect(mRectItem);

            Region oldRgn = graphics.Clip;
            RenderClip clip = new RenderClip();
            RenderClip.generateClip(ref graphics, rcTemp, ref clip);

            base.doPaint(ref graphics, ref bitmap, rectPaint);

            if (mItems.Count > 0)
            {
                Rectangle rc = mRectItem;
                int newLeft = rc.Left + mRectInset.Left;
                int newRight = rc.Right - mRectInset.Right;
                int newTop = rc.Top + mRectInset.Top;
                int newBottom = rc.Bottom - mRectInset.Bottom;

                rc.X = newLeft;
                rc.Width = newRight - newLeft;
                rc.Y = newTop;
                rc.Height = newBottom - newTop;

                // 绘制滚动条
                if (mVerticalScrollbar != null && mVerticalScrollbar.isVisible())
                {
                    rc.Width = rc.Right - mVerticalScrollbar.getFixedWidth() - rc.Left;
                }
                if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
                {
                    rc.Height = rc.Bottom - mHorizontalScrollbar.getFixedHeight() - rc.Top;
                }

                // 绘制子控件
                if (rectPaint.IntersectsWith(rc) == false)
                {
                    foreach (var item in mItems)
                    {
                        if (item.isVisible() == false)
                        {
                            continue;
                        }
                        if (rectPaint.IntersectsWith(item.getPos()) == false)
                        {
                            continue;
                        }
                        if (item.isFloat())
                        {
                            if (mRectItem.IntersectsWith(item.getPos()) == false)
                            {
                                continue;
                            }
                            item.doPaint(ref graphics, ref bitmap, rectPaint);
                        }
                    }
                }
                else
                {
                    RenderClip childClip = new RenderClip();
                    RenderClip.generateClip(ref graphics, rcTemp, ref childClip);

                    foreach (var item in mItems)
                    {
                        if (item.isVisible() == false)
                        {
                            continue;
                        }
                        if (rectPaint.IntersectsWith(item.getPos()) == false)
                        {
                            continue;
                        }
                        if (item.isFloat())
                        {
                            if (mRectItem.IntersectsWith(item.getPos()) == false)
                            {
                                continue;
                            }
                            RenderClip.useOldClipBegin(ref graphics, ref childClip);
                            item.doPaint(ref graphics, ref bitmap, rectPaint);
                            RenderClip.useOldClipEnd(ref graphics, ref childClip);
                        }
                        else
                        {
                            if (rc.IntersectsWith(item.getPos()) == false)
                            {
                                continue;
                            }
                            item.doPaint(ref graphics, ref bitmap, rectPaint);
                        }
                    }
                }
            }

            if (mVerticalScrollbar != null &&
                mVerticalScrollbar.isVisible())
            {
                if (rectPaint.IntersectsWith(mVerticalScrollbar.getPos()))
                {
                    mVerticalScrollbar.doPaint(ref graphics, ref bitmap, rectPaint);
                }
            }

            if (mHorizontalScrollbar != null &&
                mHorizontalScrollbar.isVisible())
            {
                if (rectPaint.IntersectsWith(mHorizontalScrollbar.getPos()))
                {
                    mHorizontalScrollbar.doPaint(ref graphics, ref bitmap, rectPaint);
                }
            }
            graphics.Clip = oldRgn;
        }
        public override void setAttribute(string name, string value)
        {
            if (name == "inset")
            {
                string[] listValue = value.Split(',');
                if (listValue.Length != 4)
                {
                    throw new Exception("inset 参数不为4个");
                }
                int left = int.Parse(listValue[0]);
                int right = int.Parse(listValue[2]);
                int top = int.Parse(listValue[1]);
                int bottom = int.Parse(listValue[3]);
                setInset(new Rectangle(left, top, right - left, bottom-top));
            }
            else if (name == "mousechild") setMouseChildEnabled(value == "true");
            else if (name == "vscrollbar")
            {
                enableScrollBar(value == "true", getHorizontalScrollbar() != null);
            }
            else if (name == "hscrollbar")
            {
                enableScrollBar(getVerticalScrollbar() != null, value == "true");
            }
            else if (name == "childpadding") setChildPadding(int.Parse(value));
            else base.setAttribute(name, value);
        }
        public override void setManager(PaintManagerUI manager, ControlUI parent)
        {
            foreach (var item in mItems)
            {
                item.setManager(manager, this);
            }

            if (mVerticalScrollbar != null)
            {
                mVerticalScrollbar.setManager(manager, this);
            }
            if (mHorizontalScrollbar != null)
            {
                mHorizontalScrollbar.setManager(manager, this);
            }

            base.setManager(manager, parent);
        }
        public override ControlUI findControl(FINDCONTROLPROC proc, ref object data, uint flags)
        {
            ControlUI pResult = null;

            if ((flags & ControlFlag.UIFIND_VISIBLE) != 0 &&
                isVisible() == false)
            {
                return null;
            }
            if ((flags & ControlFlag.UIFIND_ENABLED) != 0 &&
                isVisible() == false)
            {
                return null;
            }
            if ((flags & ControlFlag.UIFIND_HITTEST) != 0)
            {
                // 检测水平滚动条控件
                Point pos = (Point)data;
                if (mRectItem.Contains(pos) == false)
                {
                    return null;
                }

                if (!mMouseChildEnabled)
                {
                    if (mVerticalScrollbar != null)
                    {
                        pResult = mVerticalScrollbar.findControl(proc, ref data, flags);
                    }
                    if (pResult == null && mHorizontalScrollbar != null)
                    {
                        pResult = mHorizontalScrollbar.findControl(proc, ref data, flags);
                    }
                    if (pResult == null)
                    {
                        pResult = base.findControl(proc, ref data, flags);
                    }

                    return pResult;
                }
            }

            // 检测垂直滚动条控件
            if (mVerticalScrollbar != null)
            {
                pResult = mVerticalScrollbar.findControl(proc, ref data, flags);
            }
            if (pResult == null && mHorizontalScrollbar != null)
            {
                pResult = mHorizontalScrollbar.findControl(proc, ref data, flags);
            }
            if (pResult != null)
            {
                return pResult;
            }

            if ((flags & ControlFlag.UIFIND_ME_FIRST) != 0)
            {
                pResult = base.findControl(proc, ref data, flags);
                if (pResult != null)
                {
                    return pResult;
                }
            }
            if ((flags & ControlFlag.UIFIND_TOP_FIRST) != 0)
            {

                for (int i = mItems.Count - 1; i >= 0; i--)
                {
                    pResult = mItems[i].findControl(proc, ref data, flags);
                    if (pResult != null)
                    {
                        return pResult;
                    }
                }
            }
            else
            {
                foreach (var item in mItems)
                {
                    pResult = item.findControl(proc, ref data, flags);
                    if (pResult != null)
                    {
                        return pResult;
                    }
                }
            }


            if (pResult == null)
            {
                pResult = base.findControl(proc, ref data, flags);
            }

            return pResult;
        }

        public virtual Size getScrollPos()
        {
            Size sz = new Size(0, 0);
            if (mVerticalScrollbar != null && mVerticalScrollbar.isVisible())
            {
                sz.Height = mVerticalScrollbar.getScrollPos();
            }
            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
            {
                sz.Width = mHorizontalScrollbar.getScrollPos();
            }

            return sz;
        }
        public virtual Size getScrollRange()
        {
            Size sz = new Size(0, 0);

            if (mVerticalScrollbar != null && mVerticalScrollbar.isVisible())
            {
                sz.Height = mVerticalScrollbar.getScrollRange();
            }
            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
            {
                sz.Width = mHorizontalScrollbar.getScrollRange();
            }

            return sz;
        }
        public virtual void setScrollPos(Size szPos)
        {
            int cx = 0;
            int cy = 0;
            if (mVerticalScrollbar != null && mVerticalScrollbar.isVisible())
            {
                int iLastScrollPos = mVerticalScrollbar.getScrollPos();
                mVerticalScrollbar.setScrollPos(szPos.Height);
                cy = mVerticalScrollbar.getScrollPos() - iLastScrollPos;
            }

            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
            {
                int iLastScrollPos = mHorizontalScrollbar.getScrollPos();
                mHorizontalScrollbar.setScrollPos(szPos.Width);
                cx = mHorizontalScrollbar.getScrollPos() - iLastScrollPos;
            }

            if (cx == 0 && cy == 0) return;

            Rectangle rcPos;
            for (int it2 = 0; it2 < mItems.Count; it2++)
            {
                ControlUI pControl = (mItems[it2]);
                if (!pControl.isVisible()) continue;
                if (pControl.isFloat()) continue;

                rcPos = pControl.getPos();
                int newLeft = rcPos.Left - cx;
                int newRight = rcPos.Right - cx;
                int newTop = rcPos.Top - cy;
                int newBottom = rcPos.Bottom - cy;
                Rectangle newRect = new Rectangle(newLeft, newTop, newRight - newLeft, newBottom - newTop);
                pControl.setPos(newRect);
            }

            invalidate();
        }
        public virtual void lineUp()
        {
            int cyLine = 8;
            if (mManager != null) cyLine = mManager.getDefaultFont().Height + 8;

            Size sz = getScrollPos();
            sz.Height -= cyLine;
            setScrollPos(sz);

        }
        public virtual void lineDown()
        {
            int cyLine = 8;
            if (mManager != null) cyLine = mManager.getDefaultFont().Height + 8;

            Size sz = getScrollPos();
            sz.Height += cyLine;
            setScrollPos(sz);
        }
        public virtual void pageUp()
        {
            Size sz = getScrollPos();
            int iOffset = mRectItem.Right - mRectItem.Left - mRectInset.Left - mRectInset.Right;
            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible()) iOffset -= mHorizontalScrollbar.getFixedHeight();
            sz.Height -= iOffset;
            setScrollPos(sz);

        }
        public virtual void pageDown()
        {
            Size sz = getScrollPos();
            int iOffset = mRectItem.Right - mRectItem.Left - mRectInset.Left - mRectInset.Right;
            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible()) iOffset -= mHorizontalScrollbar.getFixedHeight();
            sz.Height += iOffset;
            setScrollPos(sz);
        }
        public virtual void homeUp()
        {
            Size sz = getScrollPos();
            sz.Height = 0;
            setScrollPos(sz);
        }
        public virtual void endDown()
        {
            Size sz = getScrollPos();
            sz.Height = getScrollRange().Height;
            setScrollPos(sz);
        }
        public virtual void lineLeft()
        {
            Size sz = getScrollPos();
            sz.Width -= 8;
            setScrollPos(sz);
        }
        public virtual void lineRight()
        {
            Size sz = getScrollPos();
            sz.Width += 8;
            setScrollPos(sz);
        }
        public virtual void pageLeft()
        {
            Size sz = getScrollPos();
            int iOffset = mRectItem.Right - mRectItem.Left - mRectInset.Left - mRectInset.Right;
            if (mVerticalScrollbar != null && mVerticalScrollbar.isVisible()) iOffset -= mVerticalScrollbar.getFixedWidth();
            sz.Width -= iOffset;
            setScrollPos(sz);
        }
        public virtual void pageRight()
        {
            Size sz = getScrollPos();
            int iOffset = mRectItem.Right - mRectItem.Left - mRectInset.Left - mRectInset.Right;
            if (mVerticalScrollbar != null && mVerticalScrollbar.isVisible()) iOffset -= mVerticalScrollbar.getFixedWidth();
            sz.Width += iOffset;
            setScrollPos(sz);
        }
        public virtual void homeLeft()
        {
            Size sz = getScrollPos();
            sz.Width = 0;
            setScrollPos(sz);
        }
        public virtual void homeRight()
        {
            Size sz = getScrollPos();
            sz.Width = getScrollRange().Width;
            setScrollPos(sz);
        }
        public virtual void endRight()
        {
            Size sz = getScrollPos();
            sz.Width = getScrollRange().Width;
            setScrollPos(sz);
        }
        public virtual void enableScrollBar(bool bEnableVertical = true, bool bEnableHorizontal = false)
        {
            if (bEnableVertical && mVerticalScrollbar == null)
            {
                mVerticalScrollbar = new ScrollbarUI();
                mVerticalScrollbar.setOwner(this);
                mVerticalScrollbar.setManager(mManager, null);
                if (mManager != null)
                {
                    string pDefaultAttributes = mManager.getDefaultAttributeList("VScrollBar");
                    if (pDefaultAttributes != "")
                    {
                        mVerticalScrollbar.applyAttributeList(pDefaultAttributes);
                    }
                }
            }
            else if (!bEnableVertical && mVerticalScrollbar != null)
            {
                mVerticalScrollbar = null;
            }

            if (bEnableHorizontal && mHorizontalScrollbar == null)
            {
                mHorizontalScrollbar = new ScrollbarUI();
                mHorizontalScrollbar.setHorizontal(true);
                mHorizontalScrollbar.setOwner(this);
                mHorizontalScrollbar.setManager(mManager, null);
                if (mManager != null)
                {
                    string pDefaultAttributes = mManager.getDefaultAttributeList("HScrollBar");
                    if (pDefaultAttributes != "")
                    {
                        mHorizontalScrollbar.applyAttributeList(pDefaultAttributes);
                    }
                }
            }
            else if (!bEnableHorizontal && mHorizontalScrollbar != null)
            {
                mHorizontalScrollbar = null;
            }

            needUpdate();
        }
        public virtual ScrollbarUI getVerticalScrollbar()
        {
            return mVerticalScrollbar;
        }
        public virtual ScrollbarUI getHorizontalScrollbar()
        {
            return mHorizontalScrollbar;
        }

        public virtual void processScrollbar(Rectangle rc, int cxRequired, int cyRequired)
        {
            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
            {
                int newLeft = rc.Left;
                int newRight = rc.Right;
                int newTop = rc.Bottom;
                int newBottom = rc.Bottom + mHorizontalScrollbar.getFixedHeight();
                Rectangle rcScrollbarPos = new Rectangle(newLeft, newTop, newRight - newLeft, newBottom - newTop);
                mHorizontalScrollbar.setPos(rcScrollbarPos);
            }

            if (mVerticalScrollbar == null) return;

            if (cyRequired > rc.Bottom - rc.Top && !mVerticalScrollbar.isVisible())
            {
                mVerticalScrollbar.setVisible(true);
                mVerticalScrollbar.setScrollRange(cyRequired - (rc.Bottom - rc.Top));
                mVerticalScrollbar.setScrollPos(0);
                mScrollProcess = true;
                setPos(mRectItem);
                mScrollProcess = false;
                return;
            }
            if (!mVerticalScrollbar.isVisible()) return;

            int cyScroll = cyRequired - (rc.Bottom - rc.Top);
            if (cyScroll <= 0 && !mScrollProcess)
            {
                mVerticalScrollbar.setVisible(false);
                mVerticalScrollbar.setScrollPos(0);
                mVerticalScrollbar.setScrollRange(0);
                setPos(mRectItem);
            }
            else
            {
                int newLeft = rc.Right;
                int newRight = rc.Right + mVerticalScrollbar.getFixedWidth();
                int newTop = rc.Top;
                int newBottom = rc.Bottom;

                Rectangle rcScrollbarPos = new Rectangle(newLeft, newTop, newRight - newLeft, newBottom - newTop);
                mVerticalScrollbar.setPos(rcScrollbarPos);

                if (mVerticalScrollbar.getScrollRange() != cyScroll)
                {
                    int iScrollPos = mVerticalScrollbar.getScrollPos();
                    mVerticalScrollbar.setScrollRange(Math.Abs(cyScroll));
                    if (mVerticalScrollbar.getScrollRange() == 0)
                    {
                        mVerticalScrollbar.setVisible(false);
                        mVerticalScrollbar.setScrollPos(0);
                    }
                    if (iScrollPos > mVerticalScrollbar.getScrollPos())
                    {
                        setPos(mRectItem);
                    }
                }
            }
        }

        protected List<ControlUI> mItems;
        protected Rectangle mRectInset;
        protected int mChildPadding;
        protected bool mAutoDestroy;
        protected bool mMouseChildEnabled;
        protected bool mScrollProcess;  // 防止setPos循环调用

        protected ScrollbarUI mVerticalScrollbar;
        protected ScrollbarUI mHorizontalScrollbar;
    }
}
