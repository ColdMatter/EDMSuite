using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace MOTMaster
{
    public static class ParameterFileManager
    {
        // On-disk JSON shape: { "groups": [ { "name": ..., "parameters": [ { "name", "value", "type" }, ... ] }, ... ] }
        private class ParameterEntryDto
        {
            public string name;
            public string value;
            public string type;
        }

        private class ParameterGroupDto
        {
            public string name;
            public List<ParameterEntryDto> parameters;
        }

        private class ParameterFileDto
        {
            public List<ParameterGroupDto> groups;
        }

        public static List<ParameterGroup> ReadFile(string filePath, Type fallbackType)
        {
            var groups = new List<ParameterGroup>();

            if (!File.Exists(filePath))
                return groups;

            var serializer = new JavaScriptSerializer();
            string json = File.ReadAllText(filePath);
            var dto = serializer.Deserialize<ParameterFileDto>(json);

            if (dto?.groups == null)
                return groups;

            foreach (var groupDto in dto.groups)
            {
                var group = new ParameterGroup(groupDto.name ?? string.Empty);

                if (groupDto.parameters != null)
                {
                    foreach (var entryDto in groupDto.parameters)
                    {
                        Type resolvedType = Type.GetType(entryDto.type ?? string.Empty) ?? fallbackType;
                        group.Parameters.Add(new ParameterEntry(entryDto.name, entryDto.value, resolvedType));
                    }
                }

                groups.Add(group);
            }

            return groups;
        }

        public static void WriteFile(string filePath, List<ParameterGroup> groups)
        {
            var dto = new ParameterFileDto { groups = new List<ParameterGroupDto>() };

            foreach (var group in groups)
            {
                var groupDto = new ParameterGroupDto { name = group.Name, parameters = new List<ParameterEntryDto>() };

                foreach (var entry in group.Parameters)
                {
                    groupDto.parameters.Add(new ParameterEntryDto
                    {
                        name = entry.Name,
                        value = entry.Value,
                        type = entry.Type.FullName
                    });
                }

                dto.groups.Add(groupDto);
            }

            var serializer = new JavaScriptSerializer();
            File.WriteAllText(filePath, serializer.Serialize(dto));
        }

        /// <summary>
        /// Flattens grouped parameters for callers that don't care about grouping
        /// (e.g. MOTMasterScript.LoadGlobalParameters). Throws if the same parameter
        /// name appears in more than one group, rather than letting one group's value
        /// silently win.
        /// </summary>
        public static List<ParameterEntry> Flatten(List<ParameterGroup> groups)
        {
            var entries = new List<ParameterEntry>();
            var seenNames = new HashSet<string>();

            foreach (var group in groups)
            {
                foreach (var entry in group.Parameters)
                {
                    if (!seenNames.Add(entry.Name))
                        throw new InvalidOperationException($"Parameter \"{entry.Name}\" is defined in more than one group.");

                    entries.Add(entry);
                }
            }

            return entries;
        }
    }
}
