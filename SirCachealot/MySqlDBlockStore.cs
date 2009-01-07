using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Data.Common;
using System.Data.Sql;

using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters.Binary;

using MySql.Data.MySqlClient;
using Analysis.EDM;

namespace SirCachealot
{
    class MySqlDBlockStore : MarshalByRefObject, DBlockStore
    {
        public Cache cache;
        private MySqlConnection mySql;
        private MySqlCommand mySqlComm;

        public UInt32[] GetUIDsByCluster(string clusterName, UInt32[] fromUIDs)
        {
            return GetByStringParameter("CLUSTER", clusterName, fromUIDs);
        }

        public UInt32[] GetUIDsByCluster(string clusterName)
        {
            return GetByStringParameter("CLUSTER", clusterName);
        }

        public UInt32[] GetUIDsByBlock(string clusterName, int clusterIndex, UInt32[] fromUIDs)
        {
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
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText =
                "SELECT UID FROM DBLOCKS WHERE CLUSTER = ?cluster AND CLUSTERINDEX = ?index";
            mySqlComm.Parameters.AddWithValue("?cluster", clusterName);
            mySqlComm.Parameters.AddWithValue("?index", clusterIndex);
            return GetUIDsFromCommand(mySqlComm);
        }

        public UInt32[] GetUIDsByTag(string tag, UInt32[] fromUIDs)
        {
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
            return GetByStringParameter("ATAG", tag, fromUIDs);
        }

        public UInt32[] GetUIDsByAnalysisTag(string tag)
        {
            return GetByStringParameter("ATAG", tag);
        }

        public UInt32[] GetUIDsByMachineState(bool eState, bool bState, uint[] fromUIDs)
        {
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText = 
                "SELECT UID FROM DBLOCKS WHERE ESTATE = ?eState AND BSTATE = ?bState AND UID IN " +
                MakeSQLArrayString(fromUIDs);
            mySqlComm.Parameters.AddWithValue("?eState", eState);
            mySqlComm.Parameters.AddWithValue("?bState", bState);
            return GetUIDsFromCommand(mySqlComm);
        }

        public uint[] GetUIDsByMachineState(bool eState, bool bState)
        {
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText = "SELECT UID FROM DBLOCKS WHERE ESTATE = ?eState AND BSTATE = ?bState";
            mySqlComm.Parameters.AddWithValue("?eState", eState);
            mySqlComm.Parameters.AddWithValue("?bState", bState);
            return GetUIDsFromCommand(mySqlComm);
        }

        public uint[] GetUIDsByDateRange(DateTime start, DateTime end, uint[] fromUIDs)
        {
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
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText = "SELECT UID FROM DBLOCKS WHERE BLOCKTIME >= ?start AND BLOCKTIME < ?end";
            mySqlComm.Parameters.AddWithValue("?start", start);
            mySqlComm.Parameters.AddWithValue("?end", end);
            return GetUIDsFromCommand(mySqlComm);
        }

        public uint[] GetUIDsByPredicate(PredicateFunction func, uint[] fromUIDs)
        {
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

        public UInt32 AddDBlock(DemodulatedBlock db)
        {
            // extract the data that we're going to put in the sql database
            string clusterName = db.Config.Settings["cluster"] as string;
            int clusterIndex = (int)db.Config.Settings["clusterIndex"];
            string aTag = db.DemodulationConfig.AnalysisTag;
            bool eState = (bool)db.Config.Settings["eState"];
            bool bState = (bool)db.Config.Settings["bState"];
            DateTime timeStamp = db.TimeStamp;
            byte[] dBlockBytes = serializeDBlockAsByteArray(db);

            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText = 
                "INSERT INTO DBLOCKS VALUES(?uint, ?cluster, ?clusterIndex, ?aTag, ?eState, ?bState, ?ts);";
            // the uid column is defined auto_increment
            mySqlComm.Parameters.AddWithValue("?uint", null);
            mySqlComm.Parameters.AddWithValue("?cluster", clusterName);
            mySqlComm.Parameters.AddWithValue("?clusterIndex", clusterIndex);
            mySqlComm.Parameters.AddWithValue("?aTag", aTag);
            mySqlComm.Parameters.AddWithValue("?eState", eState);
            mySqlComm.Parameters.AddWithValue("?bState", bState);
            mySqlComm.Parameters.AddWithValue("?ts", timeStamp);
 
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

        public void RemoveDBlock(UInt32 uid)
        {
            executeNonQuery("DELETE FROM DBLOCKS WHERE UID = " + uid);
            executeNonQuery("DELETE FROM DBLOCKDATA WHERE UID = " + uid);
        }

        public UInt32[] GetAllUIDs()
        {
            mySqlComm = mySql.CreateCommand();
            mySqlComm.CommandText = "SELECT UID FROM DBLOCKS";
            return GetUIDsFromCommand(mySqlComm);
        }

        public void AddTagToBlock(string clusterName, int blockIndex, string tag)
        {
            executeNonQuery("INSERT INTO TAGS VALUES('" + clusterName + "', " + blockIndex + ", '" + tag + "')");
        }

        public void RemoveTagFromBlock(string clusterName, int blockIndex, string tag)
        {
            executeNonQuery(
                "DELETE FROM TAGS WHERE CLUSTER = '" + clusterName + "' AND CLUSTERINDEX = " + blockIndex +
                " AND TAG = '" + tag + "'"
                );
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
            MySqlDataReader rd = cm.ExecuteReader();
            List<UInt32> uids = new List<UInt32>();
            while (rd.Read()) uids.Add((UInt32)rd["UID"]);
            rd.Close();
            return uids.ToArray();
        }

        internal void Start()
        {
            //TODO: support multiple DBs
            mySql = new MySqlConnection("Server=127.0.0.1;Uid=root;Pwd=atomic1;");
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
                "CLUSTER VARCHAR(30), CLUSTERINDEX INT, " +
                "ATAG VARCHAR(30), ESTATE BOOL, BSTATE BOOL, BLOCKTIME DATETIME," +
                "PRIMARY KEY (UID))"
                );
            executeNonQuery(
                "CREATE TABLE TAGS (CLUSTER VARCHAR(30), CLUSTERINDEX INT, TAG VARCHAR(30))"
                );
            executeNonQuery(
                "CREATE TABLE DBLOCKDATA (UID INT UNSIGNED NOT NULL, DBDAT MEDIUMBLOB, PRIMARY KEY (UID))"
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
