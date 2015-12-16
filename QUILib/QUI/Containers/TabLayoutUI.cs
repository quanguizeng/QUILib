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
    public class TabLayoutUI : ContainerUI
    {
        public TabLayoutUI()
        {
            mCurSel = -1;
        }
        ~TabLayoutUI()
        {
        }

        public override string getClass()
        {
            return "TabLayoutUI";
        }
        public override ControlUI getInterface(string name)
        {
            if ("TabLayout" == name)
            {
                return this;
            }

            return base.getInterface(name);
        }

        public override bool add(ControlUI pControl)
        {
            bool ret = base.add(pControl);
            if (!ret) return ret;

            if (mCurSel == -1 && pControl.isVisible())
            {
                mCurSel = getItemIndex(pControl);
            }
            else
            {
                pControl.setVisible(false);
            }

            return ret;
        }
        public override bool addAt(ControlUI pControl, int iIndex)
        {
            bool ret = base.addAt(pControl, iIndex);
            if (!ret) return ret;

            if (mCurSel == -1 && pControl.isVisible())
            {
                mCurSel = getItemIndex(pControl);
            }
            else if (mCurSel != -1 && iIndex <= mCurSel)
            {
                mCurSel += 1;
            }
            else
            {
                pControl.setVisible(false);
            }

            return ret;
        }
        public override bool remove(ControlUI pControl)
        {
            if (pControl == null) return false;

            int index = getItemIndex(pControl);
            bool ret = base.remove(pControl);
            if (!ret) return false;

            if (mCurSel == index)
            {
                if (getCount() > 0) getItemAt(0).setVisible(true);
                needParentUpdate();
            }
            else if (mCurSel > index)
            {
                mCurSel -= 1;
            }

            return ret;
        }
        public override void removeAll()
        {
            mCurSel = -1;
            base.removeAll();
            needParentUpdate();
        }
        public int getCurSel()
        {
            return mCurSel;
        }
        public bool selectItem(int iIndex)
        {
            if (iIndex < 0 || iIndex >= mItems.Count) return false;
            if (iIndex == mCurSel) return true;

            mCurSel = iIndex;
            for (int it = 0; it < mItems.Count; it++)
            {
                if (it == iIndex)
                {
                    getItemAt(it).setVisible(true);
                    getItemAt(it).setFocus();
                }
                else getItemAt(it).setVisible(false);
            }
            needParentUpdate();

            if (mManager != null) mManager.setNextTabControl();
            return true;
        }
        public override void setPos(Rectangle rc)
        {
            setPos0(rc);
            rc = mRectItem;

            // Adjust for inset
            rc.X += mRectInset.X;
            rc.Y += mRectInset.Y;
            rc.Width = rc.Right - mRectInset.Right - rc.X;
            rc.Height = rc.Bottom - mRectInset.Bottom - rc.Y;

            for (int it = 0; it < mItems.Count; it++)
            {
                ControlUI pControl = mItems[it];
                if (!pControl.isVisible()) continue;
                if (pControl.isFloat())
                {
                    setFloatPos(it);
                    continue;
                }

                if (it != mCurSel) continue;

                Rectangle rcPadding = pControl.getPadding();
                rc.X += rcPadding.Left;
                rc.Y += rcPadding.Top;
                rc.Width = rc.Right - rcPadding.Right - rc.X;
                rc.Height = rc.Bottom - rcPadding.Bottom - rc.Y;

                Size szAvailable = new Size(rc.Right - rc.Left, rc.Bottom - rc.Top);

                Size sz = pControl.estimateSize(szAvailable);
                if (sz.Width == 0)
                {
                    sz.Width = Math.Max(0, szAvailable.Width);
                }
                if (sz.Width < pControl.getMinWidth()) sz.Width = pControl.getMinWidth();
                if (sz.Width > pControl.getMaxWidth()) sz.Width = pControl.getMaxWidth();

                if (sz.Height == 0)
                {
                    sz.Height = Math.Max(0, szAvailable.Height);
                }
                if (sz.Height < pControl.getMinHeight()) sz.Height = pControl.getMinHeight();
                if (sz.Height > pControl.getMaxHeight()) sz.Height = pControl.getMaxHeight();

                Rectangle rcCtrl = new Rectangle(rc.Left, rc.Top, rc.Left + sz.Width, rc.Top + sz.Height);
                pControl.setPos(rcCtrl);
            }
        }

        protected int mCurSel;
    }
}
