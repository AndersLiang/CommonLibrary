using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace Concurrent
{
    public class SimpleProducerConsumer
    {
        private ConcurrentQueue<int> mProducts = new ConcurrentQueue<int>();
        private int mThreshold = 10;
        private AutoResetEvent mProducerAutoResetEvent = new AutoResetEvent(false);
        private AutoResetEvent mConsumerAutoResetEvent = new AutoResetEvent(true);

        public SimpleProducerConsumer()
        {
        }

        public void Start()
        {
            var producer = Task.Factory.StartNew(() =>
            {
                foreach (var p in Products)
                {
                    Thread.Sleep(100);
                    mProducts.Enqueue(p);
                    mProducerAutoResetEvent.Set();

                    if (mProducts.Count > mThreshold)
                        mConsumerAutoResetEvent.WaitOne();
                }
            });

            var consumer1 = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    int p = 0;
                    if (mProducts.TryDequeue(out p))
                    {
                        Thread.Sleep(200);
                        Console.WriteLine(string.Format("Consumer 1:{0}",p));
                        mConsumerAutoResetEvent.Set();
                    }
                    else
                        mProducerAutoResetEvent.WaitOne();
                }
            });

            var consumer2 = Task.Factory.StartNew(() =>
           {
               while (true)
               {
                   int p = 0;
                   if (mProducts.TryDequeue(out p))
                   {
                       Thread.Sleep(300);
                       Console.WriteLine(string.Format("Consumer 2:{0}", p));
                       mConsumerAutoResetEvent.Set();
                   }
                   else
                       mProducerAutoResetEvent.WaitOne();
               }
           });

            Task.WaitAll(consumer1, consumer2);
        }

        private IEnumerable<int> Products
        {
            get
            {
                int i = 0;
                while (true)
                    yield return i++;
            }
        }
    }


}
