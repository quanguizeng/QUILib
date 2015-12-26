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
    public class VerticalLayoutUI : ContainerUI
    {
        public VerticalLayoutUI()
        {

        }
        ~VerticalLayoutUI()
        {

        }

        public override string getClass()
        {
            return "VerticalLayoutUI";
        }
        public override void setPos(Rectangle rc)
        {
            setPos0(rc);

            rc = mRectItem;

            // mRectInset 为垂直布局控件的边界限制，在使用时，要忽略Right和Bottom属性，而使用Width和Height
            int newLeft = rc.Left + mRectInset.Left;
            int newTop = rc.Top + mRectInset.Top;
            int newRight = rc.Right - mRectInset.Right;
            int newBottom = rc.Bottom - mRectInset.Bottom;

            rc.X = newLeft;
            rc.Width = newRight - newLeft;
            rc.Y = newTop;
            rc.Height = newBottom - newTop;

            if (mVerticalScrollbar != null && mVerticalScrollbar.isVisible())
            {
                newRight = rc.Right - mVerticalScrollbar.getFixedWidth();
                Rectangle newRect = new Rectangle(rc.Left, rc.Top, newRight - rc.Left, rc.Bottom - rc.Top);
                rc = newRect;
            }
            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
            {
                newBottom = rc.Bottom - mHorizontalScrollbar.getFixedHeight();
                Rectangle newRect = new Rectangle(rc.Left, rc.Top, rc.Width, newBottom - rc.Top);
                rc = newRect;
            }

            if (mItems.Count == 0)
            {
                processScrollbar(rc, 0, 0);
                return;
            }

            // Determine the minimum size
            Size szAvailable = new Size(rc.Right - rc.Left, rc.Bottom - rc.Top);
            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
                szAvailable.Width += mHorizontalScrollbar.getScrollRange();

            int nAdjustables = 0;
            int cyFixed = 0;
            int nEstimateNum = 0;
            for (int it1 = 0; it1 < mItems.Count; it1++)
            {
                ControlUI pControl = mItems[it1];
                if (!pControl.isVisible()) continue;
                if (pControl.isFloat()) continue;
                Size sz = pControl.estimateSize(szAvailable);
                if (sz.Height == 0)
                {
                    nAdjustables++;
                }
                else
                {
                    if (sz.Height < pControl.getMinHeight()) sz.Height = pControl.getMinHeight();
                    if (sz.Height > pControl.getMaxHeight()) sz.Height = pControl.getMaxHeight();
                }
                cyFixed += sz.Height + pControl.getPadding().Top + pControl.getPadding().Bottom;
                nEstimateNum++;
            }
            cyFixed += (nEstimateNum - 1) * mChildPadding;

            // Place elements
            int cyNeeded = 0;
            int cyExpand = 0;
            if (nAdjustables > 0) cyExpand = (0 - (szAvailable.Height - cyFixed) / nAdjustables) > 0 ? 0 : (szAvailable.Height - cyFixed) / nAdjustables;
            // Position the elements
            Size szRemaining = szAvailable;
            int iPosY = rc.Top;
            if (mVerticalScrollbar != null && mVerticalScrollbar.isVisible())
            {
                iPosY -= mVerticalScrollbar.getScrollPos();
            }
            int iPosX = rc.Left;
            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
            {
                iPosX -= mHorizontalScrollbar.getScrollPos();
            }
            int iAdjustable = 0;
            int cyFixedRemaining = cyFixed;
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
                szRemaining.Height -= rcPadding.Top;
                Size sz = pControl.estimateSize(szRemaining);
                if (sz.Height == 0)
                {
                    iAdjustable++;
                    sz.Height = cyExpand;
                    // Distribute remaining to last element (usually round-off left-overs)
                    if (iAdjustable == nAdjustables)
                    {
                        sz.Height = (0 - szRemaining.Height - rcPadding.Bottom - cyFixedRemaining) > 0 ? 0 : szRemaining.Height - rcPadding.Bottom - cyFixedRemaining;
                    }
                    if (sz.Height < pControl.getMinHeight()) sz.Height = pControl.getMinHeight();
                    if (sz.Height > pControl.getMaxHeight()) sz.Height = pControl.getMaxHeight();
                }
                else
                {
                    if (sz.Height < pControl.getMinHeight()) sz.Height = pControl.getMinHeight();
                    if (sz.Height > pControl.getMaxHeight()) sz.Height = pControl.getMaxHeight();
                    cyFixedRemaining -= sz.Height;
                }

                sz.Width = (0 - szAvailable.Width - rcPadding.Left - rcPadding.Right) > 0 ? 0 : (szAvailable.Width - rcPadding.Left - rcPadding.Right);

                if (sz.Width < pControl.getMinWidth()) sz.Width = pControl.getMinWidth();
                if (sz.Width > pControl.getMaxWidth()) sz.Width = pControl.getMaxWidth();
                Rectangle rcCtrl = new Rectangle(iPosX + rcPadding.Left,
                   iPosY + rcPadding.Top,
                   iPosX + rcPadding.Left + sz.Width - (iPosX + rcPadding.Left),
                   iPosY + sz.Height + rcPadding.Top + rcPadding.Bottom - (iPosY + rcPadding.Top));

                pControl.setPos(rcCtrl);

                iPosY += sz.Height + mChildPadding + rcPadding.Top + rcPadding.Bottom;
                cyNeeded += sz.Height + rcPadding.Top + rcPadding.Bottom;
                szRemaining.Height -= sz.Height + mChildPadding + rcPadding.Bottom;
            }
            cyNeeded += (nEstimateNum - 1) * mChildPadding;

            // 计算滚动条大小
            processScrollbar(rc, 0, cyNeeded);
        }
    }

}
