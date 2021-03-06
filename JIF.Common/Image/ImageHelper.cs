﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace JIF.Common
{
    public static class ImageHelper
    {
        public enum ThumbnailType
        {
            /// <summary>
            /// 指定高宽缩放（可能变形）
            /// </summary>
            HW,
            /// <summary>
            /// 指定宽，高按比例
            /// </summary>
            W,
            /// <summary>
            /// 指定高，宽按比例
            /// </summary>
            H,
            /// <summary>
            /// 指定高宽裁减（不变形）, 裁剪图片正中位置
            /// </summary>
            Cut,
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImagePath">源图路径（物理路径）</param>
        /// <param name="thumbnailPath">缩略图路径（物理路径）</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式（w:指定宽，高度自适应；h:指定高度，宽度自适应；cut:裁剪,图片中心向外；默认w）</param>   
        public static void MakeThumbnail(string originalImagePath, string thumbnailPath, int width, int height, ThumbnailType mode)
        {
            try
            {
                using (System.Drawing.Image originalImage = System.Drawing.Image.FromFile(originalImagePath))
                {

                    if (mode != ThumbnailType.Cut)
                    {
                        double d1 = double.Parse(width.ToString()) / double.Parse(height.ToString());

                        //宽高比
                        double wd = double.Parse(originalImage.Width.ToString()) / double.Parse(originalImage.Height.ToString());
                        //高宽比
                        double hd = double.Parse(originalImage.Height.ToString()) / double.Parse(originalImage.Width.ToString());


                        double itsw = 0d;
                        double itsh = 0d;
                        int itsin = 0;

                        //与宽高比对比
                        if (d1 > wd)
                        {
                            itsw = d1 - wd;
                        }
                        else
                        {
                            itsw = wd - d1;
                        }

                        //与高宽比对比
                        if (d1 > hd)
                        {
                            itsh = d1 - hd;
                        }
                        else
                        {
                            itsh = hd - d1;
                        }

                        //如果高宽比更接近比例
                        if (itsw > itsh)
                        {
                            mode = ThumbnailType.W;
                        }
                        else
                        {
                            mode = ThumbnailType.H;
                        }
                    }


                    int towidth = width;
                    int toheight = height;

                    int x = 0;
                    int y = 0;
                    int ow = originalImage.Width;
                    int oh = originalImage.Height;

                    switch (mode)
                    {
                        case ThumbnailType.HW://指定高宽缩放（可能变形） 
                            break;
                        case ThumbnailType.W://指定宽，高按比例
                            toheight = originalImage.Height * width / originalImage.Width;
                            break;
                        case ThumbnailType.H://指定高，宽按比例
                            towidth = originalImage.Width * height / originalImage.Height;
                            break;
                        case ThumbnailType.Cut://指定高宽裁减（不变形）
                            if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                            {
                                oh = originalImage.Height;
                                ow = originalImage.Height * towidth / toheight;
                                y = 0;
                                x = (originalImage.Width - ow) / 2;
                            }
                            else
                            {
                                ow = originalImage.Width;
                                oh = originalImage.Width * height / towidth;
                                x = 0;
                                y = (originalImage.Height - oh) / 2;
                            }
                            break;
                        default:
                            break;
                    }

                    //新建一个bmp图片
                    System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);

                    //新建一个画板
                    System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

                    //设置高质量插值法
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

                    //设置高质量,低速度呈现平滑程度
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    //清空画布并以透明背景色填充
                    g.Clear(System.Drawing.Color.Transparent);

                    //在指定位置并且按指定大小绘制原图片的指定部分
                    g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, towidth, toheight),
                        new System.Drawing.Rectangle(x, y, ow, oh),
                        System.Drawing.GraphicsUnit.Pixel);

                    try
                    {
                        //以jpg格式保存缩略图
                        bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    finally
                    {
                        originalImage.Dispose();
                        bitmap.Dispose();
                        g.Dispose();
                    }
                }
            }
            catch
            {

            }
        }


        /// <summary>
        /// 生成验证码图片
        /// </summary>
        /// <param name="vcode">验证码文字</param>
        /// <param name="imgWidth">图片宽度</param>
        /// <param name="imgHeight">图片高度</param>
        /// <returns></returns>
        public static byte[] GenValidateCode(string vcode, int width = 120, int height = 40)
        {
            if (string.IsNullOrWhiteSpace(vcode) || width < 1 || height < 1)
            {
                throw new ArgumentException("ImageHelper : GenerateValidateCode param err.");
            }

            // 可选字体
            string[] oFontNames = { "华文彩云", "方正舒体", "华文琥珀", "华文行楷", "Calibri (西文正文)", "Arial Black" };

            // 字符栅格大小, 用于控制每个字符书写位置
            var lattice = width / vcode.Length;

            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);

            try
            {

                g.Clear(Color.White);

                //背景噪点生成
                for (int i = 0; i < 100; i++)
                {
                    Pen blackPen = new Pen(RandomHelper.GenColor(100), 0);
                    int x1 = RandomHelper.Gen(0, width);
                    int y1 = RandomHelper.Gen(0, height);
                    g.DrawRectangle(blackPen, x1, y1, 1, 1);
                }

                // 背景干扰线
                for (int i = 0; i < 10; i++)
                {
                    Pen pen = new Pen(RandomHelper.GenColor(100), RandomHelper.Gen(0, 5));
                    var p1 = new Point(RandomHelper.Gen(0, width), RandomHelper.Gen(0, height));
                    var p2 = new Point(RandomHelper.Gen(0, width), RandomHelper.Gen(0, height));

                    g.DrawLine(pen, p1, p2);
                }


                for (int i = 0; i < vcode.Length; i++)
                {
                    var s = vcode[i].ToString();

                    //文字距中
                    var format = new StringFormat(StringFormatFlags.NoClip);
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    var font = new Font(
                        oFontNames[RandomHelper.Gen(0, oFontNames.Length - 1)],
                        RandomHelper.Gen(18, 36),
                        FontStyle.Bold);

                    var brush = new SolidBrush(RandomHelper.GenColor());

                    //var x = i * lattice + RandomHelper.Gen(0, 5);
                    //var y = (height - font.Height) / 2 + RandomHelper.Gen(-5, 5);

                    //var x = 14;
                    //var y = 14;

                    //Point dot = new Point(x, y);


                    // 字符旋转角度
                    //var angel = RandomHelper.Gen(-45, 45);

                    //g.TranslateTransform(dot.X, dot.Y);
                    //g.RotateTransform(angel);

                    var x = i * lattice + RandomHelper.Gen(0, 5);
                    var y = (height - font.Height) / 2 + RandomHelper.Gen(-5, 5);
                    g.DrawString(s, font, brush, x, y);
                    //g.DrawString(s, font, brush, 1, 1, format);

                    //g.ResetTransform();
                    //g.RotateTransform(-angel);

                    //g.TranslateTransform(-2, -dot.Y);//移动光标到指定位置，每个字符紧凑显示，避免被软件识别

                }

                //保存图片数据
                using (var stream = new MemoryStream())
                {
                    bmp.Save(stream, ImageFormat.Png);
                    return stream.ToArray();
                }
            }
            finally
            {
                g.Dispose();
                bmp.Dispose();
            }

        }

    }
}
