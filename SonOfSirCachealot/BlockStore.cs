using System;
using System.Collections.Generic;
using System.Text;

using Analysis.EDM;
using Data.EDM;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using System.Threading;

namespace SonOfSirCachealot.Database
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

        private void AddBlock(string path, string normConfig)
        {
            Monitor.JobStarted();
            string fileName = path.Split('\\').Last();
            try
            {
                Console.WriteLine("Adding block " + fileName);
                using (BlockDatabaseDataContext dc = new BlockDatabaseDataContext())
                {
                    BlockSerializer bls = new BlockSerializer();
                    Block b = bls.DeserializeBlockFromZippedXML(path, "block.xml");
                    //Controller.log("Loaded " + fileName);
                    // at the moment the block data is normalized by dividing each "top" TOF through
                    // by the integral of the corresponding "norm" TOF over the gate in the function below.
                    // TODO: this could be improved!
                    b.Normalise(DemodulationConfig.GetStandardDemodulationConfig(normConfig, b).GatedDetectorExtractSpecs["norm"]);
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
                    Console.WriteLine("Demodulated " + fileName);

                    // add to the database
                    dc.DBBlocks.InsertOnSubmit(dbb);
                    dc.SubmitChanges();
                    Console.WriteLine("Added " + fileName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error adding " + fileName);
                Console.WriteLine("Error adding block " + path + "\n" + e.StackTrace);
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

        #endregion


        #region Querying

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