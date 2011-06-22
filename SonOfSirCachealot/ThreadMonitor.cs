using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            b.AppendLine("Analysis threads: " + analysisThreadCount);
            b.AppendLine("Queued: " + queueLength);
            b.AppendLine("Analysed this run: " + currentAnalysisTotal);
            b.AppendLine("Run time: " + (DateTime.Now.Subtract(currentAnalysisStart)));
            b.AppendLine("Estimated time to go: " + EstimateFinishTime());
            b.AppendLine("Total analysed: " + totalAnalysed);
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
    }
}
