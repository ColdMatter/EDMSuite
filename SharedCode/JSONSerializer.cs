using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Data
{
    public class JSONSerializer
    {
        /// <summary>
        /// JSON filename
        /// </summary>
        private string fileName;

        /// <summary>
        /// Datalog count incremented by adding data
        /// </summary>
        private int serializedCount;

        /// <summary>
        /// Thread safe first in first out queue for holding
        /// data until it has been writen to file
        /// </summary>
        private BlockingCollection<DataLog> dataQueue;

        /// <summary>
        /// Worker thread for serializing the datalog
        /// </summary>
        private Thread serializeThread;

        /// <summary>
        /// Constructor
        /// </summary>
        public JSONSerializer(){ }

        /// <summary>
        /// Method to prepare the JSON file for data to be added
        /// </summary>
        /// <param name="fn">Filename for log file</param>
        public void StartLogFile(string fn)
        {
            // Initialize data queue
            dataQueue = new BlockingCollection<DataLog>(new ConcurrentQueue<DataLog>());

            // Initialze JSON file
            fileName = fn;
            using (FileStream fs = File.Open(fn, FileMode.Create))
            {
                // initialize count
                serializedCount = 0; 
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write("[");
                }
            }
        }

        /// <summary>
        /// Serialize dataLog to previously opened JSON file
        /// </summary>
        /// <param name="dataLog">Datalog to be serialized</param>
        public void SerializeData(DataLog dataLog)
        {
            using (FileStream fs = File.Open(fileName, FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    // add comma if not first file in datalog
                    if (serializedCount > 0) sw.Write(",");
                //    // add data delimited with braces
                    sw.Write("{\r\n");
                    sw.Write(dataLog.ToString());
                    sw.Write("\r\n}");
                }
            }
            // increment counter
            serializedCount++;
        }

        /// <summary>
        /// Added dataLog queue to be serialized
        /// </summary>
        /// <param name="dataLog">Datalog to be serialized</param>
        public void AddData(DataLog dataLog)
        {
            dataQueue.Add(dataLog);
        }

        /// <summary>
        /// Add closing braces to JSON file
        /// </summary>
        public void EndLogFile()
        {
            using (FileStream fs = File.Open(fileName, FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write("]");
                }
            }
        }

        /// <summary>
        /// Worker method for processing the data queue
        /// </summary>
        public void ProcessDataQueue()
        {
            // Note this will block if dataqueue is empty
            // and will end when CompleteAdding is called
            foreach (DataLog log in dataQueue.GetConsumingEnumerable()) 
            {
                SerializeData(log);
            }

            // Wrap up the JSON file
            EndLogFile();
        }

        /// <summary>
        /// Method to begin the worker thread for
        /// serializing the queue of data logs
        /// </summary>
        public void StartProcessingData()
        {
            ThreadStart start = new ThreadStart(ProcessDataQueue);
            serializeThread = new Thread(start);
            serializeThread.Start();
        }

        /// <summary>
        /// Method to notify the worker thread that
        /// no more data logs will be added to the 
        /// queue. Complete serializing process when
        /// queue is empty.
        /// </summary>
        public void CompleteAdding()
        {
            dataQueue.CompleteAdding();
        }
    }
}
