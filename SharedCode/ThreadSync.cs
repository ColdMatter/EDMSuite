using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Data
{
    public class ThreadSync
    {
        /*
            This is a class managing delegate threads, each doing parallel tasks. It holds a pair of mutexes for each class to guarantee synchronisation at two points.

            Control mode is for managing the flow of the program. When ThreadSync is in control mode, the delegates are running their tasks.
            Switching to control mode guarantees all delegates wait for main at that point, i.e. synchronisation to all delegates at that point.

            Data mode is for accessing the result of the computation. Switching to data mode guarantees that the main thread waits on each delegate.
            The delegates might not synchronise.

            This is a helper and does NOT provide thread-safe access to data for helper threads. Ensure all used variables are properly interlocked/accept only atomic
            operations.
         */

        private class MutexPair
        {
            public Mutex dataMutex;
            public Mutex controlMutex;

            public MutexPair()
            {
                dataMutex = new Mutex();
                controlMutex = new Mutex();
            }
        }

        private bool finishThread;
        private List<MutexPair> Mutexes;
        private bool inControl;

        List<Thread> threads;


        public ThreadSync()
        {
            finishThread = false;
            Mutexes = new List<MutexPair>();
            threads = new List<Thread>();
            inControl = true;
        }

        // Guaranteed for the delegate to wait for the main. Also synchronises all delegates.
        public void SwitchToControl()
        {
            if (inControl) throw new InvalidOperationException("Already in control mode");
            foreach (MutexPair mutexPair in Mutexes)
            {
                mutexPair.controlMutex.WaitOne();
            }
            foreach (MutexPair mutexPair in Mutexes)
            {
                mutexPair.dataMutex.ReleaseMutex();
            }
            inControl = true;
        }

        // Guaranteed for main thread to wait on the delegate
        public void SwitchToData()
        {
            if (!inControl) throw new InvalidOperationException("Already in data mode");
            foreach (MutexPair mutexPair in Mutexes)
            {
                mutexPair.controlMutex.ReleaseMutex();
                mutexPair.dataMutex.WaitOne();
            }
            inControl = false;
        }

        public void CreateDelegateThread(Action delegateTask)
        {
            MutexPair newMutex = new MutexPair();
            if (!inControl) newMutex.dataMutex.WaitOne();
            else newMutex.controlMutex.WaitOne();
            Mutexes.Add(newMutex);
            Thread newThread = new Thread(new ThreadStart(() => {
                while (true)
                {
                    newMutex.dataMutex.WaitOne();
                    delegateTask();
                    newMutex.controlMutex.WaitOne();
                    newMutex.dataMutex.ReleaseMutex();
                    if (finishThread) break;
                    newMutex.controlMutex.ReleaseMutex();
                }
                newMutex.controlMutex.ReleaseMutex();
            }));
            threads.Add(newThread);
            newThread.Start();
        }

        public void JoinThreads()
        {
            if (!inControl) SwitchToControl();
            finishThread = true;
            SwitchToData();
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }

        public void AbortThreads()
        {

            foreach (Thread thread in threads)
            {
                thread.Abort();
            }
        }

    }
}
