using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AlFHardwareControl
{
    [Serializable]
    public class MacroConfigurationCollection : List<MacroConfiguration>
    {
        public List<MacroConfiguration> macros
        {
            get
            {
                return this;
            }
        }

        public void AddMacro()
        {
            int id = 0;
            reset:
            ++id;
            foreach (MacroConfiguration macro in macros)
                if (macro.Name == id.ToString())
                    goto reset;

            macros.Add(new MacroConfiguration(id.ToString()));
        }

        public void RemoveMacro(int id)
        {
            macros.RemoveAt(id);
        }

        public string RenameMacro(int id,string newName)
        {
            foreach (MacroConfiguration macro in macros)
                if (macro.Name == newName)
                    return macros[id].Name;

            macros[id].Name = newName;
            return newName;
        }
        
        public void PrepSerialization()
        {
            foreach (MacroConfiguration macro in macros)
                macro.PrepSerialization();
        }
        public void UndoSerialization()
        {
            foreach (MacroConfiguration macro in macros)
                macro.UndoSerialization();
        }
    }
}
