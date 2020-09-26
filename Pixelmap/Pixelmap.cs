using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace Pixelmap
{
    /// <summary>
    /// Bitmapをピクセル単位で高速に扱える
    /// 常にFormat32bppArgbに変換する
    /// Dispose()時にコンストラクタで設定したBitmapに変更を反映する
    /// 
    /// 注意:
    /// Bitmapの開放は行わない
    /// </summary>
    public class Pixelmap : IDisposable, IEnumerable<Pixel>
    {
        Pixel[] pixels;
        BitmapData data;
        private Bitmap bitmap;

        /// <summary>
        /// ピクセルの形式
        /// </summary>
        public PixelFormat PixelFormat { get; private set; }
        /// <summary>
        /// 幅
        /// </summary>
        public int Width { get; }
        /// <summary>
        /// 高
        /// </summary>
        public int Height { get; }
        /// <summary>
        /// ピクセル数
        /// </summary>
        public int Count => pixels.Length;
        /// <summary>
        /// byte長
        /// </summary>
        public int Length => Math.Abs(data.Stride) * data.Height;

        public Bitmap Bitmap
        {
            get
            {
                Commit();
                return bitmap;
            }

            private set => bitmap = value;
        }

        private void Commit()
        {
            System.Runtime.InteropServices.Marshal.Copy(pixels.SelectMany(p => p.ToBytes()).ToArray(), 0, data.Scan0, pixels.Length * 4);
            bitmap.UnlockBits(data);

            data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
        }

        /// <summary>
        /// 2次元位置指定
        /// </summary>
        /// <param name="x">横座標</param>
        /// <param name="y">縦座標</param>
        /// <returns>指定ピクセル</returns>
        public Pixel this[int x, int y]
        {
            get
            {
                return pixels[x + y * Width];
            }
            set
            {
                pixels[x + y * Width] = value;
            }
        }

        /// <summary>
        /// 1次元位置指定
        /// </summary>
        /// <param name="i">順番号</param>
        /// <returns>指定ピクセル</returns>
        public Pixel this[int i]
        {
            get
            {
                return pixels[i];
            }
            set
            {
                pixels[i] = value;
            }
        }

        /// <summary>
        /// PixelMapの元になるBitmapを指定
        /// BitmapのDisposeは呼び出し側ですること
        /// </summary>
        /// <param name="bitmap">元となるBitmap</param>
        public Pixelmap(Bitmap bitmap)
        {
            PixelFormat = bitmap.PixelFormat;

            Width = bitmap.Width;
            Height = bitmap.Height;
            this.Bitmap = bitmap;
            data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat);

            pixels = MakePixels();
        }

        private Pixel[] MakePixels()
        {
            IEnumerable<Pixel> pixels = PixelFormat switch
            {
                PixelFormat.Indexed => throw new NotImplementedException(),
                PixelFormat.Gdi => throw new NotImplementedException(),
                PixelFormat.Alpha => throw new NotImplementedException(),
                PixelFormat.PAlpha => throw new NotImplementedException(),
                PixelFormat.Extended => throw new NotImplementedException(),
                PixelFormat.Canonical => throw new NotImplementedException(),
                PixelFormat.Undefined => throw new NotImplementedException(),
                PixelFormat.Format1bppIndexed => throw new NotImplementedException(),
                PixelFormat.Format4bppIndexed => throw new NotImplementedException(),
                PixelFormat.Format8bppIndexed => MakePixelsFrom8bppIndexed(),
                PixelFormat.Format16bppGrayScale => throw new NotImplementedException(),
                PixelFormat.Format16bppRgb555 => throw new NotImplementedException(),
                PixelFormat.Format16bppRgb565 => throw new NotImplementedException(),
                PixelFormat.Format16bppArgb1555 => throw new NotImplementedException(),
                PixelFormat.Format24bppRgb => throw new NotImplementedException(),
                PixelFormat.Format32bppRgb => throw new NotImplementedException(),
                PixelFormat.Format32bppArgb => MakePixelsFrom32bppArgb(),
                PixelFormat.Format32bppPArgb => throw new NotImplementedException(),
                PixelFormat.Format48bppRgb => throw new NotImplementedException(),
                PixelFormat.Format64bppArgb => throw new NotImplementedException(),
                PixelFormat.Format64bppPArgb => throw new NotImplementedException(),
                PixelFormat.Max => throw new NotImplementedException(),
                _ => throw new InvalidEnumArgumentException("PixelFormatが無効です。")
            };

            // dataの中身が変わる場合があるので,ここで評価して値を確定させる
            Pixel[] pixelArray = pixels.ToArray();

            // PixelFormatが32bppArgb以外の時はバイト列の大きさが変わるので領域を確保し直す
            if (PixelFormat != PixelFormat.Format32bppArgb)
            {
                bitmap.UnlockBits(data);

                Bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
                data = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadWrite,
                    PixelFormat.Format32bppArgb);

                PixelFormat = PixelFormat.Format32bppArgb;
            }

            return pixelArray;
        }

        private IEnumerable<Pixel> MakePixelsFrom32bppArgb()
        {
            var bps = new byte[Length];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bps, 0, bps.Length);

            byte selector = 0;
            Pixel tmpPixel = new Pixel();
            foreach (var v in bps)
            {
                switch (selector)
                {
                    case 0:
                        tmpPixel.B = v;
                        break;
                    case 1:
                        tmpPixel.G = v;
                        break;
                    case 2:
                        tmpPixel.R = v;
                        break;
                    case 3:
                        tmpPixel.A = v;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Pixelmap::MakePixels()::selector Error");
                }

                selector = (byte)(++selector % 4);

                if (selector == 0)
                {
                    yield return tmpPixel;
                    tmpPixel = new Pixel();
                }
            }
        }

        private IEnumerable<Pixel> MakePixelsFrom8bppIndexed()
        {
            var bps = new byte[Length];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bps, 0, bps.Length);
            ColorPalette palette = bitmap.Palette;

            foreach (var v in bps)
            {
                yield return new Pixel(palette.Entries[v]);
            }
        }

        #region Interface
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            data.PixelFormat = PixelFormat;
            if (disposing)
            {
                System.Runtime.InteropServices.Marshal.Copy(pixels.SelectMany(p => p.ToBytes()).ToArray(), 0, data.Scan0, pixels.Length * 4);
            }
            bitmap.UnlockBits(data);
        }

        public IEnumerator<Pixel> GetEnumerator()
        {
            return ((IEnumerable<Pixel>)pixels).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return pixels.GetEnumerator();
        }
        #endregion

        /// <summary>
        /// Marshal.Copy()はしない
        /// UnlockBits()はする
        /// </summary>
        ~Pixelmap()
        {
            Dispose(false);
        }
    }
}
