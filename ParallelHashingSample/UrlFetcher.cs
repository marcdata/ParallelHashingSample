using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace ParallelHashingSample
{
    public class UrlFetcher
    {
        private int maxParallel;
        private FileInfo fileInfo;

        public UrlFetcher(FileInfo fileIn, int maxParallel)
        {
            this.fileInfo = fileIn;
            this.maxParallel = maxParallel;
        }

        public async Task Go()
        {
            // batching by m=100 lines in the file
            // within each batch, gate by n concurrent url fetches at a time.

            var batchsize = 100;
            var sequenceNum = 1;
            var semaphore = new SemaphoreSlim(maxParallel, this.maxParallel);

            var outputCollector = new List<(int sequenceNum, string url, string md5hash)>();
            var currentTasks = new List<Task<(int sequenceNum, string url, string md5hash)>>();

            var filestream = new System.IO.FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
            using (var streamreader = new StreamReader(filestream, true))
            {
                string line;
                while((line = streamreader.ReadLine()) != null)
                {
                    if(line == "")
                    {
                        continue;
                    }

                    var task = GetMd5TupleForUrl(sequenceNum, line, semaphore);
                    currentTasks.Add(task);

                    sequenceNum += 1;

                    // batch by 100
                    if(sequenceNum % batchsize == 0)
                    {
                        await ClearCurrentBatch(currentTasks, outputCollector) ;
                    }

                }
                if (currentTasks.Any())
                {
                    await ClearCurrentBatch(currentTasks, outputCollector);
                }
            }

            foreach(var x in outputCollector)
            {
                Console.WriteLine(x.md5hash);
            }
        }

        private async Task ClearCurrentBatch(List<Task<(int sequenceNum, string url, string md5hash)>> tasks, List<(int sequenceNum, string url, string md5hash)> outputCollector)
        {
            await Task.WhenAll(tasks.ToArray());
            foreach (var tuple in tasks.OrderBy(x => x.Result.sequenceNum).Select(x => x.Result))
            {
                outputCollector.Add(tuple);
            }

            tasks.Clear();
        }

        // Convenience method
        public async static Task<string> GetMd5HashForUrl(string url)
        {
            var semaphore = new SemaphoreSlim(1, 1);
            return (await GetMd5TupleForUrl(1, url, semaphore)).md5hash;
        }

        private async static Task<(int sequenceNum, string url, string md5hash)> GetMd5TupleForUrl(int sequenceNum, string url, SemaphoreSlim semaphore)
        {
            try
            {
                await semaphore.WaitAsync();

                var httpClient = new System.Net.Http.HttpClient();

                var response = await httpClient.GetAsync(url);

                var md5 = MD5.Create();

                var hash = md5.ComputeHash(response.Content.ReadAsStreamAsync().Result);
                var hashAsString = BitConverter.ToString(hash).Replace("-", "");
                return (sequenceNum, url, hashAsString);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}