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
    public class HorizontalLayoutUI : ContainerUI
    {
        public HorizontalLayoutUI()
        {
            mSepWidth = 0;
            mButtonState = 0;
            mLastMouse = new Point(0, 0);
            mRectNewPos = new Rectangle(0, 0, 0, 0); ;
            mImmMode = false;
        }
        ~HorizontalLayoutUI()
        {

        }

        public override string getClass()
        {
            return "HorizontalLayoutUI";
        }
        public override ControlUI getInterface(string name)
        {
            if ("HorizontalLayout" == name)
            {
                return this;
            }

            return base.getInterface(name);
        }
        public override int getControlFlags()
        {
            if (isEnabled() && mSepWidth != 0)
            {
                return (int)ControlFlag.UIFLAG_SETCURSOR;
            }

            return 0;
        }
        public override void doPostPaint(ref Graphics graphics, ref Bitmap bitmap, ref Rectangle rectPaint)
        {
            if ((mButtonState & (int)PaintFlags.UISTATE_CAPTURED) != 0 && !mImmMode)
            {
                Rectangle rcSeparator = getThumbRect(true);

                RenderEngine.drawColor(ref graphics, ref bitmap, ref rcSeparator, Color.FromArgb(0xAA, 00, 00, 00).ToArgb());
            }
        }
        public void setSepWidth(int width)
        {
            mSepWidth = width;
        }
        public void setSepImmMode(bool immediately)
        {
            if (mImmMode == immediately) return;
            if ((mButtonState & (int)ControlFlag.UISTATE_CAPTURED) != 0 && !mImmMode && mManager != null)
            {
                mManager.removePostPaint(this);
            }

            mImmMode = immediately;
        }
        public override void setAttribute(string name, string value)
        {
            if (name == "sepwidth") setSepWidth(int.Parse(value));
            else if (name == "sepimm") setSepImmMode(value == "true");
            else base.setAttribute(name, value);
        }
        public override void eventProc(ref TEventUI newEvent)
        {
            if (mSepWidth != 0)
            {
                if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONDOWN && isEnabled())
                {
                    Rectangle rcSeparator = getThumbRect(false);
                    if (rcSeparator.Contains(newEvent.mMousePos))
                    {
                        mButtonState |= (int)PaintFlags.UISTATE_CAPTURED;
                        mLastMouse = newEvent.mMousePos;
                        mRectNewPos = mRectItem;
                        if (!mImmMode && mManager != null)
                        {
                            ControlUI ctl = this;
                            mManager.addPostPaint(ref ctl);
                        }
                        return;
                    }
                }
                if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONUP)
                {
                    if ((mButtonState & (int)PaintFlags.UISTATE_CAPTURED) != 0)
                    {
                        mButtonState &= ~(int)PaintFlags.UISTATE_CAPTURED;
                        mRectItem = mRectNewPos;
                        if (!mImmMode && mManager != null)
                        {
                            mManager.removePostPaint(this);
                        }
                        needParentUpdate();
                        return;
                    }
                }
                if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSEMOVE)
                {
                    if ((mButtonState & (int)PaintFlags.UISTATE_CAPTURED) != 0)
                    {
                        int cx = newEvent.mMousePos.X - mLastMouse.X;
                        mLastMouse = newEvent.mMousePos;
                        Rectangle rc = mRectNewPos;
                        if (mSepWidth >= 0)
                        {
                            if (cx > 0 && newEvent.mMousePos.X < mRectNewPos.Right - mSepWidth) return;
                            if (cx < 0 && newEvent.mMousePos.X > mRectNewPos.Right) return;
                            rc.Width = rc.Right + cx - rc.Left;
                            if (rc.Right - rc.Left <= getMinWidth())
                            {
                                if (mRectNewPos.Right - mRectNewPos.Left <= getMinWidth()) return;
                                rc.Width = getMinWidth();
                            }
                            if (rc.Right - rc.Left >= getMaxWidth())
                            {
                                if (mRectNewPos.Right - mRectNewPos.Left >= getMaxWidth()) return;
                                rc.Width = getMaxWidth();
                            }
                        }
                        else
                        {
                            if (cx > 0 && newEvent.mMousePos.X < mRectNewPos.Left) return;
                            if (cx < 0 && newEvent.mMousePos.X > mRectNewPos.Left - mSepWidth) return;
                            rc.X += cx;
                            if (rc.Right - rc.Left <= getMinWidth())
                            {
                                if (mRectNewPos.Right - mRectNewPos.Left <= getMinWidth()) return;
                                rc.X = rc.Right - getMinWidth();
                            }
                            if (rc.Right - rc.Left >= getMaxWidth())
                            {
                                if (mRectNewPos.Right - mRectNewPos.Left >= getMaxWidth()) return;
                                rc.X = rc.Right - getMaxWidth();
                            }
                        }

                        Rectangle rcInvalidate = getThumbRect(true);
                        mRectNewPos = rc;
                        mXYFixed.Width = mRectNewPos.Right - mRectNewPos.Left;

                        if (mImmMode)
                        {
                            mRectItem = mRectNewPos;
                            needParentUpdate();
                        }
                        else
                        {
                            rcInvalidate.Intersect(getThumbRect(true));
                            rcInvalidate.Intersect(getThumbRect(false));
                            if (mManager != null)
                            {
                                mManager.invalidate(ref rcInvalidate);
                            }

                        }
                        return;
                    }
                }
                if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SETCURSOR)
                {
                    Rectangle rcSeparator = getThumbRect(false);
                    if (isEnabled() && rcSeparator.Contains(newEvent.mMousePos))
                    {
                        if (mManager.getPaintWindow().Cursor != Cursors.SizeWE)
                        {
                            mManager.getPaintWindow().Cursor = Cursors.SizeWE;
                        }
                        return;
                    }
                }
            }
            base.eventProc(ref newEvent);
        }

        public override void setPos(Rectangle rc)
        {
            setPos0(rc);
            rc = mRectItem;

            // Adjust for inset
            int newLeft = rc.Left + mRectInset.Left;
            int newTop = rc.Top + mRectInset.Top;
            int newRight = rc.Right - mRectInset.Right;
            int newBottom = rc.Bottom - mRectInset.Bottom;

            rc.X = newLeft;
            rc.Width = newRight - newLeft;
            rc.Y = newTop;
            rc.Height = newBottom - newTop;

            if (mItems.Count == 0)
            {
                processScrollbar(rc, 0, 0);
                return;
            }

            if (mVerticalScrollbar != null && mVerticalScrollbar.isVisible())
            {
                rc.Width -= mVerticalScrollbar.getFixedWidth();
            }
            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
            {
                rc.Height -= mHorizontalScrollbar.getFixedHeight();
            }

            // Determine the width of elements that are sizeable
            Size szAvailable = new Size(rc.Right - rc.Left, rc.Bottom - rc.Top);
            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
                szAvailable.Width += mHorizontalScrollbar.getScrollRange();

            int nAdjustables = 0;
            int cxFixed = 0;
            int nEstimateNum = 0;
            for (int it1 = 0; it1 < mItems.Count; it1++)
            {
                ControlUI pControl = (mItems[it1]);
                if (!pControl.isVisible()) continue;
                if (pControl.isFloat()) continue;
                Size sz = pControl.estimateSize(szAvailable);
                if (sz.Width == 0)
                {
                    nAdjustables++;
                }
                else
                {
                    if (sz.Width < pControl.getMinWidth()) sz.Width = pControl.getMinWidth();
                    if (sz.Width > pControl.getMaxWidth()) sz.Width = pControl.getMaxWidth();
                }
                cxFixed += sz.Width + pControl.getPadding().Left + pControl.getPadding().Right;
                nEstimateNum++;
            }
            cxFixed += (nEstimateNum - 1) * mChildPadding;

            int cxExpand = 0;
            if (nAdjustables > 0) cxExpand = (0 - (szAvailable.Width - cxFixed) / nAdjustables) > 0 ? 0 : ((szAvailable.Width - cxFixed) / nAdjustables);
            // Position the elements
            Size szRemaining = szAvailable;
            int iPosX = rc.Left;
            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
            {
                iPosX -= mHorizontalScrollbar.getScrollPos();
            }
            int iAdjustable = 0;
            int cxFixedRemaining = cxFixed;
            for (int it2 = 0; it2 < mItems.Count; it2++)
            {
                ControlUI pControl = (mItems[it2]);
                if (!pControl.isVisible()) continue;
                if (pControl.isFloat())
                {
                    setFloatPos(it2);
                    continue;
                }
                Rectangle rcPadding = pControl.getPadding();
                szRemaining.Width -= rcPadding.Left;
                Size sz = pControl.estimateSize(szRemaining);
                if (sz.Width == 0)
                {
                    iAdjustable++;
                    sz.Width = cxExpand;
                    // Distribute remaining to last element (usually round-off left-overs)
                    if (iAdjustable == nAdjustables)
                    {
                        sz.Width = (0 - (szRemaining.Width - rcPadding.Right - cxFixedRemaining)) > 0 ? 0 : szRemaining.Width - rcPadding.Right - cxFixedRemaining;
                    }
                    if (sz.Width < pControl.getMinWidth()) sz.Width = pControl.getMinWidth();
                    if (sz.Width > pControl.getMaxWidth()) sz.Width = pControl.getMaxWidth();
                }
                else
                {
                    if (sz.Width < pControl.getMinWidth()) sz.Width = pControl.getMinWidth();
                    if (sz.Width > pControl.getMaxWidth()) sz.Width = pControl.getMaxWidth();

                    cxFixedRemaining -= sz.Width;
                }

                sz.Height = (0 - (rc.Bottom - rc.Top - rcPadding.Top - rcPadding.Bottom)) > 0 ? 0 : rc.Bottom - rc.Top - rcPadding.Top - rcPadding.Bottom;

                if (sz.Height < pControl.getMinHeight()) sz.Height = pControl.getMinHeight();
                if (sz.Height > pControl.getMaxHeight()) sz.Height = pControl.getMaxHeight();

                Rectangle rcCtrl = new Rectangle(iPosX + rcPadding.Left,
                    rc.Top + rcPadding.Top,
                    sz.Width + rcPadding.Right,
                    sz.Height);

                pControl.setPos(rcCtrl);
                iPosX += sz.Width + mChildPadding + rcPadding.Left + rcPadding.Right;
                szRemaining.Width -= sz.Width + mChildPadding + rcPadding.Right;
            }
        }
        public Rectangle getThumbRect(bool bUseNew)
        {
            if ((mButtonState & (int)PaintFlags.UISTATE_CAPTURED) != 0 && bUseNew)
            {
                if (mSepWidth >= 0)
                {
                    int newLeft = mRectNewPos.Right - mSepWidth;
                    int newTop = mRectNewPos.Top;
                    int newRight = mRectNewPos.Right;
                    int newBottom = mRectNewPos.Bottom;
                    return new Rectangle(newLeft, newTop, newRight - newLeft, newBottom - newTop);
                }
                else
                {
                    int newLeft = mRectNewPos.Left;
                    int newTop = mRectNewPos.Top;
                    int newRight = mRectNewPos.Left - mSepWidth;
                    int newBottom = mRectNewPos.Bottom;

                    return new Rectangle(newLeft, newTop, newRight - newLeft, newBottom - newTop);
                }
            }
            else
            {
                if (mSepWidth >= 0)
                {
                    int newLeft = mRectItem.Right - mSepWidth;
                    int newTop = mRectItem.Top;
                    int newRight = mRectItem.Right;
                    int newBottom = mRectItem.Bottom;

                    return new Rectangle(newLeft, newTop, newRight - newLeft, newBottom - newTop);
                }
                else
                {
                    int newLeft = mRectItem.Left;
                    int newTop = mRectItem.Top;
                    int newRight = mRectItem.Left - mSepWidth;
                    int newBottom = mRectItem.Bottom;

                    return new Rectangle(newLeft, newTop, newRight - newLeft, newBottom - newTop);
                }
            }
        }

        protected int mSepWidth;
        protected int mButtonState;
        protected Point mLastMouse;
        protected Rectangle mRectNewPos;
        protected bool mImmMode;
    }
}
