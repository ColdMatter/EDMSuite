using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Analysis.EDM;
using Data.EDM;
using EDMConfig;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;


namespace SonOfSirCachealot
{
    public class BlockStore
    {
        #region Adding blocks

        public ThreadMonitor Monitor = new ThreadMonitor();

        public void AddBlocks(string[] paths)
        {
            Monitor.ClearStats();
            Monitor.SetQueueLength(paths.Length);

            new Thread(new ThreadStart(() =>
                paths.AsParallel().ForAll((e) => AddBlock(e, "cgate11Fixed"))
            )).Start();

            // spawn a progress monitor thread
            Monitor.UpdateProgressUntilFinished();
        }

        public void AddBlocksJSON(string jsonPaths)
        {
            string[] paths = BsonSerializer.Deserialize<string[]>(jsonPaths);
            AddBlocks(paths);
        }

        private void AddBlock(string path, string normConfig)
        {
            Monitor.JobStarted();
            string fileName = path.Split('\\').Last();
            try
            {
                Controller.log("Adding block " + fileName);
                using (BlockDatabaseDataContext dc = new BlockDatabaseDataContext())
                {
                    BlockSerializer bls = new BlockSerializer();
                    Block b = bls.DeserializeBlockFromZippedXML(path, "block.xml");
                    Controller.log("Loaded " + fileName);
                    // at the moment the block data is normalized by dividing each "top" TOF through
                    // by the integral of the corresponding "norm" TOF over the gate in the function below.
                    // TODO: this could be improved!
                    //b.Normalise(DemodulationConfig.GetStandardDemodulationConfig(normConfig, b).GatedDetectorExtractSpecs["norm"]);
                    // add some of the single point data to the Shot TOFs so that it gets analysed
                    string[] spvsToTOFulise = new string[] { "NorthCurrent", "SouthCurrent", "MiniFlux1",
                        "MiniFlux2", "MiniFlux3", "ProbePD", "PumpPD"};
                    b.TOFuliseSinglePointData(spvsToTOFulise);

                    // extract the metadata and config into a DB object
                    DBBlock dbb = new DBBlock();
                    dbb.cluster = (string)b.Config.Settings["cluster"];
                    dbb.clusterIndex = (int)b.Config.Settings["clusterIndex"];
                    dbb.include = false;
                    dbb.eState = (bool)b.Config.Settings["eState"];
                    dbb.bState = (bool)b.Config.Settings["bState"];
                    try
                    {
                        dbb.rfState = (bool)b.Config.Settings["rfState"];
                    }
                    catch (Exception)
                    {
                        // blocks from the old days had no rfState recorded in them.
                        dbb.rfState = true;
                    }
                    dbb.ePlus = (double)b.Config.Settings["ePlus"];
                    dbb.eMinus = (double)b.Config.Settings["eMinus"];
                    dbb.blockTime = (DateTime)b.TimeStamp;

                    byte[] bts = serializeAsByteArray(b.Config);
                    dbb.configBytes = bts;

                    // extract the TOFChannelSets
                    List<string> detectorsToExtract = new List<string>
                        { "top", "norm", "magnetometer", "gnd", "battery","topNormed","NorthCurrent", "SouthCurrent",
                            "MiniFlux1", "MiniFlux2", "MiniFlux3", "ProbePD", "PumpPD" };
                    foreach (string detector in detectorsToExtract)
                    {
                        BlockTOFDemodulator demod = new BlockTOFDemodulator();
                        TOFChannelSet tcs = demod.TOFDemodulateBlock(b, b.detectors.IndexOf(detector), true);
                        byte[] tcsBytes = serializeAsByteArray(tcs);
                        DBTOFChannelSet t = new DBTOFChannelSet();
                        t.tcsData = tcsBytes;
                        t.detector = detector;
                        t.FileID = Guid.NewGuid();
                        dbb.DBTOFChannelSets.Add(t);
                    }
                    Controller.log("Demodulated " + fileName);

                    // add to the database
                    dc.DBBlocks.InsertOnSubmit(dbb);
                    dc.SubmitChanges();
                    Controller.log("Added " + fileName);
                }
            }
            catch (Exception e)
            {
                Controller.errorLog("Error adding " + fileName);
                Controller.errorLog("Error adding block " + path + "\n" + e.StackTrace);
            }
            finally
            {
                Monitor.JobFinished();
            }
        }

