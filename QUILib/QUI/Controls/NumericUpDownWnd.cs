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
using System.Text.RegularExpressions;

namespace QUI
{
    public class NumericUpDownWnd : TextBox
    {
        public NumericUpDownWnd()
        {

        }
        ~NumericUpDownWnd()
        {

        }
        public void init(NumericUpDownUI pOwner)
        {
            Rectangle rcPos = pOwner.getPos();
            Rectangle rcInset = pOwner.getTextPadding();
            rcPos.X += rcInset.X;
            rcPos.Y += rcInset.Y;
            rcPos.Width = rcPos.Right - rcInset.Right - rcPos.X - pOwner.getRectButton1().Width;
            rcPos.Height = rcPos.Bottom - rcInset.Bottom - rcPos.Y;

            this.Size = new Size(rcPos.Width, rcPos.Height);
            this.Location = new Point(rcPos.Left, rcPos.Top);
            this.Font = pOwner.getManager().getDefaultFont();
            pOwner.getManager().getPaintWindow().Controls.Add(this);
            mOwner = pOwner;

            this.MaxLength = pOwner.getMaxChar();

            this.Text = pOwner.getValue().ToString();

            this.Focus();
            this.SelectionStart = this.Text.Length;
            this.KeyPress += onTextBoxKeyPress;
            this.SelectAll();
        }
        public string getWindowClassName()
        {
            return "NumericUpDownWnd";
        }
        public string getSuperClassName()
        {
            return "TextBox";
        }
        public void onFinalMessage()
        {
            mOwner.setNumericUpDownWnd(null);

            this.Dispose();
        }
        public int onMessageHandle(int uMsg, ref object wParam, ref object lParam)
        {
            int lRes = 1;

            bool bHandled = true;
            if (uMsg == (int)WindowMessage.WM_KILLFOCUS)
            {
                lRes = onKillFocus(uMsg, ref wParam, ref lParam, ref bHandled);
            }
            else if (uMsg == (int)ReflectedWindowMessage.OCM_COMMAND && getWindowCommand(ref wParam, ref lParam) == (int)EditControlCodes.EN_CHANGE)
            {
                lRes = onEditChanged(uMsg, ref wParam, ref lParam, ref bHandled);
            }
            else if (uMsg == (int)WindowMessage.WM_CHAR)
            {
                IntPtr code = (IntPtr)wParam;
                Keys c = (Keys)code;
                lRes = processCharMsg(c);
            }

            return lRes;
        }

        public int getWindowCommand(ref object wParam, ref object lParam)
        {
            IntPtr ptr = (IntPtr)wParam;
            int result = (int)ptr;

            result = (Int16)(result >> 16) & 0xffff;

            return result;
        }

        public int onKillFocus(int uMsg, ref object wParam, ref object lParam, ref bool bHandled)
        {
            Regex reg = new Regex("^([+-])?\\d*(\\.\\d*)?$");
            if (reg.IsMatch(this.Text))
            {
                double value;
                if (double.TryParse(this.Text, out value))
                {
                    mOwner.setValue(value);
                }
            }
            mOwner.setNumericUpDownWnd(null);
            this.Hide();
            this.Dispose();

            return 0;
        }
        public int onEditChanged(int uMsg, ref object wParam, ref object lParam, ref bool bHandled)
        {
            bHandled = false;

            return 1;
        }
        protected override void WndProc(ref Message m)
        {
            int uMsg = m.Msg;
            object wParam = m.WParam;
            object lParam = m.LParam;

            if (onMessageHandle(m.Msg, ref wParam, ref lParam) != 0)
            {
                base.WndProc(ref m);
            }
        }
        private void onTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            Regex reg = new Regex("^([+-])?\\d*(\\.\\d*)?$");
            if (e.KeyChar != '\b')
            {
                if (!reg.IsMatch(e.KeyChar.ToString()))
                {
                    e.Handled = true;
                }
            }
        }
        private int processCharMsg(Keys c)
        {
            int lRes = 1;

            if (!((c >= Keys.D0 && c <= Keys.D9) || c == Keys.Back || c == Keys.Delete || c == Keys.Insert))
            {
                lRes = 0;

                return lRes;
            }
            if (c == Keys.Back)
            {
                return lRes;
            }
            if (c == Keys.Delete)
            {
                //if ((this.Text.Contains(".") && this.SelectedText.Contains(".") == false))
                {
                    lRes = 0;
                }

                return lRes;
            }
            if (c == Keys.Insert)
            {
                if (mOwner.getMinValue() >= 0 || (this.Text.Contains("-") && this.SelectionLength == 0))
                {
                    lRes = 0;
                    return lRes;
                }
                if (this.SelectionStart == 0 || (this.SelectionLength > 0 && this.SelectedText.Contains("-")))
                {
                    return lRes;
                }
                if (this.Text == "")
                {
                    return lRes;
                }
            }

            int pos = this.SelectionStart;
            string str = this.Text;
            string num = Enum.GetName(typeof(Keys), (int)c).Remove(0, 1);
            if (this.SelectionLength == 0)
            {
                str = str.Insert(pos, num);
            }
            else
            {
                int start = this.SelectionStart;
                str = str.Remove(start, this.SelectionLength);
                str = str.Insert(start, num);
            }
            double newNum = double.Parse(str);
            if (mOwner.getMaxValue() <= 0)
            {
                if (!(newNum >= mOwner.getMinValue() && newNum <= 0))
                {
                    lRes = 0;
                }

                return lRes;
            }
            if (mOwner.getMinValue() >= 0)
            {
                if (!(newNum >= 0 && newNum <= mOwner.getMaxValue()))
                {
                    lRes = 0;
                }

                return lRes;
            }
            if (mOwner.getMinValue() < 0 && mOwner.getMaxValue() > 0)
            {
                if (!(newNum >= mOwner.getMinValue() && newNum <= mOwner.getMaxValue()))
                {
                    lRes = 0;
                }

                return lRes;
            }

            return lRes;
        }


        protected NumericUpDownUI mOwner;
    }
}
