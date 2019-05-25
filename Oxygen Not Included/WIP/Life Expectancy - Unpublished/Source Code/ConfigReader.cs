using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;


namespace DuplicantLifeExpectancy
{
    public static class ConfigReader
    {
        private const string SteamModID = "1720103574";
        public static ConfigObject ReadConfig()
        {
            string savePath = SaveLoader.GetActiveSaveFilePath();

            if (savePath == "")
            {
                return null;
            }
            FileInfo file = new FileInfo(savePath);
            DirectoryInfo saveFolder = file.Directory;
            if (saveFolder == null)
            {
                return null;
            }
            DirectoryInfo rootFolder = null;
            DirectoryInfo modFolder = null;
            DirectoryInfo steamFolder = null;
            DirectoryInfo targetFolder = null;

            rootFolder = saveFolder.Parent;
            if (rootFolder == null)
                return null;

            if (rootFolder.Name == "save_files")
                rootFolder = saveFolder.Parent;

            if (rootFolder == null)
                return null;

            foreach (DirectoryInfo d in rootFolder.GetDirectories())
            {
                if (d.Name == "mods")
                {
                    modFolder = d;
                    break;
                }
            }

            if (modFolder == null)
            {
                return null;
            }
            foreach (DirectoryInfo d in modFolder.GetDirectories())
            {
                if (d.Name == "Steam")
                {
                    steamFolder = d;
                    break;
                }
            }

            if (steamFolder == null)
            {
                return null;
            }
            foreach (DirectoryInfo d in steamFolder.GetDirectories())
            {
                if (d.Name == SteamModID)
                {
                    targetFolder = d;
                    break;
                }
            }

            if (targetFolder == null)
            {
                return null;
            }
            try
            {
                FileInfo target = null;
                foreach (FileInfo f in targetFolder.GetFiles())
                {
                    if ($"{f.Name}".ToLower() == "config.json")
                    {
                        target = f;
                        break;
                    }
                }
                if (target != null)
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    using (StreamReader sr = new StreamReader(target.FullName))
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        object _result = serializer.Deserialize<ConfigObject>(reader);
                        return (ConfigObject)_result;
                    }
                }
                else
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    using (StreamWriter sw = new StreamWriter(targetFolder.FullName + @"/config.json"))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        ConfigObject co = new ConfigObject();
                        co.LifeSpanOption = 2;
                        co.DieOfOldAge = true;
                        serializer.Serialize(writer, co);
                        return co;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
