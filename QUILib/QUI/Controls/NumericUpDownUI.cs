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
using System.Diagnostics;

namespace QUI
{
    public class NumericUpDownUI : LabelUI
    {
        public NumericUpDownUI()
        {
            mMinValue = 0;
            mMaxValue = 100;
            mCurValue = 0;
            mOwner = null;
            mButton1State = 0;
            mButton2State = 0;

            mXYFixed.Width = DEFAULT_SCROLLBAR_SIZE;


            mBkNormalImage = "";
            mBkHotImage = "";
            mBkPushedImage = "";
            mBkDisabledImage = "";

            mButton1NormalImage = "";
            mButton1HotImage = "";
            mButton1PushedImage = "";
            mButton1DisabledImage = "";

            mButton2NormalImage = "";
            mButton2HotImage = "";
            mButton2PushedImage = "";
            mButton2DisabledImage = "";

            mImageModify = "";

            mMaxChar = 255;
        }
        ~NumericUpDownUI()
        {

        }
        public override string getClass()
        {
            return "NumericUpDownUI";
        }
        public override ControlUI getInterface(string name)
        {
            if (name == "NumericUpDown")
            {
                return this;
            }
            return base.getInterface(name);
        }
        public ContainerUI getOwner()
        {
            return mOwner;
        }
        public void setOwner(ContainerUI owner)
        {
            mOwner = owner;
        }
        public override void setEnabled(bool enable = true)
        {
            base.setEnabled(enable);
            if (isEnabled() == false)
            {
                mButton1State = 0;
                mButton2State = 0;
            }
        }
        public double getMaxValue()
        {
            return mMaxValue;
        }
        public double getValue()
        {
            return mCurValue;
        }
        public void setValue(double value, bool notify = true)
        {
            if (value == mCurValue)
            {
                return;
            }

            mCurValue = value;
            if (value < mMinValue)
            {
                mCurValue = mMinValue;
            }
            if (value > mMaxValue)
            {
                mCurValue = mMaxValue;
            }

            if (notify)
            {
                mManager.sendNotify(this, "valuechanged");
            }
        }
        public string getButton1NormalImage()
        {
            return mButton1NormalImage;
        }
        public void setButton1NormalImage(string strImage)
        {
            mButton1NormalImage = strImage;
            invalidate();
        }
        public string getButton1HotImage()
        {
            return mButton1HotImage;
        }
        public void setButton1HotImage(string strImage)
        {
            mButton1HotImage = strImage;
            invalidate();
        }
        public string getButton1PushedImage()
        {
            return mButton1PushedImage;
        }
        public void setButton1PushedImage(string strImage)
        {
            mButton1PushedImage = strImage;
            invalidate();
        }
        public string getButton1DisabledImage()
        {
            return mButton1DisabledImage;
        }
        public void setButton1DisabledImage(string strImage)
        {
            mButton1DisabledImage = strImage;
            invalidate();
        }
        public string getButton2NormalImage()
        {
            return mButton2NormalImage;
        }
        public void setButton2NormalImage(string strImage)
        {
            mButton2NormalImage = strImage;
            invalidate();
        }
        public string getButton2HotImage()
        {
            return mButton2HotImage;
        }
        public void setButton2HotImage(string strImage)
        {
            mButton2HotImage = strImage;
            invalidate();
        }
        public string getButton2PushedImage()
        {
            return mButton2PushedImage;
        }
        public void setButton2PushedImage(string strImage)
        {
            mButton2PushedImage = strImage;
            invalidate();
        }
        public string getButton2DisabledImage()
        {
            return mButton2DisabledImage;
        }
        public void setButton2DisabledImage(string strImage)
        {
            mButton2DisabledImage = strImage;
            invalidate();
        }
        public string getBkNormalImage()
        {
            return mBkNormalImage;
        }
        public void setBkNormalImage(string strImage)
        {
            mBkNormalImage = strImage;
            invalidate();
        }
        public string getBkHotImage()
        {
            return mBkHotImage;
        }
        public void seBkHotImage(string strImage)
        {
            mBkHotImage = strImage;
            invalidate();
        }
        public string getBkPushedImage()
        {
            return mBkPushedImage;
        }
        public void setBkPushedImage(string strImage)
        {
            mBkPushedImage = strImage;
            invalidate();
        }
        public string gBkDisabledImage()
        {
            return mBkDisabledImage;
        }
        public void setBkDisabledImage(string strImage)
        {
            mBkDisabledImage = strImage;
            invalidate();
        }
        public override void setPos(Rectangle rc)
        {
            base.setPos(rc);

            rc = mRectItem;
            {
                int cx = rc.Right - rc.Left - 2 * mXYFixed.Height;
                if (cx > mXYFixed.Height)
                {
                    mRectButton1.X = rc.Right - mXYFixed.Height / 2;
                    mRectButton1.Width = mXYFixed.Height / 2;
                    mRectButton1.Y = rc.Top;
                    mRectButton1.Height = mXYFixed.Height / 2;


                    mRectButton2.X = rc.Right - mXYFixed.Height / 2;
                    mRectButton2.Width = mXYFixed.Height / 2;
                    mRectButton2.Y = rc.Top + mXYFixed.Height / 2;
                    mRectButton2.Height = mXYFixed.Height / 2;
                }
                else
                {
                    int cxButton = (rc.Right - rc.Left) / 2;
                    if (cxButton > mXYFixed.Height) cxButton = mXYFixed.Height;

                    mRectButton1.X = rc.Right - cxButton / 2;
                    mRectButton1.Width = cxButton / 2;
                    mRectButton1.Y = rc.Top;
                    mRectButton1.Height = mXYFixed.Height / 2;


                    mRectButton2.X = rc.Right - cxButton / 2;
                    mRectButton2.Width = cxButton / 2;
                    mRectButton2.Y = rc.Top + mXYFixed.Height / 2;
                    mRectButton2.Height = mXYFixed.Height / 2;
                }
            }
        }
        public override void eventProc(ref TEventUI newEvent)
        {
            if (!isMouseEnabled() && newEvent.mType > (int)EventTypeUI.UIEVENT__MOUSEBEGIN && newEvent.mType < (int)EventTypeUI.UIEVENT__MOUSEEND)
            {
                if (mOwner != null) mOwner.eventProc(ref newEvent);
                else base.eventProc(ref newEvent);
                return;
            }

            if (newEvent.mType == (int)EventTypeUI.UIEVENT_KILLFOCUS)
            {
                return;
            }
            if (newEvent.mType == (int)EventTypeUI.UIEVENT_BUTTONDOWN || newEvent.mType == (int)EventTypeUI.UIEVENT_DBLCLICK)
            {
                if (!isEnabled()) return;

                if (mRectButton1.Contains(newEvent.mMousePos))
                {
                    mButton1State |= (int)PaintFlags.UISTATE_PUSHED;
                    setValue(mCurValue + 1);
                }
                else if (mRectButton2.Contains(newEvent.mMousePos))
                {
                    mButton2State |= (int)PaintFlags.UISTATE_PUSHED;
                    setValue(mCurValue - 1);
                }
                else if (isFocused() && mWindow == null)
                {
                    mWindow = new NumericUpDownWnd();
                    mWindow.init(this);
                }

                return;
            }
            if (newEvent.mType == (int)EventTypeUI.UIEVENT_BUTTONUP)
            {
                if ((mButton1State & (int)PaintFlags.UISTATE_PUSHED) != 0)
                {
                    mButton1State &= ~(int)PaintFlags.UISTATE_PUSHED;
                    invalidate();
                }
                else if ((mButton2State & (int)PaintFlags.UISTATE_PUSHED) != 0)
                {
                    mButton2State &= ~(int)PaintFlags.UISTATE_PUSHED;
                    invalidate();
                }
                return;
            }
            if (newEvent.mType == (int)EventTypeUI.UIEVENT_MOUSEENTER)
            {
                if (isEnabled())
                {
                    mButton1State |= (int)PaintFlags.UISTATE_HOT;
                    mButton2State |= (int)PaintFlags.UISTATE_HOT;
                    invalidate();
                }
                return;
            }
            if (newEvent.mType == (int)EventTypeUI.UIEVENT_MOUSELEAVE)
            {
                if (isEnabled())
                {
                    mButton1State &= ~(int)PaintFlags.UISTATE_HOT;
                    mButton2State &= ~(int)PaintFlags.UISTATE_HOT;
                    invalidate();
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SCROLLWHEEL)
            {
                ControlUI ctlUI = null;
                if (mWindow != null)
                {
                    closeEditWnd();
                    mManager.setFocus(ref ctlUI);
                }
            }

            if (mOwner != null)
            {
                mOwner.eventProc(ref newEvent);
            }
            else
            {
                base.eventProc(ref newEvent);
            }
        }
        public void setBkHotImage(string strImage)
        {
            if (mBkHotImage == strImage)
            {
                return;
            }
            mBkHotImage = strImage;
            invalidate();
        }
        public override void setAttribute(string name, string value)
        {
            if (name == "button1normalimage")
            {
                setButton1NormalImage(value);
            }
            else if (name == "button1hotimage")
            {
                setButton1HotImage(value);
            }
            else if (name == "button1pushedimage")
            {
                setButton1PushedImage(value);
            }
            else if (name == "button1disabledimage")
            {
                setButton1DisabledImage(value);
            }
            else if (name == "button2normalimage")
            {
                setButton2NormalImage(value);
            }
            else if (name == "button2hotimage")
            {
                setButton2HotImage(value);
            }
            else if (name == "button2pushedimage")
            {
                setButton2PushedImage(value);
            }
            else if (name == "button2disabledimage")
            {
                setButton2DisabledImage(value);
            }
            else if (name == "bknormalimage")
            {
                setBkNormalImage(value);
            }
            else if (name == "bkhotimage")
            {
                setBkHotImage(value);
            }
            else if (name == "bkpushedimage")
            {
                setBkPushedImage(value);
            }
            else if (name == "bkdisabledimage")
            {
                setBkDisabledImage(value);
            }
            else if (name == "max")
            {
            }
            else if (name == "value")
            {
                setValue(double.Parse(value));
            }
            else if(name == "disabledcolor")
            {
                value = value.TrimStart('#');
                Color color = Color.FromArgb(Convert.ToInt32(value, 16));

                setDisabledColor(color);
            }
            else
            {
                base.setAttribute(name, value);
            }
        }
        public override void doPaint(ref Graphics graphics, ref Bitmap bitmap, Rectangle rectPaint)
        {
            base.doPaint(ref graphics, ref bitmap, rectPaint);

            if (rectPaint.IntersectsWith(mRectItem) == false)
            {
                return;
            }

            paintBk(ref graphics, ref bitmap);
            paintButton1(ref graphics, ref bitmap);
            paintButton2(ref graphics, ref bitmap);
            paintText(ref graphics, ref bitmap);
            paintStatusImage(ref graphics, ref bitmap);
            paintDisabledColor(ref graphics, ref bitmap);
        }
        public void paintBk(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (mBkNormalImage != "")
            {
                if (!drawImage(ref graphics, ref bitmap, mBkNormalImage)) mBkNormalImage = "";
                else return;
            }
        }
        void paintButton1(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (!isEnabled()) mButton1State |= (int)ControlFlag.UISTATE_DISABLED;
            else mButton1State &= ~(int)ControlFlag.UISTATE_DISABLED;

            mImageModify = "";
            mImageModify = string.Format("dest='{0},{1},{2},{3}'",
                mRectButton1.Left - mRectItem.Left,
                mRectButton1.Top - mRectItem.Top,
                mRectButton1.Right - mRectItem.Left,
                mRectButton1.Bottom - mRectItem.Top);

            if ((mButton1State & (int)ControlFlag.UISTATE_DISABLED) != 0)
            {
                if (mButton1DisabledImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mButton1DisabledImage, mImageModify)) mButton1DisabledImage = "";
                    else return;
                }
            }
            else if ((mButton1State & (int)ControlFlag.UISTATE_PUSHED) != 0)
            {
                if (mButton1PushedImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mButton1PushedImage, mImageModify)) mButton1PushedImage = "";
                    else return;
                }
            }
            else if ((mButton1State & (int)ControlFlag.UISTATE_HOT) != 0)
            {
                if (mButton1HotImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mButton1HotImage, mImageModify)) mButton1HotImage = "";
                    else return;
                }
            }

            if (mButton1NormalImage != "")
            {
                if (!drawImage(ref graphics, ref bitmap, mButton1NormalImage, mImageModify)) mButton1NormalImage = "";
                else return;
            }

            uint dwBorderColor = 0xFF85E4FF;
            int nBorderSize = 2;
            RenderEngine.drawRect(ref graphics, ref bitmap, ref mRectButton1, nBorderSize, (int)dwBorderColor);
        }
        void paintButton2(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (!isEnabled()) mButton2State |= (int)ControlFlag.UISTATE_DISABLED;
            else mButton2State &= ~(int)ControlFlag.UISTATE_DISABLED;

            mImageModify = "";
            mImageModify = string.Format("dest='{0},{1},{2},{3}'",
                mRectButton2.Left - mRectItem.Left,
                mRectButton2.Top - mRectItem.Top,
                mRectButton2.Right - mRectItem.Left,
                mRectButton2.Bottom - mRectItem.Top);

            if ((mButton2State & (int)ControlFlag.UISTATE_DISABLED) != 0)
            {
                if (mButton2DisabledImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mButton2DisabledImage, mImageModify)) mButton2DisabledImage = "";
                    else return;
                }
            }
            else if ((mButton2State & (int)ControlFlag.UISTATE_PUSHED) != 0)
            {
                if (mButton2PushedImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mButton2PushedImage, mImageModify)) mButton2PushedImage = "";
                    else return;
                }
            }
            else if ((mButton2State & (int)ControlFlag.UISTATE_HOT) != 0)
            {
                if (mButton2HotImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mButton2HotImage, mImageModify)) mButton2HotImage = "";
                    else return;
                }
            }

            if (mButton2NormalImage != "")
            {
                if (!drawImage(ref graphics, ref bitmap, mButton2NormalImage, mImageModify)) mButton2NormalImage = "";
                else return;
            }

            uint dwBorderColor = 0xFF85E4FF;
            int nBorderSize = mBorderSize;
            if (nBorderSize != 0)
            {
                RenderEngine.drawRect(ref graphics, ref bitmap, ref mRectButton2, nBorderSize, (int)dwBorderColor);
            }
        }
        public override void paintText(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (mTextColor.ToArgb() == 0)
            {
                mTextColor = mManager.getDefaultFontColor();
            }

            string sText = mCurValue.ToString();

            Rectangle rc = mRectItem;
            rc.X += mRectTextPadding.X;
            rc.Width = rc.Right - mRectTextPadding.Right - rc.X - 12;

            rc.Y += mRectTextPadding.Y;
            rc.Height = rc.Bottom - mRectTextPadding.Bottom - rc.Y;

            RenderEngine.drawText(ref graphics, ref bitmap, ref mManager, ref rc, sText, mTextColor.ToArgb(), mFont, (int)FormatFlags.DT_SINGLELINE | mTextStyle);
        }
        public bool setNumericUpDownWnd(NumericUpDownWnd wnd)
        {
            mWindow = wnd;

            return true;
        }
        public int getMaxChar()
        {
            return mMaxChar;
        }
        public Rectangle getRectButton1()
        {
            return mRectButton1;
        }
        public Rectangle getRectButton2()
        {
            return mRectButton2;
        }
        public override void paintStatusImage(ref Graphics graphics, ref Bitmap bitmap)
        {
            uint dwBorderColor = 0xFF4EA0D1;
            int nBorderSize = 1;

            RenderEngine.drawRect(ref graphics, ref bitmap, ref mRectItem, nBorderSize, (int)dwBorderColor);
        }
        public void closeEditWnd()
        {
            if (mWindow == null)
            {
                return;
            }
            object obj = null;
            bool result = false;
            mWindow.onKillFocus(0, ref obj, ref obj, ref result);
        }
        public void paintDisabledColor(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (mDisabledColor != null && mDisabledColor.ToArgb() != 0 && isEnabled() == false)
            {
                RenderEngine.drawColor(ref graphics, ref bitmap, ref mRectPaint, mDisabledColor.ToArgb());
            }
        }
        public void setDisabledColor(Color color)
        {
            mDisabledColor = color;
        }
        public double getMinValue()
        {
            return mMinValue;
        }
        public void setRange(double min = 0, double max = 100)
        {
            min = (int)min;
            max = (int)max;

            mMinValue = min < max ? min : max;
            mMaxValue = min > max ? min : max;

            setValue(mCurValue);
        }

        protected const int DEFAULT_SCROLLBAR_SIZE = 16;

        protected double mMaxValue;
        protected double mMinValue;
        protected double mCurValue;
        protected ContainerUI mOwner;

        protected string mBkNormalImage;
        protected string mBkHotImage;
        protected string mBkPushedImage;
        protected string mBkDisabledImage;

        protected Rectangle mRectButton1;
        protected int mButton1State;
        protected string mButton1NormalImage;
        protected string mButton1HotImage;
        protected string mButton1PushedImage;
        protected string mButton1DisabledImage;

        protected Rectangle mRectButton2;
        protected int mButton2State;
        protected string mButton2NormalImage;
        protected string mButton2HotImage;
        protected string mButton2PushedImage;
        protected string mButton2DisabledImage;

        protected string mImageModify;
        protected NumericUpDownWnd mWindow;

        protected int mMaxChar;
        protected Color mDisabledColor;

    }
}
