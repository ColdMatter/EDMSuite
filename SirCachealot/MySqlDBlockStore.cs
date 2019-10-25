using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Data.Common;
using System.Data.Sql;

using System.Linq;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters.Binary;

using MySql.Data.MySqlClient;
using Analysis.EDM;

namespace SirCachealot.Database
{
    class MySqlDBlockStore : MarshalByRefObject, DBlockStore
    {
        private MySqlConnection mySql;
        private MySqlCommand mySqlComm;
        private string kConnectionString = "server=localhost;user=root;port=3306;password=atomic1;default command timeout=300;";
        public long QueryCount;
        public long DBlockCount;

        public UInt32[] GetUIDsByCluster(string clusterName, UInt32[] fromUIDs)
        {
            QueryCount++;
            return GetByStringParameter("CLUSTER", clusterName, fromUIDs);
        }

        public UInt32[] GetUIDsByCluster(string clusterName)
        {
            QueryCount++;
            return GetByStringParameter("CLUSTER", clusterName);
        }

        public UInt32[] GetUIDsByBlock(string clusterName, int clusterIndex, UInt32[] fromUIDs)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT UID FROM DBLOCKS WHERE CLUSTER = ?cluster AND CLUSTERINDEX = ?index AND UID IN " +
                MakeSQLArrayString(fromUIDs);
            mySqlComm.Parameters.AddWithValue("?cluster", clusterName);
            mySqlComm.Parameters.AddWithValue("?index", clusterIndex);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByBlock(string clusterName, int clusterIndex)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT UID FROM DBLOCKS WHERE CLUSTER = ?cluster AND CLUSTERINDEX = ?index";
            mySqlComm.Parameters.AddWithValue("?cluster", clusterName);
            mySqlComm.Parameters.AddWithValue("?index", clusterIndex);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByTag(string tag, UInt32[] fromUIDs)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT DISTINCT UID FROM DBLOCKS, TAGS WHERE TAGS.TAG = ?tag AND " +
                "TAGS.CLUSTER = DBLOCKS.CLUSTER AND " +
                "TAGS.CLUSTERINDEX = DBLOCKS.CLUSTERINDEX AND UID IN " + MakeSQLArrayString(fromUIDs);
            mySqlComm.Parameters.AddWithValue("?tag", tag);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByTag(string tag)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT DISTINCT UID FROM DBLOCKS, TAGS WHERE TAGS.TAG = ?tag AND " +
                "TAGS.CLUSTER = DBLOCKS.CLUSTER AND " +
                "TAGS.CLUSTERINDEX = DBLOCKS.CLUSTERINDEX";
            mySqlComm.Parameters.AddWithValue("?tag", tag);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByAnalysisTag(string tag, UInt32[] fromUIDs)
        {
            QueryCount++;
            return GetByStringParameter("ATAG", tag, fromUIDs);
        }

        public UInt32[] GetUIDsByAnalysisTag(string tag)
        {
            QueryCount++;
            return GetByStringParameter("ATAG", tag);
        }

        public UInt32[] GetUIDsByGateTag(string tag, UInt32[] fromUIDs)
        {
            QueryCount++;
            return GetByStringParameter("GATETAG", tag, fromUIDs);
        }

        public UInt32[] GetUIDsByGateTag(string tag)
        {
            QueryCount++;
            return GetByStringParameter("GATETAG", tag);
        }

