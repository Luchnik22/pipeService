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
using System.Net;
using System.Net.Sockets;

using System.Drawing;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace RS_Srever
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private delegate void AsynkWorker();

        public MainWindow()
        {
            InitializeComponent();
            new AsynkWorker(Run).BeginInvoke(null, null);
        }

        private void Run()
        {
            UdpClient udp = new UdpClient(new IPEndPoint(IPAddress.Any, 34000));
            var ep = new IPEndPoint(IPAddress.None, 0);
            var mass = udp.Receive(ref ep);

            PixelFormat pf = PixelFormats.Bgr32;
            int width = 200;
            int height = 200;
            int rawStride = (width * pf.BitsPerPixel + 7) / 8;
            byte[] rawImage = new byte[rawStride * height];

            // Initialize the image with data.
            Random value = new Random();
            value.NextBytes(rawImage);

            // Create a BitmapSource.
            BitmapSource bitmap = BitmapSource.Create(width, height,
                96, 96, pf, null,
                rawImage, rawStride);
            
            this.Background = new ImageBrush(bitmap);
        }
    }
}
