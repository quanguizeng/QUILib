using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QUI;

namespace GameDemo
{
    public partial class Form1 : Form,INotifyUI
    {
        public Form1()
        {
            InitializeComponent();

            {
                Form form = this;
                mManager = new PaintManagerUI();

                mManager.init(ref form);

                DialogBuilder builder = new DialogBuilder();

                ControlUI rootNode = builder.createFromFile("login.xml", null, mManager);
                mManager.attachDialog(ref rootNode);

                mManager.addNotifier(this);
                this.ClientSize = mManager.getInitSize();
                mRectClient = new Rectangle(0, 0, mManager.getInitSize().Width, mManager.getInitSize().Height);

                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                rootNode.setPos(new Rectangle(0, 0, this.Size.Width, this.Size.Height));

                this.StartPosition = FormStartPosition.CenterScreen;

                init();
            }

        }


        ~Form1()
        {
            if (mManager != null)
            {
                mManager.release();
                mManager = null;
            }

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            {
                if (mManager != null)
                {
                    mManager.paintMessageEvent(mRectClient);
                }
            }
        }
        protected override void WndProc(ref Message m)
        {
            object wParam = m.WParam;
            object lParam = m.LParam;
            int lRes = 0;

            bool handled = true;


            switch (m.Msg)
            {
                case WM_NCHITTEST:
                    {
                        lRes = onNcHitTest(m.Msg, m.WParam, m.LParam, ref handled);
                        break;
                    }
                default:
                    {
                        handled = false;

                        break;
                    }
            }

            if (handled == false)
            {
                if (mManager.messageHandler((uint)m.Msg, ref wParam, ref lParam, ref lRes))
                {
                    m.Result = (IntPtr)lRes;

                    return;
                }
                if (this.IsDisposed == false)
                {
                    base.WndProc(ref m);
                    return;
                }
            }
            else
            {
                m.Result = (IntPtr)lRes;
            }
        }
        protected Int16 LOWORD(int x)
        {
            Int16 result = (Int16)(x & 0xffff);

            return result;
        }
        protected Int16 HIWORD(int x)
        {
            Int16 result = (Int16)((x >> 16) & 0xffff);

            return result;

        }
        protected int onNcHitTest(int msg, object wParam, object lParam, ref bool handled)
        {
            Point pt = new Point();
            IntPtr pos = (IntPtr)lParam;
            int nPos = (int)pos;
            pt.X = LOWORD(nPos);
            pt.Y = HIWORD(nPos);
            pt = this.PointToClient(pt);

            Rectangle rcClient;
            rcClient = this.ClientRectangle;

            Rectangle rcCaption = mManager.getCaptionRect();
            if (pt.X >= rcClient.Left + rcCaption.Left && pt.X < rcClient.Right - rcCaption.Right && pt.Y >= rcCaption.Top && pt.Y < rcCaption.Bottom)
            {
                ControlUI pControl = mManager.findControl(ref pt);
                if (pControl != null &&
                    pControl.getClass() != "ButtonUI" &&
                    pControl.getClass() != "OptionUI" &&
                    pControl.getClass() != "TextUI"
                    )
                {
                    return HTCAPTION;
                }
            }

            return HTCLIENT;
        }
        public int notify(ref TNofityUI msg)
        {
            if (msg.mType == "click")
            {
                if (msg.mSender == mButtonClose)
                {
                    this.Dispose();
                }
                else if (msg.mSender == mButtonLogin)
                {
                    Action action = () =>
                    {
                        MessageBox.Show("OK");
                    };
                    this.BeginInvoke(action);
                }
            }
            else if (msg.mType == "itemselect")
            {
                Action<string, string> action = (controlName, strText) =>
                {
                    if (controlName == "accountcombo")
                    {
                        EditUI pAccountEdit = (EditUI)mManager.findControl("accountedit");
                        if (pAccountEdit != null)
                        {
                            pAccountEdit.setText(strText);
                        }
                    }
                };
                this.BeginInvoke(action, msg.mSender.getName(), msg.mSender.getText());

            }

            return 0;
        }

        public void init()
        {
            mButtonClose = mManager.findControl("closebtn");
            mButtonLogin = mManager.findControl("loginBtn");


            {
                ComboUI pAccountCombo = (ComboUI)mManager.findControl("accountcombo");
                EditUI pAccountEdit = (EditUI)mManager.findControl("accountedit");
                if (pAccountCombo != null && pAccountEdit != null)
                {
                    pAccountEdit.setText(pAccountCombo.getText());
                }
            }
        }

        public const int HTCLIENT = 0x1;
        public const int HTCAPTION = 0x2;
        public PaintManagerUI mManager;
        public const int WM_NCHITTEST = 0x0084;

        protected ControlUI mButtonClose;
        protected ControlUI mButtonLogin;
        protected Rectangle mRectClient;

    }
}
