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
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        private UdpClient udpClient;
        private const UInt16 udpSize = 65507;
        private const UInt16 controlBlockSize = 5;                
        private delegate void AsynkWorker();
        private delegate void DrawEvent(BitmapImage bitmapImage);
        private delegate void Invoke(byte[] data);
        private Decoder decoder = new Decoder();
        private UInt16 port;

        public MainWindow()
        {
            InitializeComponent();
            decoder.FrameReady += new Decoder.EventReady(decoder_FrameReady);
            new AsynkWorker(Run).BeginInvoke(null, null);
        }

        void decoder_FrameReady(BitmapImage img)
        {
            this.Background = new ImageBrush(img);
        }

        void MainWindow_Invoke(byte[] data)
        {
            try
            {
                decoder.addPacked(data);
            }
            catch (Exception ex)
            {
            }
        }

        private void Run()
        {
            using (StreamReader SR = new StreamReader("port.txt"))
            {
                port = UInt16.Parse(SR.ReadLine());
            }

            UdpClient udp = new UdpClient(new IPEndPoint(IPAddress.Any, port));
            IPEndPoint ep = new IPEndPoint(IPAddress.None, 0);

            while (true)
            {
                byte[] mass = udp.Receive(ref ep);
                Dispatcher.Invoke(new Invoke(MainWindow_Invoke), mass);
            }

            /* byte[] test = new byte[5];
            test[0] = 1;
            test[1] = 2;
            test[2] = 3;
            test[3] = 4;
            test[4] = 5;

            decoder.addPacked(test); */            
        }
    }
}
