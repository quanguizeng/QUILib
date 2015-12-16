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
    public class OptionUI : ButtonUI
    {
        public OptionUI()
        {
            mSelected = false;
            mSelectedImage = "";
            mForeImage = "";
            mGroupName = "";
        }
        ~OptionUI()
        {

        }

        public override string getClass()
        {
            return "OptionUI";
        }
        public override ControlUI getInterface(string name)
        {
            if ("Option" == name)
            {
                return this;
            }

            return base.getInterface(name);
        }
        public bool isSelected()
        {
            return mSelected;
        }
        public override bool activate()
        {
            if (!base.activate()) return false;
            if (mGroupName != "") selected(true);
            else selected(!mSelected);

            return true;
        }
        public string getSelectedImage()
        {
            return mSelectedImage;
        }
        public void setSelectedImage(string strImage)
        {
            mSelectedImage = strImage;
            invalidate();
        }
        public string getForeImage()
        {
            return mForeImage;
        }
        public void setForeImage(string strImage)
        {
            mForeImage = strImage;
            invalidate();
        }
        public override Size estimateSize(Size szAvailable)
        {
            if (mXYFixed.Height == 0)
            {
                return new Size(mXYFixed.Width, mManager.getDefaultFont().Height + 8);
            }

            return estimateSize0(szAvailable);
        }
        public override void setAttribute(string name, string value)
        {
            if (name == "group") setGroup(value);
            else if (name == "selected") selected(value == "true");
            else if (name == "selectedimage") setSelectedImage(value);
            else if (name == "foreimage")
            {
                setForeImage(value);
            }
            else base.setAttribute(name, value);
        }
        public override void paintStatusImage(ref Graphics graphics, ref Bitmap bitmap)
        {
            mButtonState &= ~(int)PaintFlags.UISTATE_PUSHED;

            if ((mButtonState & (int)PaintFlags.UISTATE_SELECTED) != 0)
            {
                if (mSelectedImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mSelectedImage))
                    {
                        mSelectedImage = "";
                    }
                    else
                    {
                        if (mForeImage != "")
                        {
                            if (!drawImage(ref graphics, ref bitmap, mForeImage)) mForeImage = "";
                        }

                        return;
                    }
                }
            }

            base.paintStatusImage(ref graphics, ref bitmap);

            if (mForeImage != "")
            {
                if (!drawImage(ref graphics, ref bitmap, mForeImage)) mForeImage = "";
            }
        }
        public void setGroup(string groupName)
        {
            if (groupName == "")
            {
                if (mGroupName == "")
                {
                    return;
                }

                mGroupName = "";
            }
            else
            {
                if (mGroupName == groupName)
                {
                    return;
                }
                if (mGroupName != "" && mManager != null)
                {
                    mManager.removeOptionGroup(mGroupName, this);
                }
                mGroupName = groupName;
            }

            if (mGroupName != "")
            {
                if (mManager != null)
                {
                    mManager.addOptionGroup(mGroupName, this);
                }
            }

            selected(mSelected);
        }
        public string getGroup()
        {
            return mGroupName;
        }
        public void selected(bool bSelected)
        {
            if (mSelected == bSelected) return;
            mSelected = bSelected;
            if (mSelected) mButtonState |= (int)ControlFlag.UISTATE_SELECTED;
            else mButtonState &= ~(int)ControlFlag.UISTATE_SELECTED;

            if (mManager != null)
            {
                if (mGroupName != "")
                {
                    if (mSelected)
                    {
                        List<ControlUI> aOptionGroup = mManager.getOptionGroup(mGroupName);
                        for (int i = 0; i < aOptionGroup.Count; i++)
                        {
                            OptionUI pControl = (OptionUI)(aOptionGroup[i]);
                            if (pControl != this)
                            {
                                pControl.selected(false);
                            }
                        }
                        mManager.sendNotify(this, "selectchanged");
                    }
                }
                else
                {
                    mManager.sendNotify(this, "selectchanged");
                }
            }
            invalidate();
        }

        protected bool mSelected;
        protected string mSelectedImage;
        protected string mForeImage;
        protected string mGroupName;
    }
}
