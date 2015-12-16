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
    public class ListBodyUI : VerticalLayoutUI
    {
        public ListBodyUI(ListUI pOwner)
        {
            mOwner = pOwner;
            setInset(new Rectangle(0, 0, 0, 0));
        }
        ~ListBodyUI()
        {

        }
        public override void setScrollPos(Size szPos)
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
                ControlUI pControl = mItems[it2];
                if (!pControl.isVisible()) continue;
                if (pControl.isFloat()) continue;

                rcPos = pControl.getPos();
                int newLeft = rcPos.Left - cx;
                int newRight = rcPos.Right - cx;
                int newTop = rcPos.Top - cy;
                int newBottom = rcPos.Bottom - cy;

                rcPos.X = newLeft;
                rcPos.Width = newRight - newLeft;
                rcPos.Y = newTop;
                rcPos.Height = newBottom - newTop;
                pControl.setPos(rcPos);
            }

            invalidate();

            if (cx != 0 && mOwner != null)
            {
                ListHeaderUI pHeader = mOwner.getHeader();
                if (pHeader == null) return;
                TListInfoUI pInfo = mOwner.getListInfo();
                pInfo.mColumns = Math.Min(pHeader.getCount(), ListUI.UILIST_MAX_COLUMNS);

                if (!pHeader.isVisible()) pHeader.setInternVisible(true);
                for (int i = 0; i < pInfo.mColumns; i++)
                {
                    ControlUI pControl = pHeader.getItemAt(i);
                    if (!pControl.isVisible()) continue;
                    if (pControl.isFloat()) continue;

                    Rectangle rcPos1 = pControl.getPos();
                    int newLeft = rcPos1.Left - cx;
                    int newRight = rcPos1.Right - cx;
                    rcPos1.X = newLeft;
                    rcPos1.Width = newRight - newLeft;
                    pControl.setPos(rcPos1);
                    pInfo.mListColumn[i] = pControl.getPos();
                }
                if (!pHeader.isVisible()) pHeader.setInternVisible(false);
            }
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

            if (mVerticalScrollbar != null && mVerticalScrollbar.isVisible())
            {
                rc.Width = rc.Right - mVerticalScrollbar.getFixedWidth() - rc.Left;
            }
            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
            {
                rc.Height = rc.Bottom - mHorizontalScrollbar.getFixedHeight() - rc.Top;
            }

            // 计算最小大小
            Size szAvailable = new Size(rc.Right - rc.Left, rc.Bottom - rc.Top);

            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
            {
                szAvailable.Width += mHorizontalScrollbar.getScrollRange();
            }

            int cxNeeded = 0;
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

                Rectangle rcPadding = pControl.getPadding();
                sz.Width = Math.Max(sz.Width, 0);
                if (sz.Width < pControl.getMinWidth()) sz.Width = pControl.getMinWidth();
                if (sz.Width > pControl.getMaxWidth()) sz.Width = pControl.getMaxWidth();
                cxNeeded = Math.Max(cxNeeded, sz.Width);
                nEstimateNum++;
            }
            cyFixed += (nEstimateNum - 1) * mChildPadding;

            if (mOwner != null)
            {
                ListHeaderUI pHeader = mOwner.getHeader();
                if (pHeader != null && pHeader.getCount() > 0)
                {
                    cxNeeded = Math.Max(0, pHeader.estimateSize(new Size(rc.Right - rc.Left, rc.Bottom - rc.Top)).Width);
                }
            }

            // Place elements
            int cyNeeded = 0;
            int cyExpand = 0;
            if (nAdjustables > 0) cyExpand = Math.Max(0, (szAvailable.Height - cyFixed) / nAdjustables);
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
                ControlUI pControl = mItems[it2];
                if (!pControl.isVisible())
                {
                    continue;
                }
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
                        sz.Height = Math.Max(0, szRemaining.Height - rcPadding.Bottom - cyFixedRemaining);
                    }
                    if (sz.Height < pControl.getMinHeight())
                    {
                        sz.Height = pControl.getMinHeight();
                    }
                    if (sz.Height > pControl.getMaxHeight())
                    {
                        sz.Height = pControl.getMaxHeight();
                    }
                }
                else
                {
                    if (sz.Height < pControl.getMinHeight()) sz.Height = pControl.getMinHeight();
                    if (sz.Height > pControl.getMaxHeight()) sz.Height = pControl.getMaxHeight();
                    cyFixedRemaining -= sz.Height;
                }

                sz.Width = Math.Max(cxNeeded, szAvailable.Width - rcPadding.Left - rcPadding.Right);

                if (sz.Width < pControl.getMinWidth()) sz.Width = pControl.getMinWidth();
                if (sz.Width > pControl.getMaxWidth()) sz.Width = pControl.getMaxWidth();

                newLeft = iPosX + rcPadding.Left;
                newRight = iPosX + rcPadding.Left + sz.Width;
                newTop = iPosY + rcPadding.Top;
                newBottom = iPosY + sz.Height + rcPadding.Top + rcPadding.Bottom;
                Rectangle rcCtrl = new Rectangle(newLeft,
                    newTop,
                    newRight - newLeft,
                    newBottom - newTop);
                pControl.setPos(rcCtrl);

                iPosY += sz.Height + mChildPadding + rcPadding.Top + rcPadding.Bottom;
                cyNeeded += sz.Height + rcPadding.Top + rcPadding.Bottom;
                szRemaining.Height -= sz.Height + mChildPadding + rcPadding.Bottom;
            }
            cyNeeded += (nEstimateNum - 1) * mChildPadding;

            if (mHorizontalScrollbar != null)
            {
                if (cxNeeded > rc.Right - rc.Left)
                {
                    if (mHorizontalScrollbar.isVisible())
                    {
                        mHorizontalScrollbar.setScrollRange(cxNeeded - (rc.Right - rc.Left));
                    }
                    else
                    {
                        mHorizontalScrollbar.setVisible(true);
                        mHorizontalScrollbar.setScrollRange(cxNeeded - (rc.Right - rc.Left));
                        mHorizontalScrollbar.setScrollPos(0);
                        newBottom = rc.Bottom - mHorizontalScrollbar.getFixedHeight();
                        rc.Height = newBottom - rc.Top;
                    }
                }
                else
                {
                    if (mHorizontalScrollbar.isVisible())
                    {
                        mHorizontalScrollbar.setVisible(false);
                        mHorizontalScrollbar.setScrollRange(0);
                        mHorizontalScrollbar.setScrollPos(0);
                        newBottom = rc.Bottom + mHorizontalScrollbar.getFixedHeight();
                        rc.Height = newBottom - rc.Top;
                    }
                }
            }

            // 计算滚动条大小
            processScrollbar(rc, cxNeeded, cyNeeded);
        }

        protected ListUI mOwner;
    }
}
