using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace AlFHardwareControl
{
    [Serializable]
    public class MacroConfiguration
    {

        [Serializable]
        public class TaskNode
        {
            public List<int> prerequisites = new List<int> { };
            public List<int> follows = new List<int> { };

            public int id;
            public string TaskName = "";
            public bool conditional = false;
            public string conditionResource = "";
            public string conditionComparison = "";
            public string conditionValue = "";
            public int delaySS = 0;
            public int delayMM = 0;
            public int delayHH = 0;

            public bool start = true;

            public TaskNode(int _id)
            {
                id = _id;
            }

            public TaskNode() { }

            public string Name
            {
                get
                {
                    return id.ToString() + " " + TaskName;
                }
            }

        }

        public class KVSerializeable<T, K>
        {
            public T Key;
            public K Value;

            public KVSerializeable(T k, K v)
            {
                Key = k;
                Value = v;
            }

            public KVSerializeable()
            {
            }

        }

        public void AddPrerequisite(int tnid, int id)
        {
            TaskNode tn = tasks[tnid];
            if (tn.prerequisites.Contains(id)) return;
            tn.prerequisites.Add(id);

            tasks[id].follows.Add(tnid);
            tn.start = false;
        }
        public void RemovePrerequisite(int tnid, int id)
        {
            TaskNode tn = tasks[tnid];
            tn.prerequisites.Remove(id);

            tasks[id].follows.Remove(tnid);
            if (tn.prerequisites.Count == 0)
                tn.start = true;
        }

        public string Name;
        public bool repeat = false;

        public int id_index = 0;
        public List<KVSerializeable<int, TaskNode>> tasksSerializable = new List<KVSerializeable<int, TaskNode>>{};
        
        [XmlIgnore]
        public Dictionary<int, TaskNode> tasks = new Dictionary<int, TaskNode> { };

        
        
        public void PrepSerialization()
        {
            foreach (KeyValuePair<int, TaskNode> kv in tasks)
                tasksSerializable.Add(new KVSerializeable<int, TaskNode>(kv.Key, kv.Value));
        }

        
        public void UndoSerialization()
        {
            foreach (KVSerializeable<int, TaskNode> kv in tasksSerializable)
                tasks[kv.Key] = kv.Value;
        }

        public MacroConfiguration(string name)
        {
            Name = name;
        }

        public MacroConfiguration()
        {
            Name = "";
        }

        public int AddTask()
        {
            tasks.Add(id_index, new TaskNode(id_index));
            return id_index++;
        }

        public void RemoveTask(int id)
        {
            foreach (int tn in tasks[id].prerequisites)
                tasks[tn].follows.Remove(id);

            foreach (int tn in tasks[id].follows)
                tasks[tn].prerequisites.Remove(id);

            tasks.Remove(id);
        }

    }
}
