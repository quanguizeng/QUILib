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
    public class PaintManagerUI
    {
        public PaintManagerUI()
        {
            mWndPaint = null;
            mDcPaint = null;
            mDcOffscreen = null;
            mShowUpdateRect = false;
            mTimerID = 0x1000;
            mRoot = null;
            mFocus = null;
            mEventHover = null;
            mEventClick = null;
            mEventKey = null;
            mFirstLayout = true;
            mFocusNeeded = false;
            mUpdateNeeded = false;
            mMouseTracking = false;
            mMouseCapture = false;
            mOffscreenPaint = true;
            mParentResourcePM = null;

            mDefaultAttrHash = new Dictionary<string, string>();
            mPreMessages = new List<PaintManagerUI>();
            mNotifiers = new List<INotifyUI>();
            mMessageFilters = new List<IMessageFilterUI>();
            mNameHash = new Dictionary<string, ControlUI>();
            mCustomFonts = new List<Font>();
            mImageHash = new Dictionary<string, TImageInfo>();
            mDefaultFont = new Font("微软雅黑", 10, FontStyle.Regular);
            mDefaultFontColor = Color.FromArgb(0xff, 0, 0, 0);
            mDefaultLinkFont = new Font("微软雅黑", 10, FontStyle.Regular | FontStyle.Underline);
            mDefaultLinkFontColor = Color.FromArgb(0xff, 0, 0, 0xFF);
            mDefaultBoldFontColor = Color.FromArgb(0xff, 0, 0, 0);
            mDefaultBoldFont = new Font("微软雅黑", 10, FontStyle.Regular | FontStyle.Bold);

            mRenderBufferManager = new RenderBufferManager();
            mTimers = new List<TimerInfo>();

            mGroupList = new Dictionary<string, List<ControlUI>>();
        }
        ~PaintManagerUI()
        {
            mDefaultAttrHash.Clear();
            mPreMessages.Clear();
            mNotifiers.Clear();
            mMessageFilters.Clear();
            mNameHash.Clear();


            removeAllDefaultAttributeList();
            removeAllFonts();
            removeAllImages();
            removeAllOptionGroups();
            removeAllTimers();

            release();
        }

        public void init(ref Form form)
        {
            mWndPaint = form;
            mPreMessages.Add(this);
        }
        public void needUpdate()
        {
            mUpdateNeeded = true;
        }
        public void invalidate(ref Rectangle rectItem)
        {
            if (mWndPaint == null)
            {
                return;
            }
            mWndPaint.Invalidate(rectItem, false);
        }

        public Graphics getPaintDC()
        {
            return mDcPaint;
        }
        public Form getPaintWindow()
        {
            return mWndPaint;
        }

        public Point getMousePos()
        {
            return mLastMousePos;
        }
        public Size getClientSize()
        {
            return mWndPaint.Size;
        }
        public Size getInitSize()
        {
            return mInitWindowSize;
        }
        public void setInitSize(int cx, int cy)
        {
            mInitWindowSize.Width = cx;
            mInitWindowSize.Height = cy;
        }
        public Rectangle getSizeBox()
        {
            return mSizeBox;
        }
        public void setSizeBox(ref Rectangle sizeBox)
        {
            mSizeBox = sizeBox;
        }
        public Rectangle getCaptionRect()
        {
            return mRectCaption;
        }

        public void setCaptionRect(ref Rectangle rectCaption)
        {
            mRectCaption = rectCaption;
        }
        public Size getRoundCorner()
        {
            return mRoundCorner;
        }
        public void setRoundCorner(int cx, int cy)
        {
            mRoundCorner.Width = cx;
            mRoundCorner.Height = cy;
        }
        public void setMinMaxInfo(int cx, int cy)
        {
            mMinWindow.Width = cx;
            mMinWindow.Height = cy;
        }
        public void setShowUpdateRect(bool show)
        {
            mShowUpdateRect = show;
        }

        public bool useParentResource(PaintManagerUI pm)
        {
            if (pm == null)
            {
                mParentResourcePM = null;

                return true;
            }

            PaintManagerUI parentPM = pm.getParentResource();
            while (parentPM != null)
            {
                if (parentPM == this)
                {
                    return false;
                }
                parentPM = parentPM.getParentResource();
            }
            mParentResourcePM = pm;

            return true;
        }
        public PaintManagerUI getParentResource()
        {
            return mParentResourcePM;
        }

        public Color getDefaultDisabledColor()
        {
            if (mParentResourcePM != null)
            {
                return mParentResourcePM.getDefaultDisabledColor();
            }

            return mDefaultDisabledColor;
        }
        public Color setDefaultDisabledColor(Color color)
        {
            return mDefaultDisabledColor = color;
        }
        public Font getDefaultFont()
        {
            if (mParentResourcePM != null)
            {
                return mParentResourcePM.getDefaultFont();
            }

            return mDefaultFont;
        }
        public Color getDefaultFontColor()
        {
            if (mParentResourcePM != null)
            {
                return mParentResourcePM.getDefaultFontColor();
            }

            return mDefaultFontColor;
        }
        // 获取缺省字体信息 GetDefaultFontInfo
        public void setDefaultFont(Font font, uint color = 0xFF000000)
        {
            if (font == null)
            {
                return;
            }
            if (mWndPaint != null)
            {
                mWndPaint.Font = font;
            }
            if (mDefaultFont != null && findFont(ref mDefaultFont) == false)
            {
                mDefaultFont = null;
            }
            mDefaultFont = font;
            Color defaultColor = Color.FromArgb((int)color);
            mDefaultFontColor = defaultColor;
        }
        public Font getDefaultBoldFont()
        {
            if (mParentResourcePM != null)
            {
                return mParentResourcePM.getDefaultBoldFont();
            }

            return mDefaultBoldFont;
        }
        public Color getDefaultBoldFontColor()
        {
            if (mParentResourcePM != null)
            {
                return mParentResourcePM.getDefaultBoldFontColor();
            }

            return mDefaultBoldFontColor;
        }
        // 获取默认粗体信息 GetDefaultBoldFontInfo
        public void setDefaultBoldFont(Font font, uint color = 0xFF000000)
        {
            if (mWndPaint != null)
            {
                mWndPaint.Font = font;
            }

            if (mDefaultBoldFont != null && findFont(ref mDefaultBoldFont) == false)
            {
                mDefaultBoldFont = null;
            }
            Color defaultColor = Color.FromArgb((int)color);
            mDefaultBoldFont = font;
            mDefaultBoldFontColor = defaultColor;
        }
        public Font getDefaultLinkFont()
        {
            if (mParentResourcePM != null)
            {
                return mParentResourcePM.getDefaultLinkFont();
            }

            return mDefaultLinkFont;
        }
        public Color getDefaultLinkFontColor()
        {
            if (mParentResourcePM != null)
            {
                return mParentResourcePM.getDefaultLinkFontColor();
            }

            return mDefaultLinkFontColor;
        }
        public Color getDefaultLinkFontHoverColor()
        {
            if (mParentResourcePM != null)
            {
                return mParentResourcePM.getDefaultLinkFontHoverColor();
            }

            return mDefaultLinkFontHoverColor;
        }
        // 获取缺省链接盘旋字体颜色 GetDefaultLinkFontInfo
        public void setDefaultLinkFont(Font font, uint color = 0xFF0000FF, uint hoverColor = 0xFFD3215F)
        {
            if (font == null)
            {
                return;
            }

            mDefaultLinkFont = font;
            Color defaultColor = Color.FromArgb((int)color);

            mDefaultLinkFont = font;
            mDefaultLinkFontColor = defaultColor;
        }
        public bool addFont(ref Font font)
        {
            if (font == null)
            {
                return false;
            }

            foreach (var f in mCustomFonts)
            {
                if (f == font)
                {
                    return false;
                }
            }

            mCustomFonts.Add(font);

            return true;
        }
        public Font addFont(string fontName, int size, bool bold, bool underLine, bool italic)
        {
            int style = 0;
            style = bold ? (int)FontStyle.Bold | style : style;
            style = underLine ? (int)FontStyle.Underline | style : style;
            style = italic ? (int)FontStyle.Italic | style : style;

            Font newFont = new Font(fontName, size, (FontStyle)style);

            if (addFont(ref newFont))
            {
                return newFont;
            }

            return null;
        }
        public bool addFontAt(int idx, ref Font font)
        {
            if (idx < 0)
            {
                return false;
            }
            idx = idx > mCustomFonts.Count ? mCustomFonts.Count : idx;
            foreach (var f in mCustomFonts)
            {
                if (f == font)
                {
                    return false;
                }
            }

            mCustomFonts.Insert(idx, font);

            return true;
        }
        public Font addFontAt(int idx, string fontName, int size, bool bold, bool underLine)
        {
            int style = 0;
            style = bold ? (int)FontStyle.Bold | style : style;
            style = underLine ? (int)FontStyle.Underline | style : style;

            Font newFont = new Font(fontName, size, (FontStyle)style);

            if (addFontAt(idx, ref newFont) == false)
            {
                newFont.Dispose();
                newFont = null;
                return null;
            }

            return newFont;
        }
        public Font getFont(int idx)
        {
            if (mParentResourcePM != null)
            {
                return mParentResourcePM.getFont(idx);
            }
            if (idx < 0 || idx >= mCustomFonts.Count || mCustomFonts.Count == 0)
            {
                return getDefaultFont();
            }

            return mCustomFonts[idx];
        }
        public bool findFont(ref Font font)
        {
            if (mParentResourcePM != null)
            {
                return mParentResourcePM.findFont(ref font);
            }

            foreach (var f in mCustomFonts)
            {
                if (f == font)
                {
                    return true;
                }
            }

            return false;
        }
        public bool removeFont(ref Font font)
        {
            foreach (var f in mCustomFonts)
            {
                if (f == font)
                {
                    return mCustomFonts.Remove(font);
                }
            }

            return true;
        }
        public bool removeFontAt(int idx)
        {
            if (idx < 0 || idx >= mCustomFonts.Count)
            {
                return false;
            }

            return mCustomFonts.Remove(mCustomFonts[idx]);
        }
        public void removeAllFonts()
        {
            mCustomFonts.Clear();
        }
        // 获取指定字体信息 GetFontInfo


        public TImageInfo getImage(string bitmap)
        {
            if (bitmap == null || bitmap == "")
            {
                throw new Exception("文件名不能为空");
            }
            if (mParentResourcePM != null)
            {
                return mParentResourcePM.getImage(bitmap);
            }
            if (mImageHash.ContainsKey(bitmap) == false)
            {
                if (addImage(bitmap) != null)
                {
                    return mImageHash[bitmap];
                }
            }

            return mImageHash[bitmap];
        }
        public TImageInfo getImageEx(string bitmap, string type = "", int mask = 0)
        {
            if (bitmap == null || bitmap == "")
            {
                return null;
                //throw new Exception("文件名不能为空");
            }

            if (mImageHash.ContainsKey(bitmap) == false)
            {
                if (addImage(bitmap, type, mask) != null)
                {
                    return mImageHash[bitmap];
                }
            }

            return mImageHash[bitmap];
        }
        public TImageInfo addImage(string bitmapFileName, string type = "", int mask = 0)
        {
            if (File.Exists(bitmapFileName) == false)
            {
                string msg = string.Format("找不到该图片\r\n{0}", bitmapFileName);
                //MessageBox.Show(msg);
                return null;
            }
            if (mImageHash.ContainsKey(bitmapFileName) == true)
            {
                return mImageHash[bitmapFileName];
            }

            TImageInfo image = new TImageInfo();
            Bitmap bitmap = new Bitmap(bitmapFileName);
            bitmap.MakeTransparent(Color.FromArgb(mask));

            image.mBitmap = bitmap;
            image.mX = bitmap.Width;
            image.mY = bitmap.Height;
            image.mAlphaChannel = false;

            mImageHash.Add(bitmapFileName, image);

            return image;
        }
        public bool removeImage(string bitmapFileName)
        {
            if (mImageHash.ContainsKey(bitmapFileName) == false)
            {
                return false;
            }

            return mImageHash.Remove(bitmapFileName);
        }
        public void removeAllImages()
        {
            foreach (var b in mImageHash)
            {
                b.Value.mBitmap.Dispose();
            }

            mImageHash.Clear();
        }

        public void addDefaultAttributeList(string controlName, string attrList)
        {
            if (mDefaultAttrHash.ContainsKey(controlName) == false)
            {
                mDefaultAttrHash.Add(controlName, attrList);
            }
            else
            {
                mDefaultAttrHash.Remove(controlName);

                mDefaultAttrHash.Add(controlName, attrList);
            }
        }
        public string getDefaultAttributeList(string controlName)
        {
            if (mDefaultAttrHash.ContainsKey(controlName) == false &&
                mParentResourcePM != null)
            {
                return mParentResourcePM.getDefaultAttributeList(controlName);
            }

            if (mDefaultAttrHash.ContainsKey(controlName))
            {
                return mDefaultAttrHash[controlName];
            }

            return "";
        }
        public bool removeDefaultAttributeList(string controlName)
        {
            if (mDefaultAttrHash.ContainsKey(controlName) == true)
            {
                return mDefaultAttrHash.Remove(controlName);
            }

            return false;
        }
        public bool removeAllDefaultAttributeList()
        {
            mDefaultAttrHash.Clear();

            return true;
        }

        public bool attachDialog(ref ControlUI control)
        {
            if (mWndPaint == null)
            {
                return false;
            }
            ControlUI curControl = null;
            setFocus(ref curControl);

            mEventKey = null;
            mEventHover = null;
            mEventClick = null;

            if (mRoot != null)
            {
                // 删除已经存在的控件树
                mPostPaintControls.Clear();
                //mDelayedCleanup.Add(mRoot);
            }
            mRoot = control;
            mUpdateNeeded = true;
            mFirstLayout = true;
            mFocusNeeded = true;

            return initControls(control);
        }
        public bool initControls(ControlUI control, ControlUI parent = null)
        {
            if (control == null)
            {
                return false;
            }
            control.setManager(this, parent != null ? parent : control.getParent());
            object obj = this;
            control.findControl(findControlFromNameHash, ref obj, ControlFlag.UIFIND_ALL);

            return true;
        }
        // 释放控件 ReapObjects

        public ControlUI getFocus()
        {
            return mFocus;
        }
        public void setFocus(ref ControlUI control)
        {
            if (mWndPaint != null && mWndPaint.Focused)
            {
                mWndPaint.Focus();
            }

            if (mFocus == control)
            {
                return;
            }

            if (mFocus != null)
            {
                TEventUI newEvent = new TEventUI();
                newEvent.mType = (Int16)EVENTTYPE_UI.UIEVENT_KILLFOCUS;
                newEvent.mSender = control;
                newEvent.mTimestamp = GetTickCount();
                mFocus.eventProc(ref newEvent);
                sendNotify(mFocus, "killfocus");
                mFocus = null;
            }

            if (control != null &&
                control.getManager() == this &&
                control.isVisible() &&
                control.isEnabled())
            {
                mFocus = control;
                TEventUI newEvent = new TEventUI();
                newEvent.mType = (Int16)EVENTTYPE_UI.UIEVENT_SETFOCUS;
                newEvent.mSender = control;
                newEvent.mTimestamp = GetTickCount();
                mFocus.eventProc(ref newEvent);
                sendNotify(mFocus, "setfocus");
            }

        }

        public bool setNextTabControl(bool forward = true)
        {
            if (mUpdateNeeded && forward)
            {
                mFocusNeeded = true;
                mWndPaint.Invalidate(null, false);

                return true;
            }

            FindTabInfo info1 = new FindTabInfo();
            info1.mFocus = mFocus;
            info1.mForward = forward;
            object obj = (object)info1;
            ControlUI control = mRoot.findControl(findControlFromTab, ref obj, (uint)(ControlFlag.UIFIND_ENABLED | ControlFlag.UIFIND_VISIBLE | ControlFlag.UIFIND_ME_FIRST));
            if (control == null)
            {
                if (forward)
                {
                    FindTabInfo info2 = new FindTabInfo();
                    info2.mFocus = forward ? null : info1.mLast;
                    info2.mForward = forward;
                    obj = (object)info2;
                    control = mRoot.findControl(findControlFromTab, ref obj, (uint)(ControlFlag.UIFIND_VISIBLE | ControlFlag.UIFIND_ENABLED | ControlFlag.UIFIND_ME_FIRST));
                }
                else
                {
                    control = info1.mLast;
                }
            }
            if (control != null)
            {
                setFocus(ref control);
            }
            mFocusNeeded = false;

            return true;
        }

        // 创建定时器
        public bool setTimer(ControlUI control, int timerID, int elapse)
        {
            foreach (var timer in mTimers)
            {
                if (timer.mSender == control &&
                    timer.mForm == mWndPaint &&
                    timer.mLocalID == timerID)
                {
                    if (timer.mKilled == true)
                    {
                        timer.mTimer.Interval = elapse;
                        timer.mTimer.Start();
                        timer.mKilled = false;
                        return true;
                    }

                    return false;
                }
            }

            mTimerID = (++mTimerID) % 0xFF;
            TimerInfo timerInfo = new TimerInfo();
            timerInfo.mTimer.Interval = elapse;
            timerInfo.mForm = mWndPaint;
            timerInfo.mSender = control;
            timerInfo.mLocalID = timerID;
            timerInfo.mWinTimer = mTimerID;
            timerInfo.mKilled = false;
            timerInfo.mTimer.Tick += timerEvent;
            timerInfo.mTimer.Start();
            mTimers.Add(timerInfo);

            return true;
        }
        // 删除定时器
        public bool killTimer(ControlUI control, int timerID)
        {
            foreach (var timer in mTimers)
            {
                if (timer.mSender == control &&
                    timer.mForm == mWndPaint &&
                    timer.mLocalID == timerID)
                {
                    if (timer.mKilled == false)
                    {
                        timer.mTimer.Stop();
                        timer.mKilled = true;

                        return true;
                    }
                }
            }

            return false;
        }
        private void timerEvent(object Sender, EventArgs e)
        {
            foreach (var timer in mTimers)
            {
                if (timer.mTimer == Sender)
                {
                    if (timer.mForm == mWndPaint &&
                        timer.mKilled == false)
                    {
                        TEventUI newEvent = new TEventUI();
                        newEvent.mType = (int)EventTypeUI.UIEVENT_TIMER;
                        newEvent.mWParam = timer.mLocalID;
                        newEvent.mTimestamp = GetTickCount();
                        timer.mSender.eventProc(ref newEvent);
                    }
                }
            }
        }
        public bool addNotifier(INotifyUI control)
        {
            mNotifiers.Add(control);

            return true;
        }
        public bool removeNotifier(ref INotifyUI notifier)
        {
            foreach (var value in mNotifiers)
            {
                if (notifier == value)
                {
                    return mNotifiers.Remove(notifier);
                }
            }

            return false;
        }
        public void sendNotify(ref TNofityUI msg)
        {
            msg.mMousePos = mLastMousePos;
            msg.mTimestamp = GetTickCount();
            foreach (var no in mNotifiers)
            {
                no.notify(ref msg);
            }
        }
        public void sendNotify(ControlUI control, string message, object wParam = null, object lParam = null)
        {
            TNofityUI msg = new TNofityUI();
            msg.mSender = control;
            msg.mType = message;
            msg.mWParam = wParam;
            msg.mLParam = lParam;
            sendNotify(ref msg);
        }

        public bool addMessageFilter(ref IMessageFilterUI filter)
        {
            mMessageFilters.Add(filter);

            return true;
        }
        public bool removeMessageFilter(ref IMessageFilterUI filter)
        {
            foreach (var value in mMessageFilters)
            {
                if (value == filter)
                {
                    return mMessageFilters.Remove(value);
                }
            }

            return false;
        }
        public int getPostPaintCount()
        {
            return mPostPaintControls.Count;
        }
        public bool addPostPaint(ref ControlUI control)
        {
            mPostPaintControls.Add(control);

            return true;
        }
        public bool removePostPaint(ref ControlUI control)
        {
            foreach (var value in mPostPaintControls)
            {
                if (value == control)
                {
                    return mPostPaintControls.Remove(control);
                }
            }

            return false;
        }
        public bool setPostPaintIndex(ref ControlUI control, int idx)
        {
            removePostPaint(ref control);

            mPostPaintControls.Insert(idx, control);

            return true;
        }

        public ControlUI getRoot()
        {
            return mRoot;
        }
        public ControlUI findControl(ref Point point)
        {
            object obj = (object)point;
            return mRoot.findControl(findControlFromPoint, ref obj, (uint)(ControlFlag.UIFIND_VISIBLE | ControlFlag.UIFIND_HITTEST | ControlFlag.UIFIND_TOP_FIRST));
        }
        public ControlUI findControl(string name)
        {
            if (mNameHash.ContainsKey(name) == false)
            {
                return null;
            }

            return mNameHash[name];
        }

        public static void messageLoop()
        {

        }
        // 翻译消息 TranslateMessage

        protected Point getPoint(ref object lParam)
        {
            IntPtr ptr = (IntPtr)lParam;
            int pos = (int)ptr;
            Point point = new Point(GET_X_LPARAM(pos), GET_Y_LPARAM(pos));

            return point;
        }
        public bool messageHandler(uint msg, ref object wParam, ref object lParam, ref int lRes)
        {
            if (mRoot == null)
            {
                return false;
            }
            WindowMessage msgID = (WindowMessage)msg;

            {
                // 输出调试信息

                switch (msgID)
                {
                    case WindowMessage.WM_NCPAINT:
                    case WindowMessage.WM_NCHITTEST:
                    case WindowMessage.WM_SETCURSOR:
                        break;
                    default:
                        {
                            //Console.WriteLine("MSG:{0},{1}", msgID, GetTickCount());

                            break;
                        }
                }
            }

            if (mWndPaint == null)
            {
                return false;
            }

            {
                // 遍历事件侦听器
                foreach (var filter in mMessageFilters)
                {
                    bool handled = false;
                    int result = filter.MessageHandler(msg, wParam, lParam, ref handled);
                    if (handled)
                    {
                        lRes = result;
                        return true;
                    }
                }
            }

            // 用户事件处理
            switch (msgID)
            {
                case WindowMessage.WM_APP + 1:
                    {
                        // 清除控件树

                        break;
                    }
                case WindowMessage.WM_CLOSE:
                    {
                        {
                            // 发送关闭中事件
                            TEventUI newEvent = new TEventUI();
                            newEvent.mMousePos = mLastMousePos;
                            newEvent.mTimestamp = GetTickCount();
                            if (mEventHover != null)
                            {
                                newEvent.mType = (int)EVENTTYPE_UI.UIEVENT_MOUSELEAVE;
                                newEvent.mSender = mEventHover;
                                mEventHover.eventProc(ref newEvent);
                            }

                            if (mEventClick != null)
                            {
                                newEvent.mType = (int)EVENTTYPE_UI.UIEVENT_BUTTONUP;
                                newEvent.mSender = mEventClick;
                                mEventClick.eventProc(ref newEvent);
                            }

                            ControlUI curControl = null;
                            setFocus(ref curControl);
                        }

                        // 需要设置父窗口获得焦点

                        break;
                    }
                case WindowMessage.WM_SIZE:
                    {
                        {
                            if (mFocus != null)
                            {
                                TEventUI newEvent = new TEventUI();
                                newEvent.mType = (int)EVENTTYPE_UI.UIEVENT_WINDOWSIZE;
                                newEvent.mTimestamp = GetTickCount();
                                mFocus.eventProc(ref newEvent);
                            }

                            mRoot.needUpdate();
                        }
                        break;
                    }
                case WindowMessage.WM_ERASEBKGND:
                    {
                        // We'll do the painting here...
                        lRes = 1;
                        return true;
                    }

                case WindowMessage.WM_MOUSEHOVER:
                    {
                        {
                            // 鼠标盘旋消息，用于显示Tooltip
                        }
                        break;
                    }
                case WindowMessage.WM_MOUSELEAVE:
                    {
                        {
                            // 鼠标离开消息，用于隐藏Tooltip
                        }

                        break;
                    }
                case WindowMessage.WM_MOUSEMOVE:
                    {

                        {
                            // 产生鼠标消息发送给控件,鼠标离开，鼠标进入，鼠标移动三个消息
                            Point point = getPoint(ref lParam);
                            ControlUI curHover = findControl(ref point);
                            if (curHover != null && curHover.getManager() != this)
                            {
                                break;
                            }

                            TEventUI newEvent = new TEventUI();
                            newEvent.mMousePos = point;
                            newEvent.mTimestamp = GetTickCount();
                            if (curHover != mEventHover && mEventHover != null)
                            {
                                newEvent.mType = (int)EVENTTYPE_UI.UIEVENT_MOUSELEAVE;
                                newEvent.mSender = curHover;
                                mEventHover.eventProc(ref newEvent);
                                mEventHover = null;
                                // 关闭Tooltip
                            }
                            if (curHover != mEventHover && curHover != null)
                            {
                                newEvent.mType = (int)EVENTTYPE_UI.UIEVENT_MOUSEENTER;
                                newEvent.mSender = mEventHover;
                                curHover.eventProc(ref newEvent);
                                mEventHover = curHover;
                            }
                            if (mEventClick != null)
                            {
                                newEvent.mType = (int)EVENTTYPE_UI.UIEVENT_MOUSEMOVE;
                                newEvent.mSender = null;
                                mEventClick.eventProc(ref newEvent);
                            }
                            else if (curHover != null)
                            {
                                newEvent.mType = (int)EVENTTYPE_UI.UIEVENT_MOUSEMOVE;
                                newEvent.mSender = null;
                                curHover.eventProc(ref newEvent);
                            }

                        }

                        break;
                    }
                case WindowMessage.WM_LBUTTONDOWN:
                    {
                        {
                            // 设置窗口获得焦点
                            mWndPaint.Focus();

                            Point point = getPoint(ref lParam);
                            mLastMousePos = point;
                            ControlUI curControl = findControl(ref point);
                            if (curControl == null)
                            {
                                break;
                            }

                            if (curControl.getManager() != this)
                            {
                                break;
                            }

                            mEventClick = curControl;
                            curControl.setFocus();
                            IntPtr ptr = (IntPtr)wParam;
                            int pos = (int)ptr;
                            TEventUI newEvent = new TEventUI();
                            newEvent.mType = (int)EVENTTYPE_UI.UIEVENT_BUTTONDOWN;
                            newEvent.mWParam = wParam;
                            newEvent.mLParam = lParam;
                            newEvent.mMousePos = point;
                            newEvent.mKeyState = (Int16)pos;
                            newEvent.mTimestamp = GetTickCount();
                            curControl.eventProc(ref newEvent);

                            mMouseCapture = true;
                        }
                        break;
                    }
                case WindowMessage.WM_LBUTTONUP:
                    {
                        {
                            Point point = getPoint(ref lParam);
                            mLastMousePos = point;
                            if (mEventClick == null)
                            {
                                break;
                            }

                            mMouseCapture = false;
                            IntPtr ptr = (IntPtr)wParam;
                            int pos = (int)ptr;
                            TEventUI newEvent = new TEventUI();
                            newEvent.mType = (int)EVENTTYPE_UI.UIEVENT_BUTTONUP;
                            newEvent.mWParam = wParam;
                            newEvent.mLParam = lParam;
                            newEvent.mMousePos = point;
                            newEvent.mKeyState = (Int16)pos;
                            newEvent.mTimestamp = GetTickCount();
                            mEventClick.eventProc(ref newEvent);
                            mEventClick = null;
                        }
                        break;
                    }
                case WindowMessage.WM_LBUTTONDBLCLK:
                    {
                        {
                            Point point = getPoint(ref lParam);

                            mLastMousePos = point;
                            ControlUI curControl = findControl(ref point);

                            if (curControl == null)
                            {
                                break;
                            }

                            if (curControl.getManager() != this)
                            {
                                break;
                            }
                            IntPtr p = (IntPtr)wParam;
                            int param = (int)p;
                            TEventUI newEvent = new TEventUI();
                            newEvent.mType = (int)EVENTTYPE_UI.UIEVENT_DBLCLICK;
                            newEvent.mMousePos = point;
                            newEvent.mKeyState = (Int16)param;
                            newEvent.mTimestamp = GetTickCount();
                            curControl.eventProc(ref newEvent);
                            mEventClick = curControl;

                            mMouseCapture = true;
                        }

                        break;
                    }
                case WindowMessage.WM_CHAR:
                    {
                        {
                            if (mFocus == null)
                            {
                                break;
                            }
                            IntPtr ptr = (IntPtr)wParam;
                            int key = (int)ptr;

                            TEventUI newEvent = new TEventUI();
                            newEvent.mType = (int)EVENTTYPE_UI.UIEVENT_CHAR;
                            newEvent.mKey = (char)key;
                            newEvent.mMousePos = mLastMousePos;
                            newEvent.mKeyState = (Int16)mapKeyState();
                            newEvent.mTimestamp = GetTickCount();
                            mFocus.eventProc(ref newEvent);
                        }

                        break;
                    }
                case WindowMessage.WM_KEYDOWN:
                    {
                        {
                            if (mFocus == null)
                            {
                                break;
                            }
                            IntPtr ptr = (IntPtr)wParam;
                            int key = (int)ptr;

                            TEventUI newEvent = new TEventUI();
                            newEvent.mType = (int)EVENTTYPE_UI.UIEVENT_KEYDOWN;
                            newEvent.mKey = (char)key;
                            newEvent.mMousePos = mLastMousePos;
                            newEvent.mKeyState = (Int16)mapKeyState();
                            newEvent.mTimestamp = GetTickCount();
                            mFocus.eventProc(ref newEvent);
                            mEventKey = mFocus;
                        }

                        break;
                    }
                case WindowMessage.WM_KEYUP:
                    {
                        {
                            if (mEventKey == null)
                            {
                                break;
                            }
                            IntPtr ptr = (IntPtr)wParam;
                            int key = (int)ptr;

                            TEventUI newEvent = new TEventUI();
                            newEvent.mType = (int)EVENTTYPE_UI.UIEVENT_KEYUP;
                            newEvent.mKey = (char)key;
                            newEvent.mMousePos = mLastMousePos;
                            newEvent.mKeyState = (Int16)mapKeyState();
                            newEvent.mTimestamp = GetTickCount();
                            mEventKey.eventProc(ref newEvent);
                            mEventKey = null;
                        }

                        break;
                    }
                case WindowMessage.WM_SETCURSOR:
                    {
                        IntPtr ptr = (IntPtr)lParam;
                        int state = (int)LOWORD((int)ptr);
                        if (state != (int)HitTestFlags.HTCLIENT)
                        {
                            break;
                        }
                        if (mMouseCapture)
                        {
                            return true;
                        }
                        Point pos = Control.MousePosition;
                        pos = getPaintWindow().PointToClient(pos);
                        ControlUI curControl = findControl(ref pos);
                        if (curControl == null)
                        {
                            break;
                        }
                        if ((curControl.getControlFlags() & (int)ControlFlag.UIFLAG_SETCURSOR) == 0)
                        {
                            break;
                        }
                        TEventUI newEvent = new TEventUI();
                        newEvent.mType = (int)EventTypeUI.UIEVENT_SETCURSOR;
                        newEvent.mWParam = wParam;
                        newEvent.mLParam = lParam;
                        newEvent.mMousePos = pos;
                        newEvent.mKey = (char)mapKeyState();
                        newEvent.mTimestamp = GetTickCount();
                        curControl.eventProc(ref newEvent);

                        return true;
                    }
                //case WindowMessage.WM_PAINT:
                //    {
                //        return true;
                //    }
            }

            return false;
        }
        public bool paintMessageEvent(Rectangle rcPaint)
        {
            {
                // 重新布局控件大小和位置,在WM_SIZE消息里里设置控件大小和位置会浪费大量时间
                if (mUpdateNeeded && mWndPaint != null)
                {
                    Rectangle rectClient;
                    {
                        // 判断是否需要重新分配后台缓存
                        if (mRoot.isUpdateNeeded() || mRenderBufferManager.isNeedAllocate())
                        {
                            rectClient = new Rectangle(new Point(0, 0), mWndPaint.Size);
                            mRenderBufferManager.tryAllocateBuffer(rectClient);
                        }
                    }

                    mUpdateNeeded = false;
                    rectClient = new Rectangle(new Point(0, 0), mWndPaint.Size);
                    if (rectClient.IsEmpty == false)
                    {
                        if (mRoot.isUpdateNeeded())
                        {
                            mRoot.setPos(rcPaint);
                        }
                        else
                        {
                            ControlUI control = null;
                            object data = null;
                            while ((control = mRoot.findControl(findControlFromUpdate, ref data, (int)ControlFlag.UIFIND_VISIBLE | ControlFlag.UIFIND_ME_FIRST)) != null)
                            {
                                control.setPos(control.getPos());
                            }
                        }

                        // 首次设置布局时，通知窗口大小已经初始化，有动画时可以在这里初始化
                        if (mFirstLayout)
                        {
                            mFirstLayout = false;
                            sendNotify(mRoot, "windowinit");
                        }
                    }
                }
            }

            {
                // 设置第一个控件获得焦点
                if (mFocusNeeded)
                {
                    setNextTabControl();
                }
            }

            {
                // 渲染控件树,可以使用双缓冲避免闪烁
                Rectangle rectClient = new Rectangle(mWndPaint.Location, mWndPaint.Size);
                Graphics g = mWndPaint.CreateGraphics();
                Graphics gg = mRenderBufferManager.mGraphics;
                Bitmap buffer = mRenderBufferManager.getBackBuffer();
                mRoot.doPaint(ref gg, ref buffer, rcPaint);

                {
                    // 交换前台、后台缓存，显示最新的后台内容
                    mRenderBufferManager.switchBuffer();
                    g.DrawImage(mRenderBufferManager.getFrontBuffer(), new Rectangle(0, 0, mWndPaint.Size.Width, mWndPaint.Size.Height), new Rectangle(0, 0, mWndPaint.Size.Width, mWndPaint.Size.Height), GraphicsUnit.Pixel);
                }

                {
                    // 因为窗口大小发生改变后，后台缓存会首先自适应，所以尝试重新分配交换后的后台缓存
                    mRenderBufferManager.tryAllocateBackBuffer(rectClient);
                }
            }

            return true;
        }
        public bool removePostPaint(ControlUI pControl)
        {
            for (int i = 0; i < mPostPaintControls.Count; i++)
            {
                if ((mPostPaintControls[i]) == pControl)
                {
                    return mPostPaintControls.Remove(pControl);
                }
            }
            return false;
        }
        public static Int16 LOWORD(int x)
        {
            Int16 result = (Int16)((short)x & (int)0xffff);

            return result;
        }
        public static Int16 HIWORD(int x)
        {
            Int16 result = (Int16)((x >> 16) & (int)0xffff);

            return result;
        }
        public static int GET_X_LPARAM(int x)
        {
            return ((int)(short)LOWORD(x));
        }
        public static int GET_Y_LPARAM(int x)
        {
            return ((int)(short)HIWORD(x));
        }
        public bool preMessageHandler(uint msg, object wParam, object lParam, ref int lRes)
        {
            WindowMessage msgID = (WindowMessage)msg;

            switch (msgID)
            {
                case WindowMessage.WM_KEYDOWN:
                    {
                        Keys key = (Keys)wParam;

                        {
                            // 切换焦点控件
                            if (Keys.Tab == key)
                            {
                                setNextTabControl(true);

                                return true;
                            }
                        }

                        {
                            // 执行缺省的ok或者cancel按钮单击事件
                            if (key == Keys.Return)
                            {
                                ControlUI curControl = findControl("ok");
                                if (curControl != null && mFocus != curControl)
                                {
                                    if (mFocus == null || (mFocus.getControlFlags() & ControlFlag.UIFLAG_WANTRETURN) == 0)
                                    {
                                        curControl.activate();

                                        return true;
                                    }
                                }
                            }

                            if (key == Keys.Escape)
                            {
                                ControlUI curControl = findControl("cancel");
                                if (curControl != null)
                                {
                                    curControl.activate();
                                    return true;
                                }
                            }
                        }

                        break;
                    }
                case WindowMessage.WM_SYSCHAR:
                    {
                        {
                            // 响应ALT快捷组合键功能
                            FindShortCut shortcut = new FindShortCut();
                            shortcut.mChar = char.ToUpper((char)wParam);
                            object data = (object)shortcut;
                            ControlUI curControl = mRoot.findControl(findControlFromShortcut,
                                ref data,
                                ControlFlag.UIFIND_ENABLED | ControlFlag.UIFIND_ME_FIRST | ControlFlag.UIFIND_TOP_FIRST);
                            if (curControl != null)
                            {
                                curControl.setFocus();
                                curControl.activate();

                                return true;
                            }
                        }

                        break;
                    }
                case WindowMessage.WM_SYSKEYDOWN:
                    {
                        {
                            if (mFocus != null)
                            {
                                TEventUI newEvent = new TEventUI();
                                newEvent.mType = (int)EventTypeUI.UIEVENT_SYSKEY;
                                newEvent.mKey = (char)wParam;
                                newEvent.mMousePos = mLastMousePos;
                                newEvent.mKeyState = (Int16)mapKeyState();
                                newEvent.mTimestamp = (int)GetTickCount();
                                mFocus.eventProc(ref newEvent);
                            }
                        }

                        break;
                    }
            }
            return true;
        }

        protected static ControlUI findControlFromNameHash(ref ControlUI control, ref object data)
        {
            PaintManagerUI manager = (PaintManagerUI)data;
            string name = control.getName();
            if (control.getName() == "")
            {
                return null;
            }
            manager.mNameHash.Add(name, control);

            return null;
        }
        protected static ControlUI findControlFromCount(ref ControlUI control, ref object data)
        {

            return null;

        }

        protected static ControlUI findControlFromPoint(ref ControlUI control, ref object data)
        {
            Point point = (Point)data;
            Rectangle rect = control.getPos();

            return rect.Contains(point) ? control : null;
        }
        protected static ControlUI findControlFromTab(ref ControlUI control, ref object data)
        {
            FindTabInfo info = (FindTabInfo)data;
            if (info.mFocus == control)
            {
                if (info.mForward)
                {
                    info.mNextIsIt = true;
                }
                return info.mForward ? null : info.mLast;
            }
            if ((control.getControlFlags() & ControlFlag.UIFLAG_TABSTOP) == 0)
            {
                return null;
            }
            info.mLast = control;
            if (info.mNextIsIt)
            {
                return control;
            }
            if (info.mFocus == null)
            {
                return control;
            }

            return null;

        }
        protected static ControlUI findControlFromShortcut(ref ControlUI control, ref object data)
        {
            if (control.isVisible() == false)
            {
                return null;
            }

            FindShortCut fs = (FindShortCut)data;
            if (fs.mChar == char.ToUpper(control.getShortcut()))
            {
                fs.mPickNext = true;
            }
            if (control.getClass() == "LabelUI")
            {
                return null;
            }

            return fs.mPickNext ? control : null;
        }
        protected static ControlUI findControlFromUpdate(ref ControlUI control, ref object data)
        {
            return control.isUpdateNeeded() ? control : null;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetKeyState")]
        public static extern int GetKeyState(int nVirkey);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", EntryPoint = "GetTickCount")]
        public static extern uint GetTickCount();

        public static uint mapKeyState()
        {
            uint state = 0;
            if (GetKeyState((int)Keys.Control) < 0)
            {
                state |= (int)KeyState.MK_CONTROL;
            }
            if (GetKeyState((int)Keys.RButton) < 0)
            {
                state |= (int)KeyState.MK_LBUTTON;
            }
            if (GetKeyState((int)Keys.LButton) < 0)
            {
                state |= (int)KeyState.MK_RBUTTON;
            }
            if (GetKeyState((int)Keys.Shift) < 0)
            {
                state |= (int)KeyState.MK_SHIFT;
            }
            if (GetKeyState((int)Keys.Menu) < 0)
            {
                state |= (int)KeyState.MK_ALT;
            }

            return state;
        }
        public RenderBufferManager getBufferManager()
        {
            return mRenderBufferManager;
        }

        public void release()
        {
            if (mRenderBufferManager != null)
            {
                mRenderBufferManager.release();
                mRenderBufferManager = null;
            }

            if (mImageHash != null)
            {
                foreach (var image in mImageHash)
                {
                    image.Value.mBitmap.Dispose();
                }
                mImageHash.Clear();
            }

            if (mCustomFonts != null)
            {
                foreach (var font in mCustomFonts)
                {
                    font.Dispose();
                }
                mCustomFonts.Clear();
            }

            if (mTimers != null)
            {
                foreach (var timer in mTimers)
                {
                    timer.mTimer.Stop();
                    timer.mTimer.Dispose();
                }
                mTimers.Clear();
            }
        }
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public bool addOptionGroup(string groupName, ControlUI newControl)
        {
            if (mGroupList.ContainsKey(groupName))
            {
                foreach (var control in mGroupList[groupName])
                {
                    if (control == newControl)
                    {
                        return false;
                    }
                }
                mGroupList[groupName].Add(newControl);
            }
            else
            {
                List<ControlUI> list = new List<ControlUI>();
                list.Add(newControl);
                mGroupList.Add(groupName, list);
            }

            return true;
        }
        public List<ControlUI> getOptionGroup(string groupName)
        {
            if (mGroupList.ContainsKey(groupName))
            {
                return mGroupList[groupName];
            }
            return null;
        }
        public void removeOptionGroup(string groupName, ControlUI newControl)
        {
            if (mGroupList.ContainsKey(groupName))
            {
                foreach (var control in mGroupList[groupName])
                {
                    if (control == newControl)
                    {
                        mGroupList[groupName].Remove(newControl);
                        break;
                    }
                }

                if (mGroupList[groupName].Count == 0)
                {
                    mGroupList.Remove(groupName);
                }
            }
        }
        public void removeAllOptionGroups()
        {
            foreach (var i in mGroupList)
            {
                i.Value.Clear();
            }
            mGroupList.Clear();
        }
        public void removeAllTimers()
        {
            foreach (var i in mTimers)
            {
                if (i.mKilled == false && i.mTimer != null)
                {
                    i.mTimer.Stop();
                }
                if (i.mTimer != null)
                {
                    i.mTimer.Dispose();
                }

            }
            mTimers.Clear();
        }



        protected Form mWndPaint;

        protected Graphics mDcPaint;
        protected Graphics mDcOffscreen;
        protected Bitmap mBMPOffscreen;
        protected bool mShowUpdateRect;

        protected ControlUI mRoot;
        protected ControlUI mFocus;
        protected ControlUI mEventHover;
        protected ControlUI mEventClick;
        protected ControlUI mEventKey;

        protected Point mLastMousePos;
        protected Size mMinWindow;
        protected Size mInitWindowSize;
        protected Rectangle mSizeBox;
        protected Size mRoundCorner;
        protected Rectangle mRectCaption;
        protected UInt32 mTimerID;
        protected bool mFirstLayout;
        protected bool mUpdateNeeded;
        protected bool mFocusNeeded;
        protected bool mOffscreenPaint;
        protected bool mMouseTracking;
        protected bool mMouseCapture;

        protected List<INotifyUI> mNotifiers;
        protected List<TimerInfo> mTimers;
        protected List<IMessageFilterUI> mMessageFilters;
        protected List<ControlUI> mPostPaintControls;
        protected List<ControlUI> mDelayedCleanup;
        protected Dictionary<string, ControlUI> mNameHash;

        //
        protected PaintManagerUI mParentResourcePM;
        protected Color mDefaultDisabledColor;
        protected Font mDefaultFont;
        protected Color mDefaultFontColor;
        protected Font mDefaultBoldFont;
        protected Color mDefaultBoldFontColor;
        protected Font mDefaultLinkFont;
        protected Color mDefaultLinkFontColor;
        protected Color mDefaultLinkFontHoverColor;
        protected List<Font> mCustomFonts;

        protected Dictionary<string, TImageInfo> mImageHash;
        protected Dictionary<string, string> mDefaultAttrHash;

        protected static List<PaintManagerUI> mPreMessages;

        public RenderBufferManager mRenderBufferManager;

        protected Dictionary<string, List<ControlUI>> mGroupList;
    }
}
