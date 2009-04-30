using System;
using System.Collections.Generic;
using System.Text;

using Analysis.EDM;

namespace SirCachealot
{
    
    public interface DBlockStore
    {
        UInt32[] GetUIDsByCluster(string clusterName, UInt32[] fromUIDs);
        UInt32[] GetUIDsByCluster(string clusterName);

        UInt32[] GetUIDsByBlock(string clusterName, int clusterIndex, UInt32[] fromUIDs);
        UInt32[] GetUIDsByBlock(string clusterName, int clusterIndex);

        UInt32[] GetUIDsByTag(string tag, UInt32[] fromUIDs);
        UInt32[] GetUIDsByTag(string tag);

        UInt32[] GetUIDsByAnalysisTag(string tag, UInt32[] fromUIDs);
        UInt32[] GetUIDsByAnalysisTag(string tag);

        UInt32[] GetUIDsByMachineState(bool eState, bool bState, bool rfState, UInt32[] fromUIDs);
        UInt32[] GetUIDsByMachineState(bool eState, bool bState, bool rfState);

        UInt32[] GetUIDsByEState(bool eState, UInt32[] fromUIDs);
        UInt32[] GetUIDsByEState(bool eState);

        UInt32[] GetUIDsByBState(bool bState, UInt32[] fromUIDs);
        UInt32[] GetUIDsByBState(bool bState);

        UInt32[] GetUIDsByRFState(bool rfState, UInt32[] fromUIDs);
        UInt32[] GetUIDsByRFState(bool rfState);
        
        UInt32[] GetUIDsByDateRange(DateTime start, DateTime end, UInt32[] fromUIDs);
        UInt32[] GetUIDsByDateRange(DateTime start, DateTime end);

        UInt32[] GetUIDsByVoltageRange(double low, double high, UInt32[] fromUIDs);
        UInt32[] GetUIDsByVoltageRange(double low, double high);

        UInt32[] GetUIDsByPredicate(PredicateFunction func, UInt32[] fromUIDs);

        DemodulatedBlock GetDBlock(UInt32 uid);
       
        UInt32 AddDBlock(DemodulatedBlock db);
  
        void RemoveDBlock(UInt32 uid);

        UInt32[] GetAllUIDs();
        
        void AddTagToBlock(string clusterName, int blockIndex, string tag);
        
        void RemoveTagFromBlock(string clusterName, int blockIndex, string tag);
    }

    public delegate bool PredicateFunction(DemodulatedBlock dblock);

    public class BlockNotFoundException : Exception { }

}