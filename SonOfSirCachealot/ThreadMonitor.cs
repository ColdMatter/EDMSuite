using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SonOfSirCachealot
{
    public class ThreadMonitor
    {
        int analysisThreadCount = 0;
        int queueLength = 0;
        int totalAnalysed = 0;
        int currentAnalysisTotal = 0;
        DateTime currentAnalysisStart = DateTime.Now;
        Object threadStatsLock = new Object();

        public void SetQueueLength(int l)
        {
            queueLength = l;
        }

        public string GetStats()
        {
            StringBuilder b = new StringBuilder();
            b.Append("Analysis threads: " + analysisThreadCount);
            b.Append("; Queued: " + queueLength);
            b.Append("; Analysed this run: " + currentAnalysisTotal);
            b.Append("; Run time: " + (DateTime.Now.Subtract(currentAnalysisStart)));
            b.Append("; Estimated time to go: " + EstimateFinishTime());
            b.Append("; Total analysed: " + totalAnalysed +".");
            return b.ToString();
        }

        public void ClearStats()
        {
            currentAnalysisTotal = 0;
            currentAnalysisStart = DateTime.Now;
        }

        public void JobStarted()
        {
            lock (threadStatsLock)
            {
                queueLength--;
                analysisThreadCount++;
            }
        }

        public void JobFinished()
        {
            lock (threadStatsLock)
            {
                analysisThreadCount--;
                totalAnalysed++;
                currentAnalysisTotal++;
            }
        }

        private TimeSpan EstimateFinishTime()
        {
            if (queueLength == 0) return TimeSpan.FromSeconds(0);
            else
            {
                if (currentAnalysisTotal == 0) return TimeSpan.FromSeconds(0);
                else
                {
                    long ticksGone = DateTime.Now.Ticks - currentAnalysisStart.Ticks;
                    long ticksPerBlock = ticksGone / currentAnalysisTotal;
                    return TimeSpan.FromTicks(ticksPerBlock * (queueLength + analysisThreadCount));
                }
            }
        }

        public bool IsFinished()
        {
            lock (threadStatsLock)
            {
                return (queueLength + analysisThreadCount) == 0;
            }
        }

        internal void UpdateProgressUntilFinished()
        {
            Thread progressThread = new Thread(new ThreadStart(() =>
                {
                    while (!IsFinished())
                    {
                        Console.WriteLine(GetStats());
                        Thread.Sleep(5000);
                    }
                }
            ));
            progressThread.Start();
            progressThread.Join();
        }
    }
}
