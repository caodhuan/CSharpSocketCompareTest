using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLib
{
    public class Reactor
    {
        private Dictionary<Socket, NetworkInterface> sockets = new Dictionary<Socket, NetworkInterface>();
        List<Socket> checkRead = new List<Socket>();
        bool run = true;
        public Reactor()
        {

        }

        public void Add(NetworkInterface ni)
        {
            sockets.Add(ni.GetSocket(), ni);
        }

        public void Remove(NetworkInterface ni)
        {
            sockets.Remove(ni.GetSocket());
        }

        public void Stop ()
        {
            run = false;
        }
        public void Run()
        {
            while (run)
            {
                checkRead.Clear();
                foreach (var item in sockets)
                {
                    checkRead.Add(item.Key);
                    
                }

                if (checkRead.Count == 0)
                {
                    break;
                }
                
                Socket.Select(checkRead, null, null, -1);

                foreach (var item in checkRead)
                {

                    NetworkInterface ni = sockets[item];
                    Action callback = ni.GetReadCallback();
                    if (callback != null)
                    {
                        callback();
                    }
                }
            }
        }
    }
}
