using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FastBitmapLib;

namespace Plot2DImage
{
    struct DataLine
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Intensity { get; set; }
    }
    struct PolarIntensity
    {
        public float HZ { get; set; }
        public float Z { get; set; }
        public float Intensity { get; set; }
    }
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ReadFile(ofd.FileName);
            }
        }

        private void ReadFile(string ofdFileName)
        {
            var content = File.ReadAllLines(ofdFileName);
            var converted = content.Select(ConvertLine).ToList();
            var polar = converted.Select(ConvertToPolarCoordinates).ToList();
            Bitmap i = new Bitmap((int) width.Value,(int)width.Value,PixelFormat.Format4bppIndexed);
            using (var fastBitmap = i.FastLock())
            {
                /*for (int x = 0; x < width.Value; x++)
                {
                    for (int y = 0; y < width.Value; y++)
                    {
                        i.SetPixel(x,y,Color.Magenta);
                    }
                }*/
                fastBitmap.Clear(Color.Magenta);
                foreach (var polarIntensity in polar)
                {
                    var x = ConvertToSquarePixel(polarIntensity.HZ);
                    var y = ConvertToSquarePixel(polarIntensity.Z);
                    var grayValue = (int) ((polarIntensity.Intensity + 20) / 30 * 255);
                    grayValue = Math.Min(255, grayValue);
                    fastBitmap.SetPixel(x, y, Color.FromArgb(255, grayValue, grayValue, grayValue));
                    //i.SetPixel(x, y, Color.FromArgb(255, grayValue, grayValue, grayValue));
                }
            }

            i.Save("output.bmp");
        }

        private int ConvertToSquarePixel(float polarIntensity)
        {
            return (int)(polarIntensity / (Math.PI * 2) * (double) width.Value);
        }

        private PolarIntensity ConvertToPolarCoordinates(DataLine arg)
        {
            var len = Math.Sqrt(arg.X * arg.X + arg.Y * arg.Y + arg.Z * arg.Z);
            var hz = Math.Atan2(arg.Y, arg.X);
            var z = Math.Cos(arg.Z / len);
            if (hz < 0)
            {
                hz += 2 * Math.PI;
            }
            return new PolarIntensity()
            {
                HZ = (float) hz,
                Z = (float) z,
                Intensity = arg.Intensity
            };
        }

        private DataLine ConvertLine(string arg)
        {
            var split = arg.Split(',').Select(str => float.Parse(str,CultureInfo.InvariantCulture)).ToArray();
            return new DataLine()
            {
                X = split[0],
                Y = split[1],
                Z = split[2],
                Intensity = split[3],
            };
        }
    }
}
