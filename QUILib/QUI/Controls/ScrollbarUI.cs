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
    public class ScrollbarUI : ControlUI
    {
        public ScrollbarUI()
        {
            mHorizontal = false;
            mRange = 100;
            mScrollPos = 0;
            mLineSize = 8;
            mOwner = null;
            mLastScrollPos = 0;
            mLastScrollOffset = 0;
            mScrollRepeatDelay = 0;
            mButton1State = 0;
            mButton2State = 0;
            mThumbState = 0;

            mXYFixed.Width = DEFAULT_SCROLLBAR_SIZE;
            mLastMouse.X = mLastMouse.Y = 0;


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

            mThumbNormalImage = "";
            mThumbHotImage = "";
            mThumbPushedImage = "";
            mThumbDisabledImage = "";

            mRailNormalImage = "";
            mRailHotImage = "";
            mRailPushedImage = "";
            mRailDisabledImage = "";

            mImageModify = "";
        }
        ~ScrollbarUI()
        {

        }

        public override string getClass()
        {
            return "ScrollbarUI";
        }
        public override ControlUI getInterface(string name)
        {
            if (name == "Scrollbar")
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
                mThumbState = 0;
            }
        }

        public bool isHorizontal()
        {
            return mHorizontal;
        }
        public void setHorizontal(bool horizontal = true)
        {
            if (mHorizontal == horizontal)
            {
                return;
            }

            mHorizontal = horizontal;
            if (mHorizontal)
            {
                if (mXYFixed.Height == 0)
                {
                    mXYFixed.Width = 0;
                    mXYFixed.Height = DEFAULT_SCROLLBAR_SIZE;
                }
            }
            else
            {
                if (mXYFixed.Width == 0)
                {
                    mXYFixed.Width = DEFAULT_SCROLLBAR_SIZE;
                    mXYFixed.Height = 0;
                }
            }

            if (mOwner != null)
            {
                mOwner.needUpdate();
            }
            else
            {
                needParentUpdate();
            }
        }
        public int getScrollRange()
        {
            return mRange;
        }
        public void setScrollRange(int range)
        {
            if (mRange == range)
            {
                return;
            }

            mRange = range;
            if (mRange < 0)
            {
                mRange = 0;
            }
            if (mScrollPos > mRange)
            {
                mScrollPos = mRange;
            }
            setPos(mRectItem);
        }
        public int getScrollPos()
        {
            return mScrollPos;
        }
        public void setScrollPos(int pos)
        {
            if (pos == mScrollPos)
            {
                return;
            }

            mScrollPos = pos;
            if (mScrollPos < 0)
            {
                mScrollPos = 0;
            }
            if (mScrollPos > mRange)
            {
                mScrollPos = mRange;
            }
            setPos(mRectItem);
        }
        public int getLineSize()
        {
            return mLineSize;
        }
        public void setLineSize(int size)
        {
            mLineSize = size;
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
        public string getThumbNormalImage()
        {
            return mThumbNormalImage;
        }
        public void setThumbNormalImage(string strImage)
        {
            mThumbNormalImage = strImage;
            invalidate();
        }
        public string getThumbHotImage()
        {
            return mThumbHotImage;
        }
        public void setThumbHotImage(string strImage)
        {
            mThumbHotImage = strImage;
            invalidate();
        }
        public string getThumbPushedImage()
        {
            return mThumbPushedImage;
        }
        public void setThumbPushedImage(string strImage)
        {
            mThumbPushedImage = strImage;
            invalidate();
        }
        public string geThumbDisabledImage()
        {
            return mThumbDisabledImage;
        }
        public void setThumbDisabledImage(string strImage)
        {
            mThumbDisabledImage = strImage;
            invalidate();
        }
        public string getRailNormalImage()
        {
            return mRailNormalImage;
        }
        public void setRailNormalImage(string strImage)
        {
            mRailNormalImage = strImage;
            invalidate();
        }
        public string getRailHotImage()
        {
            return mRailHotImage;
        }
        public void setRailHotImage(string strImage)
        {
            mRailHotImage = strImage;
            invalidate();
        }
        public string getRailPushedImage()
        {
            return mRailPushedImage;
        }
        public void setRailPushedImage(string strImage)
        {
            mRailPushedImage = strImage;
            invalidate();
        }
        public string geRailDisabledImage()
        {
            return mRailDisabledImage;
        }
        public void setRailDisabledImage(string strImage)
        {
            mRailDisabledImage = strImage;
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
            int newLeft = 0;
            int newRight = 0;

            if (mHorizontal)
            {
                int cx = rc.Right - rc.Left - 2 * mXYFixed.Height;
                if (cx > mXYFixed.Height)
                {
                    mRectButton1.X = rc.Left;
                    mRectButton1.Width = mXYFixed.Height;
                    mRectButton1.Y = rc.Top;
                    mRectButton1.Height = mXYFixed.Height;

                    mRectButton2.X = rc.Right - mXYFixed.Height;
                    mRectButton2.Width = mXYFixed.Height;
                    mRectButton2.Y = rc.Top;
                    mRectButton2.Height = mXYFixed.Height;

                    mRectThumb.Y = rc.Top;
                    mRectThumb.Height = mXYFixed.Height;

                    if (mRange > 0)
                    {
                        int cxThumb = cx * (rc.Right - rc.Left) / (mRange + rc.Right - rc.Left);
                        if (cxThumb < mXYFixed.Height)
                        {
                            cxThumb = mXYFixed.Height;
                        }
                        newLeft = mScrollPos * (cx - cxThumb) / mRange + mRectButton1.Right;
                        newRight = newLeft + cxThumb;

                        mRectThumb.X = newLeft;
                        mRectThumb.Width = newRight - newLeft;

                        if (mRectThumb.Right > mRectButton2.Left)
                        {
                            mRectThumb.X = mRectButton2.Left - cxThumb;
                            mRectThumb.Width = cxThumb;
                        }
                    }
                    else
                    {
                        mRectThumb.X = mRectButton1.Right;
                        mRectThumb.Width = mRectButton2.Left - mRectThumb.Left;
                    }
                }
                else
                {
                    int cxButton = (rc.Right - rc.Left) / 2;
                    if (cxButton > mXYFixed.Height) cxButton = mXYFixed.Height;

                    mRectButton1.X = rc.Left;
                    mRectButton1.Width = cxButton;
                    mRectButton1.Y = rc.Top;
                    mRectButton1.Height = mXYFixed.Height;

                    mRectButton2.X = rc.Right - cxButton;
                    mRectButton2.Width = cxButton;
                    mRectButton2.Y = rc.Top;
                    mRectButton2.Height = mXYFixed.Height;

                    mRectThumb.X = 0;
                    mRectThumb.Y = 0;
                    mRectThumb.Width = 0;
                    mRectThumb.Height = 0;
                }
            }
            else
            {
                int cy = rc.Bottom - rc.Top - 2 * mXYFixed.Width;
                if (cy > mXYFixed.Width)
                {

                    mRectButton1.X = rc.Left;
                    mRectButton1.Y = rc.Top;
                    mRectButton1.Width = mXYFixed.Width;
                    mRectButton1.Height = mXYFixed.Width;

                    mRectButton2.X = rc.Left;
                    mRectButton2.Y = rc.Bottom - mXYFixed.Width;
                    mRectButton2.Width = mXYFixed.Width;
                    mRectButton2.Height = mXYFixed.Width;

                    mRectThumb.X = rc.Left;
                    mRectThumb.Width = mXYFixed.Width;
                    if (mRange > 0)
                    {
                        int cyThumb = cy * (rc.Bottom - rc.Top) / (mRange + rc.Bottom - rc.Top);
                        if (cyThumb < mXYFixed.Width) cyThumb = mXYFixed.Width;

                        mRectThumb.Y = mScrollPos * (cy - cyThumb) / mRange + mRectButton1.Bottom;
                        mRectThumb.Height = mRectThumb.Top + cyThumb - mRectThumb.Y;
                        if (mRectThumb.Bottom > mRectButton2.Top)
                        {
                            mRectThumb.Y = mRectButton2.Top - cyThumb;
                            mRectThumb.Height = cyThumb;
                        }
                    }
                    else
                    {
                        mRectThumb.Y = mRectButton1.Bottom;
                        mRectThumb.Height = mRectButton2.Top - mRectThumb.Y;
                    }
                }
                else
                {
                    int cyButton = (rc.Bottom - rc.Top) / 2;
                    if (cyButton > mXYFixed.Width) cyButton = mXYFixed.Width;

                    mRectButton1.X = rc.Left;
                    mRectButton1.Y = rc.Top;
                    mRectButton1.Width = mXYFixed.Width;
                    mRectButton1.Height = cyButton;

                    mRectButton2.X = rc.Left;
                    mRectButton2.Y = rc.Bottom - cyButton;
                    mRectButton2.Width = mXYFixed.Width;
                    mRectButton2.Height = cyButton;

                    mRectThumb.X = mRectThumb.Y = mRectThumb.Width = mRectThumb.Height = 0;
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

            if (newEvent.mType == (int)EventTypeUI.UIEVENT_SETFOCUS)
            {
                return;
            }
            if (newEvent.mType == (int)EventTypeUI.UIEVENT_KILLFOCUS)
            {
                return;
            }
            if (newEvent.mType == (int)EventTypeUI.UIEVENT_BUTTONDOWN || newEvent.mType == (int)EventTypeUI.UIEVENT_DBLCLICK)
            {
                if (!isEnabled()) return;

                mLastScrollOffset = 0;
                mScrollRepeatDelay = 0;
                mManager.setTimer(this, DEFAULT_TIMERID, 50);

                if (mRectButton1.Contains(newEvent.mMousePos))
                {
                    mButton1State |= (int)PaintFlags.UISTATE_PUSHED;
                    if (!mHorizontal)
                    {
                        if (mOwner != null) mOwner.lineUp();
                        else setScrollPos(mScrollPos - mLineSize);
                    }
                    else
                    {
                        if (mOwner != null) mOwner.lineLeft();
                        else setScrollPos(mScrollPos - mLineSize);
                    }
                }
                else if (mRectButton2.Contains(newEvent.mMousePos))
                {
                    mButton2State |= (int)PaintFlags.UISTATE_PUSHED;
                    if (!mHorizontal)
                    {
                        if (mOwner != null) mOwner.lineDown();
                        else setScrollPos(mScrollPos + mLineSize);
                    }
                    else
                    {
                        if (mOwner != null) mOwner.lineRight();
                        else setScrollPos(mScrollPos + mLineSize);
                    }
                }
                else if (mRectThumb.Contains(newEvent.mMousePos))
                {
                    mThumbState |= (int)PaintFlags.UISTATE_CAPTURED | (int)PaintFlags.UISTATE_PUSHED;
                    mLastMouse = newEvent.mMousePos;
                    mLastScrollPos = mScrollPos;
                }
                else
                {
                    if (!mHorizontal)
                    {
                        // 垂直滚动条鼠标点击轨迹事件处理
                        if (newEvent.mMousePos.Y < mRectThumb.Top)
                        {
                            if (mOwner != null) mOwner.pageUp();
                            else setScrollPos(mScrollPos + mRectItem.Top - mRectItem.Bottom);
                        }
                        else if (newEvent.mMousePos.Y > mRectThumb.Bottom)
                        {
                            if (mOwner != null) mOwner.pageDown();
                            else setScrollPos(mScrollPos - mRectItem.Top + mRectItem.Bottom);
                        }
                    }
                    else
                    {
                        // 水平滚动条鼠标点击轨迹事件处理
                        if (newEvent.mMousePos.X < mRectThumb.Left)
                        {
                            if (mOwner != null) mOwner.pageLeft();
                            else setScrollPos(mScrollPos + mRectItem.Left - mRectItem.Right);
                        }
                        else if (newEvent.mMousePos.X > mRectThumb.Right)
                        {
                            if (mOwner != null) mOwner.pageRight();
                            else setScrollPos(mScrollPos - mRectItem.Left + mRectItem.Right);
                        }
                    }
                }
                return;
            }
            if (newEvent.mType == (int)EventTypeUI.UIEVENT_BUTTONUP)
            {
                mScrollRepeatDelay = 0;
                mLastScrollOffset = 0;
                mManager.killTimer(this, DEFAULT_TIMERID);

                if ((mThumbState & (int)PaintFlags.UISTATE_CAPTURED) != 0)
                {
                    mThumbState &= ~((int)PaintFlags.UISTATE_CAPTURED | (int)PaintFlags.UISTATE_PUSHED);
                    invalidate();
                }
                else if ((mButton1State & (int)PaintFlags.UISTATE_PUSHED) != 0)
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
            if (newEvent.mType == (int)EventTypeUI.UIEVENT_MOUSEMOVE)
            {
                if ((mThumbState & (int)PaintFlags.UISTATE_CAPTURED) != 0)
                {
                    if (!mHorizontal)
                    {
                        if ((mRectItem.Bottom - mRectItem.Top - mRectThumb.Bottom + mRectThumb.Top - 2 * mXYFixed.Width) != 0)
                        {
                            mLastScrollOffset = (newEvent.mMousePos.Y - mLastMouse.Y) * mRange / (mRectItem.Bottom - mRectItem.Top - mRectThumb.Bottom + mRectThumb.Top - 2 * mXYFixed.Width);
                        }
                    }
                    else
                    {
                        if ((mRectItem.Right - mRectItem.Left - mRectThumb.Right + mRectThumb.Left - 2 * mXYFixed.Height) != 0)
                        {
                            mLastScrollOffset = (newEvent.mMousePos.X - mLastMouse.X) * mRange / (mRectItem.Right - mRectItem.Left - mRectThumb.Right + mRectThumb.Left - 2 * mXYFixed.Height);
                        }
                    }
                }
                else
                {
                    if ((mThumbState & (int)PaintFlags.UISTATE_HOT) != 0)
                    {
                        if (!mRectThumb.Contains(newEvent.mMousePos))
                        {
                            mThumbState &= ~(int)PaintFlags.UISTATE_HOT;
                            invalidate();
                        }
                    }
                    else
                    {
                        if (!isEnabled()) return;
                        if (mRectThumb.Contains(newEvent.mMousePos))
                        {
                            mThumbState |= (int)PaintFlags.UISTATE_HOT;
                            invalidate();
                        }
                    }
                }
                return;
            }
            if (newEvent.mType == (int)EventTypeUI.UIEVENT_TIMER && (int)newEvent.mWParam == DEFAULT_TIMERID)
            {
                ++mScrollRepeatDelay;
                if ((mThumbState & (int)PaintFlags.UISTATE_CAPTURED) != 0)
                {
                    if (!mHorizontal)
                    {
                        if (mOwner != null)
                        {
                            mOwner.setScrollPos(new Size(mOwner.getScrollPos().Width, mLastScrollPos + mLastScrollOffset));
                        }
                        else
                        {
                            setScrollPos(mLastScrollPos + mLastScrollOffset);
                        }
                    }
                    else
                    {
                        if (mOwner != null)
                        {
                            mOwner.setScrollPos(new Size(mLastScrollPos + mLastScrollOffset, mOwner.getScrollPos().Height));
                        }
                        else
                        {
                            setScrollPos(mLastScrollPos + mLastScrollOffset);
                        }
                    }
                    invalidate();
                }
                else if ((mButton1State & (int)PaintFlags.UISTATE_PUSHED) != 0)
                {
                    if (mScrollRepeatDelay <= 5) return;
                    if (!mHorizontal)
                    {
                        if (mOwner != null) mOwner.lineUp();
                        else setScrollPos(mScrollPos - mLineSize);
                    }
                    else
                    {
                        if (mOwner != null) mOwner.lineLeft();
                        else setScrollPos(mScrollPos - mLineSize);
                    }
                }
                else if ((mButton2State & (int)PaintFlags.UISTATE_PUSHED) != 0)
                {
                    if (mScrollRepeatDelay <= 5) return;
                    if (!mHorizontal)
                    {
                        if (mOwner != null) mOwner.lineDown();
                        else setScrollPos(mScrollPos + mLineSize);
                    }
                    else
                    {
                        if (mOwner != null) mOwner.lineRight();
                        else setScrollPos(mScrollPos + mLineSize);
                    }
                }
                else
                {
                    if (mScrollRepeatDelay <= 5) return;
                    Point pt;
                    pt = mManager.getPaintWindow().PointToClient(Control.MousePosition);
                    if (!mHorizontal)
                    {
                        if (pt.Y < mRectThumb.Top)
                        {
                            if (mOwner != null) mOwner.pageUp();
                            else setScrollPos(mScrollPos + mRectItem.Top - mRectItem.Bottom);
                        }
                        else if (pt.Y > mRectThumb.Bottom)
                        {
                            if (mOwner != null) mOwner.pageDown();
                            else setScrollPos(mScrollPos - mRectItem.Top + mRectItem.Bottom);
                        }
                    }
                    else
                    {
                        if (pt.X < mRectThumb.Left)
                        {
                            if (mOwner != null) mOwner.pageLeft();
                            else setScrollPos(mScrollPos + mRectItem.Left - mRectItem.Right);
                        }
                        else if (pt.X > mRectThumb.Right)
                        {
                            if (mOwner != null) mOwner.pageRight();
                            else setScrollPos(mScrollPos - mRectItem.Left + mRectItem.Right);
                        }
                    }
                }
                return;
            }
            if (newEvent.mType == (int)EventTypeUI.UIEVENT_MOUSEENTER)
            {
                if (isEnabled())
                {
                    mButton1State |= (int)PaintFlags.UISTATE_HOT;
                    mButton2State |= (int)PaintFlags.UISTATE_HOT;
                    if (mRectThumb.Contains(newEvent.mMousePos)) mThumbState |= (int)PaintFlags.UISTATE_HOT;
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
                    mThumbState &= ~(int)PaintFlags.UISTATE_HOT;
                    invalidate();
                }
                return;
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
            else if (name == "thumbnormalimage")
            {
                setThumbNormalImage(value);
            }
            else if (name == "thumbhotimage")
            {
                setThumbHotImage(value);
            }
            else if (name == "thumbpushedimage")
            {
                setThumbPushedImage(value);
            }
            else if (name == "thumbdisabledimage")
            {
                setThumbDisabledImage(value);
            }
            else if (name == "railnormalimage")
            {
                setRailNormalImage(value);
            }
            else if (name == "railhotimage")
            {
                setRailHotImage(value);
            }
            else if (name == "railpushedimage")
            {
                setRailPushedImage(value);
            }
            else if (name == "raildisabledimage")
            {
                setRailDisabledImage(value);
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
            else if (name == "hor")
            {
                setHorizontal(value == "true");
            }
            else if (name == "linesize")
            {
                setLineSize(int.Parse(value));
            }
            else if (name == "min")
            {
                setScrollRange(int.Parse(value));
            }
            else if (name == "value")
            {
                setScrollPos(int.Parse(value));
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
            paintThumb(ref graphics, ref bitmap);
            paintRail(ref graphics, ref bitmap);
        }
        public void paintBk(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (isEnabled() == false)
            {
                mThumbState |= (int)ControlFlag.UISTATE_DISABLED;
            }
            else
            {
                mThumbState &= ~(int)ControlFlag.UISTATE_DISABLED;
            }

            if ((mThumbState & (int)ControlFlag.UISTATE_DISABLED) != 0)
            {
                if (mBkDisabledImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mBkDisabledImage)) mBkDisabledImage = "";
                    else return;
                }
            }
            else if ((mThumbState & (int)ControlFlag.UISTATE_PUSHED) != 0)
            {
                if (mBkPushedImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mBkPushedImage)) mBkPushedImage = "";
                    else return;
                }
            }
            else if ((mThumbState & (int)ControlFlag.UISTATE_HOT) != 0)
            {
                if (mBkHotImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mBkHotImage)) mBkHotImage = "";
                    else return;
                }
            }

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
            int nBorderSize = 2;
            RenderEngine.drawRect(ref graphics, ref bitmap, ref mRectButton2, nBorderSize, (int)dwBorderColor);
        }

        void paintThumb(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (mRectThumb.Left == 0 && mRectThumb.Top == 0 && mRectThumb.Right == 0 && mRectThumb.Bottom == 0) return;
            if (!isEnabled()) mThumbState |= (int)ControlFlag.UISTATE_DISABLED;
            else mThumbState &= ~(int)ControlFlag.UISTATE_DISABLED;

            mImageModify = "";
            mImageModify = string.Format("dest='{0},{1},{2},{3}'", mRectThumb.Left - mRectItem.Left, mRectThumb.Top - mRectItem.Top, mRectThumb.Right - mRectItem.Left, mRectThumb.Bottom - mRectItem.Top);

            if ((mThumbState & (int)ControlFlag.UISTATE_DISABLED) != 0)
            {
                if (mThumbDisabledImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mThumbDisabledImage, mImageModify)) mThumbDisabledImage = "";
                    else return;
                }
            }
            else if ((mThumbState & (int)ControlFlag.UISTATE_PUSHED) != 0)
            {
                if (mThumbPushedImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mThumbPushedImage, mImageModify)) mThumbPushedImage = "";
                    else return;
                }
            }
            else if ((mThumbState & (int)ControlFlag.UISTATE_HOT) != 0)
            {
                if (mThumbHotImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mThumbHotImage, mImageModify)) mThumbHotImage = "";
                    else return;
                }
            }

            if (mThumbNormalImage != "")
            {
                if (!drawImage(ref graphics, ref bitmap, mThumbNormalImage, mImageModify)) mThumbNormalImage = "";
                else return;
            }

            uint dwBorderColor = 0xFF85E4FF;
            int nBorderSize = 2;
            RenderEngine.drawRect(ref graphics, ref bitmap, ref mRectThumb, nBorderSize, (int)dwBorderColor);
        }
        void paintRail(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (mRectThumb.Left == 0 && mRectThumb.Top == 0 && mRectThumb.Right == 0 && mRectThumb.Bottom == 0) return;
            if (!isEnabled()) mThumbState |= (int)ControlFlag.UISTATE_DISABLED;
            else mThumbState &= ~(int)ControlFlag.UISTATE_DISABLED;

            mImageModify = "";
            if (!mHorizontal)
            {
                mImageModify = string.Format("dest='{0},{1},{2},{3}'",
                    mRectThumb.Left - mRectItem.Left,
                    (mRectThumb.Top + mRectThumb.Bottom) / 2 - mRectItem.Top - mXYFixed.Width / 2,
                    mRectThumb.Right - mRectItem.Left,
                    (mRectThumb.Top + mRectThumb.Bottom) / 2 - mRectItem.Top + mXYFixed.Width - mXYFixed.Width / 2);
            }
            else
            {
                mImageModify = string.Format("dest='{0},{1},{2},{3}'",
                    (mRectThumb.Left + mRectThumb.Right) / 2 - mRectItem.Left - mXYFixed.Height / 2,
                    mRectThumb.Top - mRectItem.Top,
                    (mRectThumb.Left + mRectThumb.Right) / 2 - mRectItem.Left + mXYFixed.Height - mXYFixed.Height / 2,
                    mRectThumb.Bottom - mRectItem.Top);
            }

            if ((mThumbState & (int)ControlFlag.UISTATE_DISABLED) != 0)
            {
                if (mRailDisabledImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mRailDisabledImage, mImageModify)) mRailDisabledImage = "";
                    else return;
                }
            }
            else if ((mThumbState & (int)ControlFlag.UISTATE_PUSHED) != 0)
            {
                if (mRailPushedImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mRailPushedImage, mImageModify)) mRailPushedImage = "";
                    else return;
                }
            }
            else if ((mThumbState & (int)ControlFlag.UISTATE_HOT) != 0)
            {
                if (mRailHotImage != "")
                {
                    if (!drawImage(ref graphics, ref bitmap, mRailHotImage, mImageModify)) mRailHotImage = "";
                    else return;
                }
            }

            if (mRailNormalImage != "")
            {
                if (!drawImage(ref graphics, ref bitmap, mRailNormalImage, mImageModify)) mRailNormalImage = "";
                else return;
            }
        }

        protected const int DEFAULT_SCROLLBAR_SIZE = 16;
        protected const int DEFAULT_TIMERID = 10;

        protected bool mHorizontal;
        protected int mRange;
        protected int mScrollPos;
        protected int mLineSize;
        protected ContainerUI mOwner;
        protected Point mLastMouse;
        protected int mLastScrollPos;
        protected int mLastScrollOffset;
        protected int mScrollRepeatDelay;

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

        protected Rectangle mRectThumb;
        protected int mThumbState;
        protected string mThumbNormalImage;
        protected string mThumbHotImage;
        protected string mThumbPushedImage;
        protected string mThumbDisabledImage;

        protected string mRailNormalImage;
        protected string mRailHotImage;
        protected string mRailPushedImage;
        protected string mRailDisabledImage;

        protected string mImageModify;
    }
}
