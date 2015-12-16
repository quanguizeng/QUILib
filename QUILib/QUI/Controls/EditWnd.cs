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
    public class EditWnd : TextBox
    {
        public EditWnd()
        {
        }
        ~EditWnd()
        {
        }
        public void init(EditUI pOwner)
        {
            if(pOwner == null)
            {
                throw new Exception("父控件不能为空");
            }
            Rectangle rcPos = pOwner.getPos();
            Rectangle rcInset = pOwner.getTextPadding();
            rcPos.X += rcInset.X;
            rcPos.Y += rcInset.Y;
            rcPos.Width = rcPos.Right - rcInset.Right - rcPos.X;
            rcPos.Height = rcPos.Bottom - rcInset.Bottom - rcPos.Y;

            this.Size = new Size(rcPos.Width, rcPos.Height);
            this.Location = new Point(rcPos.Left, rcPos.Top);
            this.Font = pOwner.getManager().getDefaultFont();
            pOwner.getManager().getPaintWindow().Controls.Add(this);
            mOwner = pOwner;

            this.MaxLength = pOwner.getMaxChar();
            if (pOwner.isPasswordMode())
            {
                this.PasswordChar = pOwner.getPasswordChar();
            }
            this.Text = pOwner.getText();

            this.Focus();
            this.SelectionStart = this.Text.Length;
        }
        public string getWindowClassName()
        {
            return "EditWnd";
        }
        public string getSuperClassName()
        {
            return "TextBox";
        }
        public void onFinalMessage()
        {
            mOwner.setEditWnd(null);

            this.Dispose();
        }
        public int handleMessage(int uMsg, ref object wParam, ref object lParam)
        {
            int lRes = 1;

            bool bHandled = true;
            if (uMsg == (int)WindowMessage.WM_KILLFOCUS)
            {
                lRes = onKillFocus(uMsg, ref wParam, ref lParam, ref bHandled);
            }
            else if (uMsg == (int)ReflectedWindowMessage.OCM_COMMAND && GET_WM_COMMAND_CMD(ref wParam, ref lParam) == (int)EditControlCodes.EN_CHANGE)
            {
                lRes = onEditChanged(uMsg, ref wParam, ref lParam, ref bHandled);
            }

            return lRes;
        }

        public int GET_WM_COMMAND_CMD(ref object wParam, ref object lParam)
        {
            IntPtr ptr = (IntPtr)wParam;
            int result = (int)ptr;

            result = (Int16)(result >> 16) & 0xffff;

            return result;
        }

        public int onKillFocus(int uMsg, ref object wParam, ref object lParam, ref bool bHandled)
        {
            // 自己删除掉自己,对父控件没有任何影响

            mOwner.setEditWnd(null);
            this.Hide();
            this.Dispose();

            return 0;
        }
        public int onEditChanged(int uMsg, ref object wParam, ref object lParam, ref bool bHandled)
        {
            if (mOwner == null)
            {
                throw new Exception("父控件为空");
            }

            mOwner.setText(this.Text);

            bHandled = false;

            return 1;
        }
        protected override void WndProc(ref Message m)
        {
            int uMsg = m.Msg;
            object wParam = m.WParam;
            object lParam = m.LParam;

            if (handleMessage(m.Msg, ref wParam, ref lParam) != 0)
            {
                base.WndProc(ref m);
            }
        }

        protected EditUI mOwner;
    }
}