        public UInt32[] GetUIDsByMachineState(bool eState, bool bState, bool rfState, bool mwState, uint[] fromUIDs)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT UID FROM DBLOCKS WHERE ESTATE = ?eState AND BSTATE = ?bState " +
                "AND RFSTATE = ?rfState AND MWSTATE = ?mwState AND UID IN " +
                MakeSQLArrayString(fromUIDs);
            mySqlComm.Parameters.AddWithValue("?eState", eState);
            mySqlComm.Parameters.AddWithValue("?bState", bState);
            mySqlComm.Parameters.AddWithValue("?rfState", rfState);
            mySqlComm.Parameters.AddWithValue("?mwState", mwState);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByMachineState(bool eState, bool bState, bool rfState, bool mwState)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT UID FROM DBLOCKS WHERE ESTATE = ?eState AND BSTATE = ?bState " +
                "AND RFSTATE = ?rfState AND MWSTATE = ?mwState";
            mySqlComm.Parameters.AddWithValue("?eState", eState);
            mySqlComm.Parameters.AddWithValue("?bState", bState);
            mySqlComm.Parameters.AddWithValue("?rfState", rfState);
            mySqlComm.Parameters.AddWithValue("?mwState", mwState);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByEState(bool eState, UInt32[] fromUIDs)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT UID FROM DBLOCKS WHERE ESTATE = ?eState AND UID IN " +
                MakeSQLArrayString(fromUIDs);
            mySqlComm.Parameters.AddWithValue("?eState", eState);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByEState(bool eState)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText = "SELECT UID FROM DBLOCKS WHERE ESTATE = ?eState";
            mySqlComm.Parameters.AddWithValue("?eState", eState);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByBState(bool bState, UInt32[] fromUIDs)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT UID FROM DBLOCKS WHERE BSTATE = ?bState AND UID IN " +
                MakeSQLArrayString(fromUIDs);
            mySqlComm.Parameters.AddWithValue("?bState", bState);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByBState(bool bState)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText = "SELECT UID FROM DBLOCKS WHERE BSTATE = ?bState";
            mySqlComm.Parameters.AddWithValue("?bState", bState);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByRFState(bool rfState, UInt32[] fromUIDs)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT UID FROM DBLOCKS WHERE RFSTATE = ?rfState AND UID IN " +
                MakeSQLArrayString(fromUIDs);
            mySqlComm.Parameters.AddWithValue("?rfState", rfState);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByRFState(bool rfState)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText = "SELECT UID FROM DBLOCKS WHERE RFSTATE = ?rfState";
            mySqlComm.Parameters.AddWithValue("?rfState", rfState);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByMWState(bool mwState, UInt32[] fromUIDs)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT UID FROM DBLOCKS WHERE MWSTATE = ?mwState AND UID IN " +
                MakeSQLArrayString(fromUIDs);
            mySqlComm.Parameters.AddWithValue("?mwState", mwState);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByMWState(bool mwState)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText = "SELECT UID FROM DBLOCKS WHERE MWSTATE = ?mwState";
            mySqlComm.Parameters.AddWithValue("?mwState", mwState);
            return GetUIDsFromCommand(mySqlComm);
        }

