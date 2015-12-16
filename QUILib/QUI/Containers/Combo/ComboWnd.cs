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
    public class ComboWnd : Form
    {
        public ComboWnd()
        {
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.ShowInTaskbar = false;

            mManager = new PaintManagerUI();
        }
        ~ComboWnd()
        {
            if (mVerticalLayout != null)
            {
                mVerticalLayout.removeAll();
                mVerticalLayout = null;
            }
            if (mManager != null)
            {
                mManager.release();
                mManager = null;
            }
        }
        public void init(ComboUI pOwner)
        {
            mOwner = pOwner;
            mVerticalLayout = null;
            mOldSel = mOwner.getCurSel();

            // 使用显示器坐标显示组合控件窗口
            Size szDrop = mOwner.getDropBoxSize();
            Rectangle rcOwner = pOwner.getPos();
            Rectangle rc = rcOwner;
            rc.Y = rc.Bottom;
            rc.Height = szDrop.Height;

            if (szDrop.Width > 0)
            {
                rc.Width = rc.Left + szDrop.Width - rc.X;
            }

            Size szAvailable = new Size(rc.Right - rc.Left, rc.Bottom - rc.Top);
            int cyFixed = 0;
            for (int it = 0; it < pOwner.getCount(); it++)
            {
                ControlUI pControl = (ControlUI)pOwner.getItemAt(it);
                if (!pControl.isVisible()) continue;
                Size sz = pControl.estimateSize(szAvailable);
                cyFixed += sz.Height;
            }
            cyFixed += 4; // CVerticalLayoutUI 默认的Inset 调整
            rc.Height = Math.Min(cyFixed, szDrop.Height);


            rc = mOwner.getManager().getPaintWindow().RectangleToScreen(rc);

            Rectangle rcMonitor = Screen.PrimaryScreen.Bounds;
            Rectangle rcWork = Screen.PrimaryScreen.WorkingArea;
            rcWork.Offset(-rcWork.Left, -rcWork.Top);
            if (rc.Bottom > rcWork.Bottom)
            {
                rc.X = rcOwner.X;
                rc.Width = rcOwner.Right - rc.X;
                if (szDrop.Width > 0)
                {
                    rc.Width = szDrop.Width;
                }
                rc.Y = rcOwner.Y - Math.Min(cyFixed, szDrop.Height);
                rc.Height = rcOwner.Top - rc.Y;
                rc = mOwner.getManager().getPaintWindow().RectangleToScreen(rc);
            }
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.ShowInTaskbar = false;
            mRectClient = rc;

            this.StartPosition = FormStartPosition.Manual;
            this.ClientSize = new Size(rc.Width, rc.Height);
            this.Location = new Point(rc.Left-1, rc.Top);


            this.Show();
        }
        public string getWindowClassName()
        {
            return "ComboWnd";
        }
        public void onFinalMessage()
        {
            mOwner.mWindow = null;
            mOwner.mButtonState &= ~(int)PaintFlags.UISTATE_PUSHED;
            mOwner.invalidate();
        }
        public bool handleMessage(int uMsg, ref object wParam, ref object lParam,ref int lRes)
        {
            if (uMsg == (int)WindowMessage.WM_CREATE)
            {
                Form frm = this;
                mManager.init(ref frm);
                // 给下拉列表子控件树重新设置父容器以及资源管理器,在组合框窗口关闭后需要还原该子树控件的父容器以及资源管理器
                mVerticalLayout = new VerticalLayoutUI();
                mManager.useParentResource(mOwner.getManager());
                mVerticalLayout.setManager(mManager, null);
                string pDefaultAttributes = mOwner.getManager().getDefaultAttributeList("VerticalLayout");
                if (pDefaultAttributes != "")
                {
                    mVerticalLayout.applyAttributeList(pDefaultAttributes);
                }
                mVerticalLayout.setInset(new Rectangle(2, 2, 0, 0));
                mVerticalLayout.setBackColor(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                mVerticalLayout.setBorderColor(Color.FromArgb(0xFF, 0x85, 0xE4, 0xFF));
                mVerticalLayout.setBorderSize(2);
                mVerticalLayout.setAutoDestroy(false);
                mVerticalLayout.enableScrollBar();
                mVerticalLayout.applyAttributeList(mOwner.getDropBoxAttributeList());
                for (int i = 0; i < mOwner.getCount(); i++)
                {
                    mVerticalLayout.add((ControlUI)mOwner.getItemAt(i));
                }
                ControlUI rootNode = (ControlUI)mVerticalLayout;
                mManager.attachDialog(ref rootNode);

                this.ClientSize = new Size(mRectClient.Width, mRectClient.Height);

                return true;
            }
            else if (uMsg == (int)WindowMessage.WM_SIZE)
            {
                this.ClientSize = new Size(mRectClient.Width, mRectClient.Height);

                return true;
            }
            else if (uMsg == (int)WindowMessage.WM_ERASEBKGND)
            {
                lRes = 1;

                return true;
            }

            else if (uMsg == (int)WindowMessage.WM_CLOSE)
            {
                mOwner.setManager(mOwner.getManager(), mOwner.getParent());
                mOwner.setPos(mOwner.getPos());
                mOwner.setFocus();
            }
            else if (uMsg == (int)WindowMessage.WM_LBUTTONUP)
            {
                Point pt = Control.MousePosition;
                pt = mManager.getPaintWindow().PointToClient(pt);
                ControlUI pControl = mManager.findControl(ref pt);
                if (pControl != null && pControl.getClass() != "ScrollbarUI")
                {
                    PaintManagerUI.PostMessage(this.Handle, (int)WindowMessage.WM_KILLFOCUS, 0, 0);
                }
            }

            else if (uMsg == (int)WindowMessage.WM_KEYDOWN)
            {
                IntPtr ptr = (IntPtr)wParam;
                int c = (int)ptr;
                char cc = (char)int.Parse(c.ToString());
                switch ((Keys)cc)
                {
                    case Keys.Escape:
                        mOwner.selectItem(mOldSel);
                        ensureVisible(mOldSel);
                        PaintManagerUI.PostMessage(this.Handle, (int)WindowMessage.WM_KILLFOCUS, 0, 0);

                        break;
                    case Keys.Return:
                        PaintManagerUI.PostMessage(this.Handle, (int)WindowMessage.WM_KILLFOCUS, 0, 0);
                        break;
                    default:
                        TEventUI newEvent = new TEventUI();
                        newEvent.mType = (int)EVENTTYPE_UI.UIEVENT_KEYDOWN;
                        newEvent.mKey = cc;
                        mOwner.eventProc(ref newEvent);
                        ensureVisible(mOwner.getCurSel());
                        return true;
                }
            }
            else if (uMsg == (int)WindowMessage.WM_MOUSEWHEEL)
            {
                IntPtr ptr = (IntPtr)wParam;
                int c = (int)ptr;

                int zDelta = (int)(short)HIWORD(c);
                TEventUI newEvent = new TEventUI();
                newEvent.mType = (int)EVENTTYPE_UI.UIEVENT_SCROLLWHEEL;
                newEvent.mWParam = MAKELONG(zDelta < 0 ? (int)ScrollBarCommands.SB_LINEDOWN : (int)ScrollBarCommands.SB_LINEUP, 0);
                newEvent.mLParam = lParam;
                newEvent.mTimestamp = PaintManagerUI.GetTickCount();
                mOwner.eventProc(ref newEvent);
                ensureVisible(mOwner.getCurSel());
                return true;
            }
            else if (uMsg == (int)WindowMessage.WM_KILLFOCUS)
            {
                close();
            }

            if (mManager != null && mManager.messageHandler((uint)uMsg, ref wParam, ref lParam, ref lRes))
            {
                return true;
            }

            return false;
        }
        public Int16 HIWORD(int x)
        {
            Int16 result = (Int16)((x >> 16) & 0xffff);

            return result;
        }
        public void close()
        {
            if (this.IsDisposed == false && mOwner != null)
            {
                // 恢复一下list控件的渲染管理器以及父节点控件
                mOwner.setManager(mOwner.getManager(), mOwner.getParent());
                mOwner.setPos(mOwner.getPos());
                mOwner.setFocus();

                // 释放资源缓存
                mManager.release();
                mOwner.mWindow = null;
                mManager = null;
                mOwner = null;

                // 删除自己
                this.Hide();
                this.Dispose();
            }
        }
        public int MAKELONG(int x, int y)
        {
            int result = (((Int16)x) & 0xffff) | ((((Int16)y) & 0xffff) >> 16);

            return result;
        }
        public void ensureVisible(int iIndex)
        {
            if (mOwner.getCurSel() < 0) return;
            mVerticalLayout.findSelectable(mOwner.getCurSel(), false);
            Rectangle rcItem = mVerticalLayout.getItemAt(iIndex).getPos();
            Rectangle rcList = mVerticalLayout.getPos();
            ScrollbarUI pHorizontalScrollbar = mVerticalLayout.getHorizontalScrollbar();
            if (pHorizontalScrollbar != null && pHorizontalScrollbar.isVisible())
            {
                rcList.Height = rcList.Bottom - pHorizontalScrollbar.getFixedHeight() - rcList.Top;
            }
            int iPos = mVerticalLayout.getScrollPos().Height;
            if (rcItem.Top >= rcList.Top && rcItem.Bottom < rcList.Bottom)
            {
                return;
            }
            int dx = 0;
            if (rcItem.Top < rcList.Top)
            {
                dx = rcItem.Top - rcList.Top;
            }
            if (rcItem.Bottom > rcList.Bottom)
            {
                dx = rcItem.Bottom - rcList.Bottom;
            }
            scroll(0, dx);
        }
        public void scroll(int dx, int dy)
        {
            if (dx == 0 && dy == 0) return;
            Size sz = mVerticalLayout.getScrollPos();
            mVerticalLayout.setScrollPos(new Size(sz.Width + dx, sz.Height + dy));
        }
        protected override void WndProc(ref Message m)
        {
            int lRes = 0;
            int uMsg = m.Msg;
            object wParam = m.WParam;
            object lParam = m.LParam;

            if (handleMessage(m.Msg, ref wParam, ref lParam, ref lRes) == false)
            {
                base.WndProc(ref m);
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            {
                Rectangle rectClient = new Rectangle(new Point(0, 0), this.ClientSize);
                if (mManager != null)
                {
                    mManager.paintMessageEvent(rectClient);
                }
            }
        }

        public PaintManagerUI mManager;
        public ComboUI mOwner;
        public VerticalLayoutUI mVerticalLayout;
        public int mOldSel;
        public Rectangle mRectClient;
    }
}