        private static byte[] serializeAsByteArray(object bc)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, bc);
            byte[] buffer = new Byte[ms.Length];
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(buffer, 0, (int)ms.Length);
            ms.Close();
            return buffer;
        }

        private static object deserializeFromByteArray(byte[] ba)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(ba);
            return bf.Deserialize(ms);
        }

        private static TOFChannelSet deserializeTCS(byte[] ba)
        {
            return (TOFChannelSet)deserializeFromByteArray(ba);
        }
        private static BlockConfig deserializeBC(byte[] ba)
        {
            return (BlockConfig)deserializeFromByteArray(ba);
        }

        #endregion


        #region Querying

        public string processJSONQuery(string jsonQuery)
        {
            BlockStoreQuery query = BsonSerializer.Deserialize<BlockStoreQuery>(jsonQuery);
            return processQuery(query).ToJson<BlockStoreResponse>();
        }

        public BlockStoreResponse processQuery(BlockStoreQuery query)
        {
            BlockStoreResponse bsr = new BlockStoreResponse();
            bsr.BlockResponses = new List<BlockStoreBlockResponse>();
            foreach(int blockID in query.BlockIDs) bsr.BlockResponses.Add(processBlockQuery(query.BlockQuery, blockID));
            return bsr;
        }


        private BlockStoreBlockResponse processBlockQuery(BlockStoreBlockQuery query, int blockID)
        {
            BlockStoreBlockResponse br = new BlockStoreBlockResponse();
            br.BlockID = blockID;
            br.DetectorResponses = new Dictionary<string, BlockStoreDetectorResponse>();
            br.Settings = getBlockConfig(blockID);

            foreach (BlockStoreDetectorQuery q in query.DetectorQueries)
                br.DetectorResponses.Add(q.Detector, processDetectorQuery(q, blockID));

            return br;
        }

        private BlockConfig getBlockConfig(int blockID)
        {
            BlockConfig bc = new BlockConfig();
            using (BlockDatabaseDataContext dc = new BlockDatabaseDataContext())
            {
                IEnumerable<DBBlock> bs = from DBBlock dbb in dc.DBBlocks
                                          where (dbb.blockID == blockID)
                                          select dbb;
                // there should only be one item - better to check or not?
                DBBlock dbbb = bs.First();
                bc = deserializeBC(dbbb.configBytes.ToArray());
            }
            return bc;
        }

        private BlockStoreDetectorResponse processDetectorQuery(BlockStoreDetectorQuery query, int blockID)
        {
            BlockStoreDetectorResponse dr = new BlockStoreDetectorResponse();
            dr.Channels = new Dictionary<string, TOFChannel>();
            using (BlockDatabaseDataContext dc = new BlockDatabaseDataContext())
            {
                IEnumerable<DBTOFChannelSet> tcss = from DBTOFChannelSet tcs in dc.DBTOFChannelSets
                                                    where (tcs.detector == query.Detector)
                                                    && (tcs.blockID == blockID)
                                                    select tcs;
                // there should only be one item - better to check or not?
                DBTOFChannelSet tc = tcss.First();
                TOFChannelSet t = deserializeTCS(tc.tcsData.ToArray());
                // TODO: Handle special channels
                foreach (string channel in query.Channels)
                    dr.Channels.Add(channel, (TOFChannel)t.GetChannel(channel));

            }
            return dr;
        }

        #endregion

        #region Querying (average)

        // querying for average TOFs uses different code - this is because of the need to keep
        // memory consumption under control, and the urge to re-use the old averaging code.
        // The result is that the averaging must be done at the TOFChannelSet level.

        public string processJSONQueryAverage(string jsonQuery)
        {
            BlockStoreQuery query = BsonSerializer.Deserialize<BlockStoreQuery>(jsonQuery);
            return processBlockQueryAverage(query.BlockQuery, query.BlockIDs).ToJson<BlockStoreBlockResponse>();
        }

        private BlockStoreBlockResponse processBlockQueryAverage(BlockStoreBlockQuery query, int[] blockIDs)
        {
            BlockStoreBlockResponse br = new BlockStoreBlockResponse();
            br.BlockID = -1;
            br.DetectorResponses = new Dictionary<string, BlockStoreDetectorResponse>();
            br.Settings = getBlockConfig(blockIDs[0]);

            foreach (BlockStoreDetectorQuery q in query.DetectorQueries)
                br.DetectorResponses.Add(q.Detector, processDetectorQueryAverage(q, blockIDs));

            return br;
        }

        private BlockStoreDetectorResponse processDetectorQueryAverage(BlockStoreDetectorQuery query, int[] blockIDs)
        {
            BlockStoreDetectorResponse dr = new BlockStoreDetectorResponse();
            dr.Channels = new Dictionary<string, TOFChannel>();
            using (BlockDatabaseDataContext dc = new BlockDatabaseDataContext())
            {
                IEnumerable<DBTOFChannelSet> tcss = from DBTOFChannelSet tcs in dc.DBTOFChannelSets
                                                    where (tcs.detector == query.Detector)
                                                    && blockIDs.Contains(tcs.blockID)
                                                    select tcs;
                // accumulate the average TCS
                TOFChannelSetAccumulator tcsa = new TOFChannelSetAccumulator();
                foreach (DBTOFChannelSet dbTcs in tcss)
                {
                    TOFChannelSet t = deserializeTCS(dbTcs.tcsData.ToArray());
                    tcsa.Add(t);
                }
                // TODO: Handle special channels
                TOFChannelSet averageTCS = tcsa.GetResult();
                foreach (string channel in query.Channels)
                    dr.Channels.Add(channel, (TOFChannel)averageTCS.GetChannel(channel));
            }
            return dr;
        }
        #endregion


        #region Including blocks

        public void SetIncluded(string cluster, int clusterIndex, bool included)
        {
            using (BlockDatabaseDataContext dc = new BlockDatabaseDataContext())
            {
                IEnumerable<DBBlock> b = from DBBlock dbb in dc.DBBlocks
                                         where (dbb.cluster == cluster)
                                         && (dbb.clusterIndex == clusterIndex)
                                         select dbb;
                foreach (DBBlock dbb in b) dbb.include = included;
                dc.SubmitChanges();              
            }
        }

        #endregion

    }

}