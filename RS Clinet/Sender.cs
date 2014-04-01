using System;
using System.Drawing;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;

namespace RS_Clinet
{
    public class Sender
    {        
        private IPEndPoint ipEndPoint;
        private UdpClient udpClient;
        private int width;
        private int height;
        private const UInt16 udpSize = 65507;
        private const UInt16 controlBlockSize = 5;
        private Random random = new Random();
        private byte lastA = 0; // Последний код цепочки
        private byte lastB = 0; // Последний код цепочки
        private float FPS = 30;

        public Sender()
        {
            // Загружаем номер порта, на которой надо встать
            using (StreamReader streamReader = new StreamReader("ip.txt"))
            {                
                string ip = streamReader.ReadLine();
                int port = Convert.ToInt32(streamReader.ReadLine());

                ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }

            width = Screen.PrimaryScreen.Bounds.Width;
            height = Screen.PrimaryScreen.Bounds.Height;            
        }

        public void Run()
        {
            udpClient = new UdpClient();
            Bitmap backGround = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(backGround);

            while (true)
            {
                System.Threading.Thread.Sleep((int)(1000 / FPS));
                graphics.CopyFromScreen(0, 0, 0, 0, new Size(width, height));   // Получаем снимок экрана

                byte [] bytes = ConvertToByte(backGround);                      // Получаем изображение в виде массива байтов
                List<byte[]> data = Package(bytes);                             // Упаковка изображения в протокол
                
                foreach (var block in data)
                {
                    udpClient.Send(block, block.Length, ipEndPoint);
                    //System.Threading.Thread.Sleep(100);
                }
                //return;
            }
        }

        // Конвертируем изображение в массив байтов со сжатием Jpeg
        private byte [] ConvertToByte(Bitmap bmp)
        {
            MemoryStream memoryStream = new MemoryStream();
            bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Упаковка в протокол передачи
        /// </summary>
        /// <param name="bt">Данные</param>
        /// <returns>Пакеты для передачи UDP</returns>
        private List<byte[]> Package(byte[] bt)
        {
            int countMsg = (int)Math.Ceiling((double)bt.Length / (double)udpSize); // Количество сообщений
            List<byte[]> chain = new List<byte[]>();                               // Цепочка сообщений

            if (countMsg > 65536)
                throw new Exception("Вы пытаетесь передать сообщение больше 4 ГБ - протокол не подерживает передачу свыше 4 ГБ");
            
            byte[] controlBlock = new byte[controlBlockSize]; // Формируем контрольный блок
            controlBlock[0] = 1;

            byte a, b;
            do 
            {
                a = (byte)random.Next(0, 256);
                b = (byte)random.Next(0, 256);

            } while(a == lastA && b == lastB); // Цикл нужен, чтобы случайно не совпал код сообщений из другой цепочки, 
                                               // в одной из параллельных вселенной этот цикл выполняется бесконечно

            controlBlock[1] = a;
            controlBlock[2] = b;
            controlBlock[3] = BitConverter.GetBytes(countMsg)[0];
            controlBlock[4] = BitConverter.GetBytes(countMsg)[1];

            chain.Add(controlBlock);

            int offset = 0;
            for (int i = 0; i < countMsg; i++)
            {
                byte[] msgBlock = new byte[udpSize]; // Формируем блок сообщения
                msgBlock[0] = 0;
                msgBlock[1] = a;
                msgBlock[2] = b;

                msgBlock[3] = BitConverter.GetBytes(i)[0];
                msgBlock[4] = BitConverter.GetBytes(i)[1];

                int msgBlockLength;
                if (bt.Length - offset <= udpSize)
                {
                    msgBlockLength = bt.Length - offset;
                }
                else
                {
                    msgBlockLength = udpSize - 7; // 5 - в данном случае первые 5 байтов
                }

                msgBlock[5] = BitConverter.GetBytes(msgBlockLength)[0];
                msgBlock[6] = BitConverter.GetBytes(msgBlockLength)[1];

                if (i == countMsg - 1)
                {
                    Array.Copy(bt, offset, msgBlock, 7, bt.Length - offset);
                }
                else
                {
                    Array.Copy(bt, offset, msgBlock, 7, udpSize - 7);
                }

                chain.Add(msgBlock);
                
                offset += udpSize;
                if (offset > bt.Length)
                {
                    offset = bt.Length;
                }
            }

            return chain;
        }

        /*  Описание протокола передачи LO поверх UDP
            1 байт - контрольный пакет (1 если контрольный и 0 если не контрольный)

            Расположение байтов для контрольного пакета
            2 - 3 байт кодовый номер цепочки пакетов
            4 - 5 байт количество пакетов

            Расположение байтов для неконтрольного пакета
            2 - 3 байт кодовый номер пакета
            4 - 5 байт номер пакета
            5 - 6 байт размер пакета, а именно данных без первых 6 байт (включая нулевой байт).
         */
    }
}