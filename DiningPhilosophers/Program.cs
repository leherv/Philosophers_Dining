using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                .WithParsed(o =>
                {
                    if (o.NumberPhilosophers <= 1)
                        throw new ArgumentException("At least 2 Philosophers necessary");


                    using var cts = new CancellationTokenSource();
                    Console.CancelKeyPress += (s, e) =>
                    {
                        Console.WriteLine("\nCancelling...\n");
                        cts.Cancel();
                        e.Cancel = true;
                    };

                    Forks = InitializeForks(o.NumberPhilosophers);

                    var waitingTimes = new List<double>();
                    var threads = Enumerable.Range(0, o.NumberPhilosophers)
                        .Select(i => i % 2 == 0
                            ? (Index: i, firstFork: Forks.ElementAt((i + 1) % o.NumberPhilosophers),
                                secondFork: Forks.ElementAt(i))
                            : (Index: i, firstFork: Forks.ElementAt(i),
                                secondFork: Forks.ElementAt((i + 1) % o.NumberPhilosophers)))
                        .Select(tuple => new Thread(() =>
                        {
                            waitingTimes.Add(Dine(tuple.Index, tuple.firstFork, tuple.secondFork,
                                o.MaxThinkingTime, o.MaxEatingTime, cts.Token));
                        }))
                        .ToList();
                    
                    var stopwatch = Stopwatch.StartNew();
                    threads.ForEach(t => t.Start());
                    threads.ForEach(t => t.Join());
                    stopwatch.Stop();

                    var totalRuntime = stopwatch.Elapsed.TotalMilliseconds;
                    var totalWaitingTime = waitingTimes.Sum();
                    var waitingPercentage = totalWaitingTime / totalRuntime * 100;
                    Console.WriteLine($"\nTotal runtime: {totalRuntime:0.00}ms");
                    Console.WriteLine($"Total waiting time: {totalWaitingTime:0.00}ms");
                    Console.WriteLine($"{waitingPercentage:0.00000}% of the total runtime was spent waiting for forks.");
                });
        }

        private static double Dine(int index, Fork firstFork, Fork secondFork, int maxThinkingTime, int maxEatingTime,
            CancellationToken cancellationToken)
        {
            var watch = new Stopwatch();
            while (!cancellationToken.IsCancellationRequested)
            {
                var thinkingTime = RandomInRange(0, maxThinkingTime);
                Thread.Sleep(thinkingTime);
                Console.WriteLine($"Phil{index.ToString()} finished thinking");
                watch.Start();
                lock (firstFork)
                {
                    watch.Stop();
                    Console.WriteLine($"Phil{index.ToString()} took first fork: {firstFork.Position.ToString()}");
                    watch.Start();
                    lock (secondFork)
                    {
                        watch.Stop();
                        Console.WriteLine($"Phil{index.ToString()} took second fork: {secondFork.Position.ToString()}");
                        Thread.Sleep(RandomInRange(0, maxEatingTime));
                        Console.WriteLine($"Phil{index.ToString()} is done eating");
                    }
                }
            }

            return watch.Elapsed.TotalMilliseconds;
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

        private class Options
        {
            [Option('p', "philosophers", Required = true, HelpText = "Enter number of philosophers at the table.")]
            public int NumberPhilosophers { get; set; }

            [Option('t', "maxThinkingTime", Required = true, HelpText = "Enter the maximum thinking time.")]
            public int MaxThinkingTime { get; set; }

            [Option('e', "maxEatingTime", Required = true, HelpText = "Enter the maximum eating time.")]
            public int MaxEatingTime { get; set; }
        }


        private class Fork
        {
            private Guid Id { get; set; }

            public int Position { get; set; }

            public Fork(int position)
            {
                Position = position;
                Id = Guid.NewGuid();
            }
        }
    }
}