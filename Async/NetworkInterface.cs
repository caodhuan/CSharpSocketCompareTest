using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsyncNetworkLib
{
    public class NetworkInterface
    {
        const Int32 CacheSize = 65536;
        NetworkStream stream;
        byte[] buff = new byte[CacheSize];
        Int32 buffIndex = 0;

        public NetworkInterface(NetworkStream stream)
        {
            this.stream = stream;
        }

        public NetworkStream GetStream()
        {
            return stream;
        }

        public async Task<Int32> StartRead()
        {
            if (buff.Length == buffIndex)
            {
                byte[] newBuff = new byte[buff.Length * 2];
                Buffer.BlockCopy(buff, 0, newBuff, 0, buff.Length);
                buff = newBuff;
            }
            Int32 readSize = await stream.ReadAsync(buff, buffIndex, buff.Length - buffIndex);
            buffIndex += readSize;
            return readSize;
        }

        public int GetReadedBuffSize()
        {
            return buffIndex;
        }
        public byte[] GetReadedBuff()
        {
            return buff;
        }

        public void AdjustReadBuff(Int32 startPos)
        {
            if (startPos == 0)
            {
                return;
            }
            if (buffIndex == startPos)
            {
                buffIndex = 0;
                return;
            }

            System.Buffer.BlockCopy(buff, startPos, buff, 0, buffIndex - startPos);
            buffIndex -= startPos;
        }

        public async Task Send(byte[] buffer, Int32 offset, Int32 size)
        {
            await stream.WriteAsync(buffer, offset, size);
        }
    }
}
