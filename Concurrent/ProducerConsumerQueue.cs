using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent
{
    // Reference: http://www.albahari.com/threading/part4.aspx
    public class ProducerConsumerQueue
    {
        readonly object _locker = new object();
        Thread[] _workers;
        Queue<Action> _itemQ = new Queue<Action>();

        public ProducerConsumerQueue(int workerCount)
        {
            _workers = new Thread[workerCount];

            // Create and start a separate thread for each worker
            for (int i = 0; i < workerCount; i++)
                (_workers[i] = new Thread(Consume)).Start();
        }

        public void Shutdown(bool waitForWorkers)
        {
            // Enqueue one null item per worker to make each exit.
            foreach (Thread worker in _workers)
                EnqueueItem(null);

            // Wait for workers to finish
            if (waitForWorkers)
                foreach (Thread worker in _workers)
                    worker.Join();
        }

        public void EnqueueItem(Action item)
        {
            lock (_locker)
            {
                _itemQ.Enqueue(item);           // We must pulse because we're
                Monitor.Pulse(_locker);         // changing a blocking condition.
            }
        }

        void Consume()
        {
            while (true)                        // Keep consuming until
            {                                   // told otherwise.
                Action item;
                lock (_locker)
                {
                    while (_itemQ.Count == 0)
                        Monitor.Wait(_locker);

                    item = _itemQ.Dequeue();
                }

                if (item == null)
                    return;         // This signals our exit.
                item();                           // Execute item.
            }
        }
    }


    public static class Test
    {
        public static void Run()
        {
            ProducerConsumerQueue q = new ProducerConsumerQueue(2);

            Console.WriteLine("Enqueuing 10 items...");

            for (int i = 0; i < 10; i++)
            {
                int itemNumber = i;      // To avoid the captured variable trap
                q.EnqueueItem(() =>
               {
                   Thread.Sleep(1000);          // Simulate time-consuming work
                   Console.Write(" Task" + itemNumber);
               });
            }

            q.Shutdown(true);
            Console.WriteLine();
            Console.WriteLine("Workers complete!");
        }
    }
}
