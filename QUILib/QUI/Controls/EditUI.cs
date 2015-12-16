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
    public class EditUI : LabelUI
    {
        public EditUI()
        {
            mWindow = null;
            mMaxChar = 255;
            mReadOnly = false;
            mPasswordChar = '*';
            mNormalImage = "";
            mHotImage = "";
            mFocusedImage = "";
            mDisabledImage = "";

        }
        ~EditUI()
        {
        }

        public override string getClass()
        {
            return "EditUI";
        }
        public override ControlUI getInterface(string name)
        {
            if (name == "Edit")
            {
                return this;
            }
            return base.getInterface(name);
        }
        public override int getControlFlags()
        {
            return base.getControlFlags();
        }

        public override void eventProc(ref TEventUI newEvent)
        {
            if (!isMouseEnabled() && newEvent.mType > (int)EVENTTYPE_UI.UIEVENT__MOUSEBEGIN && newEvent.mType < (int)EVENTTYPE_UI.UIEVENT__MOUSEEND)
            {
                if (mParent != null) mParent.eventProc(ref newEvent);
                else base.eventProc(ref newEvent);
                return;
            }

            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SETCURSOR && isEnabled())
            {
                if (mManager.getPaintWindow().Cursor != Cursors.IBeam)
                {
                    mManager.getPaintWindow().Cursor = Cursors.IBeam;
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_WINDOWSIZE)
            {
                ControlUI ctlUI = null;
                if (mWindow != null) mManager.setFocus(ref ctlUI);
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SCROLLWHEEL)
            {
                ControlUI ctlUI = null;
                if (mWindow != null) mManager.setFocus(ref ctlUI);
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SETFOCUS && isEnabled())
            {
                if (mWindow != null) return;

                mWindow = new EditWnd();
                mWindow.init(this);
                invalidate();
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_KILLFOCUS && isEnabled())
            {
                invalidate();
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONDOWN || newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_DBLCLICK)
            {
                if (isEnabled())
                {
                    if (isFocused() && mWindow == null)
                    {
                        mWindow = new EditWnd();
                        mWindow.init(this);
                    }
                    else if (mWindow == null)
                    {
                        //POINT pt = event.ptMouse;
                        //pt.x -= m_rcItem.left + m_rcTextPadding.left;
                        //pt.y -= m_rcItem.top + m_rcTextPadding.top;
                        //::SendMessage(*m_pWindow, WM_LBUTTONDOWN, event.wParam, MAKELPARAM(pt.x, pt.y));
                    }
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSEMOVE)
            {
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONUP)
            {
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSEENTER)
            {
                if (isEnabled())
                {
                    mButtonState |= (int)ControlFlag.UISTATE_HOT;
                    invalidate();
                }
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSELEAVE)
            {
                if (isEnabled())
                {
                    mButtonState &= (int)ControlFlag.UISTATE_HOT;
                    invalidate();
                }
                return;
            }
            base.eventProc(ref newEvent);
        }
        public override void setEnabled(bool enable = true)
        {
            base.setEnabled(enable);
            if (!isEnabled())
            {
                mButtonState = 0;
            }
        }
        public override void setText(string text)
        {
            mText = text;
            if (mManager != null) mManager.sendNotify(this, "textchanged");
            invalidate();
        }
        public void setMaxChar(int max)
        {
            mMaxChar = max;
            if (mWindow != null)
            {
                mWindow.MaxLength = mMaxChar;
            }
        }
        public int getMaxChar()
        {
            return mMaxChar;
        }
        public void setReadOnly(bool readOnly)
        {
            if (mReadOnly == readOnly) return;

            mReadOnly = readOnly;
            if (mWindow != null)
            {
                mWindow.ReadOnly = readOnly;
            }

            invalidate();
        }
        public bool isReadOnly()
        {
            return mReadOnly;
        }
        public void setPasswordMode(bool passwordMode)
        {
            if (mPasswordMode == passwordMode) return;
            mPasswordMode = passwordMode;
            invalidate();
        }
        public bool isPasswordMode()
        {
            return mPasswordMode;
        }

        public void setPasswordChar(char passwordChar)
        {
            if (mPasswordChar == passwordChar) return;
            mPasswordChar = passwordChar;
            if (mWindow != null)
            {
                mWindow.PasswordChar = mPasswordChar;
            }
            invalidate();
        }
        public char getPasswordChar()
        {
            return mPasswordChar;
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
        public override void setVisible(bool visible)
        {
            base.setVisible(visible);
            if (!isVisible() && mWindow != null)
            {
                ControlUI ctl = null;
                mManager.setFocus(ref ctl);
            }
        }
        public override void setInternVisible(bool visible)
        {
            if (!isVisible() && mWindow != null)
            {
                ControlUI ctl = null;
                mManager.setFocus(ref ctl);
            }
        }
        public override Size estimateSize(Size szAvailable)
        {
            if (mXYFixed.Height == 0) return new Size(mXYFixed.Width, mManager.getDefaultFont().Height + 6);
            return estimateSize0(szAvailable);
        }
        public override void setAttribute(string name, string value)
        {
            if (name == "readonly")
            {
                setReadOnly(value == "true");
            }
            else if (name == "password")
            {
                setPasswordMode(value == "true");
            }
            else if (name == "normalimage")
            {
                setNormalImage(value);
            }
            else if (name == "hotimage")
            {
                setHotImage(value);
            }
            else if (name == "focusedimage")
            {
                setFocusedImage(value);
            }
            else if (name == "disabledimage")
            {
                setDisabledImage(value);
            }
            else
            {
                base.setAttribute(name, value);
            }
        }
        public override void paintStatusImage(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (isFocused()) mButtonState |= (int)PaintFlags.UISTATE_FOCUSED;
            else mButtonState &= ~(int)PaintFlags.UISTATE_FOCUSED;
            if (!isEnabled()) mButtonState |= (int)PaintFlags.UISTATE_DISABLED;
            else mButtonState &= ~(int)PaintFlags.UISTATE_DISABLED;

            if ((mButtonState & (int)PaintFlags.UISTATE_DISABLED) != 0)
            {
                if (mDisabledImage != "")
                {
                    if (drawImage(ref graphics, ref bitmap, mDisabledImage)) mDisabledImage = "";
                    else return;
                }
            }
            else if ((mButtonState & (int)PaintFlags.UISTATE_FOCUSED) != 0)
            {
                if (mFocusedImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mFocusedImage)) mFocusedImage = "";
                    else return;
                }
            }
            else if ((mButtonState & (int)PaintFlags.UISTATE_HOT) != 0)
            {
                if (mHotImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mHotImage)) mHotImage = "";
                    else return;
                }
            }

            if (mNormalImage != "")
            {
                if (!drawImage(ref graphics, ref bitmap, mNormalImage)) mNormalImage = "";
                else return;
            }

            uint dwBorderColor = 0xFF4EA0D1;
            int nBorderSize = 1;
            if ((mButtonState & (int)PaintFlags.UISTATE_HOT) != 0 || (mButtonState & (int)PaintFlags.UISTATE_FOCUSED) != 0)
            {
                dwBorderColor = 0xFF85E4FF;
                nBorderSize = 2;
            }

            RenderEngine.drawRect(ref graphics, ref bitmap, ref mRectItem, nBorderSize, (int)dwBorderColor);
        }
        public override void paintText(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (mTextColor.ToArgb() == 0) mTextColor = mManager.getDefaultFontColor();
            if (mDisabledTextColor.ToArgb() == 0) mDisabledTextColor = mManager.getDefaultDisabledColor();

            if (mText == "") return;

            string sText = mText;
            if (mPasswordMode)
            {
                sText = "";
                for (int i = 0; i < mText.Length; i++)
                {
                    sText += mPasswordChar;
                }
            }

            Rectangle rc = mRectItem;
            rc.X += mRectTextPadding.X;
            rc.Width = rc.Right - mRectTextPadding.Right - rc.X;

            rc.Y += mRectTextPadding.Y;
            rc.Height = rc.Bottom - mRectTextPadding.Bottom - rc.Y;

            if (isEnabled())
            {
                RenderEngine.drawText(ref graphics, ref bitmap, ref mManager,ref rc, sText, mTextColor.ToArgb(), mFont, (int)FormatFlags.DT_SINGLELINE | mTextStyle);
            }
            else
            {
                RenderEngine.drawText(ref graphics, ref bitmap, ref mManager, ref rc, sText, mDisabledTextColor.ToArgb(), mFont, (int)FormatFlags.DT_SINGLELINE | mTextStyle);
            }
        }
        public bool setEditWnd(EditWnd wnd)
        {
            mWindow = wnd;

            return true;
        }

        protected int mMaxChar;
        protected bool mReadOnly;
        protected bool mPasswordMode;
        protected Char mPasswordChar;
        protected int mButtonState;
        protected string mNormalImage;
        protected string mHotImage;
        protected string mFocusedImage;
        protected string mDisabledImage;
        protected EditWnd mWindow;
    }
}
