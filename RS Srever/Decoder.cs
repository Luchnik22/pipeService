using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;

namespace RS_Srever
{
    class Decoder
    {
        private int TTL = 35;                               // Время жизни пакетов
        private List<Packet> turn = new List<Packet>();     // Очередь сообщений
        private List<Packet> controls = new List<Packet>(); // Контрольные пакеты
        
        public delegate void EventReady(BitmapImage img);
        public event EventReady FrameReady;
        
        private struct Packet
        {
            public bool isControl; // Это контрольный пакет?
            public UInt16 id;      // ID цепочки
            public byte[] date;    // Данные пакета
            public UInt16 count;   // Количество пакетов
            public UInt16 number;  // Порядковый номер пакета
            public int TTL;        // Длительность жызни пакета или контрола
        }
        
        public Decoder()
        {
        }

        public void addPacked(byte[] data)
        {
            Packet packet = decodeMsg(data);

            //for 

            if (packet.isControl)
            {
                controls.Add(packet);                
            }
            else
            {
                turn.Add(packet);
                
                for (int i = 0; i < controls.Count; i++)
                {
                    if (controls[i].id == packet.id && controls[i].count - 1== packet.number)
                    {
                        MemoryStream ms = new MemoryStream(Compare(packet.id));

                        controls.RemoveAt(i);
                        i--;

                        BitmapImage bitmapImg = new BitmapImage();
                        bitmapImg.BeginInit();
                        bitmapImg.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImg.StreamSource = ms;
                        bitmapImg.EndInit();
                        bitmapImg.Freeze();

                        FrameReady(bitmapImg);
                    }
                }
            }
        }

        /// <summary>
        /// Декодировка сообщения
        /// </summary>
        /// <param name="data">Данные</param>
        /// <returns>Сообщение</returns>
        private Packet decodeMsg(byte[] data)
        {
            Packet packet = new Packet();
            packet.TTL = 0;
            packet.isControl = data[0] == 1;
            packet.id = BitConverter.ToUInt16(data, 1);

            if (packet.isControl)
            {                
                packet.count = BitConverter.ToUInt16(data, 3);
            }
            else
            {
                packet.number = BitConverter.ToUInt16(data, 3);
                packet.date = new byte[BitConverter.ToUInt16(data, 5)];
                
                try
                {
                    Array.Copy(data, 7, packet.date, 0, packet.date.Length);
                }
                catch (Exception ex)
                {
                }
            }

            return packet;
        }

        /// <summary>
        /// Компановка всех сообщений и возврат исходных данных
        /// </summary>
        /// <param name="id">ID компонующих элементов</param>
        /// <returns>Исходные данные</returns>
        private byte[] Compare(UInt16 id)
        {
            List<byte> data = new List<byte>();
            
            for (int i = 0; i < turn.Count; i++)
            {                
                if (turn[i].id == id)
                {
                    data.AddRange(turn[i].date);
                    turn.RemoveAt(i);
                    i--;
                }
            }

            return data.ToArray();
        }

        private void RemoveChain(UInt16 id)
        {
            for (int i = 0; i < turn.Count; i++)
            {
                if (turn[i].id == id)
                {
                    turn.RemoveAt(i);
                }
            }
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
