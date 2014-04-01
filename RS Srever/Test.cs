using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace RS_Srever
{
    class Test
    {
        private UdpClient udpClient;
        private const UInt16 udpSize = 65507;
        private const UInt16 controlBlockSize = 5;
        private delegate void AsynkWorker();
        private delegate void DrawEvent(BitmapImage bitmapImage);

        /// <summary>
        /// Тестовый класс - короче говоря мусор...
        /// </summary>
        public Test()
        {
            PixelFormat pf = PixelFormats.Bgr24;
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;
            int rawStride = (width * pf.BitsPerPixel + 7) / 8;
            byte[] rawImage = new byte[rawStride * height];

            Bitmap bpm = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(bpm);

            while (true)
            {
                //System.Threading.Thread.Sleep(40);
                g.CopyFromScreen(new System.Drawing.Point(0, 0), new System.Drawing.Point(0, 0), new System.Drawing.Size(width, height));

                MemoryStream memoryStream = new MemoryStream();
                bpm.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                MemoryStream ms = new MemoryStream(memoryStream.ToArray());

                BitmapImage bitmapImg = new BitmapImage();
                bitmapImg.BeginInit();
                bitmapImg.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImg.StreamSource = ms;
                bitmapImg.EndInit();
                bitmapImg.Freeze();

               //  Dispatcher.Invoke(new DrawEvent(MainWindow_sync), bitmapImg);
            }
        }
    }
}
