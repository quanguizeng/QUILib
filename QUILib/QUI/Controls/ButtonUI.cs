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
    public class ButtonUI : LabelUI
    {
        public ButtonUI()
        {
            mNormalImage = "";
            mHotImage = "";
            mPushedImage = "";
            mFocusedImage = "";
            mDisabledImage = "";
            mTextStyle = (int)FormatFlags.DT_SINGLELINE | (int)FormatFlags.DT_VCENTER | (int)FormatFlags.DT_CENTER;

        }
        ~ButtonUI()
        {
        }

        public override string getClass()
        {
            return "ButtonUI";
        }
        public override ControlUI getInterface(string name)
        {
            if (name == "Button")
            {
                return this;
            }
            return base.getInterface(name);
        }
        public override int getControlFlags()
        {
            return (int)ControlFlag.UIFLAG_TABSTOP | (isEnabled() ? (int)ControlFlag.UIFLAG_SETCURSOR : 0);
        }

        public override bool activate()
        {
            if (!base.activate()) return false;
            if (mManager != null) mManager.sendNotify(this, "click");
            return true;
        }
        public override void eventProc(ref TEventUI param)
        {
            if (!isMouseEnabled() && param.mType > (int)EVENTTYPE_UI.UIEVENT__MOUSEBEGIN && param.mType < (int)EVENTTYPE_UI.UIEVENT__MOUSEEND)
            {
                if (mParent != null) mParent.eventProc(ref param);
                else base.eventProc(ref param);
                return;
            }

            if (param.mType == (int)EVENTTYPE_UI.UIEVENT_SETFOCUS)
            {
                invalidate();
            }
            if (param.mType == (int)EVENTTYPE_UI.UIEVENT_KILLFOCUS)
            {
                invalidate();
            }
            if (param.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONDOWN || param.mType == (int)EVENTTYPE_UI.UIEVENT_DBLCLICK)
            {
                if (isEnabled() == false)
                {
                    return;
                }

                if (mRectItem.Contains(param.mMousePos))
                {
                    mButtonState |= (int)ControlFlag.UISTATE_PUSHED | (int)ControlFlag.UISTATE_CAPTURED;
                    invalidate();
                }
                return;
            }
            if (param.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSEMOVE)
            {
                if (isEnabled() == false)
                {
                    return;
                }
                if ((mButtonState & (int)ControlFlag.UISTATE_CAPTURED) != 0)
                {
                    if (mRectItem.Contains(param.mMousePos))
                    {
                        mButtonState |= (int)ControlFlag.UISTATE_PUSHED;
                    }
                    else
                    {
                        mButtonState &= ~(int)ControlFlag.UISTATE_PUSHED;
                    }
                    invalidate();
                }
                return;
            }
            if (param.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONUP)
            {
                if (isEnabled() == false)
                {
                    return;
                }

                if ((mButtonState & (int)ControlFlag.UISTATE_CAPTURED) != 0)
                {
                    if (mRectItem.Contains(param.mMousePos))
                    {
                        activate();
                    }
                    mButtonState &= ~((int)ControlFlag.UISTATE_PUSHED | (int)ControlFlag.UISTATE_CAPTURED);
                    invalidate();
                }
                return;
            }
            if (param.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSEENTER)
            {
                if (isEnabled())
                {
                    mButtonState |= (int)ControlFlag.UISTATE_HOT;
                    invalidate();
                }
                return;
            }
            if (param.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSELEAVE)
            {
                if (isEnabled())
                {
                    mButtonState &= ~(int)ControlFlag.UISTATE_HOT;
                    invalidate();
                }
                return;
            }
            if (param.mType == (int)EVENTTYPE_UI.UIEVENT_SETCURSOR)
            {
                if (mManager.getPaintWindow().Cursor != Cursors.Hand)
                {
                    mManager.getPaintWindow().Cursor = Cursors.Hand;
                }
                return;
            }
            base.eventProc(ref param);
        }
        public string getNormalImage()
        {
            return mNormalImage;
        }
        public void setNormalImage(string strImage)
        {
            mNormalImage = strImage;
            invalidate();
        }
        public string getHotImage()
        {
            return mHotImage;
        }
        public void setHotImage(string strImage)
        {
            mHotImage = strImage;
            invalidate();
        }
        public string getPushedImage()
        {
            return mPushedImage;
        }
        public void setPushedImage(string strImage)
        {
            mPushedImage = strImage;
            invalidate();
        }
        public string getFocusedImage()
        {
            return mFocusedImage;
        }
        public void setFocusedImage(string strImage)
        {
            mFocusedImage = strImage;
            invalidate();
        }
        public string getDisabledImage()
        {
            return mDisabledImage;
        }
        public void setDisabledImage(string strImage)
        {
            mDisabledImage = strImage;
            invalidate();
        }
        public override Size estimateSize(Size szAvailable)
        {
            if (mXYFixed.Height == 0) return new Size(mXYFixed.Width, mManager.getDefaultFont().Height + 8);
            return estimateSize0(szAvailable);
        }
        public override void setAttribute(string name, string value)
        {
            if (name == "normalimage") setNormalImage(value);
            else if (name == "hotimage") setHotImage(value);
            else if (name == "pushedimage") setPushedImage(value);
            else if (name == "focusedimage") setFocusedImage(value);
            else if (name == "disabledimage") setDisabledImage(value);
            else base.setAttribute(name, value);
        }
        public override void paintStatusImage(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (isFocused()) mButtonState |= (int)ControlFlag.UISTATE_FOCUSED;
            else mButtonState &= ~(int)ControlFlag.UISTATE_FOCUSED;
            if (!isEnabled()) mButtonState |= (int)ControlFlag.UISTATE_DISABLED;
            else mButtonState &= ~(int)ControlFlag.UISTATE_DISABLED;

            if ((mButtonState & (int)ControlFlag.UISTATE_DISABLED) != 0)
            {
                if (mDisabledImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mDisabledImage)) mDisabledImage = "";
                    else return;
                }
            }
            else if ((mButtonState & (int)ControlFlag.UISTATE_PUSHED) != 0)
            {
                if (mPushedImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mPushedImage)) mPushedImage = "";
                    else return;
                }
            }
            else if ((mButtonState & (int)ControlFlag.UISTATE_HOT) != 0)
            {
                if (mHotImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mHotImage)) mHotImage = "";
                    else return;
                }
            }
            else if ((mButtonState & (int)ControlFlag.UISTATE_FOCUSED) != 0)
            {
                if (mFocusedImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mFocusedImage)) mFocusedImage = "";
                    else return;
                }
            }

            if (mNormalImage != "")
            {
                if (!drawImage(ref graphics, ref bitmap, mNormalImage)) mNormalImage = "";
                else return;
            }
        }

        protected int mButtonState;
        protected string mNormalImage;
        protected string mHotImage;
        protected string mPushedImage;
        protected string mFocusedImage;
        protected string mDisabledImage;
    }
}
