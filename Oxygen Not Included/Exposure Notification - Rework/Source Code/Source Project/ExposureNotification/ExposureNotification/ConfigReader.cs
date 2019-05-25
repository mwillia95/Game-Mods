using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace ExposureNotification
{
    public abstract class ConfigObject
    {


    }

    public static class ConfigReader<T> where T : ConfigObject, new()
    {
        private static string SteamID = "";
        private static string ModName = "DiseaseRework";

        private static string RootModString => KMod.Manager.GetDirectory();

        public static T GetConfig()
        {
            DirectoryInfo modFolder;
            if(!TryFindSteamFolder(out modFolder))
            {
                if(!TryFindLocalFolder(out modFolder))
                {
                    throw new Exception("Mod folder could not be found. Make sure the correct SteamID and ModName are configured");
                }
            }

            FileInfo configFile = modFolder.GetFiles().FirstOrDefault(f => f.Name.ToLower() == "config.json");
            T config;
            if(configFile == null)
            {
                config = CreateDefaultFile(modFolder);
            }
            else
            {
                config = ReadConfigFile(configFile);
            }

            return config;
        }

        private static T CreateDefaultFile(DirectoryInfo modFolder)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            using (StreamWriter sw = new StreamWriter(Path.Combine(modFolder.FullName, "config.json")))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                T co = new T();
                serializer.Serialize(writer, co);
                return co;
            }
        }

        private static T ReadConfigFile(FileInfo file)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            using (StreamReader sr = new StreamReader(file.FullName))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                T result = serializer.Deserialize<T>(reader);
                return result;
            }
        }

        private static bool TryFindSteamFolder(out DirectoryInfo folder)
        {
            if(SteamID != "")
            {
                DirectoryInfo modDirectory = new DirectoryInfo(RootModString);
                DirectoryInfo[] tmp = modDirectory.GetDirectories().Where(x => x.Name == "Steam").ToArray();
                if (tmp.Length != 0)
                {
                    DirectoryInfo steamDirectory = tmp[0];
                    tmp = steamDirectory.GetDirectories().Where(x => x.Name == SteamID).ToArray();
                    if (tmp.Length != 0)
                    {
                        folder = tmp[0];
                        return true;
                    }
                }
            }
            folder = null;
            return false;
        }

        private static bool TryFindLocalFolder(out DirectoryInfo folder)
        {
            if (ModName != "")
            {
                DirectoryInfo modDirectory = new DirectoryInfo(RootModString);
                DirectoryInfo[] tmp = modDirectory.GetDirectories().Where(x => x.Name == "Local").ToArray();
                if (tmp.Length != 0)
                {
                    DirectoryInfo steamDirectory = tmp[0];
                    tmp = steamDirectory.GetDirectories().Where(x => x.Name == ModName).ToArray();
                    if (tmp.Length != 0)
                    {
                        folder = tmp[0];
                        return true;
                    }
                }
            }
            folder = null;
            return false;
        }
    }
}
