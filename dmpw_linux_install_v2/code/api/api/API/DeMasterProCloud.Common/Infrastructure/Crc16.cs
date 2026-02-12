using System;

namespace DeMasterProCloud.Common.Infrastructure
{
    public class Crc16
    {
        /// <summary>
        /// Get CRC from byte array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ushort GetCrca(byte[] data)
        {
            var wCrc = new byte[2];
            var pSendData = new ushort[40960];
            Array.Copy(data, pSendData, data.Length);
            CalculateCrc(wCrc, pSendData, data.Length);
            return (ushort)((wCrc[0] << 8) | (wCrc[1] & 0xFF));
        }

        /// <summary>
        /// Calculate CRC
        /// </summary>
        /// <param name="wCrc"></param>
        /// <param name="data"></param>
        /// <param name="len"></param>
        public void CalculateCrc(byte[] wCrc, ushort[] data, int len)
        {
            var i = 0;
            wCrc[0] = 0x63;
            wCrc[1] = 0x63;
            do
            {
                var chBlock = (byte)data[i++];
                UpdateCrc(chBlock, wCrc);
            } while (--len > 0);

            var temp = wCrc[0];
            wCrc[0] = wCrc[1];
            wCrc[1] = temp;
        }

        /// <summary>
        /// Update CRC
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="lpwCrcC"></param>
        public void UpdateCrc(byte ch, byte[] lpwCrcC)
        {
            uint lpwCrc = lpwCrcC[0];
            lpwCrc <<= 8;
            lpwCrc += lpwCrcC[1];
            ch = (byte)(ch ^ lpwCrcC[1]);
            ch = (byte)(ch ^ (ch << 4));
            uint i = ch;
            lpwCrc = (lpwCrc >> 8) ^ (i << 8) ^ (i << 3) ^ (i >> 4);
            lpwCrcC[1] = (byte)(lpwCrc & 0x00FF);
            lpwCrcC[0] = (byte)((lpwCrc >> 8) & 0x00FF);
        }
    }
}