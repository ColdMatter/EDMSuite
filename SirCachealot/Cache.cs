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

        public void Start()
        {
            memcachedProcess = Process.Start("memcached.exe", "-m 800");
            MemcachedClient.Setup("EDMAnalysisMemcached",
                new string[] { "127.0.0.1:11211" });
            cache = MemcachedClient.GetInstance("EDMAnalysisMemcached");
        }

        public void Stop()
        {
            memcachedProcess.Kill();
        }

        public byte[] Get(string key)
        {
            return (byte[])cache.Get(key);
        }

        public void Set(string key, byte[] dblock)
        {
            cache.Set(key, dblock);
        }

    }
}
