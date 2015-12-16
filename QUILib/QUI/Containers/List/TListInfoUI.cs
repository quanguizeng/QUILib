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
    public class TListInfoUI
    {
        public TListInfoUI()
        {
            mImage = "";
            mSelectedImage = "";
            mHotImage = "";
            mDisabledImage = "";
        }
        public int mColumns;
        public Dictionary<int, Rectangle> mListColumn;
        public int mFontIdx;
        public int mTextStyle;
        public Rectangle mRectTextPadding;
        public Int32 mTextColor;
        public Int32 mBkColor;
        public string mImage;
        public Int32 mSelectedTextColor;
        public Int32 mSelectedBkColor;
        public string mSelectedImage;
        public Int32 mHotTextColor;
        public Int32 mHotBkColor;
        public string mHotImage;
        public Int32 mDisabledTextColor;
        public Int32 mDisabledBkColor;
        public string mDisabledImage;
        public Int32 mLineColor;
        public bool mShowHtml;
        public bool mExpandable;
        public bool mMultiExpandable;
    }
}
