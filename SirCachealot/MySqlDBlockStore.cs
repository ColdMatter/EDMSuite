using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Data.Common;
using System.Data.Sql;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using MySql.Data.MySqlClient;
using Analysis.EDM;

namespace SirCachealot
{
    class MySqlDBlockStore : DBlockStore
    {
        public Cache cache;
        private MySqlConnection mySql;
        private MySqlCommand mySqlComm;

        public uint[] GetUIDsByCluster(string clusterName, uint[] fromUIDs)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public uint[] GetUIDsByCluster(string clusterName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public uint[] GetUIDsByTag(string tag, uint[] fromUIDs)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public uint[] GetUIDsByTag(string tag)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public uint[] GetUIDsByAnalysisTag(string tag, uint[] fromUIDs)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public uint[] GetUIDsByAnalysisTag(string tag)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public uint[] GetUIDsByMachineState(bool eState, bool bState, uint[] fromUIDs)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public uint[] GetUIDsByMachineState(bool eState, bool bState)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public uint[] GetUIDsByDateRange(DateTime start, DateTime end, uint[] fromUIDs)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public uint[] GetUIDsByDateRange(DateTime start, DateTime end)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public uint[] GetUIDsByPredicate(PredicateFunction func, uint[] fromUIDs)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Analysis.EDM.DemodulatedBlock GetDBlock(uint uid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        byte[] dbb;
        public uint AddDBlock(DemodulatedBlock db)
        {
            // extract the data that we're going to put in the sql database
            string clusterName = db.Config.Settings["cluster"] as string;
            int clusterIndex = (int)db.Config.Settings["clusterIndex"];
            bool eState = (bool)db.Config.Settings["eState"];
            bool bState = (bool)db.Config.Settings["bState"];
            DateTime timeStamp = db.TimeStamp;
            //byte[] dBlockBytes = serializeDBlockAsByteArray(db);
            //dbb = dBlockBytes;
            byte[] dBlockBytes = dbb;

            mySqlComm.CommandText = 
                "INSERT INTO DBLOCKS VALUES(?uint, ?cluster, ?clusterIndex, ?aTag, ?eState, ?bState, ?ts);";
            // the uid column is defined auto_increment
            mySqlComm.Parameters.AddWithValue("?uint", null);
            mySqlComm.Parameters.AddWithValue("?cluster", clusterName);
            mySqlComm.Parameters.AddWithValue("?clusterIndex", clusterIndex);
            mySqlComm.Parameters.AddWithValue("?aTag", "fruity");
            mySqlComm.Parameters.AddWithValue("?eState", eState);
            mySqlComm.Parameters.AddWithValue("?bState", bState);
            mySqlComm.Parameters.AddWithValue("?ts", timeStamp);
 
            mySqlComm.ExecuteNonQuery();
            mySqlComm.Parameters.Clear();
            UInt32 uid = (UInt32)mySqlComm.LastInsertedId;

            mySqlComm.CommandText =
              "INSERT INTO DBLOCKDATA VALUES(?uint, ?dblock);";
            mySqlComm.Parameters.AddWithValue("?uint", uid);
            mySqlComm.Parameters.AddWithValue("?dblock", dBlockBytes);

            mySqlComm.ExecuteNonQuery();
            mySqlComm.Parameters.Clear();
           
            return uid;
        }

        public void RemoveDBlock(uint uid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void AddTagToBlock(string clusterName, int blockIndex, string tag)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RemoveTagToBlock(string clusterName, int blockIndex, string tag)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        private string MakeSQLArrayString(UInt32[] uids)
        {
            StringBuilder sqlArray = new StringBuilder("(");
            for (int i = 0; i < uids.Length - 1; i++)
            {
                sqlArray.Append(i);
                sqlArray.Append(", ");
            }
            sqlArray.Append(uids[uids.Length - 1]);
            sqlArray.Append(")");

            return sqlArray.ToString();
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
            mySqlComm.CommandText = command;
            return mySqlComm.ExecuteNonQuery();
        }

        private MySqlDataReader executeReader(string command)
        {
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
    }
}
