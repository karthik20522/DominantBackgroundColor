using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace karthik20522
{
    public class DominantColor
    {   
        public async Task<string> GetDominantColor(string source)
        {
            var _client = new HttpClient();

            var imageData = await _client.GetStreamAsync(source);

            using (var bitmap = new Bitmap(imageData))
            {
                Equalize(bitmap);

                var colorsWithCount =
                    GetPixels(bitmap)
                        .GroupBy(color => color)
                        .Select(grp =>
                            new
                            {
                                Color = grp.Key,
                                Count = grp.Count()
                            })
                        .OrderByDescending(x => x.Count)
                        .Take(1); //take top color

                return ColorTranslator.ToHtml(colorsWithCount.First().Color);
            }
        }

        private IEnumerable<Color> GetPixels(Bitmap bitmap)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    yield return pixel;
                }
            }
        }

        private string GetClosestColor(string color)
        {
            var c = ColorTranslator.FromHtml(color);
            double diff = 200000;
            var temp = new Dictionary<string, double>();

            foreach (var vColor in ColorConstants.MonoColors)
            {
                if (vColor.Value.Contains(color.Substring(1)))
                {
                    return color;
                }
                else
                {
                    foreach (string colorHex in vColor.Value)
                    {
                        Color validColor = ColorTranslator.FromHtml("#" + colorHex);

                        if (diff > (diff = Math.Pow(c.R - validColor.R, 2) + Math.Pow(c.G - validColor.G, 2) + Math.Pow(c.B - validColor.B, 2)))
                        {
                            if (temp.ContainsKey(colorHex))
                                temp[colorHex] = diff;
                            else
                                temp.Add(colorHex, diff);
                        }
                    }
                }
            }

            return string.Format("#{0}", temp.AsEnumerable().OrderBy(s => s.Value).First().Key);
        }

        private Bitmap Equalize(Bitmap bmp) // SAFE BUT SLOW
        {
            long[] GrHst = new long[256]; long HstValue = 0;
            long[] GrSum = new long[256]; long SumValue = 0;

            for (int row = 0; row < bmp.Height; row++)
                for (int col = 0; col < bmp.Width; col++)
                {
                    HstValue = (long)(255 * bmp.GetPixel(col, row).GetBrightness());
                    GrHst[HstValue]++;
                }

            for (int level = 0; level < 256; level++)
            {
                SumValue += GrHst[level];
                GrSum[level] = SumValue;
            }

            for (int row = 0; row < bmp.Height; row++)
                for (int col = 0; col < bmp.Width; col++)
                {
                    var clr = bmp.GetPixel(col, row);
                    HstValue = (long)(255 * clr.GetBrightness());
                    HstValue = (long)(255f / (bmp.Width * bmp.Height) * GrSum[HstValue] - HstValue);

                    int R = (int)Math.Min(255, clr.R + HstValue / 3); //.299
                    int G = (int)Math.Min(255, clr.G + HstValue / 3); //.587
                    int B = (int)Math.Min(255, clr.B + HstValue / 3); //.112

                    bmp.SetPixel(col, row, System.Drawing.Color.FromArgb(Math.Max(R, 0), Math.Max(G, 0), Math.Max(B, 0)));
                }

            return bmp;
        }
    }
}
