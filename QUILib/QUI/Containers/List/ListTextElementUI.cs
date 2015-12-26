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
    public class ListTextElementUI : ListLabelElementUI
    {
        public ListTextElementUI()
        {
            mNumLinks = 0;
            mHoverLink = -1;
            mOwner = null;

            mRectLinks = new Rectangle[MAX_LINK];
            mLinks = new string[MAX_LINK];
        }
        public override string getClass()
        {
            return "ListTextElementUI";
        }
        public override ControlUI getInterface(string pstrName)
        {
            if (pstrName == "ListTextElement") return (ListTextElementUI)this;
            return base.getInterface(pstrName);
        }
        public override int getControlFlags()
        {
            if (!isEnabled()) return base.getControlFlags();

            return (int)ControlFlag.UIFLAG_WANTRETURN | (int)ControlFlag.UIFLAG_SETCURSOR;
        }
        public override void setOwner(ControlUI pOwner)
        {
            base.setOwner(pOwner);
            mOwner = (IListUI)pOwner.getInterface("List");
        }
        public string getLinkContent(int iIndex)
        {
            if (iIndex >= 0 && iIndex < mNumLinks)
            {
                return mLinks[iIndex];
            }

            return "";
        }
        public override void eventProc(ref TEventUI newEvent)
        {
            if (!isMouseEnabled() && newEvent.mType > (int)EVENTTYPE_UI.UIEVENT__MOUSEBEGIN && newEvent.mType < (int)EVENTTYPE_UI.UIEVENT__MOUSEEND)
            {
                if (mOwner != null)
                {
                    mOwner.eventProc(ref newEvent);
                }
                else
                {
                    base.eventProc(ref newEvent);
                }
                return;
            }

            // When you hover over a link
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_SETCURSOR)
            {
                for (int i = 0; i < mNumLinks; i++)
                {
                    if (mRectLinks[i].Contains(newEvent.mMousePos))
                    {
                        if (mManager.getPaintWindow().Cursor != Cursors.Hand)
                        {
                            mManager.getPaintWindow().Cursor = Cursors.Hand;
                        }

                        return;
                    }
                }
            }
            if (newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_BUTTONUP && isEnabled())
            {
                for (int i = 0; i < mNumLinks; i++)
                {
                    if (mRectLinks[i].Contains(newEvent.mMousePos))
                    {
                        mManager.sendNotify(this, "link", i);
                        return;
                    }
                }
            }
            if (mNumLinks > 0 && newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSEMOVE)
            {
                int nHoverLink = -1;
                for (int i = 0; i < mNumLinks; i++)
                {
                    if (mRectLinks[i].Contains(newEvent.mMousePos))
                    {
                        nHoverLink = i;
                        break;
                    }
                }

                if (mHoverLink != nHoverLink)
                {
                    invalidate();
                    mHoverLink = nHoverLink;
                }
            }
            if (mNumLinks > 0 && newEvent.mType == (int)EVENTTYPE_UI.UIEVENT_MOUSELEAVE)
            {
                if (mHoverLink != -1)
                {
                    invalidate();
                    mHoverLink = -1;
                }
            }

            base.eventProc(ref newEvent);
        }
        public override Size estimateSize(Size szAvailable)
        {
            TListInfoUI pInfo = null;
            if (mOwner != null) pInfo = mOwner.getListInfo();

            Size cXY = mXYFixed;
            if (cXY.Height == 0 && mManager != null)
            {
                cXY.Height = mManager.getDefaultFont().Height + 8;
                if (pInfo != null) cXY.Height += pInfo.mRectTextPadding.Top + pInfo.mRectTextPadding.Bottom;
            }

            return cXY;
        }
        public override void drawItemText(ref Graphics graphics, ref Bitmap bitmap, ref Rectangle rcItem)
        {
            if (mOwner == null)
            {
                return;
            }
            TListInfoUI pInfo = mOwner.getListInfo();
            int iTextColor = pInfo.mTextColor;

            if ((mButtonState & (int)PaintFlags.UISTATE_HOT) != 0)
            {
                iTextColor = pInfo.mHotTextColor;
            }
            if (isSelected())
            {
                iTextColor = pInfo.mSelectedTextColor;
            }
            if (!isEnabled())
            {
                iTextColor = pInfo.mDisabledTextColor;
            }
            IListCallbackUI pCallback = mOwner.getTextCallback();

            if (pCallback == null)
            {
                return;
            }
            mNumLinks = 0;
            int nLinks = mRectLinks.Length;
            for (int i = 0; i < pInfo.mColumns; i++)
            {

                Rectangle rcItem1 = new Rectangle(pInfo.mListColumn[i].Left, mRectItem.Top, pInfo.mListColumn[i].Width, mRectItem.Height);
                int newLeft = rcItem1.Left + pInfo.mRectTextPadding.Left;
                int newRight = rcItem1.Right - pInfo.mRectTextPadding.Right;
                int newTop = rcItem1.Top + pInfo.mRectTextPadding.Top;
                int newBottom = rcItem1.Bottom - pInfo.mRectTextPadding.Bottom;
                rcItem1.X = newLeft;
                rcItem1.Width = newRight - newLeft;
                rcItem1.Y = newTop;
                rcItem1.Height = newBottom - newTop;

                string pstrText = pCallback.getItemText(this, mIndex, i);

                if (pInfo.mShowHtml)
                {
                    RenderEngine.drawHtmlText(ref graphics,
                        ref bitmap,
                        ref mManager,
                        ref rcItem1,
                        pstrText,
                        iTextColor,
                        ref mRectLinks,
                        ref mLinks,
                        ref nLinks,
                        (int)FormatFlags.DT_SINGLELINE | pInfo.mTextStyle);
                }
                else
                {
                    RenderEngine.drawText(ref graphics,
                        ref bitmap,
                        ref mManager,
                        ref rcItem1,
                        pstrText,
                        iTextColor,
                        pInfo.mFontIdx,
                        (int)FormatFlags.DT_SINGLELINE | pInfo.mTextStyle);
                }

                if (nLinks > 0)
                {
                    mNumLinks = nLinks;
                    nLinks = 0;
                }
                else
                {
                    nLinks = mRectLinks.Length;
                }
            }
        }


        public const int MAX_LINK = 8;
        protected int mNumLinks;
        protected Rectangle[] mRectLinks;
        protected string[] mLinks;
        protected int mHoverLink;
        protected new IListUI mOwner;
    }
}
