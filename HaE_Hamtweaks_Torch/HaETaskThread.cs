using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HaE_Hamtweaks_Torch
{
    public class HaETaskThread
    {
        private Thread thread;

        private ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();

        public HaETaskThread()
        {
            thread = new Thread(new ThreadStart(Run));
            thread.Start();
        }

        private void Run()
        {
            Action action;
            if (queue.TryDequeue(out action))
                action();
        }
    }
}
