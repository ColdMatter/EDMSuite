using System;
using System.Collections.Generic;
using System.Text;

using Analysis.EDM;

namespace SirCachealot.Database
{
    
    public interface TOFDBlockStore
    {
        UInt32[] GetUIDsByCluster(string clusterName, UInt32[] fromUIDs);
        UInt32[] GetUIDsByCluster(string clusterName);

        UInt32[] GetUIDsByBlock(string clusterName, int clusterIndex, UInt32[] fromUIDs);
        UInt32[] GetUIDsByBlock(string clusterName, int clusterIndex);

        UInt32[] GetUIDsByTag(string tag, UInt32[] fromUIDs);
        UInt32[] GetUIDsByTag(string tag);

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

        UInt32[] GetUIDsByPredicate(TOFPredicateFunction func, UInt32[] fromUIDs);

        TOFDemodulatedBlock GetDBlock(UInt32 uid);
       
        UInt32 AddDBlock(TOFDemodulatedBlock db);
  
        void RemoveDBlock(UInt32 uid);

        UInt32[] GetAllUIDs();
        
        void AddTagToBlock(string clusterName, int blockIndex, string tag);
        
        void RemoveTagFromBlock(string clusterName, int blockIndex, string tag);

        UInt32[] GetTaggedIndicesForCluster(string clusterName, string tag);
    }

    public delegate bool TOFPredicateFunction(TOFDemodulatedBlock dblock);

    public class TOFBlockNotFoundException : Exception { }

}