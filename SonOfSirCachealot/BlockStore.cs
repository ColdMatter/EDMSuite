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

        public void AddBlocks(string[] paths)
        {
            new Thread(new ThreadStart(() => 
                paths.AsParallel().ForAll((e) => AddBlock(e))
            )).Start();
        }

        public void AddBlock(string path)
        {
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
                    b.Normalise(DemodulationConfig.GetStandardDemodulationConfig("cgate11Fixed", b).GatedDetectorExtractSpecs["norm"]);

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
                    Dictionary<string, int> detectorsToExtract = new Dictionary<string, int> { { "top", 0 }, { "norm", 1 }, { "topNormed", 5 } };
                    foreach (KeyValuePair<string, int> detector in detectorsToExtract)
                    {
                        BlockTOFDemodulator demod = new BlockTOFDemodulator();
                        TOFChannelSet tcs = demod.TOFDemodulateBlock(b, detector.Value, true);
                        byte[] tcsBytes = serializeAsByteArray(tcs);
                        DBTOFChannelSet t = new DBTOFChannelSet();
                        t.tcsData = tcsBytes;
                        t.detector = detector.Key;
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
                Controller.log("Error adding " + fileName);
                Controller.errorLog("Error adding block " + path + "\n" + e.StackTrace);
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


        #region Tagging

        public void AddTagToBlock(string clusterName, int blockIndex, string tag)
        {
        }

        public void RemoveTagFromBlock(string clusterName, int blockIndex, string tag)
        {
        }
        #endregion


        #region Stats

        #endregion
    }

}