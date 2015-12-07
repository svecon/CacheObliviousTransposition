using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;

namespace MatrixTransposition
{
    class Program
    {
        private const int MatrixSize = 100000000;
        private readonly int iterations;

        static readonly int[] Ns =
        {
            64, 69, 75, 80, 87, 92, 101, 109, 119, 128, 139, 150, 161, 174, 189, 203, 220, 237, 256, 280,300, 322, 352, 378, 406, 442, 476, 512, 552, 598, 645, 705, 760, 812, 878, 960, 1024
            ,1040,1092,1146,1204,1264,1327,1393,1463,1536,1613,1694,1778,1867,1961,2059,2162,2270,2383,2503,2628,2759,2897,3042,3194,3354,3522,3698,3883,4077,4281,4495,4720,4956,5203,5464,5737,6024,6325,6641,6973,7322,7688,8073,8476,8900,9345,9812
        };

        readonly Random rnd = new Random();

        private List<int[,]> ms;
        private List<int[,]> mos;

        private readonly int N;
        private readonly bool useNaive;

        public Program(int n, bool useNaive)
        {
            N = n;
            this.useNaive = useNaive;
            iterations = MatrixSize / n / n;
        }

        void InitMatrices()
        {
            ms = new List<int[,]>();
            mos = new List<int[,]>();

            for (var i = 0; i < iterations; i++)
            {
                var m = new int[N, N];
                FillRandomMatrix(m);
#if DEBUG
                //PrintMatrix(m);
#endif
                ms.Add(m);
                mos.Add(new int[N, N]);
            }
        }

        void FillRandomMatrix(int[,] a)
        {
            for (var i = 0; i < a.GetLength(0); i++)
            {
                for (var j = 0; j < a.GetLength(1); j++)
                {
                    a[i, j] = rnd.Next(100);
                }
            }
        }

        void PrintMatrix(int[,] a)
        {
            for (var i = 0; i < a.GetLength(0); i++)
            {
                for (var j = 0; j < a.GetLength(1); j++)
                {
                    Console.Write("{0,3}, ", a[i, j]);
                }
                Console.WriteLine();
            }
        }

        void NaiveTranspose(int[,] source, int[,] dest)
        {
            for (var i = 0; i < N; i++)
            {
                for (var j = 0; j < N; j++)
                {
                    dest[j, i] = source[i, j];
                }
            }
        }

        void SmartTranspose(int[,] source, int[,] dest)
        {
            SmartTransposeRecursion(source, dest, 0, 0, N, N);
        }

        private void SmartTransposeRecursion(int[,] source, int[,] dest, int x, int y, int w, int h)
        {
            if (w * h <= 64)
            {
                // transpose
                for (var i = x; i < x + w; i++)
                {
                    for (var j = y; j < y + h; j++)
                    {
                        dest[j, i] = source[i, j];
                    }
                }
            }
            else if (w <= h)
            {
                var half = h / 2;
                SmartTransposeRecursion(source, dest, x, y, w, half);
                SmartTransposeRecursion(source, dest, x, y + half, w, h - half);
            }
            else
            {
                var half = w / 2;
                SmartTransposeRecursion(source, dest, x, y, half, h);
                SmartTransposeRecursion(source, dest, x+half, y, w - half, h);
            }
        }

        void CheckTransposition(int[,] source, int[,] dest)
        {
            for (var i = 0; i < source.GetLength(0); i++)
            {
                for (var j = 0; j < source.GetLength(1); j++)
                {
                    if (source[i, j] != dest[j, i])
                    {
                        throw new Exception(string.Format("Transposition failed at i={0}, j={1}", i, j));
                    }
                }
            }
        }

        void TransposeAll()
        {
            GC.Collect();
            
            var watch = new Stopwatch();
            watch.Start();

            for (var i = 0; i < iterations; i++)
            {
                if (useNaive) NaiveTranspose(ms[i], mos[i]);
                else SmartTranspose(ms[i], mos[i]);
#if DEBUG
                CheckTransposition(_ms[i], _mos[i]);
#endif
            }
            watch.Stop();

            Console.WriteLine("N={0}, Iterations={1}, Time={2}", N, iterations, watch.Elapsed.TotalMilliseconds * 1000000 / N / N / iterations);
        }

        static void Main(string[] args)
        {
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;

            var useNaive = args.Length >= 1 && args[0] == "--naive";

            Console.WriteLine(useNaive ? "Using naive transposition" : "Using Cache-oblivious transposition");

            foreach (var n in Ns)
            {
                var p = new Program(n, useNaive);
                p.InitMatrices();
                p.TransposeAll();
            }
        }
    }
}
