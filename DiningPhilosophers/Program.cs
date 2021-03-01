using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommandLine;

namespace DiningPhilosophers
{
    class Program
    {
        private static IEnumerable<Fork> Forks { get; set; }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    if (o.NumberPhilosophers <= 1)
                        throw new ArgumentException("At least 2 Philosophers necessary");


                    using var cts = new CancellationTokenSource();
                    Console.CancelKeyPress += (s, e) =>
                    {
                        Console.WriteLine("Canceling...");
                        cts.Cancel();
                        e.Cancel = true;
                    };

                    Forks = InitializeForks(o.NumberPhilosophers);

                    var threads = Enumerable.Range(0, o.NumberPhilosophers)
                        .Select(i => new Thread(() =>
                            Dine(i, o.NumberPhilosophers, o.MaxThinkingTime, o.MaxEatingTime, cts.Token)))
                        .ToList();
                    threads.ForEach(t => t.Start());
                    threads.ForEach(t => t.Join());
                });
        }

        private static void Dine(int index, int maxCount, int maxThinkingTime, int maxEatingTime,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var thinkingTime = RandomInRange(0, maxThinkingTime);
                Thread.Sleep(thinkingTime);
                Console.WriteLine($"Phil{index.ToString()} finished thinking");
                lock (Forks.ElementAt(index))
                {
                    Console.WriteLine($"Phil{index.ToString()} took first fork: {index.ToString()}");
                    var indexSecondFork = (index + 1) % maxCount;
                    lock (Forks.ElementAt(indexSecondFork))
                    {
                        Console.WriteLine($"Phil{index.ToString()} took second fork: {indexSecondFork.ToString()}");
                        Thread.Sleep(RandomInRange(0, maxEatingTime));
                        Console.WriteLine($"Phil{index.ToString()} is done eating");
                    }
                }
            }
        }

        private static int RandomInRange(int min, int max)
        {
            var r = new Random();
            return r.Next(min, max);
        }

        private static IEnumerable<Fork> InitializeForks(int count)
        {
            return Enumerable.Range(0, count)
                .Select(i => new Fork(i));
        }
        
        public class Options
        {
            [Option('p', "philosophers", Required = true, HelpText = "Enter number of philosophers at the table.")]
            public int NumberPhilosophers { get; set; }

            [Option('t', "maxThinkingTime", Required = true, HelpText = "Enter the maximum thinking time.")]
            public int MaxThinkingTime { get; set; }

            [Option('e', "maxEatingTime", Required = true, HelpText = "Enter the maximum eating time.")]
            public int MaxEatingTime { get; set; }
        }


        public class Fork
        {
            public Guid Id { get; set; }

            public int Position { get; set; }

            public Fork(int position)
            {
                Position = position;
                Id = Guid.NewGuid();
            }
        }
    }
}