        public uint[] GetUIDsByDateRange(DateTime start, DateTime end, uint[] fromUIDs)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT UID FROM DBLOCKS WHERE BLOCKTIME >= ?start AND BLOCKTIME < ?end AND UID IN " +
                MakeSQLArrayString(fromUIDs);
            mySqlComm.Parameters.AddWithValue("?start", start);
            mySqlComm.Parameters.AddWithValue("?end", end);
            return GetUIDsFromCommand(mySqlComm);
        }

        public uint[] GetUIDsByDateRange(DateTime start, DateTime end)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText = "SELECT UID FROM DBLOCKS WHERE BLOCKTIME >= ?start AND BLOCKTIME < ?end";
            mySqlComm.Parameters.AddWithValue("?start", start);
            mySqlComm.Parameters.AddWithValue("?end", end);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByVoltageRange(double low, double high, UInt32[] fromUIDs)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT UID FROM DBLOCKS WHERE EPLUS >= ?low AND EPLUS < ?high AND UID IN " +
                MakeSQLArrayString(fromUIDs);
            mySqlComm.Parameters.AddWithValue("?low", low);
            mySqlComm.Parameters.AddWithValue("?high", high);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByVoltageRange(double low, double high)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT UID FROM DBLOCKS WHERE EPLUS >= ?low AND EPLUS < ?high";
            mySqlComm.Parameters.AddWithValue("?low", low);
            mySqlComm.Parameters.AddWithValue("?high", high);
            return GetUIDsFromCommand(mySqlComm);
        }

        public uint[] GetUIDsByPredicate(PredicateFunction func, uint[] fromUIDs)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText = "SELECT UID FROM DBLOCKS WHERE UID IN " + MakeSQLArrayString(fromUIDs);
            UInt32[] uids = GetUIDsFromCommand(mySqlComm);
            List<UInt32> matchedUIDs = new List<UInt32>();
            foreach (UInt32 uid in uids)
            {
                DemodulatedBlock db = GetDBlock(uid);
                if (func(db)) matchedUIDs.Add(uid);
            }
            return matchedUIDs.ToArray();
        }

        public DemodulatedBlock GetDBlock(uint uid)
        {
            DBlockCount++;
            byte[] dbb;

            MySqlDataReader rd = executeReader("SELECT DBDAT FROM DBLOCKDATA WHERE UID = " + uid);
            DemodulatedBlock db;
            if (rd.Read())
            {
                dbb = (byte[])rd["DBDAT"];
                db = deserializeDBlockFromByteArray(dbb);
                rd.Close();
            }
            else
            {
                rd.Close();
                throw new BlockNotFoundException();
            }
            return db;
        }

        // this method temporarily is locked so that only one thread at a time can execute.
        // I need to fix a db problem concerning the UIDs before unlocking it.
        // Hopefully it won't hurt the performance too badly.
        private object dbAddLock = new object();
        public UInt32 AddDBlock(DemodulatedBlock db)
        {
            lock (dbAddLock)
            {
                mySqlComm = mySql.CreateCommand();
                // extract the data that we're going to put in the sql database
                string clusterName = db.Config.Settings["cluster"] as string;
                int clusterIndex = (int)db.Config.Settings["clusterIndex"];
                string aTag = db.DataType.ToString();
                string gateTag = "";
                if (db is GatedDemodulatedBlock gdb) gateTag = gdb.GateConfig.Name;
                else gateTag = null;
                bool eState = (bool)db.Config.Settings["eState"];
                bool bState = (bool)db.Config.Settings["bState"];
                bool rfState = (bool)db.Config.Settings["rfState"];
                // for some blocks, no mw state config
                bool mwState = true;
                if (db.Config.Settings.StringKeyList.Contains("mwState")) mwState = (bool)db.Config.Settings["mwState"];

                DateTime timeStamp = db.TimeStamp;
                double ePlus = (double)db.Config.Settings["ePlus"];
                double eMinus = (double)db.Config.Settings["eMinus"];
                byte[] dBlockBytes = serializeDBlockAsByteArray(db);

                mySqlComm = mySql.CreateCommand();
                mySqlComm.CommandText =
                    "INSERT INTO DBLOCKS " +
                    "VALUES(?uint, ?cluster, ?clusterIndex, ?aTag, ?gateTag, ?eState, ?bState, ?rfState, ?mwState, ?ts, " +
                    "?ePlus, ?eMinus);";
                // the uid column is defined auto_increment
                mySqlComm.Parameters.AddWithValue("?uint", null);
                mySqlComm.Parameters.AddWithValue("?cluster", clusterName);
                mySqlComm.Parameters.AddWithValue("?clusterIndex", clusterIndex);
                mySqlComm.Parameters.AddWithValue("?aTag", aTag);
                mySqlComm.Parameters.AddWithValue("?gateTag", gateTag);
                mySqlComm.Parameters.AddWithValue("?eState", eState);
                mySqlComm.Parameters.AddWithValue("?bState", bState);
                mySqlComm.Parameters.AddWithValue("?rfState", rfState);
                mySqlComm.Parameters.AddWithValue("?mwState", mwState);
                mySqlComm.Parameters.AddWithValue("?ts", timeStamp);
                mySqlComm.Parameters.AddWithValue("?ePlus", ePlus);
                mySqlComm.Parameters.AddWithValue("?eMinus", eMinus);

                mySqlComm.ExecuteNonQuery();
                mySqlComm.Parameters.Clear();

                UInt32 uid = (UInt32)mySqlComm.LastInsertedId;
                mySqlComm = mySql.CreateCommand();
                mySqlComm.CommandText =
                  "INSERT INTO DBLOCKDATA VALUES(?uint, ?dblock);";
                mySqlComm.Parameters.AddWithValue("?uint", uid);
                mySqlComm.Parameters.AddWithValue("?dblock", dBlockBytes);

                mySqlComm.ExecuteNonQuery();
                mySqlComm.Parameters.Clear();

                return uid;
            }
        }

        public void RemoveDBlock(UInt32 uid)
        {
            executeNonQuery("DELETE FROM DBLOCKS WHERE UID = " + uid);
            executeNonQuery("DELETE FROM DBLOCKDATA WHERE UID = " + uid);
        }

        public UInt32[] GetAllUIDs()
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText = "SELECT UID FROM DBLOCKS";
            return GetUIDsFromCommand(mySqlComm);
        }

        public void AddTagToBlock(string clusterName, int blockIndex, string tag)
        {
            QueryCount++;
            executeNonQuery("INSERT INTO TAGS VALUES('" + clusterName + "', " + blockIndex + ", '" + tag + "')");
        }

        public void RemoveTagFromBlock(string clusterName, int blockIndex, string tag)
        {
            QueryCount++;
            executeNonQuery(
                "DELETE FROM TAGS WHERE CLUSTER = '" + clusterName + "' AND CLUSTERINDEX = " + blockIndex +
                " AND TAG = '" + tag + "'"
                );
        }

        public UInt32[] GetTaggedIndicesForCluster(string clusterName, string tag)
        {
            QueryCount++;
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT DISTINCT TAGS.CLUSTERINDEX FROM TAGS, DBLOCKS WHERE TAGS.CLUSTER = ?cluster AND " + 
                "TAGS.TAG = ?tag AND TAGS.CLUSTER = DBLOCKS.CLUSTER AND TAGS.CLUSTERINDEX = " +
                "DBLOCKS.CLUSTERINDEX";
            mySqlComm.Parameters.AddWithValue("?cluster", clusterName);
            mySqlComm.Parameters.AddWithValue("?tag", tag);
            return GetUIntsFromCommand(mySqlComm, "CLUSTERINDEX");
        }

        private string MakeSQLArrayString(UInt32[] uids)
        {
            StringBuilder sqlArray = new StringBuilder("(");
            for (int i = 0; i < uids.Length - 1; i++)
            {
                sqlArray.Append(uids[i]);
                sqlArray.Append(", ");
            }
            sqlArray.Append(uids[uids.Length - 1]);
            sqlArray.Append(")");

            return sqlArray.ToString();
        }

        private UInt32[] GetByStringParameter(string parameter, string val)
        {
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText = "SELECT UID FROM DBLOCKS WHERE " + parameter + " = ?val";
            mySqlComm.Parameters.AddWithValue("?val", val);
            return GetUIDsFromCommand(mySqlComm);
        }

        private UInt32[] GetByStringParameter(string parameter, string val, UInt32[] fromUIDs)
        {
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT UID FROM DBLOCKS WHERE " + parameter +
                " = ?val AND UID IN " + MakeSQLArrayString(fromUIDs);
            mySqlComm.Parameters.AddWithValue("?val", val);
            return GetUIDsFromCommand(mySqlComm);
        }

        private UInt32[] GetUIDsFromCommand(MySqlCommand cm)
        {
            return GetUIntsFromCommand(cm, "UID");
        }

        private UInt32[] GetUIntsFromCommand(MySqlCommand cm, string column)
        {
            MySqlDataReader rd = cm.ExecuteReader();


            List<UInt32> uids = new List<UInt32>();
            while (rd.Read()) uids.Add((UInt32)rd[column]);
            rd.Close();
            return uids.ToArray();
        }

        internal void Start()
        {
            //TODO: support multiple DBs
            // This creates a shared connection that is used for all query methods. As a result, the query
            // methods are probably not currently thread-safe (maybe, need to check).
            mySql = new MySqlConnection(kConnectionString);
            mySql.Open();
            mySqlComm = mySql.CreateCommand();
        }

        internal List<string> GetDatabaseList()
        {
            MySqlDataReader rd = executeReader("SHOW DATABASES;");
            List<string> databases = new List<string>();
            while (rd.Read()) databases.Add(rd.GetString(0));
            rd.Close();
            return databases;
        }

        internal void Connect(string dbName)
        {
            executeNonQuery("USE " + dbName + ";");
        }

        internal void CreateDatabase(string dbName)
        {
            executeNonQuery("CREATE DATABASE " + dbName + ";");
            Connect(dbName);
            executeNonQuery(
                "CREATE TABLE DBLOCKS (UID INT UNSIGNED NOT NULL AUTO_INCREMENT, " +
                "CLUSTER VARCHAR(30), CLUSTERINDEX INT UNSIGNED, " +
                "ATAG VARCHAR(30), GATETAG VARCHAR(30), ESTATE BOOL, BSTATE BOOL, RFSTATE BOOL, MWSTATE BOOL, BLOCKTIME DATETIME, " +
                "EPLUS DOUBLE, EMINUS DOUBLE, PRIMARY KEY (UID))"
                );
            executeNonQuery(
                "CREATE TABLE TAGS (CLUSTER VARCHAR(30), CLUSTERINDEX INT UNSIGNED, TAG VARCHAR(30))"
                );
            executeNonQuery(
                "CREATE TABLE DBLOCKDATA (UID INT UNSIGNED NOT NULL, DBDAT LONGBLOB, PRIMARY KEY (UID))"
                );
        }

        internal void Stop()
        {
            mySql.Close();
        }

        private int executeNonQuery(string command)
        {
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText = command;
            return mySqlComm.ExecuteNonQuery();
        }

        private MySqlDataReader executeReader(string command)
        {
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText = command;
            return mySqlComm.ExecuteReader();


        }

        private byte[] serializeDBlockAsByteArray(DemodulatedBlock db)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, db);
            byte[] buffer = new Byte[ms.Length];
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(buffer, 0, (int)ms.Length);
            ms.Close();
            return buffer;
        }

        private DemodulatedBlock deserializeDBlockFromByteArray(byte[] ba)
        {
            MemoryStream ms = new MemoryStream(ba);
            BinaryFormatter bf = new BinaryFormatter();
            return (DemodulatedBlock)bf.Deserialize(ms);
        }

    }
}
