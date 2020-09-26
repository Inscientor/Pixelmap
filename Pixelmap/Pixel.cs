using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixelmap
{
    public class Pixel
    {
        #region プロパティ
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 不透明な黒
        /// </summary>
        public Pixel()
        {
            R = 0x00;
            G = 0x00;
            B = 0x00;
            A = 0xFF;
        }
        /// <summary>
        /// 色を256段階で指定
        /// </summary>
        /// <param name="a">不透明度</param>
        /// <param name="r">赤</param>
        /// <param name="g">緑</param>
        /// <param name="b">青</param>
        public Pixel(byte a, byte r, byte g, byte b) => FromARGB(a, r, g, b);

        /// <summary>
        /// 色を256段階で指定
        /// </summary>
        /// <param name="r">赤</param>
        /// <param name="g">緑</param>
        /// <param name="b">青</param>
        public Pixel(byte r, byte g, byte b) => FromARGB(r, g, b);

        /// <summary>
        /// Color構造体で指定
        /// </summary>
        /// <param name="color">色</param>
        public Pixel(Color color)
        {
            FromColor(color);
        }

        /// <summary>
        /// (B,G,R,A)の順として解釈
        /// 足りなければ初期値(色なら0x00、不透明度なら0xFF) 余分は無視
        /// </summary>
        /// <param name="bytes">(B,G,R,A)</param>
        public Pixel(IEnumerable<byte> bytes) => FromBytes(bytes);
        #endregion

        #region From系メソッド
        /// <summary>
        /// 色を256段階で指定
        /// </summary>
        /// <param name="a">不透明度</param>
        /// <param name="r">赤</param>
        /// <param name="g">緑</param>
        /// <param name="b">青</param>
        public void FromARGB(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// 色を256段階で指定
        /// </summary>
        /// <param name="r">赤</param>
        /// <param name="g">緑</param>
        /// <param name="b">青</param>
        public void FromARGB(byte r, byte g, byte b) => FromARGB(0xFF, r, g, b);

        /// <summary>
        /// Color構造体で指定
        /// </summary>
        /// <param name="color">色</param>
        public void FromColor(Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }

        /// <summary>
        /// (B,G,R,A)の順として解釈
        /// 足りなければ初期値(色なら0x00、不透明度なら0xFF) 余分は無視
        /// </summary>
        /// <param name="bytes">(B,G,R,A)</param>
        public void FromBytes(IEnumerable<byte> bytes)
        {
            B = bytes.ElementAtOrDefault(0);
            G = bytes.ElementAtOrDefault(1);
            R = bytes.ElementAtOrDefault(2);
            try
            {
                A = bytes.ElementAt(3);
            }
            catch (ArgumentOutOfRangeException)
            {
                A = 0xFF;
            }
        }
        #endregion

        #region To系メソッド
        /// <summary>
        /// Color構造体に変換
        /// </summary>
        /// <returns>Color</returns>
        public Color ToColor() => Color.FromArgb(A, R, G, B);
        /// <summary>
        /// (B,G,R,A)の順で格納したIEnumerable&lt;byte&gt;に変換
        /// </summary>
        /// <returns>Bytes</returns>
        public IEnumerable<byte> ToBytes() => new byte[] { B, G, R, A };
        #endregion
    }
}
