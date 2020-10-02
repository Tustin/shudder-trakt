using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace ShudderScrobbler
{
    public class FileService
    {
        private string ConfigPath = Path.Combine(Path.GetFullPath(Directory.GetCurrentDirectory()), "config.json");
        private static FileService _instance = default;
        public static FileService Instance
        {
            get
            {
                return _instance ??= new FileService();
            }
        }

        private ConfigModel _config = default;
        public ConfigModel Config
        {
            get
            {
                return _config ??= this.Read();
            }
            private set
            {
                _config = value;
            }
        }

        private ConfigModel Read()
        {
            return JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText(ConfigPath));
        }

        public void Write()
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config));
        }
    }
}
