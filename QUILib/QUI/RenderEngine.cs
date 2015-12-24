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
    public class RenderEngine
    {
        public RenderEngine()
        {
        }
        ~RenderEngine()
        {
        }
        public static bool alphaBitBlt(ref Bitmap hDC, int nDestX, int nDestY, int dwWidth, int dwHeight,
        ref Bitmap hSrcDC, int nSrcX, int nSrcY, int wSrc, int hSrc, int SourceConstantAlpha)
        {
            Bitmap dstBitmap = new Bitmap(dwWidth - nDestX, dwHeight - nDestY, PixelFormat.Format32bppArgb);

            if (dstBitmap == null)
            {
                throw new Exception("");
            }
            Bitmap srcBitmap = new Bitmap(dwWidth - nDestX, dwHeight - nDestY, PixelFormat.Format32bppArgb);

            if (srcBitmap == null)
            {
                dstBitmap.Dispose();

                throw new Exception("");
            }
            Graphics dstG = Graphics.FromImage(dstBitmap);
            Graphics srcG = Graphics.FromImage(srcBitmap);

            dstG.DrawImage(hDC, new Rectangle(0, 0, dwWidth - nDestX, dwHeight - nDestY), new Rectangle(nDestX, nDestY, dwWidth - nDestX, dwHeight - nDestY), GraphicsUnit.Pixel);
            srcG.DrawImage(hSrcDC, new Rectangle(0, 0, dwWidth - nDestX, dwHeight - nDestY), new Rectangle(nSrcX, nSrcY, wSrc - nSrcX, hSrc - nSrcY), GraphicsUnit.Pixel);

            double src_darken;
            for (int i = 0; i < dwWidth - nDestX; i++)
            {
                for (int j = 0; j < dwHeight - nDestY; j++)
                {
                    Color srcColor = srcBitmap.GetPixel(i, j);
                    Color dstColor = dstBitmap.GetPixel(i, j);
                    src_darken = srcColor.A * SourceConstantAlpha / 255.0 / 255.0;
                    if (src_darken < 0.0)
                    {
                        src_darken = 0.0;
                    }
                    Color newColor = Color.FromArgb(dstColor.A,
                        (int)(srcColor.R * src_darken + dstColor.R * (1 - src_darken)),
                        (int)(srcColor.G * src_darken + dstColor.G * (1 - src_darken)),
                        (int)(srcColor.B * src_darken + dstColor.B * (1 - src_darken)));
                    dstBitmap.SetPixel(i, j, newColor);
                }
            }

            dstG = Graphics.FromImage(hDC);
            dstG.DrawImage(dstBitmap, new Rectangle(nDestX, nDestY, dwWidth - nDestX, dwHeight - nDestY));

            dstBitmap.Dispose();
            srcBitmap.Dispose();

            dstBitmap = null;
            srcBitmap = null;

            dstG = null;
            srcG = null;

            return true;
        }
        public static TImageInfo loadImage(string strBitmap, string type, int mask = 0)
        {
            Bitmap buffer = new Bitmap(strBitmap);
            if (buffer == null)
            {
                throw new Exception(string.Format("加载图片 {0} 失败", strBitmap));
            }

            if (buffer != null)
            {
                buffer.MakeTransparent(Color.FromArgb(mask));
            }

            Rectangle rect = new Rectangle(0, 0, buffer.Width, buffer.Height);

            Bitmap bitmap = buffer.Clone(rect, PixelFormat.Format32bppArgb);

            buffer.Dispose();
            buffer = null;

            if (bitmap == null)
            {
                throw new Exception(string.Format("创建位图缓存 {0} 失败", strBitmap));
            }

            TImageInfo data = new TImageInfo();
            data.mBitmap = bitmap;
            data.mX = bitmap.Width;
            data.mY = bitmap.Height;
            data.mAlphaChannel = true;

            return data;
        }

        // 把hBitmap渲染到hDC中去
        public static void drawImage(ref Graphics graphics,
                    ref Bitmap hDC,
                    ref Bitmap hBitmap,
                    ref Rectangle rc,
                    ref Rectangle rcPaint,
                    ref Rectangle rcBmpPart,
                    ref Rectangle rcCorners,
                    bool alphaChannel,
                    byte uFade,
                    bool hole = false)
        {
            if (hBitmap == null || hDC == null) return;

            Rectangle rcDest;
            if (alphaChannel || uFade < 255)
            {
                int bf = uFade;
                // middle
                if (!hole)
                {
                    rcDest = new Rectangle(rc.Left + rcCorners.Left,
                        rc.Top + rcCorners.Top,
                        rc.Right - rcCorners.Right,
                        rc.Bottom - rcCorners.Bottom);

                    if (rcPaint.IntersectsWith(rcDest))
                    {
                        rcDest = new Rectangle(rcDest.Left,
                            rcDest.Top,
                            (rcDest.Right - rcDest.Left) - rcDest.Left,
                            rcDest.Bottom - rcDest.Top - rcDest.Top);
                        alphaBitBlt(ref hDC, rcDest.Left, rcDest.Top, rcDest.Right, rcDest.Bottom, ref hBitmap,
                            rcBmpPart.Left + rcCorners.Left, rcBmpPart.Top + rcCorners.Top,
                            rcBmpPart.Right - rcBmpPart.Left - rcCorners.Left - rcCorners.Right,
                            rcBmpPart.Bottom - rcBmpPart.Top - rcCorners.Top - rcCorners.Bottom, bf);
                    }
                }

                // left-top
                if (rcCorners.Left > 0 && rcCorners.Top > 0)
                {
                    rcDest = new Rectangle(rc.Left, rc.Top, rcCorners.Left, rcCorners.Top);

                    if (rcPaint.IntersectsWith(rcDest))
                    {
                        rcDest = new Rectangle(rcDest.Left, rcDest.Top, rcDest.Right - rcDest.Left - rcDest.Left, rcDest.Bottom - rcDest.Top - rcDest.Top);

                        alphaBitBlt(ref hDC, rcDest.Left, rcDest.Top, rcDest.Right, rcDest.Bottom, ref hBitmap,
                            rcBmpPart.Left, rcBmpPart.Top, rcCorners.Left, rcCorners.Top, bf);
                    }
                }
                // top
                if (rcCorners.Top > 0)
                {
                    rcDest = new Rectangle(rc.Left + rcCorners.Left,
                        rc.Top,
                        rc.Right - rc.Left - rcCorners.Left - rcCorners.Right,
                        rcCorners.Top);
                    if (rcPaint.IntersectsWith(rcDest))
                    {
                        rcDest = new Rectangle(rcDest.Left, rcDest.Top, rcDest.Right - rcDest.Left - rcDest.Left, rcDest.Bottom - rcDest.Top - rcDest.Top);
                        alphaBitBlt(ref hDC, rcDest.Left, rcDest.Top, rcDest.Right, rcDest.Bottom, ref hBitmap,
                            rcBmpPart.Left + rcCorners.Left, rcBmpPart.Top, rcBmpPart.Right - rcBmpPart.Left - rcCorners.Left - rcCorners.Right, rcCorners.Top, bf);

                    }
                }
                // right-top
                if (rcCorners.Right > 0 && rcCorners.Top > 0)
                {
                    rcDest = new Rectangle(rc.Right - rcCorners.Right,
                        rc.Top,
                        rcCorners.Right,
                        rcCorners.Top);
                    if (rcPaint.IntersectsWith(rcDest))
                    {
                        rcDest = new Rectangle(rcDest.Left, rcDest.Top, rcDest.Right - rcDest.Left - rcDest.Left, rcDest.Bottom - rcDest.Top - rcDest.Top);
                        alphaBitBlt(ref hDC, rcDest.Left, rcDest.Top, rcDest.Right, rcDest.Bottom, ref hBitmap,
                            rcBmpPart.Right - rcCorners.Right, rcBmpPart.Top, rcCorners.Right, rcCorners.Top, bf);

                    }
                }
                // left
                if (rcCorners.Left > 0)
                {
                    rcDest = new Rectangle(rc.Left,
                        rc.Top + rcCorners.Top,
                        rcCorners.Left,
                        rc.Bottom - rc.Top - rcCorners.Top - rcCorners.Bottom);
                    if (rcPaint.IntersectsWith(rcDest))
                    {
                        rcDest = new Rectangle(rcDest.Left, rcDest.Top, rcDest.Right - rcDest.Left - rcDest.Left, rcDest.Bottom - rcDest.Top - rcDest.Top);
                        alphaBitBlt(ref hDC, rcDest.Left, rcDest.Top, rcDest.Right, rcDest.Bottom, ref hBitmap,
                            rcBmpPart.Left, rcBmpPart.Top + rcCorners.Top, rcCorners.Left, rcBmpPart.Bottom -
                            rcBmpPart.Top - rcCorners.Top - rcCorners.Bottom, bf);

                    }
                }
                // right
                if (rcCorners.Right > 0)
                {
                    rcDest = new Rectangle(rc.Right - rcCorners.Right,
                        rc.Top + rcCorners.Top,
                        rcCorners.Right,
                        rc.Bottom - rc.Top - rcCorners.Top - rcCorners.Bottom);

                    if (rcPaint.IntersectsWith(rcDest))
                    {
                        rcDest = new Rectangle(rcDest.Left, rcDest.Top, rcDest.Right - rcDest.Left - rcDest.Left, rcDest.Bottom - rcDest.Top - rcDest.Top);
                        alphaBitBlt(ref hDC, rcDest.Left, rcDest.Top, rcDest.Right, rcDest.Bottom, ref hBitmap,
                            rcBmpPart.Right - rcCorners.Right, rcBmpPart.Top + rcCorners.Top, rcCorners.Right,
                            rcBmpPart.Bottom - rcBmpPart.Top - rcCorners.Top - rcCorners.Bottom, bf);
                    }
                }
                // left-bottom
                if (rcCorners.Left > 0 && rcCorners.Bottom > 0)
                {

                    rcDest = new Rectangle(rc.Left,
                        rc.Bottom - rcCorners.Bottom,
                        rcCorners.Left,
                        rcCorners.Bottom);

                    if (rcPaint.IntersectsWith(rcDest))
                    {
                        rcDest = new Rectangle(rcDest.Left, rcDest.Top, rcDest.Right - rcDest.Left - rcDest.Left, rcDest.Bottom - rcDest.Top - rcDest.Top);
                        alphaBitBlt(ref hDC, rcDest.Left, rcDest.Top, rcDest.Right, rcDest.Bottom, ref hBitmap,
                            rcBmpPart.Left, rcBmpPart.Bottom - rcCorners.Bottom, rcCorners.Left, rcCorners.Bottom, bf);
                    }
                }
                // bottom
                if (rcCorners.Bottom > 0)
                {
                    rcDest = new Rectangle(rc.Left + rcCorners.Left,
                        rc.Bottom - rcCorners.Bottom,
                        rc.Right - rc.Left - rcCorners.Left - rcCorners.Right,
                        rcCorners.Bottom);
                    if (rcPaint.IntersectsWith(rcDest))
                    {
                        rcDest = new Rectangle(rcDest.Left, rcDest.Top, rcDest.Right - rcDest.Left - rcDest.Left, rcDest.Bottom - rcDest.Top - rcDest.Top);
                        alphaBitBlt(ref hDC, rcDest.Left, rcDest.Top, rcDest.Right, rcDest.Bottom, ref hBitmap,
                            rcBmpPart.Left + rcCorners.Left, rcBmpPart.Bottom - rcCorners.Bottom,
                            rcBmpPart.Right - rcBmpPart.Left - rcCorners.Left - rcCorners.Right, rcCorners.Bottom, bf);
                    }
                }
                // right-bottom
                if (rcCorners.Right > 0 && rcCorners.Bottom > 0)
                {
                    rcDest = new Rectangle(rc.Right - rcCorners.Right,
                        rc.Bottom - rcCorners.Bottom,
                        rcCorners.Right,
                        rcCorners.Bottom);
                    if (rcPaint.IntersectsWith(rcDest))
                    {
                        rcDest = new Rectangle(rcDest.Left, rcDest.Top, rcDest.Right - rcDest.Left - rcDest.Left, rcDest.Bottom - rcDest.Top - rcDest.Top);
                        alphaBitBlt(ref hDC, rcDest.Left, rcDest.Top, rcDest.Right, rcDest.Bottom, ref hBitmap,
                            rcBmpPart.Right - rcCorners.Right, rcBmpPart.Bottom - rcCorners.Bottom, rcCorners.Right,
                            rcCorners.Bottom, bf);
                    }
                }
            }
            else
            {
                if (rc.Right - rc.Left == rcBmpPart.Right - rcBmpPart.Left
                    && rc.Bottom - rc.Top == rcBmpPart.Bottom - rcBmpPart.Top
                    && rcCorners.Left == 0 && rcCorners.Right == 0 && rcCorners.Top == 0 && rcCorners.Bottom == 0)
                {
                    if (rcPaint.IntersectsWith(rc))
                    {
                        Rectangle rcTemp = rcPaint;
                        rcTemp.Intersect(rc);
                        graphics.DrawImage(hBitmap,
                            rcTemp,
                            rcBmpPart,
                            GraphicsUnit.Pixel);
                    }
                }
                else
                {
                    // middle
                    if (!hole)
                    {
                        rcDest = new Rectangle(rc.Left + rcCorners.Left,
                            rc.Top + rcCorners.Top,
                            rc.Right - rc.Left - rcCorners.Left - rcCorners.Right,
                            rc.Bottom - rc.Top - rcCorners.Top - rcCorners.Bottom);
                        if (rcPaint.IntersectsWith(rcDest))
                        {
                            graphics.DrawImage(hBitmap,
                                rcDest,
                                new Rectangle(rcBmpPart.Left + rcCorners.Left, rcBmpPart.Top + rcCorners.Top, rcBmpPart.Right - rcBmpPart.Left - rcCorners.Left - rcCorners.Right, rcBmpPart.Bottom - rcBmpPart.Top - rcCorners.Top - rcCorners.Bottom),
                                GraphicsUnit.Pixel);
                        }
                    }

                    // left-top
                    if (rcCorners.Left > 0 && rcCorners.Top > 0)
                    {
                        rcDest = new Rectangle(rc.Left,
                            rc.Top,
                            rcCorners.Left,
                            rcCorners.Top);
                        if (rcPaint.IntersectsWith(rcDest))
                        {
                            graphics.DrawImage(hBitmap, rcDest,
                                new Rectangle(rcBmpPart.Left, rcBmpPart.Top, rcCorners.Left, rcCorners.Top),
                                GraphicsUnit.Pixel);
                        }
                    }
                    // top
                    if (rcCorners.Top > 0)
                    {
                        rcDest = new Rectangle(rc.Left + rcCorners.Left, rc.Top, rc.Right - rc.Left - rcCorners.Left - rcCorners.Right, rcCorners.Top);
                        if (rcPaint.IntersectsWith(rcDest))
                        {
                            graphics.DrawImage(hBitmap,
                                rcDest,
                                new Rectangle(rcBmpPart.Left + rcCorners.Left, rcBmpPart.Top, rcBmpPart.Right - rcBmpPart.Left - rcCorners.Left - rcCorners.Right, rcCorners.Top),
                                GraphicsUnit.Pixel);
                        }
                    }
                    // right-top
                    if (rcCorners.Right > 0 && rcCorners.Top > 0)
                    {
                        rcDest = new Rectangle(rc.Right - rcCorners.Right, rc.Top, rcCorners.Right, rcCorners.Top);
                        if (rcPaint.IntersectsWith(rcDest))
                        {
                            graphics.DrawImage(hBitmap,
                                rcDest,
                                new Rectangle(rcBmpPart.Right - rcCorners.Right, rcBmpPart.Top, rcCorners.Right, rcCorners.Top),
                                GraphicsUnit.Pixel);
                        }
                    }
                    // left
                    if (rcCorners.Left > 0)
                    {
                        rcDest = new Rectangle(rc.Left, rc.Top + rcCorners.Top, rcCorners.Left, rc.Bottom - rc.Top - rcCorners.Top - rcCorners.Bottom);

                        if (rcPaint.IntersectsWith(rcDest))
                        {
                            graphics.DrawImage(hBitmap,
                                rcDest,
                                new Rectangle(rcBmpPart.Left, rcBmpPart.Top + rcCorners.Top, rcCorners.Left, rcBmpPart.Bottom - rcBmpPart.Top - rcCorners.Top - rcCorners.Bottom),
                                GraphicsUnit.Pixel);
                        }
                    }
                    // right
                    if (rcCorners.Right > 0)
                    {
                        rcDest = new Rectangle(rc.Right - rcCorners.Right, rc.Top + rcCorners.Top, rcCorners.Right, rc.Bottom - rc.Top - rcCorners.Top - rcCorners.Bottom);

                        if (rcPaint.IntersectsWith(rcDest))
                        {
                            graphics.DrawImage(hBitmap,
                                rcDest,
                                new Rectangle(rcBmpPart.Right - rcCorners.Right, rcBmpPart.Top + rcCorners.Top, rcCorners.Right, rcBmpPart.Bottom - rcBmpPart.Top - rcCorners.Top - rcCorners.Bottom),
                                GraphicsUnit.Pixel);
                        }
                    }
                    // left-bottom
                    if (rcCorners.Left > 0 && rcCorners.Bottom > 0)
                    {
                        rcDest = new Rectangle(rc.Left, rc.Bottom - rcCorners.Bottom, rcCorners.Left, rcCorners.Bottom);

                        if (rcPaint.IntersectsWith(rcDest))
                        {
                            graphics.DrawImage(hBitmap,
                                rcDest,
                                new Rectangle(rcBmpPart.Left, rcBmpPart.Bottom - rcCorners.Bottom, rcCorners.Left, rcCorners.Bottom),
                                GraphicsUnit.Pixel);
                        }
                    }
                    // bottom
                    if (rcCorners.Bottom > 0)
                    {
                        rcDest = new Rectangle(rc.Left + rcCorners.Left,
                            rc.Bottom - rcCorners.Bottom,
                            rc.Right - rc.Left - rcCorners.Left - rcCorners.Right,
                            rcCorners.Bottom);

                        if (rcPaint.IntersectsWith(rcDest))
                        {
                            graphics.DrawImage(hBitmap,
                                rcDest,
                                new Rectangle(rcBmpPart.Left + rcCorners.Left, rcBmpPart.Bottom - rcCorners.Bottom, rcBmpPart.Right - rcBmpPart.Left - rcCorners.Left - rcCorners.Right, rcCorners.Bottom),
                                GraphicsUnit.Pixel);
                        }
                    }
                    // right-bottom
                    if (rcCorners.Right > 0 && rcCorners.Bottom > 0)
                    {
                        rcDest = new Rectangle(rc.Right - rcCorners.Right, rc.Bottom - rcCorners.Bottom, rcCorners.Right, rcCorners.Bottom);
                        if (rcPaint.IntersectsWith(rcDest))
                        {
                            graphics.DrawImage(hBitmap,
                                rcDest,
                                new Rectangle(rcBmpPart.Right - rcCorners.Right, rcBmpPart.Bottom - rcCorners.Bottom, rcCorners.Right, rcCorners.Bottom),
                                GraphicsUnit.Pixel);
                        }
                    }
                }
            }

        }
        public static bool drawImageString(ref Graphics graphics, ref Bitmap hDC, ref PaintManagerUI pManager, ref Rectangle rc, ref Rectangle rcPaint,
                              string pStrImage, string pStrModify)
        {
            // 1、aaa.jpg
            // 2、file='aaa.jpg' res='' restype='0' dest='0,0,0,0' source='0,0,0,0' corner='0,0,0,0' 
            // mask='#FF0000' fade='255' hole='false'
            // 字符格式必须紧凑，不能有多余空格或tab，图片本身是否带有alpha通道看图片类型

            if (pStrImage == null)
            {
                pStrImage = "";
            }
            if (pStrModify == null)
            {
                pStrModify = "";
            }

            string sImageName = pStrImage;
            string sImageResType = "";
            Rectangle rcItem = rc;
            Rectangle rcBmpPart = new Rectangle();
            Rectangle rcCorner = new Rectangle(); ;
            Int32 dwMask = 0;
            byte bFade = 0xFF;
            bool bHole = false;

            string sItem;
            string sValue;
            string buffer = pStrImage;
            int count = buffer.Length;
            for (int i = 0; i < 2; ++i)
            {
                if (i == 1)
                {
                    if (pStrModify == "")
                    {
                        break;
                    }
                    buffer = pStrModify;
                    count = buffer.Length;
                }
                if (buffer == "") continue;
                int j = 0;
                while (j < count && buffer[j] != '\0')
                {
                    sItem = "";
                    sValue = "";
                    while (j < count && buffer[j] != '\0' && buffer[j] != '=')
                    {
                        sItem += buffer[j];
                        j++;
                    }
                    if (j < count && buffer[j++] != '=') break;
                    if (j < count && buffer[j++] != '\'') break;
                    while (j < count && buffer[j] != '\0' && buffer[j] != '\'')
                    {
                        sValue += buffer[j];
                        j++;
                    }
                    if (j < count && buffer[j] != '\'') break;
                    j++;
                    if (sValue != "")
                    {
                        if (sItem == "file" || sItem == "res")
                        {
                            sImageName = sValue;
                        }
                        else if (sItem == "restype")
                        {
                            sImageResType = sValue;
                        }
                        else if (sItem == "dest")
                        {
                            string[] listValue = sValue.Split(',');
                            if (listValue.Length < 4)
                            {
                                throw new Exception("参数不足4个");
                            }
                            rcItem = new Rectangle(rc.Left + int.Parse(listValue[0]),
                                rc.Top + int.Parse(listValue[1]),
                                rc.Left + int.Parse(listValue[2]) - (rc.Left + int.Parse(listValue[0])),
                                rc.Top + int.Parse(listValue[3]) - (rc.Top + int.Parse(listValue[1])));
                        }
                        else if (sItem == "source")
                        {
                            string[] listValue = sValue.Split(',');
                            if (listValue.Length < 4)
                            {
                                throw new Exception("参数不足4个");
                            }
                            rcBmpPart = new Rectangle(int.Parse(listValue[0]),
                                int.Parse(listValue[1]),
                                int.Parse(listValue[2]) - int.Parse(listValue[0]),
                                int.Parse(listValue[3]) - int.Parse(listValue[1]));
                        }
                        else if (sItem == "corner")
                        {
                            string[] listValue = sValue.Split(',');
                            if (listValue.Length < 4)
                            {
                                throw new Exception("参数不足4个");
                            }
                            rcCorner = new Rectangle(int.Parse(listValue[0]),
                                int.Parse(listValue[1]),
                                int.Parse(listValue[2]) - int.Parse(listValue[0]),
                                int.Parse(listValue[3]) - int.Parse(listValue[1]));
                        }
                        else if (sItem == "mask")
                        {
                            sValue = sValue.TrimStart('#');
                            dwMask = Convert.ToInt32(sValue, 16);
                        }
                        else if (sItem == "fade")
                        {
                            bFade = (byte)byte.Parse(sValue);
                        }
                        else if (sItem == "hole")
                        {
                            bHole = sValue == "true";
                        }
                    }
                    if (j < count && buffer[j++] != ' ') break;
                }
            }

            TImageInfo data = null;
            if (sImageResType == "")
            {
                data = pManager.getImageEx(sImageName, null, dwMask);
            }
            else
            {
                data = pManager.getImageEx(sImageName, sImageResType, dwMask);
            }
            if (data == null) return false;

            if (hDC == null) return false;

            if (rcBmpPart.Left == 0 && rcBmpPart.Right == 0 && rcBmpPart.Top == 0 && rcBmpPart.Bottom == 0)
            {
                rcBmpPart = new Rectangle(rcBmpPart.Left, rcBmpPart.Top, data.mX - rcBmpPart.Left, data.mY - rcBmpPart.Top);
            }

            if (rcItem.IntersectsWith(rc) == false)
            {
                return true;
            }
            if (rcItem.IntersectsWith(rcPaint) == false)
            {
                return true;
            }

            drawImage(ref graphics, ref hDC, ref data.mBitmap, ref rcItem, ref rcPaint, ref  rcBmpPart, ref rcCorner, data.mAlphaChannel, bFade, bHole);

            return true;
        }
        public static void drawColor(ref Graphics graphics, ref Bitmap bitmap, ref Rectangle rc, int color)
        {
            drawColor(ref graphics, ref rc, color);
        }
        public static void drawColor(ref Graphics graphics, ref Rectangle rc, int color)
        {
            Brush brush = new SolidBrush(Color.FromArgb(color));
            graphics.FillRectangle(brush, rc);
            brush.Dispose();
            brush = null;
        }
        public static void drawGradient(ref Graphics graphics, ref Bitmap bitmap, ref Rectangle rc, int first, int second,
            bool vertical, int steps)
        {
            drawGradient(ref graphics, ref rc, first, second, vertical, steps);
        }
        public static void drawGradient(ref Graphics graphics, ref Rectangle rc, int first, int second,
            bool vertical, int steps)
        {
            LinearGradientBrush gradientBrush = new LinearGradientBrush(rc, Color.FromArgb(first), Color.FromArgb(second), (float)90.0, false);
            graphics.FillRectangle(gradientBrush, rc);
            gradientBrush.Dispose();
            gradientBrush = null;
        }
        public static void drawLine(ref Graphics graphics, ref Bitmap bitmap, ref Rectangle rc, int size, int penColor)
        {
            drawLine(ref graphics, ref rc, size, penColor);
        }
        public static void drawLine(ref Graphics graphics, ref Rectangle rc, int size, int penColor)
        {
            Pen pen = new Pen(Color.FromArgb(penColor));
            Point start = new Point(rc.Left, rc.Top);
            Point end = new Point(rc.Right, rc.Bottom);
            graphics.DrawLine(pen, start, end);
            pen.Dispose();
            pen = null;
        }
        public static void drawRect(ref Graphics graphics, ref Bitmap bitmap, ref Rectangle rc, int size, int penColor)
        {
            drawRect(ref graphics, ref rc, size, penColor);
        }
        public static void drawRect(ref Graphics graphics, ref Rectangle rc, int size, int penColor)
        {
            Pen pen = new Pen(Color.FromArgb(penColor));
            graphics.DrawRectangle(pen, rc);
            pen.Dispose();
            pen = null;
        }
        public static void drawText(ref Graphics graphics, ref Bitmap bitmap, ref PaintManagerUI manager, ref Rectangle rc,
            string strText, int textColor, int iFont, int style)
        {
            drawText(ref graphics, ref manager, ref rc, strText, textColor, iFont, style);
        }
        public static void drawText(ref Graphics graphics, ref PaintManagerUI manager, ref Rectangle rc,
            string strText, int textColor, int iFont, int style)
        {
            Font font = manager.getFont(iFont);
            Brush brush = new SolidBrush(Color.FromArgb(textColor));

            Size sz = EnginHelper.measureString(ref graphics, strText, font);
            Rectangle rc1 = rc;
            rc1.Width = sz.Width + 10;
            rc1.Height = sz.Height;
            //graphics.DrawRectangle(new Pen(Color.Yellow), rc);
            {
                Rectangle newRect = rc;
                Size rcText = EnginHelper.measureString(ref graphics, strText, font);

                if ((style & (int)FormatFlags.DT_CENTER) != 0)
                {
                    if (newRect.Width > rcText.Width + 3)
                    {
                        int newLeft = newRect.Left + (newRect.Width - rcText.Width) / 2;
                        int newRight = newRect.Right;
                        newRect.X = newLeft;
                        newRect.Width = newRight-newLeft;
                    }
                }
                if ((style & (int)FormatFlags.DT_VCENTER) != 0)
                {
                    if (newRect.Height > rcText.Height + 3)
                    {
                        int newTop = newRect.Top + (newRect.Height - rcText.Height) / 2;
                        int newBottom = newTop + rcText.Height;
                        newRect.Y = newTop;
                        newRect.Height = rcText.Height;
                    }
                }

                if ((style & (int)FormatFlags.DT_TOP) != 0)
                {
                }
                if ((style & (int)FormatFlags.DT_BOTTOM) != 0)
                {
                    if (newRect.Height > rcText.Height + 3)
                    {
                        int newTop = newRect.Bottom - rcText.Height;
                        int newBottom = newRect.Bottom;
                        newRect.Y = newTop;
                        newRect.Height = rcText.Height;
                    }
                }

                graphics.DrawString(strText, font, brush, newRect);
            }
            brush.Dispose();
            brush = null;
        }
        public static void drawHtmlText(ref Graphics hDC,
            ref Bitmap bitmap,
            ref PaintManagerUI pManager,
            ref Rectangle rc,
            string pstrText,
            int dwTextColor,
            ref Rectangle[] prcLinks,
            ref string[] sLinks,
            ref int nLinkRects,
            int uStyle)
        {
            HtmlStringPaser.parse(ref hDC,
                ref bitmap,
                ref pManager,
                ref rc,
                pstrText,
                dwTextColor,
                ref prcLinks,
                ref sLinks,
                ref nLinkRects,
                uStyle);
        }
    }

    public class HtmlStringPaser
    {
        public HtmlStringPaser()
        {
        }
        ~HtmlStringPaser()
        {
        }
        public static bool parse(ref Graphics hDC,
            ref Bitmap bitmap,
            ref PaintManagerUI pManager,
            ref Rectangle rc,
            string pstrText,
            int dwTextColor,
            ref Rectangle[] prcLinks,
            ref string[] sLinks,
            ref int nLinkRects,
            int uStyle)
        {
            // 考虑到在xml编辑器中使用<>符号不方便，可以使用{}符号代替
            // The string formatter supports a kind of "mini-html" that consists of various short tags:
            //
            //   Link:             <a x>text</a>      where x(optional) = link content, normal like app:notepad or http:www.xxx.com
            //   Bold:             <b>text</b>
            //   Color:            <c #xxxxxx>text</c>  where x = RGB in hex
            //   Change font:      <f x>text</f>        where x = font id
            //   Image:            <i x y>              where x = image name and y(optional) = imagelist id
            //   NewLine           <n>                  
            //   Paragraph:        <p>
            //   X Indent:         <x i>                where i = hor indent in pixels
            //   Y Indent:         <y i>                where i = ver indent in pixels 

            if (rc.IsEmpty)
            {
                return true;
            }

            bool bDraw = (uStyle & (int)FormatFlags.DT_CALCRECT) == 0;

            Rectangle rcClip = new Rectangle((int)hDC.ClipBounds.Left,
                (int)hDC.ClipBounds.Top,
                (int)hDC.ClipBounds.Width,
                (int)hDC.ClipBounds.Height);
            Region hOldRgn = hDC.Clip;
            Region hRgn = new Region(rc);

            if (bDraw)
            {
                hDC.Clip.Intersect(hRgn);
            }

            Font curFont = pManager.getDefaultFont();
            Color curColor = pManager.getDefaultFontColor();

            // 对于单行字符串,首先计算它的宽度或高度
            if ((uStyle & (int)FormatFlags.DT_SINGLELINE) != 0 &&
                (uStyle & (int)FormatFlags.DT_VCENTER) != 0 &&
                (uStyle & (int)FormatFlags.DT_CALCRECT) == 0)
            {
                Rectangle rcText = new Rectangle(0, 0, 9999, 100);
                int nLinks = 0;
                Rectangle[] rcLinks = null;
                string[] sLinks2 = null;
                parse(ref hDC,
                    ref bitmap,
                    ref pManager,
                    ref rcText,
                    pstrText,
                    dwTextColor,
                    ref rcLinks,
                    ref sLinks2,
                    ref nLinks,
                    uStyle | (int)FormatFlags.DT_CALCRECT);
                rc.Y = rc.Top + ((rc.Bottom - rc.Top) / 2) - ((rcText.Bottom - rcText.Top) / 2);
                rc.Height = (rcText.Bottom - rcText.Top);
            }
            if ((uStyle & (int)FormatFlags.DT_SINGLELINE) != 0 &&
                (uStyle & (int)FormatFlags.DT_CENTER) != 0 &&
                (uStyle & (int)FormatFlags.DT_CALCRECT) == 0)
            {
                Rectangle rcText = new Rectangle(0, 0, 9999, 100);
                int nLinks = 0;

                Rectangle[] rcLinks = null;
                string[] sLinks2 = null;

                parse(ref hDC,
                    ref bitmap,
                    ref pManager,
                    ref rcText,
                    pstrText,
                    dwTextColor,
                    ref rcLinks,
                    ref sLinks2,
                    ref nLinks,
                    uStyle | (int)FormatFlags.DT_CALCRECT);

                rc.X = rc.Left + ((rc.Right - rc.Left) / 2) - ((rcText.Right - rcText.Left) / 2);
                rc.Width = (rcText.Right - rcText.Left);
            }

            if ((uStyle & (int)FormatFlags.DT_SINGLELINE) != 0 &&
                (uStyle & (int)FormatFlags.DT_RIGHT) != 0 &&
                (uStyle & (int)FormatFlags.DT_CALCRECT) == 0)
            {
                Rectangle rcText = new Rectangle(0, 0, 9999, 100);
                int nLinks = 0;

                Rectangle[] rcLinks = null;
                string[] sLinks2 = null;

                parse(ref hDC,
                    ref bitmap,
                    ref pManager,
                    ref rcText,
                    pstrText,
                    dwTextColor,
                    ref rcLinks,
                    ref sLinks2,
                    ref nLinks,
                    uStyle | (int)FormatFlags.DT_CALCRECT);

                int x = rc.Right - (rcText.Right - rcText.Left);
                int width = rc.Right - x;
                rc.X = x;
                rc.Width = width;
            }

            // 判断鼠标是否落在链接上面，因为可能有多个链接，所以需要逐个判断
            // prcLinks是由计算字符串高度的时候自动计算出来的,并不能人为的指定它的位置和大小
            // nLinkRects同样是由该函数自动计算出来的
            bool bHoverLink = false;
            Point ptMouse = pManager.getMousePos();
            for (int i = 0; !bHoverLink && i < nLinkRects; i++)
            {
                sLinks[i] = "";
                if (prcLinks[i].Contains(ptMouse))
                {
                    bHoverLink = true;
                }
            }

            Size tm = EnginHelper.measureString("A", curFont);
            Point pt = new Point(rc.Left, rc.Top);
            int iLinkIndex = 0;
            int cyLine = tm.Height;
            int cyMinHeight = 0;
            int cxMaxWidth = 0;
            Point ptLinkStart = new Point();
            bool bInLink = false;

            int idx = 0;
            while (idx < pstrText.Length && pstrText[idx] != '\0')
            {
                if (pt.X >= rc.Right || pstrText[idx] == '\n')
                {
                    // 字符串换行，重新计算链接点击判断区域
                    if (bInLink && iLinkIndex < nLinkRects)
                    {
                        prcLinks[iLinkIndex].X = ptLinkStart.X;
                        prcLinks[iLinkIndex].Y = ptLinkStart.Y;
                        prcLinks[iLinkIndex].Width = Math.Min(pt.X, rc.Right) - ptLinkStart.X;
                        prcLinks[iLinkIndex].Height = (pt.Y + tm.Height) - ptLinkStart.Y;
                        iLinkIndex++;
                    }

                    if (bInLink && iLinkIndex < nLinkRects)
                    {
                        sLinks[iLinkIndex] = sLinks[iLinkIndex - 1];
                    }

                    if ((uStyle & (int)FormatFlags.DT_SINGLELINE) != 0)
                    {
                        break;
                    }

                    if (pstrText[idx] == '\n')
                    {
                        idx++;
                    }
                    pt.X = rc.Left;
                    pt.Y += cyLine;

                    if (pt.Y > rc.Bottom && bDraw)
                    {
                        break;
                    }

                    ptLinkStart = pt;
                    cyLine = tm.Height;
                    if (pt.X >= rc.Right)
                    {
                        break;
                    }
                    while (pstrText[idx] == ' ')
                    {
                        idx++;
                    }
                }
                else if ((pstrText[idx] == '<' || pstrText[idx] == '{')
                        && (pstrText[idx + 1] >= 'a' && pstrText[idx + 1] <= 'z')
                        && (pstrText[idx + 2] == ' ' || pstrText[idx + 2] == '>' || pstrText[idx + 2] == '}'))
                {
                    idx++;
                    switch (pstrText[idx])
                    {
                        case 'a':  // Link
                            {
                                idx++;
                                if (pstrText[idx] == ' ')
                                {
                                    idx++;
                                }
                                if (iLinkIndex < nLinkRects)
                                {
                                    while (idx < pstrText.Length && pstrText[idx] != '\0' && pstrText[idx] != '>' && pstrText[idx] != '}')
                                    {
                                        sLinks[iLinkIndex] += pstrText[idx];
                                        idx++;
                                    }
                                }

                                Color clrColor = bHoverLink ? pManager.getDefaultLinkFontHoverColor() : pManager.getDefaultLinkFontColor();
                                curColor = clrColor;
                                curFont = pManager.getDefaultLinkFont();
                                tm = EnginHelper.measureString("A", curFont);
                                cyLine = Math.Max(cyLine, tm.Height);
                                ptLinkStart = pt;
                                bInLink = true;

                                break;
                            }
                        case 'f':   // Font
                            {
                                idx++;

                                {
                                    // 获取整数字符串
                                    string sNum = getToken(ref pstrText, ref idx, ">}");
                                    int iFont;
                                    if (pstrText[idx] == '\0' || int.TryParse(sNum, out iFont) == false)
                                    {
                                        throw new Exception("非法字体标签");
                                    }
                                    curFont = pManager.getFont(iFont);
                                    tm = EnginHelper.measureString("A", curFont);
                                    cyLine = Math.Max(cyLine, tm.Height);
                                }

                                break;
                            }
                        case 'b':  // Bold text
                            {
                                idx++;

                                curFont = pManager.getDefaultBoldFont();
                                tm = EnginHelper.measureString("A", curFont);
                                cyLine = Math.Max(cyLine, tm.Height);

                                break;
                            }
                        case 'x':  // Indent
                            {
                                idx++;

                                {
                                    string sNum = getToken(ref pstrText, ref idx, ">}");
                                    int iWidth;
                                    if (pstrText[idx] == '\0' || int.TryParse(sNum, out iWidth) == false)
                                    {
                                        throw new Exception("非法X坐标标签");
                                    }
                                    pt.X += iWidth;
                                    cxMaxWidth = Math.Max(cxMaxWidth, pt.X);
                                }

                                break;
                            }
                        case 'n':  // Newline
                            {
                                idx++;

                                {
                                    if ((uStyle & (int)FormatFlags.DT_SINGLELINE) != 0)
                                    {
                                        break;
                                    }
                                    if (bInLink && iLinkIndex < nLinkRects)
                                    {
                                        prcLinks[iLinkIndex].X = ptLinkStart.X;
                                        prcLinks[iLinkIndex].Y = ptLinkStart.Y;
                                        prcLinks[iLinkIndex].Width = Math.Min(pt.X, rc.Right) - ptLinkStart.X;
                                        prcLinks[iLinkIndex].Height = pt.Y + tm.Height - ptLinkStart.Y;

                                        iLinkIndex++;
                                    }
                                    if (bInLink && iLinkIndex < nLinkRects)
                                    {
                                        sLinks[iLinkIndex] = sLinks[iLinkIndex - 1];
                                    }
                                    pt.X = rc.Left;
                                    pt.Y += cyLine;
                                    if (pt.Y > rc.Bottom && bDraw)
                                    {
                                        break;
                                    }
                                    ptLinkStart = pt;
                                    cyLine = tm.Height;
                                    if (pt.X >= rc.Right)
                                    {
                                        break;
                                    }

                                }

                                break;
                            }
                        case 'p':  // Paragraph
                            {
                                idx++;

                                //{
                                //    pt.X = rc.Right;
                                //    cyLine = Math.Max(cyLine, tm.Height);
                                //    curFont = pManager.getDefaultFont();
                                //    curColor = pManager.getDefaultFontColor();
                                //    tm = EnginHelper.measureString("A", curFont);
                                //}
                                break;
                            }
                        case 'y':
                            {
                                idx++;
                                {
                                    string sNum = getToken(ref pstrText, ref idx, ">}");
                                    if (pstrText[idx] == '\0' || int.TryParse(sNum, out cyLine) == false)
                                    {
                                        throw new Exception("非法Y坐标标签");
                                    }
                                }

                                break;
                            }
                        case 'i':  // Image
                            {
                                idx++;
                                {
                                    int iWidth = 0;
                                    int iHeight = 0;
                                    if (pstrText[idx] == ' ')
                                    {
                                        idx++;
                                    }
                                    TImageInfo pImageInfo = null;

                                    string sName = "";
                                    int imageListIndex = -1;
                                    sName = getToken(ref pstrText, ref idx, ">}");

                                    if (pstrText[idx] == ' ')
                                    {
                                        idx++;
                                    }
                                    string sNum = getToken(ref pstrText, ref idx, ">}");
                                    if (pstrText[idx] == '\0' || int.TryParse(sNum, out imageListIndex) == false)
                                    {
                                        throw new Exception("缺少图像索引,非法图像标签");
                                    }
                                    if (sName != "")
                                    {
                                        pImageInfo = pManager.getImage(sName);
                                    }
                                    if (pImageInfo != null)
                                    {
                                        iWidth = pImageInfo.mX;
                                        iHeight = pImageInfo.mY;
                                        if (imageListIndex != -1)
                                        {
                                            if (imageListIndex >= pImageInfo.mX / pImageInfo.mY)
                                            {
                                                imageListIndex = 0;
                                            }
                                            iWidth = iHeight;
                                        }
                                        if (pt.X + iWidth >= rc.Right &&
                                            iWidth <= rc.Right - rc.Left &&
                                            (uStyle & (int)FormatFlags.DT_SINGLELINE) == 0)
                                        {
                                            if (bInLink && iLinkIndex < nLinkRects)
                                            {
                                                prcLinks[iLinkIndex].X = ptLinkStart.X;
                                                prcLinks[iLinkIndex].Y = ptLinkStart.Y;
                                                prcLinks[iLinkIndex].Width = Math.Min(pt.X, rc.Right) - ptLinkStart.X;
                                                prcLinks[iLinkIndex].Height = pt.Y + tm.Height - ptLinkStart.Y;
                                                iLinkIndex++;
                                            }

                                            if (bInLink && iLinkIndex < nLinkRects)
                                            {
                                                sLinks[iLinkIndex] = sLinks[iLinkIndex - 1];
                                            }
                                            pt.X = rc.Left;
                                            pt.Y += cyLine;
                                            if (pt.Y > rc.Bottom && bDraw) break;
                                            ptLinkStart = pt;

                                        }

                                        if (bDraw)
                                        {
                                            Rectangle rcImage = new Rectangle(pt.X, pt.Y, iWidth, iHeight);
                                            Rectangle rcBmpPart = new Rectangle(0, 0, iWidth, iHeight);
                                            if (imageListIndex != -1)
                                            {
                                                rcBmpPart.X = iWidth * imageListIndex;
                                            }

                                            Rectangle rcCorner = new Rectangle(0, 0, 0, 0);
                                            RenderEngine.drawImage(ref hDC,
                                                ref bitmap,
                                                ref pImageInfo.mBitmap,
                                                ref rcImage,
                                                ref rcImage,
                                                ref rcBmpPart,
                                                ref rcCorner,
                                                pImageInfo.mAlphaChannel,
                                                255);
                                        }


                                        cyLine = Math.Max(iHeight, cyLine);
                                        pt.X += iWidth;
                                        cyMinHeight = pt.Y + iHeight;
                                        cxMaxWidth = Math.Max(cxMaxWidth, pt.X);
                                    }
                                }

                                break;
                            }
                        case 'c':  // Color
                            {
                                idx++;
                                {
                                    string value = getToken(ref pstrText, ref idx, ">}");
                                    if (pstrText[idx] == '\0')
                                    {
                                        throw new Exception("颜色标签有误");
                                    }

                                    value = value.TrimStart('#');
                                    if (value.Length == 6)
                                    {
                                        value = "FF" + value;
                                    }
                                    curColor = Color.FromArgb(Convert.ToInt32(value, 16));
                                }

                                break;
                            }

                    }
                    while (idx < pstrText.Length && pstrText[idx] != '\0' && pstrText[idx] != '>' && pstrText[idx] != '}')
                    {
                        idx++;
                    }
                    idx++;
                }
                else if ((pstrText[idx] == '<' || pstrText[idx] == '{') && pstrText[idx + 1] == '/')
                {
                    idx++;
                    idx++;
                    switch (pstrText[idx])
                    {
                        case 'a':
                            {
                                idx++;

                                if (iLinkIndex < nLinkRects)
                                {
                                    prcLinks[iLinkIndex].X = ptLinkStart.X;
                                    prcLinks[iLinkIndex].Y = ptLinkStart.Y;
                                    prcLinks[iLinkIndex].Width = Math.Max(pt.X, rc.Right) - ptLinkStart.X;
                                    prcLinks[iLinkIndex].Height = pt.Y + tm.Height - ptLinkStart.Y;
                                    iLinkIndex++;
                                }
                                // 恢复用户设置的颜色以及字体
                                curColor = Color.FromArgb(dwTextColor);
                                curFont = pManager.getDefaultFont();
                                tm = EnginHelper.measureString("A", curFont);
                                bInLink = false;

                                break;
                            }
                        case 'f':
                        case 'b':
                            {
                                idx++;

                                curFont = pManager.getDefaultFont();
                                tm = EnginHelper.measureString("A", curFont);

                                break;
                            }
                        case 'c':
                            {
                                idx++;

                                curColor = Color.FromArgb(dwTextColor);

                                break;
                            }
                    }
                    while (idx < pstrText.Length && pstrText[idx] != '\0' && pstrText[idx] != '>' && pstrText[idx] != '}')
                    {
                        idx++;
                    }
                    idx++;
                }
                else if (pstrText[idx] == '<' && pstrText[idx + 2] == '>' && (pstrText[idx + 1] == '{' || pstrText[idx + 1] == '}'))
                {
                    Size szSpace = EnginHelper.measureString("" + pstrText[idx + 1], curFont);
                    if (bDraw)
                    {
                        hDC.DrawString("" + pstrText[idx + 1], curFont, new SolidBrush(curColor), pt);
                    }
                    pt.X += szSpace.Width;
                    cxMaxWidth = Math.Max(cxMaxWidth, pt.X);
                    idx++;
                    idx++;
                    idx++;
                }
                else if (pstrText[idx] == '{' && pstrText[idx + 2] == '}' && (pstrText[idx + 1] == '<' || pstrText[idx + 1] == '>'))
                {
                    Size szSpace = EnginHelper.measureString("" + pstrText[idx + 1], curFont);
                    if (bDraw)
                    {
                        hDC.DrawString("" + pstrText[idx + 1], curFont, new SolidBrush(curColor), pt);
                    }
                    pt.X += szSpace.Width;
                    cxMaxWidth = Math.Max(cxMaxWidth, pt.X);
                    idx++;
                    idx++;
                    idx++;
                }
                else if (pstrText[idx] == ' ')
                {
                    Size szSpace = EnginHelper.measureString(" ", curFont);
                    if (bDraw)
                    {
                        hDC.DrawString(" ", curFont, new SolidBrush(curColor), pt);
                    }
                    pt.X += szSpace.Width;
                    cxMaxWidth = Math.Max(cxMaxWidth, pt.X);
                    idx++;
                }
                else
                {
                    Point ptPos = pt;
                    int cchChars = 0;
                    int cchSize = 0;
                    int cchLastGoodWord = 0;
                    int cchLastGoodSize = 0;

                    int idx2 = idx;
                    Size szText = new Size(0, 0);
                    if (pstrText[idx2] == '<' || pstrText[idx2] == '{')
                    {
                        idx2++;
                        cchChars++;
                        cchSize++;
                    }
                    while (idx2 < pstrText.Length && pstrText[idx2] != '\0' && pstrText[idx2] != '<' && pstrText[idx2] != '{' && pstrText[idx2] != '\n')
                    {
                        cchChars++;
                        cchSize++;
                        szText.Width = cchChars * tm.Width * 2;
                        if (pt.X + szText.Width >= rc.Right)
                        {
                            string value = "";
                            for (int j = 0; j < cchSize; j++)
                            {
                                value += pstrText[idx + j];
                            }
                            szText = EnginHelper.measureString(value, curFont);
                        }
                        if (pt.X + szText.Width > rc.Right)
                        {
                            if (pt.X + szText.Width > rc.Right && pt.X != rc.Left)
                            {
                                cchChars--;
                                cchSize--;
                            }
                            if ((uStyle & (int)FormatFlags.DT_WORDBREAK) != 0 && cchLastGoodWord > 0)
                            {
                                cchChars = cchLastGoodWord;
                                cchSize = cchLastGoodSize;
                            }
                            if ((uStyle & (int)FormatFlags.DT_END_ELLIPSIS) != 0 && cchChars > 2)
                            {
                                cchChars = cchLastGoodWord;
                                cchSize = cchLastGoodSize;
                            }
                            pt.X = rc.Right;
                            cxMaxWidth = Math.Max(cxMaxWidth, pt.X);

                            break;
                        }

                        if (!((pstrText[idx2] >= 'a' && pstrText[idx2] <= 'z') || (pstrText[idx2] >= 'A' && pstrText[idx2] <= 'Z')))
                        {
                            cchLastGoodWord = cchChars;
                            cchLastGoodSize = cchSize;
                        }
                        if (pstrText[idx2] == ' ')
                        {
                            cchLastGoodWord = cchChars;
                            cchLastGoodSize = cchSize;
                        }
                        idx2++;
                    }
                    if (cchChars > 0)
                    {
                        string value = "";
                        for (int j = 0; j < cchSize; j++)
                        {
                            value += pstrText[idx + j];
                        }
                        szText = EnginHelper.measureString(value, curFont);
                        if (bDraw)
                        {
                            hDC.DrawString(value, curFont, new SolidBrush(curColor), ptPos);
                            if (pt.X >= rc.Right && (uStyle & (int)FormatFlags.DT_END_ELLIPSIS) != 0)
                            {
                                hDC.DrawString("...", curFont, new SolidBrush(curColor), new Point(rc.Right - 10, ptPos.Y));
                            }
                        }
                        pt.X += szText.Width;
                        cxMaxWidth = Math.Max(cxMaxWidth, pt.X);
                        idx += cchSize;
                    }
                }
                if (iLinkIndex > nLinkRects)
                {
                    throw new Exception("链接过多");
                }
            }
            for (int i = iLinkIndex; i < nLinkRects; i++)
            {
                prcLinks[i].X = 0;
                prcLinks[i].Y = 0;
                prcLinks[i].Width = 0;
                prcLinks[i].Height = 0;
            }
            nLinkRects = iLinkIndex;

            if ((uStyle & (int)FormatFlags.DT_CALCRECT) != 0)
            {
                int newBottom = Math.Max(cyMinHeight, pt.Y + cyLine);
                int newRight = Math.Min(rc.Right, cxMaxWidth);

                rc.Height = newBottom - rc.Top;
                rc.Width = newRight - rc.Left;
            }
            if (bDraw)
            {
                hDC.Clip = hOldRgn;
            }
            hOldRgn = null;
            hRgn = null;

            return true;
        }
        protected static string getToken(ref string str, ref int idx, string endChar = "")
        {
            string token = "";

            // 忽略白字符
            while (idx < str.Length && str[idx] != '\0' && char.IsWhiteSpace(str[idx]) &&
                (endChar == "" || endChar.Contains("" + str[idx]) == false))
            {
                idx++;
            }
            // 获取整数字符串
            while (idx < str.Length && str[idx] != '\0' && char.IsWhiteSpace(str[idx]) == false &&
                (endChar == "" || endChar.Contains("" + str[idx]) == false))
            {
                token += str[idx];
                idx++;
            }

            return token;
        }
    }

    public class EnginHelper
    {
        public EnginHelper()
        {
        }
        ~EnginHelper()
        {
        }

        public static Size measureString(ref Graphics graphics, string str, Font font, StringFormat format = null, int maxSize = 500)
        {
            return measureString(str, font);
        }

        public static Size measureString(string str, Font font)
        {
            return TextRenderer.MeasureText(str, font);
        }
    }
}
