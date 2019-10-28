using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SirCachealot.Parallel
{
    public class ThreadManager
    {
        Controller controller;
        int totalAnalysed;
        int queueLength;
        int analysisThreadCount;
        int currentAnalysisTotal;
        object counterLock = new object();
        DateTime currentAnalysisStart = DateTime.Now;

        #region Parallel analysis methods

        public void InitialiseThreading(Controller c)
        {
            this.controller = c;
            ThreadPool.SetMaxThreads(16, 16);
        }

        // this function adds an item to the queue, and takes care of updating the counters.
        // All functions added to the queue should be wrapped in this wrapper.
        public void QueueItemWrapper(WaitCallback workFunction, object parametersIn)
        {
            lock (counterLock)
            {
                queueLength--;
                analysisThreadCount++;
            }
            try
            {
                workFunction(parametersIn);
            }
            catch (Exception e)
            {
                // if there's an exception thrown while adding a block then we're
                // pretty much stuck. The best we can do is log it and eat it to
                // stop it killing the rest of the program.
                controller.log("Exception thrown analysing " + parametersIn.ToString());
                controller.errorLog("Exception thrown analysing " + parametersIn.ToString());
                controller.errorLog(e.ToString());
                controller.errorLog("======================");
                controller.errorLog("");
                return;
            }
            finally
            {
                lock (counterLock) analysisThreadCount--;
            }
            lock (counterLock)
            {
                totalAnalysed++;
                currentAnalysisTotal++;
            }
        }

        public void AddToQueue(WaitCallback func, object parameters)
        {
            ThreadPool.QueueUserWorkItem(func, parameters);
            lock (counterLock) queueLength++;
        }

        // This method resets SirCachealot's analysis stats. Call it before an analysis run.
        public void ClearAnalysisRunStats()
        {
            currentAnalysisTotal = 0;
            currentAnalysisStart = DateTime.Now;
        }

        public DateTime GetCurrentAnalysisStart()
        {
            return currentAnalysisStart;
        }

        public string GetThreadStats()
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

        public int RemainingJobs
        {
            get
            {
                lock (counterLock)
                {
                    return queueLength + analysisThreadCount;
                }
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
                    return TimeSpan.FromTicks(ticksPerBlock * queueLength);
                }
            }
        }

        #endregion

    }
}
