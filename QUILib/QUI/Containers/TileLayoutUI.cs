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
    public class TileLayoutUI : ContainerUI
    {
        public TileLayoutUI()
        {
            mColumns = 2;
        }
        ~TileLayoutUI()
        {
        }
        public override string getClass()
        {
            return "TileLayoutUI";
        }

        public override ControlUI getInterface(string name)
        {
            if (name == "TileLayout")
            {
                return this;
            }

            return base.getInterface(name);
        }

        public int getColumns()
        {
            return mColumns;
        }
        public void setColumns(int nCols)
        {
            if (nCols <= 0) return;
            mColumns = nCols;
            needUpdate();
        }
        public override void setAttribute(string name, string value)
        {
            if (name == "columns") setColumns(int.Parse(value));
            else base.setAttribute(name, value);
        }

        public override void setPos(Rectangle rc)
        {
            setPos0(rc);

            rc = mRectItem;

            // Adjust for inset
            rc.X += mRectInset.Left;
            rc.Width = rc.Right - mRectInset.Right - rc.X;
            rc.Y += mRectInset.Top;
            rc.Height = rc.Bottom - mRectInset.Bottom - rc.Y;

            if (mItems.Count == 0)
            {
                processScrollbar(rc, 0, 0);
                return;
            }

            if (mVerticalScrollbar != null && mVerticalScrollbar.isVisible())
            {
                rc.Width = rc.Right - mVerticalScrollbar.getFixedWidth() - rc.Left;
            }
            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
            {
                rc.Height = rc.Bottom - mHorizontalScrollbar.getFixedHeight() - rc.Top;
            }

            // Position the elements
            int cyNeeded = 0;
            int cxWidth = (rc.Right - rc.Left) / mColumns;
            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
            {
                cxWidth = (rc.Right - rc.Left + mHorizontalScrollbar.getScrollRange()) / mColumns;
            }

            int cyHeight = 0;
            int iCount = 0;
            Point ptTile = new Point(rc.Left, rc.Top);
            if (mVerticalScrollbar != null && mVerticalScrollbar.isVisible())
            {
                ptTile.Y -= mVerticalScrollbar.getScrollPos();
            }
            int iPosX = rc.Left;
            if (mHorizontalScrollbar != null && mHorizontalScrollbar.isVisible())
            {
                iPosX -= mHorizontalScrollbar.getScrollPos();
                ptTile.X = iPosX;
            }
            for (int it1 = 0; it1 < mItems.Count; it1++)
            {
                ControlUI pControl = mItems[it1];
                if (!pControl.isVisible()) continue;
                if (pControl.isFloat())
                {
                    setFloatPos(it1);
                    continue;
                }
                Rectangle rcPadding;
                Size szAvailable;
                Size szTile;
                // Determine size
                Rectangle rcTile = new Rectangle(ptTile.X, ptTile.Y, cxWidth, 0);
                if ((iCount % mColumns) == 0)
                {
                    int iIndex = iCount;
                    for (int it2 = it1; it2 < mItems.Count; it2++)
                    {
                        ControlUI pLineControl = mItems[it2];
                        if (!pLineControl.isVisible()) continue;
                        if (pLineControl.isFloat()) continue;

                        rcPadding = pLineControl.getPadding();
                        szAvailable = new Size(rcTile.Right - rcTile.Left - rcPadding.Left - rcPadding.Right, 9999);
                        if (iIndex == iCount || (iIndex + 1) % mColumns == 0)
                        {
                            szAvailable.Width -= mChildPadding / 2;
                        }
                        else
                        {
                            szAvailable.Width -= mChildPadding;
                        }

                        if (szAvailable.Width < pControl.getMinWidth()) szAvailable.Width = pControl.getMinWidth();
                        if (szAvailable.Width > pControl.getMaxWidth()) szAvailable.Width = pControl.getMaxWidth();

                        szTile = pLineControl.estimateSize(szAvailable);
                        if (szTile.Width < pControl.getMinWidth()) szTile.Width = pControl.getMinWidth();
                        if (szTile.Width > pControl.getMaxWidth()) szTile.Width = pControl.getMaxWidth();
                        if (szTile.Height < pControl.getMinHeight()) szTile.Height = pControl.getMinHeight();
                        if (szTile.Height > pControl.getMaxHeight()) szTile.Height = pControl.getMaxHeight();

                        cyHeight = Math.Max(cyHeight, szTile.Height + rcPadding.Top + rcPadding.Bottom);
                        if ((++iIndex % mColumns) == 0) break;
                    }
                }

                rcPadding = pControl.getPadding();

                rcTile.X += rcPadding.X + mChildPadding / 2;
                rcTile.Width = rcTile.Right - (rcPadding.Right + mChildPadding / 2) - rcTile.X;

                if ((iCount % mColumns) == 0)
                {
                    rcTile.X -= mChildPadding / 2;
                }

                if (((iCount + 1) % mColumns) == 0)
                {
                    rcTile.Width = rcTile.Right + mChildPadding / 2 - rcTile.X;
                }

                // Set position
                rcTile.Y = ptTile.Y + rcPadding.Top;
                rcTile.Height = ptTile.Y + cyHeight - rcTile.Y;

                szAvailable = new Size(rcTile.Right - rcTile.Left, rcTile.Bottom - rcTile.Top);
                szTile = pControl.estimateSize(szAvailable);
                if (szTile.Width == 0) szTile.Width = szAvailable.Width;
                if (szTile.Height == 0) szTile.Height = szAvailable.Height;
                if (szTile.Width < pControl.getMinWidth()) szTile.Width = pControl.getMinWidth();
                if (szTile.Width > pControl.getMaxWidth()) szTile.Width = pControl.getMaxWidth();
                if (szTile.Height < pControl.getMinHeight()) szTile.Height = pControl.getMinHeight();
                if (szTile.Height > pControl.getMaxHeight()) szTile.Height = pControl.getMaxHeight();
                int newX, newY, newWidth, newHeight;
                {
                    newX = (rcTile.Left + rcTile.Right - szTile.Width) / 2;
                    if(newX < rcTile.Left)
                    {
                        newX = rcTile.Left;
                    }
                    newY = (rcTile.Top + rcTile.Bottom - szTile.Height) / 2;
                    newWidth = szTile.Width;
                    newHeight = szTile.Height;
                }
                Rectangle rcPos = new Rectangle(newX, newY,
                    newWidth, newHeight);
                pControl.setPos(rcPos);

                if ((++iCount % mColumns) == 0)
                {
                    ptTile.X = iPosX;
                    ptTile.Y += cyHeight + mChildPadding;
                    cyHeight = 0;
                }
                else
                {
                    ptTile.X += cxWidth;
                }
                cyNeeded = rcTile.Bottom - rc.Top;
                if (mVerticalScrollbar != null && mVerticalScrollbar.isVisible()) cyNeeded += mVerticalScrollbar.getScrollPos();
            }

            // 计算滚动条大小
            processScrollbar(rc, 0, cyNeeded);
        }

        protected int mColumns;
    }
}
