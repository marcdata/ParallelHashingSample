using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace ParallelHashingSample
{
    class Program
    {
        static void Main(string[] args)
        {
            (var fileIn, var maxParallel) = HandleArgs(args);

            if(fileIn == "" || args.Length == 0)
            {
                Console.WriteLine($"Usage: ParallelHashingSample.exe [filename] [max parallelization]");
                return;
            }

            FileInfo fileInfo = new FileInfo(fileIn);

            var urlFetcher = new UrlFetcher(fileInfo, maxParallel);
            urlFetcher.Go().Wait();
            
        }

        private static (string fileIn, int maxParallel) HandleArgs(string[] args)
        {
            // arg 0 is fileIn, default to empty, if empty, default to a sample list of 1 url
            // arg 1 is the maxn of urls to fetch at one time; max parallelism arg.

            // Plan, replace this with better CLI argument tooling; have seen a few packages, but either wrong framework, or not what was looking for.

            var fileIn = "";
            var maxParallel = 1;

            if (args.Length == 0)
            {
                return (fileIn, maxParallel);
            }
            if (args.Length == 1)
            {
                return (args[0], maxParallel);
            }
            if (args.Length == 2)
            {
                return (args[0], int.Parse(args[1]));
            }
            throw new ArgumentException($"Command line arguments not recognized, more than 2.");
        }



    }
}
