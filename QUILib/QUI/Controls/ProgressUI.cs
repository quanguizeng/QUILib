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
    public class ProgressUI : LabelUI
    {
        public ProgressUI()
        {
            mHorizontal = true;
            mMin = 0;
            mMax = 100;
            mValue = 0;
            mFgImage = "";
            mFgImageModify = "";
            mTextStyle = (int)FormatFlags.DT_SINGLELINE | (int)FormatFlags.DT_CENTER;
            setFixedHeight(12);
        }
        public override string getClass()
        {
            return "ProgressUI";
        }
        public override ControlUI getInterface(string name)
        {
            if (name == "Progress")
            {
                return this;
            }
            return base.getInterface(name);
        }
        public bool isHorizontal()
        {
            return mHorizontal;
        }
        public int getMinValue()
        {
            return mMin;
        }
        public void setMinValue(int nMin)
        {
            mMin = nMin;
            invalidate();
        }
        public int getMaxValue()
        {
            return mMax;
        }
        public void setMaxValue(int nMax)
        {
            mMax = nMax;
            invalidate();
        }
        public int getValue()
        {
            return mValue;
        }

        public void setValue(int nValue)
        {
            mValue = nValue;
            invalidate();
        }
        public string getFgImage()
        {
            return mFgImage;
        }
        public void setFgImage(string strImage)
        {
            if (mFgImage == strImage) return;

            mFgImage = strImage;
            invalidate();
        }
        public override void setAttribute(string name, string value)
        {
            if (name == "fgimage") setFgImage(value);
            else if (name == "hor")
            {
                setHorizontal(value == "true");
            }
            else if (name == "min") setMinValue(int.Parse(value));
            else if (name == "min") setMaxValue(int.Parse(value));
            else if (name == "value") setValue(int.Parse(value));
            else base.setAttribute(name, value);
        }
        public void setHorizontal(bool bHorizontal)
        {
            if (mHorizontal == bHorizontal) return;

            mHorizontal = bHorizontal;
            invalidate();
        }
        public override void paintStatusImage(ref Graphics graphics,ref Bitmap bitmap)
        {
            if (mMax <= mMin) mMax = mMin + 1;
            if (mValue > mMax) mValue = mMax;
            if (mValue < mMin) mValue = mMin;

            if (mFgImage != "")
            {
                Rectangle rc = new Rectangle(0, 0, 0, 0);
                if (mHorizontal)
                {
                    rc.Width = (mValue - mMin) * (mRectItem.Right - mRectItem.Left) / (mMax - mMin) - rc.Left;
                    rc.Height = mRectItem.Bottom - mRectItem.Top - rc.Top;
                }
                else
                {
                    rc.Height = (mRectItem.Bottom - mRectItem.Top) - ((mRectItem.Bottom - mRectItem.Top) * (mMax - mValue) / (mMax - mMin));
                    rc.Width = mRectItem.Right - mRectItem.Left - rc.Left;
                }

                mFgImageModify = "";
                mFgImageModify = string.Format("dest='{0},{1},{2},{3}'", rc.Left, rc.Top, rc.Right, rc.Bottom);

                if (!drawImage(ref graphics, ref bitmap, mFgImage, mFgImageModify))
                {
                    mFgImage = "";
                }
                else
                {
                    return;
                }
            }
        }

        protected bool mHorizontal;
        protected int mMax;
        protected int mMin;
        protected int mValue;

        protected string mFgImage;
        protected string mFgImageModify;
    }
}
