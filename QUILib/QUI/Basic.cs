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
    public enum WindowMessage
    {
        WM_CREATE = 0x0001,
        WM_DESTROY = 0x0002,
        WM_MOVE = 0x0003,
        WM_SIZE = 0x0005,

        WM_ACTIVATE = 0x0006,

        WM_PAINT = 0x0317,
        WM_KEYFIRST = 0x0100,
        WM_KEYDOWN = 0x0100,
        WM_KEYUP = 0x0101,
        WM_CHAR = 0x0102,
        WM_DEADCHAR = 0x0103,
        WM_SYSKEYDOWN = 0x0104,
        WM_SYSKEYUP = 0x0105,
        WM_SYSCHAR = 0x0106,
        WM_SYSDEADCHAR = 0x0107,

        WM_INITDIALOG = 0x0110,
        WM_COMMAND = 0x0111,
        WM_SYSCOMMAND = 0x0112,
        WM_TIMER = 0x0113,
        WM_HSCROLL = 0x0114,
        WM_VSCROLL = 0x0115,
        WM_INITMENU = 0x0116,
        WM_INITMENUPOPUP = 0x0117,
        WM_MENUSELECT = 0x011F,
        WM_MENUCHAR = 0x0120,
        WM_ENTERIDLE = 0x0121,
        WM_MENURBUTTONUP = 0x0122,
        WM_MENUDRAG = 0x0123,
        WM_MENUGETOBJECT = 0x0124,
        WM_UNINITMENUPOPUP = 0x0125,
        WM_MENUCOMMAND = 0x0126,

        WM_MOUSEFIRST = 0x0200,
        WM_MOUSEMOVE = 0x0200,
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_LBUTTONDBLCLK = 0x0203,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205,
        WM_RBUTTONDBLCLK = 0x0206,
        WM_MBUTTONDOWN = 0x0207,
        WM_MBUTTONUP = 0x0208,
        WM_MBUTTONDBLCLK = 0x0209,
        WM_MOUSEWHEEL = 0x020A,

        WM_MOUSEHOVER = 0x02A1,
        WM_MOUSELEAVE = 0x02A3,

        WM_NCHITTEST = 0x0084,
        WM_NCPAINT = 0x0085,
        WM_SETCURSOR = 0x0020,

        WM_APP = 0x8000,

        WM_CLOSE = 0x0010,

        WM_KILLFOCUS = 0x0008,

        WM_USER = 0x0400,
        WM_ERASEBKGND = 0x0014

    }

    public enum ReflectedWindowMessage
    {
        OCM__BASE = ((int)WindowMessage.WM_USER + 0x1c00),
        OCM_COMMAND = (OCM__BASE + (int)WindowMessage.WM_COMMAND)
    }
    public enum EditControlCodes
    {
        EN_CHANGE = 0x0300
    }
    public enum PaintFlags
    {
        UISTATE_FOCUSED = 0x00000001,
        UISTATE_SELECTED = 0x00000002,
        UISTATE_DISABLED = 0x00000004,
        UISTATE_HOT = 0x00000008,
        UISTATE_PUSHED = 0x00000010,
        UISTATE_READONLY = 0x00000020,
        UISTATE_CAPTURED = 0x00000040
    }
    public enum EVENTTYPE_UI
    {
        UIEVENT__FIRST = 0,
        UIEVENT__KEYBEGIN,
        UIEVENT_KEYDOWN,
        UIEVENT_KEYUP,
        UIEVENT_CHAR,
        UIEVENT_SYSKEY,
        UIEVENT__KEYEND,
        UIEVENT__MOUSEBEGIN,
        UIEVENT_MOUSEMOVE,
        UIEVENT_MOUSELEAVE,
        UIEVENT_MOUSEENTER,
        UIEVENT_MOUSEHOVER,
        UIEVENT_BUTTONDOWN,
        UIEVENT_BUTTONUP,
        UIEVENT_DBLCLICK,
        UIEVENT_CONTEXTMENU,
        UIEVENT_SCROLLWHEEL,
        UIEVENT__MOUSEEND,
        UIEVENT_KILLFOCUS,
        UIEVENT_SETFOCUS,
        UIEVENT_WINDOWSIZE,
        UIEVENT_SETCURSOR,
        UIEVENT_MEASUREITEM,
        UIEVENT_DRAWITEM,
        UIEVENT_TIMER,
        UIEVENT_NOTIFY,
        UIEVENT_COMMAND,
        UIEVENT__LAST
    };
    public enum HitTestFlags
    {
        HTCLIENT = 1,
        HTCAPTION = 2
    }

    public static class ControlFlag
    {
        // Flags for CControlUI::GetControlFlags()
        public const UInt32 UIFLAG_TABSTOP = 0x00000001;
        public const UInt32 UIFLAG_SETCURSOR = 0x00000002;
        public const UInt32 UIFLAG_WANTRETURN = 0x00000004;

        // Flags for FindControl()
        public const UInt32 UIFIND_ALL = 0x00000000;
        public const UInt32 UIFIND_VISIBLE = 0x00000001;
        public const UInt32 UIFIND_ENABLED = 0x00000002;
        public const UInt32 UIFIND_HITTEST = 0x00000004;
        public const UInt32 UIFIND_TOP_FIRST = 0x00000008;
        public const UInt32 UIFIND_ME_FIRST = 0x80000000;

        // Flags for Draw Style
        public const UInt32 UIDRAWSTYLE_INPLACE = 0x00000001;
        public const UInt32 UIDRAWSTYLE_FOCUS = 0x00000002;

        // Flags for the CDialogLayout stretching
        public const UInt32 UISTRETCH_NEWGROUP = 0x00000001;
        public const UInt32 UISTRETCH_NEWLINE = 0x00000002;
        public const UInt32 UISTRETCH_MOVE_X = 0x00000004;
        public const UInt32 UISTRETCH_MOVE_Y = 0x00000008;
        public const UInt32 UISTRETCH_SIZE_X = 0x00000010;
        public const UInt32 UISTRETCH_SIZE_Y = 0x00000020;

        // Flags used for controlling the paint
        public const UInt32 UISTATE_FOCUSED = 0x00000001;
        public const UInt32 UISTATE_SELECTED = 0x00000002;
        public const UInt32 UISTATE_DISABLED = 0x00000004;
        public const UInt32 UISTATE_HOT = 0x00000008;
        public const UInt32 UISTATE_PUSHED = 0x00000010;
        public const UInt32 UISTATE_READONLY = 0x00000020;
        public const UInt32 UISTATE_CAPTURED = 0x00000040;
    }

    public interface INotifyUI
    {
         int notify(ref TNofityUI msg);
    }

    public class TImageInfo
    {
        public Bitmap mBitmap;
        public int mX;
        public int mY;
        public bool mAlphaChannel;
    }

    public class IMessageFilterUI
    {
        public IMessageFilterUI()
        {
        }
        public virtual int MessageHandler(UInt32 msgID, object wParam, object lParam, ref bool bHandled)
        {
            return 0;
        }
    }

    public class FindTabInfo
    {
        public ControlUI mFocus;
        public ControlUI mLast;
        public bool mForward;
        public bool mNextIsIt;
    }

    public class FindShortCut
    {
        public char mChar;
        public bool mPickNext;
    }

    public class TimerInfo
    {
        public TimerInfo()
        {
            mTimer = new Timer();
        }
        ~TimerInfo()
        {
            mSender = null;
            mForm = null;
            if(mTimer != null)
            {
                if(mKilled == false)
                {
                    mTimer.Stop();
                    mTimer.Dispose();
                    mTimer = null;
                }
            }
        }
        public ControlUI mSender;
        public int mLocalID;
        public Form mForm;
        public uint mWinTimer;
        public bool mKilled;
        public Timer mTimer;
    }

    public enum EventTypeUI
    {
        UIEVENT__FIRST = 0,
        UIEVENT__KEYBEGIN,
        UIEVENT_KEYDOWN,
        UIEVENT_KEYUP,
        UIEVENT_CHAR,
        UIEVENT_SYSKEY,
        UIEVENT__KEYEND,
        UIEVENT__MOUSEBEGIN,
        UIEVENT_MOUSEMOVE,
        UIEVENT_MOUSELEAVE,
        UIEVENT_MOUSEENTER,
        UIEVENT_MOUSEHOVER,
        UIEVENT_BUTTONDOWN,
        UIEVENT_BUTTONUP,
        UIEVENT_DBLCLICK,
        UIEVENT_CONTEXTMENU,
        UIEVENT_SCROLLWHEEL,
        UIEVENT__MOUSEEND,
        UIEVENT_KILLFOCUS,
        UIEVENT_SETFOCUS,
        UIEVENT_WINDOWSIZE,
        UIEVENT_SETCURSOR,
        UIEVENT_MEASUREITEM,
        UIEVENT_DRAWITEM,
        UIEVENT_TIMER,
        UIEVENT_NOTIFY,
        UIEVENT_COMMAND,
        UIEVENT__LAST
    }
    public enum KeyState
    {
        MK_LBUTTON = 0x0001,
        MK_RBUTTON = 0x0002,
        MK_SHIFT = 0x0004,
        MK_CONTROL = 0x0008,
        MK_MBUTTON = 0x0010,
        MK_ALT = 0x20
    }

    public class TNofityUI
    {
        public string mType;
        public ControlUI mSender;
        public uint mTimestamp;
        public Point mMousePos;
        public object mWParam;
        public object mLParam;
    }

    public delegate int WNDPROC(int wndHandle, UInt32 msgID, object wParam, object lParam);

    public class WindowWnd
    {
        public WindowWnd()
        {

        }
        ~WindowWnd()
        {

        }

        protected int mHandleWnd;
    }
    public enum FormatFlags
    {
        DT_TOP = 0x00000000,
        DT_LEFT = 0x00000000,
        DT_CENTER = 0x00000001,
        DT_RIGHT = 0x00000002,
        DT_VCENTER = 0x00000004,
        DT_BOTTOM = 0x00000008,
        DT_WORDBREAK = 0x00000010,
        DT_SINGLELINE = 0x00000020,
        DT_EXPANDTABS = 0x00000040,
        DT_TABSTOP = 0x00000080,
        DT_NOCLIP = 0x00000100,
        DT_EXTERNALLEADING = 0x00000200,
        DT_CALCRECT = 0x00000400,
        DT_NOPREFIX = 0x00000800,
        DT_INTERNAL = 0x00001000,
        DT_END_ELLIPSIS = 0x00008000
    }

    public class RenderBufferManager
    {

        public RenderBufferManager()
        {
        }
        ~RenderBufferManager()
        {
            release();
        }
        public void tryAllocateBuffer(Rectangle rectClient)
        {
            // 判断是否需要重新分配后台缓存
            if (mBackBuffer == null || mFrontBuffer == null)
            {
                Size sz = new Size(getBufferWidth(rectClient), getBufferHeight(rectClient));

                if (mBackBuffer == null)
                {
                    mBackBuffer = new Bitmap(sz.Width, sz.Height, PixelFormat.Format32bppPArgb);

                    mGraphics = Graphics.FromImage(mBackBuffer);
                }
                if (mFrontBuffer == null)
                {
                    mFrontBuffer = new Bitmap(sz.Width, sz.Height, PixelFormat.Format32bppPArgb);
                }
            }
            else
            {
                Size sz = new Size(getBufferWidth(rectClient), getBufferHeight(rectClient));
                if (mBackBuffer.Width < sz.Width || mBackBuffer.Height < sz.Height)
                {
                    mBackBuffer.Dispose();
                    mBackBuffer = null;
                    mBackBuffer = new Bitmap(sz.Width, sz.Height, PixelFormat.Format32bppPArgb);
                }
            }
        }

        public void tryAllocateBackBuffer(Rectangle rectClient)
        {
            // 因为窗口大小发生改变后，后台缓存会首先自适应，所以尝试重新分配后台缓存
            if (mFrontBuffer != null && mBackBuffer != null)
            {
                if (mFrontBuffer.Width != mBackBuffer.Width || mFrontBuffer.Height != mBackBuffer.Height)
                {
                    Size sz = new Size(getBufferWidth(rectClient), getBufferHeight(rectClient));
                    mBackBuffer.Dispose();
                    mBackBuffer = null;
                    mBackBuffer = new Bitmap(sz.Width, sz.Height, PixelFormat.Format32bppPArgb);

                    mGraphics = Graphics.FromImage(mBackBuffer);
                }
            }
        }

        // 交换缓存链
        public void switchBuffer()
        {
            Bitmap bitmap1 = mFrontBuffer;
            mFrontBuffer = mBackBuffer;
            mBackBuffer = bitmap1;

            mGraphics = Graphics.FromImage(mBackBuffer);
        }

        public Bitmap getFrontBuffer()
        {
            return mFrontBuffer;
        }
        public Bitmap getBackBuffer()
        {
            return mBackBuffer;
        }

        protected int getBufferWidth(Rectangle rectWindow)
        {
            int width = 0;

            if(rectWindow.Width < 400)
            {
                width = 400;
            }
            else if (rectWindow.Width < 800)
            {
                width = 800;
            }
            else if (rectWindow.Width < 1024)
            {
                width = 1024;
            }
            else if (rectWindow.Width < 1280)
            {
                width = 1280;
            }
            else if (rectWindow.Width < 1360)
            {
                width = 1360;
            }
            else if (rectWindow.Width < 1400)
            {
                width = 1400;
            }
            else if (rectWindow.Width < 1680)
            {
                width = 1680;
            }
            else if (rectWindow.Width < 1920)
            {
                width = 1920;
            }
            else
            {
                width = rectWindow.Width;
            }

            return width;
        }

        protected int getBufferHeight(Rectangle rectWindow)
        {
            int heigh = 0;
            if (rectWindow.Height < 300)
            {
                heigh = 300;
            }
            else if (rectWindow.Height < 600)
            {
                heigh = 600;
            }
            else if (rectWindow.Height < 720)
            {
                heigh = 720;
            }
            else if (rectWindow.Height < 768)
            {
                heigh = 768;
            }
            else if (rectWindow.Height < 800)
            {
                heigh = 800;
            }
            else if (rectWindow.Height < 900)
            {
                heigh = 900;
            }
            else if (rectWindow.Height < 1024)
            {
                heigh = 1024;
            }
            else if (rectWindow.Height < 1050)
            {
                heigh = 1050;
            }
            else if (rectWindow.Height < 1080)
            {
                heigh = 1080;
            }
            else
            {
                heigh = rectWindow.Height;
            }

            return heigh;
        }

        public void release()
        {
            if (mFrontBuffer != null)
            {
                mFrontBuffer.Dispose();
                mFrontBuffer = null;
            }
            if (mBackBuffer != null)
            {
                mBackBuffer.Dispose();
                mBackBuffer = null;
            }
        }

        public Graphics getGraphics()
        {
            if (mBackBuffer == null)
            {
                throw new Exception("后台缓存未分配");
            }
            mGraphics = Graphics.FromImage(mBackBuffer);

            return mGraphics;
        }

        public bool isNeedAllocate()
        {
            return mFrontBuffer == null || mBackBuffer == null;
        }

        // 前台图形缓存
        public Bitmap mFrontBuffer;
        // 后台图形缓存
        public Bitmap mBackBuffer;
        public Graphics mGraphics;
    }
}
