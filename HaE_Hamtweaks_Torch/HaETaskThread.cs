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
        private bool keepRunning = true;

        private ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();

        public HaETaskThread()
        {
            thread = new Thread(new ThreadStart(Run));
            thread.Start();
        }

        public void Enqueue(Action action)
        {
            queue.Enqueue(action);
        }

        public void End()
        {
            keepRunning = false;
        }

        private void Run()
        {
            while (keepRunning)
            {
                if (queue.IsEmpty)
                    Thread.Yield();

                Action action;
                if (queue.TryDequeue(out action))
                    action();
            }
        }
    }
}
