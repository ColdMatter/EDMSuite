using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using BeIT.MemCached;

using Analysis.EDM;

namespace SirCachealot
{

    class Cache
    {
        private Process memcachedProcess;
        private MemcachedClient cache;
        //private Dictionary<string, Dictionary<string, string>> memcachedStats;

        internal void Start()
        {
            memcachedProcess = Process.Start("memcached.exe", "-m 200");
            MemcachedClient.Setup("EDMAnalysisMemcached",
                new string[] { "127.0.0.1:11211" });
            cache = MemcachedClient.GetInstance("EDMAnalysisMemcached");
        }

        internal void Stop()
        {
            memcachedProcess.Kill();
        }

        internal DemodulatedBlock Get(string key)
        {
            return new DemodulatedBlock();
        }

        //private void Put(string key, DemodulatedBlock dblock)
        //{
        //}

    }
}
