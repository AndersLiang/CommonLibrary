using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent
{
    public class ProducerConsumerWithBlockingCollection
    {
        private BlockingCollection<int> mProducts = new BlockingCollection<int>();

        public ProducerConsumerWithBlockingCollection()
        {
        }

        public void Start()
        {
            var producer = Task.Factory.StartNew(() =>
            {
                int i = 0;
                while (true)
                {
                    Thread.Sleep(100);
                    mProducts.Add(i++);
                    if (i > 100)
                        mProducts.CompleteAdding();
                }
            });

            var consumer = Task.Factory.StartNew(() =>
            {
                foreach (var p in mProducts.GetConsumingEnumerable())
                {
                    Thread.Sleep(120);
                    Console.WriteLine(p);
                }
            });


            Task.WaitAll(consumer);
        }
    }
}
