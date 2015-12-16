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
    public interface IListCallbackUI
    {
        string getItemText(ControlUI pList, int iItem, int iSubItem);
    };

    public interface IListOwnerUI
    {
        TListInfoUI getListInfo();
        int getCurSel();
        bool selectItem(int iIndex);
        void eventProc(ref TEventUI newEvent);
    };

    public interface IListUI : IListOwnerUI
    {
        ListHeaderUI getHeader();
        ContainerUI getList();
        IListCallbackUI getTextCallback();
        void setTextCallback(IListCallbackUI pCallback);
        bool expandItem(int iIndex, bool bExpand = true);
        int getExpandedItem();
    };

    public interface IListItemUI
    {
        int getIndex();
        void setIndex(int iIndex);
        IListOwnerUI getOwner();
        void setOwner(ControlUI pOwner);
        bool isSelected();
        bool select(bool bSelect = true);
        bool isExpanded();
        bool expand(bool bExpand = true);
        void drawItemText(ref Graphics graphics, ref Bitmap bitmap, ref Rectangle rcItem);
    };

    public enum ScrollBarCommands
    {
        SB_LINEUP = 0,
        SB_LINELEFT = 0,
        SB_LINEDOWN = 1,
        SB_LINERIGHT = 1,
        SB_PAGEUP = 2,
        SB_PAGELEFT = 2,
        SB_PAGEDOWN = 3,
        SB_PAGERIGHT = 3,
        SB_THUMBPOSITION = 4,
        SB_THUMBTRACK = 5,
        SB_TOP = 6,
        SB_LEFT = 6,
        SB_BOTTOM = 7,
        SB_RIGHT = 7,
        SB_ENDSCROLL = 8
    }

}
