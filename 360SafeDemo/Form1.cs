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

namespace _360SafeDemo
{
    public partial class Safe360Window : Form, INotifyUI
    {
        public Safe360Window()
        {
            ControlUI rootNode;
            Form form = this;
            {
                {
                    /*** 1. 创建资源管理器并绑定宿主窗体 ***/
                    mManager = new PaintManagerUI();
                    mManager.init(ref form);
                }

                {
                    /*** 2. 创建360安全卫士控件树 ***/
                    DialogBuilder builder = new DialogBuilder(true);
                    C360SafeDialogBuilderCallbackEx builderCallback = new C360SafeDialogBuilderCallbackEx();
                    rootNode = builder.createFromFile("skin.xml", builderCallback, mManager);
                }
                {
                    /*** 3. 绑定控件树、资源管理器、宿主窗体事件侦听器 ***/
                    mManager.attachDialog(ref rootNode);
                    mManager.addNotifier(this);
                    this.ClientSize = mManager.getInitSize();
                }

                {
                    /*** 4. 创建显示缓存对象 ***/
                    mRectClient = new Rectangle(0, 0, mManager.getInitSize().Width, mManager.getInitSize().Height);
                    mManager.getBufferManager().tryAllocateBuffer(mRectClient);
                }


                {
                    /*** 5. 初始化控件变量 ***/
                    init();
                }

                {
                    /*** 6. 初始化窗体显示参数并显示窗体 ***/
                    //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                    rootNode.setPos(new Rectangle(0, 0, this.Size.Width, this.Size.Height));
                    this.StartPosition = FormStartPosition.CenterScreen;
                    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

                    this.Show();
                }
            }

        }
        ~Safe360Window()
        {

        }
        public void init()
        {
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
                    if (m.Msg == (int)WindowMessage.WM_SETCURSOR)
                    {
                        if (this.IsDisposed == false)
                        {
                            base.WndProc(ref m);
                            return;
                        }
                    }
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            {
                if (mManager != null)
                {
                    mManager.paintMessageEvent(new Rectangle(0, 0, this.Size.Width, this.Size.Height));
                }
            }
        }

        public int notify(ref TNofityUI msg)
        {
            if (msg.mType == "selectchanged")
            {
                string name = msg.mSender.getName();
                TabLayoutUI pControl = (TabLayoutUI)mManager.findControl("switch");
                if (name == "examine")
                    pControl.selectItem(0);
                else if (name == "trojan")
                    pControl.selectItem(1);
                else if (name == "plugins")
                    pControl.selectItem(2);
                else if (name == "vulnerability")
                    pControl.selectItem(3);
                else if (name == "rubbish")
                    pControl.selectItem(4);
                else if (name == "cleanup")
                    pControl.selectItem(5);
                else if (name == "fix")
                    pControl.selectItem(6);
                else if (name == "tool")
                    pControl.selectItem(7);
            }
            else if(msg.mType == "click")
            {
                if(msg.mSender.getName() == "closebtn")
                {
                    this.Dispose();
                }
            }

            return 0;
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


        public const int HTCLIENT = 0x1;
        public const int HTCAPTION = 0x2;
        public PaintManagerUI mManager;
        public const int WM_NCHITTEST = 0x0084;
        protected Rectangle mRectClient;

    }
}
