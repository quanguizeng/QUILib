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
    public class TEventUI
    {
        public int mType;
        public ControlUI mSender;
        public long mTimestamp;
        public Point mMousePos;
        public char mKey;
        public Int16 mKeyState;
        public object mWParam;
        public object mLParam;
    }


    public delegate ControlUI FINDCONTROLPROC(ref ControlUI control, ref object data);
    public class ControlUI
    {
        public ControlUI()
        {
            mManager = null;
            mParent = null;
            mUpdateNeeded = true;
            mVisible = true;
            mInternVisible = true;
            mFocused = false;
            mEnabled = true;
            mMouseEnabled = true;
            mFloat = false;
            mFloatSetPos = false;
            mShortcut = (char)'\0';
            mTag = null;
            mBackColor = Color.FromArgb(0);
            mBackColor2 = Color.FromArgb(0);
            mBorderColor = Color.FromArgb(0);
            mBorderSize = 1;

            mXY.Width = mXY.Height = 0;
            mXYFixed.Width = mXYFixed.Height = 0;
            mXYMin.Width = mXYMin.Height = 0;
            mXYMax.Width = mXYMax.Height = 9999;

            mName = "";
            mText = "";
            mListAttribute = new Dictionary<string, string>();
            mBkImage = "";
            mToolTip = "";
            mUserData = "";
        }
        ~ControlUI()
        {
            mListAttribute.Clear();
        }
        public virtual string getName()
        {
            return mName;
        }
        public virtual void setName(string name)
        {
            mName = name;
        }
        public virtual string getClass()
        {
            return "ControlUI";
        }
        public virtual ControlUI getInterface(string name)
        {
            if (name == "Control")
            {
                return this;
            }

            return null;
        }
        public virtual int getControlFlags()
        {
            if (!isEnabled()) return 0;

            return (int)ControlFlag.UIFLAG_SETCURSOR | (int)ControlFlag.UIFLAG_TABSTOP;
        }

        public virtual bool activate()
        {
            bool result = isVisible() && isEnabled() ? true : false;

            return result;
        }
        public virtual PaintManagerUI getManager()
        {
            return mManager;
        }
        public virtual void setManager(PaintManagerUI manager, ControlUI parent)
        {
            mManager = manager;
            mParent = parent;
        }
        public virtual ControlUI getParent()
        {
            return mParent;
        }

        public virtual string getText()
        {
            return mText;
        }
        public virtual void setText(string text,bool notify = true)
        {
            mText = text;
        }

        public Color getBackColor()
        {
            return mBackColor;
        }
        public void setBackColor(Color color)
        {
            if (color == mBackColor)
            {
                return;
            }
            mBackColor = color;
            invalidate();
        }
        public Color getBackColor2()
        {
            return mBackColor2;
        }
        public void setBackColor2(Color color)
        {
            if (color == mBackColor2)
            {
                return;
            }
            mBackColor2 = color;
            invalidate();
        }
        public string getBackImage()
        {
            return mBkImage;
        }
        public void setBackImage(string strImage)
        {
            if (strImage == mBkImage)
            {
                return;
            }

            mBkImage = strImage;

        }
        public Color getBorderColor()
        {
            return mBorderColor;
        }
        public void setBorderColor(Color color)
        {
            if (color == mBorderColor)
            {
                return;
            }
            mBorderColor = color;
        }
        public int getBorderSize()
        {
            return mBorderSize;
        }
        public void setBorderSize(int size)
        {
            mBorderSize = size;
        }
        public bool drawImage(ref Graphics graphics, ref Bitmap bitmap, string strImage, string strModify = "")
        {
            return RenderEngine.drawImageString(ref graphics, ref bitmap, ref mManager, ref mRectItem, ref mRectPaint, strImage, strModify);
        }

        // 位置相关
        public virtual Rectangle getPos()
        {
            return mRectItem;
        }
        public virtual void setPos(Rectangle rc)
        {

            if (rc.Right < rc.Left)
            {
                int left = rc.Left;
                rc.Width = 0;
                rc.X = left;
            }
            if (rc.Bottom < rc.Top)
            {
                int top = rc.Top;
                rc.Height = 0;
                rc.Y = top;
            }

            Rectangle invalidateRc = mRectItem;
            if (invalidateRc.IsEmpty) invalidateRc = rc;

            mRectItem = rc;
            if (mManager == null) return;
            ControlUI pParent = null;
            if (mFloat)
            {
                if (!mFloatSetPos)
                {
                    mFloatSetPos = true;
                    mManager.sendNotify(this, "setpos");
                    mFloatSetPos = false;
                }

                pParent = getParent();
                if (pParent != null)
                {
                    Rectangle rcParentPos = pParent.getPos();
                    if (mXY.Width >= 0) mXY.Width = mRectItem.Left - rcParentPos.Left;
                    else mXY.Width = mRectItem.Right - rcParentPos.Right;

                    if (mXY.Height >= 0) mXY.Height = mRectItem.Top - rcParentPos.Top;
                    else mXY.Height = mRectItem.Bottom - rcParentPos.Bottom;
                }
            }

            mUpdateNeeded = false;

            // NOTE: SetPos() is usually called during the WM_PAINT cycle where all controls are
            //       being laid out. Calling UpdateLayout() again would be wrong. Refreshing the
            //       window won't hurt (if we're already inside WM_PAINT we'll just validate it out).
            invalidateRc.Intersect(mRectItem);

            pParent = this;
            Rectangle rcTemp;
            Rectangle rcParent;
            while ((pParent = pParent.getParent()) != null)
            {
                rcTemp = invalidateRc;
                rcParent = pParent.getPos();
                if (rcTemp.IntersectsWith(rcParent))
                {
                    rcTemp.Intersect(rcParent);
                    invalidateRc.Intersect(rcTemp);
                }
                else
                {
                    return;
                }
            }
            mManager.invalidate(ref invalidateRc);
        }
        protected void setPos0(Rectangle rc)
        {
            if (rc.Right < rc.Left)
            {
                int left = rc.Left;
                rc.Width = 0;
                rc.X = left;
            }
            if (rc.Bottom < rc.Top)
            {
                int top = rc.Top;
                rc.Height = 0;
                rc.Y = top;
            }

            Rectangle invalidateRc = mRectItem;
            if (invalidateRc.IsEmpty) invalidateRc = rc;

            mRectItem = rc;
            if (mManager == null) return;
            ControlUI pParent = null;
            if (mFloat)
            {
                if (!mFloatSetPos)
                {
                    mFloatSetPos = true;
                    mManager.sendNotify(this, "setpos");
                    mFloatSetPos = false;
                }

                pParent = getParent();
                if (pParent != null)
                {
                    Rectangle rcParentPos = pParent.getPos();
                    if (mXY.Width >= 0) mXY.Width = mRectItem.Left - rcParentPos.Left;
                    else mXY.Width = mRectItem.Right - rcParentPos.Right;

                    if (mXY.Height >= 0) mXY.Height = mRectItem.Top - rcParentPos.Top;
                    else mXY.Height = mRectItem.Bottom - rcParentPos.Bottom;
                }
            }

            mUpdateNeeded = false;

            // NOTE: SetPos() is usually called during the WM_PAINT cycle where all controls are
            //       being laid out. Calling UpdateLayout() again would be wrong. Refreshing the
            //       window won't hurt (if we're already inside WM_PAINT we'll just validate it out).
            invalidateRc.Intersect(mRectItem);

            pParent = this;
            Rectangle rcTemp;
            Rectangle rcParent;
            while ((pParent = pParent.getParent()) != null)
            {
                rcTemp = invalidateRc;
                rcParent = pParent.getPos();
                if (rcTemp.IntersectsWith(rcParent) == false)
                {
                    rcTemp.Intersect(rcParent);
                    invalidateRc.Intersect(rcTemp);
                }
                else
                {
                    return;
                }
            }
            mManager.invalidate(ref invalidateRc);
        }
        public virtual int getWidth()
        {
            return mRectItem.Width;
        }
        public virtual int getHeight()
        {
            return mRectItem.Height;
        }
        public virtual int getX()
        {
            return mRectItem.X;
        }
        public virtual int getY()
        {
            return mRectItem.Y;
        }
        public virtual Rectangle getPadding()
        {
            return mRectPadding;
        }
        public virtual void setPadding(Rectangle padding)
        {
            mRectPadding = padding;
            needParentUpdate();
        }
        public virtual Size getFixedXY()
        {
            return mXY;
        }
        public virtual void setFixedXY(Size xy)
        {
            mXY.Width = xy.Width;
            mXY.Height = xy.Height;

            if (mFloat == false)
            {
                needParentUpdate();
            }
            else needUpdate();
        }
        public virtual int getFixedWidth()
        {
            return mXYFixed.Width;
        }
        public virtual void setFixedWidth(int cx)
        {
            if (cx < 0)
            {
                return;
            }
            mXYFixed.Width = cx;
            if (mFloat == false)
            {
                needParentUpdate();
            }
            else needUpdate();
        }
        public virtual int getFixedHeight()
        {
            return mXYFixed.Height;
        }
        public virtual void setFixedHeight(int cy)
        {
            if (cy < 0)
            {
                return;
            }
            mXYFixed.Height = cy;
            if (mFloat == false)
            {
                needParentUpdate();
            }
            else needUpdate();
        }
        public virtual int getMinWidth()
        {
            return mXYMin.Width;
        }
        public virtual void setMinWidth(int width)
        {
            if (mXYMin.Width == width)
            {
                return;
            }

            if (width < 0)
            {
                return;
            }
            mXYMin.Width = width;
            if (mFloat == false)
            {
                needParentUpdate();
            }
            else needUpdate();
        }
        public virtual int getMaxWidth()
        {
            return mXYMax.Width;
        }
        public virtual void setMaxWidth(int width)
        {
            if (mXYMax.Width == width)
            {
                return;
            }
            if (width < 0)
            {
                return;
            }
            mXYMax.Width = width;
            if (mFloat == false)
            {
                needParentUpdate();
            }
            else needUpdate();
        }
        public virtual int getMinHeight()
        {
            return mXYMin.Height;
        }
        public virtual void setMinHeight(int height)
        {
            if (mXYMin.Height == height)
            {
                return;
            }
            if (height < 0)
            {
                return;
            }
            mXYMin.Height = height;
            if (mFloat == false)
            {
                needParentUpdate();
            }
            else needUpdate();
        }
        public virtual int getMaxHeight()
        {
            return mXYMax.Height;
        }
        public virtual void setMaxHeight(int cy)
        {
            if (mXYMax.Height == cy)
            {
                return;
            }
            if (cy < 0)
            {
                return;
            }
            mXYMax.Height = cy;
            if (mFloat == false)
            {
                needParentUpdate();
            }
            else
            {
                needUpdate();
            }
        }

        // 鼠标提示
        public virtual string getToolTip()
        {
            return mToolTip;
        }
        public void setToolTip(string strText)
        {
            mToolTip = strText;
        }
        public virtual char getShortcut()
        {
            return mShortcut;
        }
        public virtual void setShortcut(char shortcut)
        {
            mShortcut = shortcut;
        }

        // 用户属性
        public virtual string getUserData()
        {
            return mUserData;
        }
        public virtual void setUserData(string text)
        {
            mUserData = text;
        }
        public virtual object getTag()
        {
            return mTag;
        }
        public void setTag(object tag)
        {
            mTag = tag;
        }

        // 一些重要的属性
        public virtual bool isVisible()
        {
            return mVisible && mInternVisible;
        }
        public virtual void setVisible(bool visible = true)
        {
            if (mVisible == visible)
            {
                return;
            }
            bool v = isVisible();
            mVisible = visible;
            if (mFocused)
            {
                mFocused = false;
            }
            if (isVisible() != v)
            {
                if (mFloat == false)
                {
                    needParentUpdate();
                }
                else
                {
                    needUpdate();
                }
            }
        }
        public void setVisible0(bool visible = true)
        {
            if (mVisible == visible)
            {
                return;
            }
            bool v = isVisible();
            mVisible = visible;
            if (mFocused)
            {
                mFocused = false;
            }
            if (isVisible() != v)
            {
                if (mFloat == false)
                {
                    needParentUpdate();
                }
                else
                {
                    needUpdate();
                }
            }
        }

        public virtual void setInternVisible(bool visible = true)
        {
            mInternVisible = visible;
        }
        public void setInternVisible0(bool visible = true)
        {
            mInternVisible = visible;
        }

        public virtual bool isEnabled()
        {
            return mEnabled;
        }
        public virtual void setEnabled(bool enable = true)
        {
            if (mEnabled == enable)
            {
                return;
            }
            mEnabled = enable;
            invalidate();
        }
        public void setEnabled0(bool enable = true)
        {
            if (mEnabled == enable)
            {
                return;
            }
            mEnabled = enable;
            invalidate();
        }

        public virtual bool isMouseEnabled()
        {
            return mMouseEnabled;
        }
        public virtual void setMouseEnabled(bool enable = true)
        {
            mMouseEnabled = enable;
        }
        public virtual bool isFocused()
        {
            return mFocused;
        }
        public virtual void setFocus()
        {
            if (mManager != null)
            {
                ControlUI v = (ControlUI)this;
                mManager.setFocus(ref v);
            }
        }
        public virtual bool isFloat()
        {
            return mFloat;
        }
        public virtual void setFloat(bool bFloat)
        {
            if (mFloat == bFloat)
            {
                return;
            }

            mFloat = bFloat;
            needParentUpdate();
        }

        public virtual ControlUI findControl(FINDCONTROLPROC proc, ref object data, UInt32 flags)
        {

            if ((flags & ControlFlag.UIFIND_VISIBLE) != 0 && isVisible() == false)
            {
                return null;
            }
            if ((flags & ControlFlag.UIFIND_ENABLED) != 0 && isEnabled() == false)
            {
                return null;
            }
            if ((flags & ControlFlag.UIFIND_HITTEST) != 0 && mMouseEnabled == false)
            {
                return null;
            }
            if (data is Point)
            {
                Point rc = (Point)data;
                if ((flags & ControlFlag.UIFIND_HITTEST) != 0 && mRectItem.Contains(rc) == false)
                {
                    return null;
                }
            }

            ControlUI ctlUI = (ControlUI)this;

            return proc(ref ctlUI, ref data);
        }

        public virtual void invalidate()
        {
            if (isVisible() == false)
            {
                return;
            }

            Rectangle invalidateRc = mRectItem;

            ControlUI parent = this;
            Rectangle rcTtemp;
            Rectangle rcParent;

            while ((parent = parent.getParent()) != null)
            {
                rcTtemp = invalidateRc;
                rcParent = parent.getPos();
                if (rcTtemp.IntersectsWith(rcParent) == true)
                {
                    invalidateRc.Intersect(rcParent);
                }
                else
                {
                    return;
                }
            }

            if (mManager != null)
            {
                mManager.invalidate(ref invalidateRc);
            }
        }
        public bool isUpdateNeeded()
        {
            return mUpdateNeeded;
        }
        public void needUpdate()
        {
            mUpdateNeeded = true;
            invalidate();

            if (mManager != null)
            {
                mManager.needUpdate();
            }
        }
        public void needParentUpdate()
        {
            if (getParent() != null)
            {
                getParent().needUpdate();
                getParent().invalidate();
            }
            else
            {
                needUpdate();
            }

            if (mManager != null)
            {
                mManager.needUpdate();
            }
        }

        public virtual void eventProc(ref TEventUI newEvent)
        {
            // 设置鼠标形状
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SETCURSOR)
            {
                if (mManager.getPaintWindow().Cursor != Cursors.Arrow)
                {
                    mManager.getPaintWindow().Cursor = Cursors.Arrow;
                }
                return;
            }


            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SETFOCUS)
            {
                mFocused = true;
                invalidate();
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_KILLFOCUS)
            {
                mFocused = true;
                invalidate();
                return;
            }

            if (mParent != null)
            {
                mParent.eventProc(ref newEvent);
            }
        }

        public void eventProc0(ref TEventUI newEvent)
        {
            // 设置鼠标形状
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SETCURSOR)
            {
                if (mManager.getPaintWindow().Cursor != Cursors.Arrow)
                {
                    mManager.getPaintWindow().Cursor = Cursors.Arrow;
                }
                return;
            }

            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SETFOCUS)
            {
                mFocused = true;
                invalidate();
                return;
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_KILLFOCUS)
            {
                mFocused = true;
                invalidate();
                return;
            }

            if (mParent != null)
            {
                mParent.eventProc(ref newEvent);
            }
        }

        public void addAttribute(string name, string value)
        {
            if (mListAttribute.ContainsKey(name) == false)
            {
                mListAttribute.Add(name, value);

                return;
            }
            else
            {
                mListAttribute[name] = value;
                return;
            }

            //throw new Exception("控件出现重名属性");
        }
        public virtual void setAttribute(string name, string value)
        {
            if (name == "pos")
            {
                string[] listValues = value.Split(',');
                if (listValues.Length != 4)
                {
                    throw new Exception("位置参数个数不对");
                }
                Rectangle rcPos = new Rectangle(int.Parse(listValues[0]),
                    int.Parse(listValues[1]),
                    int.Parse(listValues[2]) - int.Parse(listValues[0]),
                    int.Parse(listValues[3]) - int.Parse(listValues[1]));
                Size szXY = new Size(rcPos.Left >= 0 ? rcPos.Left : rcPos.Right,
                    rcPos.Top >= 0 ? rcPos.Top : rcPos.Bottom);
                setFixedXY(szXY);
                setFixedWidth(rcPos.Width);
                setFixedHeight(rcPos.Height);
            }
            else if (name == "padding")
            {
                string[] listValues = value.Split(',');
                if (listValues.Length != 4)
                {
                    throw new Exception("对齐参数个数不对");
                }
                Rectangle rcPos = new Rectangle(int.Parse(listValues[0]),
                    int.Parse(listValues[1]),
                    int.Parse(listValues[2]),
                    int.Parse(listValues[3]));
                setPadding(rcPos);
            }
            else if (name == "bkcolor")
            {
                value = value.TrimStart('#');
                Color color = Color.FromArgb(Convert.ToInt32(value, 16));
                setBackColor(color);
            }
            else if (name == "bkcolor2")
            {
                value = value.TrimStart('#');
                Color color = Color.FromArgb(Convert.ToInt32(value, 16));
                setBackColor2(color);
            }
            else if (name == "bordercolor")
            {
                value = value.TrimStart('#');
                if(value.StartsWith("00"))
                {
                    value = "FF" + value.Substring(2);
                }
                Color color = Color.FromArgb(Convert.ToInt32(value, 16));
                setBorderColor(color);
            }
            else if (name == "bordersize")
            {
                setBorderSize(int.Parse(value));
            }
            else if (name == "bkimage")
            {
                setBkImage(value);
            }
            else if (name == "width")
            {
                setFixedWidth(int.Parse(value));
            }
            else if (name == "height")
            {
                setFixedHeight(int.Parse(value));
            }
            else if (name == "minwidth")
            {
                setMinWidth(int.Parse(value));
            }
            else if (name == "minheight")
            {
                setMinHeight(int.Parse(value));
            }
            else if (name == "maxwidth")
            {
                setMaxWidth(int.Parse(value));
            }
            else if (name == "maxheight")
            {
                setMaxHeight(int.Parse(value));
            }
            else if (name == "name")
            {
                setName(value);
            }
            else if (name == "text")
            {
                setText(value);
            }
            else if (name == "tooltip")
            {
                setToolTip(value);
            }
            else if (name == "userdata")
            {
                setUserData(value);
            }
            else if (name == "enabled")
            {
                setEnabled(value == "true");
            }
            else if (name == "mouse")
            {
                setMouseEnabled(value == "true");
            }
            else if (name == "visible")
            {
                setVisible(value == "true");
            }
            else if (name == "float")
            {
                setFloat(value == "true");
            }
            else if (name == "shortcut")
            {
                setShortcut(value[0]);
            }
        }
        public void setBkImage(string strImage)
        {
            if (mBkImage == strImage)
            {
                return;
            }
            mBkImage = strImage;
            invalidate();

        }
        public ControlUI applyAttributeList(string strList)
        {
            string name = "";
            string value = "";

            int curIdx = 0;

            while (curIdx < strList.Length && strList[curIdx] != (char)0)
            {
                name = "";
                value = "";
                while (strList[curIdx] != (char)0 && strList[curIdx] != '=')
                {
                    name += strList[curIdx];
                    curIdx++;
                }
                if (strList[curIdx] != '=')
                {
                    return this;
                }
                curIdx++;
                if (strList[curIdx] != '"')
                {
                    return this;
                }
                curIdx++;
                while (strList[curIdx] != (char)0 && strList[curIdx] != '"')
                {
                    value += strList[curIdx];
                    curIdx++;
                }
                if (strList[curIdx] != '"')
                {
                    return this;
                }
                curIdx++;
                setAttribute(name, value);
                if (strList[curIdx] != ' ')
                {
                    return this;
                }
                curIdx++;
            }

            return this;
        }

        public virtual Size estimateSize(Size available)
        {
            return mXYFixed;
        }
        public Size estimateSize0(Size available)
        {
            return mXYFixed;
        }
        public virtual void doPaint(ref Graphics graphics, ref Bitmap bitmap, Rectangle rectPaint)
        {
            if (rectPaint.IntersectsWith(mRectItem) == false)
            {
                return;
            }

            rectPaint.Intersect(mRectItem);
            mRectPaint = rectPaint;

            // 绘制顺序:背景颜色->背景图->状态图->文本
            paintBkColor(ref graphics, ref bitmap);
            paintBkImage(ref graphics, ref bitmap);
            paintStatusImage(ref graphics, ref bitmap);
            paintText(ref graphics, ref bitmap);
            paintBorder(ref graphics, ref bitmap);
        }
        public void doPaint0(ref Graphics graphics, ref Bitmap bitmap, Rectangle rectPaint)
        {
            if (rectPaint.IntersectsWith(mRectItem) == false)
            {
                return;
            }

            rectPaint.Intersect(mRectItem);
            mRectPaint = rectPaint;

            // 绘制顺序:背景颜色->背景图->状态图->文本
            paintBkColor(ref graphics, ref bitmap);
            paintBkImage(ref graphics, ref bitmap);
            paintStatusImage(ref graphics, ref bitmap);
            paintText(ref graphics, ref bitmap);
            paintBorder(ref graphics, ref bitmap);
        }

        public virtual void paintBkColor(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (mBackColor != null && mBackColor.ToArgb() != 0)
            {
                if (mBackColor2 != null && mBackColor2.ToArgb() != 0)
                {
                    RenderEngine.drawGradient(ref graphics, ref bitmap, ref mRectItem, mBackColor.ToArgb(), mBackColor2.ToArgb(), true, 16);
                }
                else if ((uint)mBackColor.ToArgb() >= (uint)(0xFF000000))
                {
                    RenderEngine.drawColor(ref graphics, ref bitmap, ref mRectPaint, mBackColor.ToArgb());
                }
                else
                {
                    RenderEngine.drawColor(ref graphics, ref bitmap, ref mRectItem, mBackColor.ToArgb());
                }
            }
        }
        public virtual void paintBkImage(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (mBkImage == "")
            {
                return;
            }
            if (drawImage(ref graphics, ref bitmap, mBkImage) == false)
            {
                mBkImage = "";
            }
        }
        public virtual void paintStatusImage(ref Graphics graphics, ref Bitmap bitmap)
        {
        }
        public virtual void paintText(ref Graphics graphics, ref Bitmap bitmap)
        {
        }
        public virtual void paintBorder(ref Graphics graphics, ref Bitmap bitmap)
        {
            if (mBorderColor != null && mBorderColor.ToArgb() != 0 && mBorderSize > 0)
            {
                Rectangle rc = new Rectangle(mRectItem.X, mRectItem.Top, mRectItem.Width - mBorderSize, mRectItem.Height - mBorderSize);
                RenderEngine.drawRect(ref graphics, ref bitmap, ref rc, mBorderSize, mBorderColor.ToArgb());
            }
        }
        public virtual void doPostPaint(ref Graphics graphics, ref Bitmap bitmap, ref Rectangle rectPaint)
        {

        }

        public Dictionary<string, string> getListAttr()
        {
            return mListAttribute;
        }
        public static Int16 HIWORD(int x)
        {
            Int16 result = (Int16)((x >> 16) & 0xffff);

            return result;
        }


        protected PaintManagerUI mManager;
        protected ControlUI mParent;
        protected string mName;
        protected bool mUpdateNeeded;
        protected Rectangle mRectItem;
        protected Rectangle mRectPadding;
        protected Size mXY;
        protected Size mXYFixed;
        protected Size mXYMin;
        protected Size mXYMax;
        protected bool mVisible;
        protected bool mInternVisible;
        protected bool mEnabled;
        protected bool mMouseEnabled;
        protected bool mFocused;
        protected bool mFloat;
        protected bool mFloatSetPos;

        protected string mText;
        protected string mToolTip;
        protected char mShortcut;
        protected string mUserData;
        protected object mTag;
        protected Color mBackColor;
        protected Color mBackColor2;
        protected string mBkImage;
        protected Color mBorderColor;
        protected int mBorderSize;
        protected Rectangle mRectPaint;
        protected Dictionary<string, string> mListAttribute;
    }
}
