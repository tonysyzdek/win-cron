using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;

namespace WinCron.Util
{
    public class Configuration : IConfiguration
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<string, string> _items = new Dictionary<string, string>();
        
        private static string EntryDirectory()
        {
            return new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
        }

        public static Configuration Load(string filePath = null)
        {
            if (filePath == null)
            {
                var entryDirectory = EntryDirectory();
                filePath = Path.Combine(entryDirectory, "settings.ini");
            }

            if (!System.IO.File.Exists(filePath))
            {
                throw new Exception("File does not exist: " + filePath);
            }

            var configuration = new Configuration();
            var machineName = Environment.MachineName.ToLower();

            foreach (var line in System.IO.File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.Trim().StartsWith("#"))
                {
                    continue;
                }

                var splitLine = line.Split(new[] {'='}, StringSplitOptions.None);

                var length = splitLine.Length;
                if (length != 2)
                {
                    throw new Exception("Line in {0} is not a key=value pair. It has {1} parts: {2}".Fmt(filePath,
                        length, line));
                }

                var key = splitLine[0];

                var envOverride = false;
                var environmentWithKey = key.Split('|');
                if (environmentWithKey.Length > 1)
                {
                    var intendedMachine = environmentWithKey[0];
                    if (intendedMachine.ToLower() != machineName)
                    {
                        continue;
                    }
                    key = environmentWithKey[1];
                    envOverride = true;
                }

                var value = splitLine[1];

                configuration.Add(key, value, envOverride);

                Log.Info("Added Configuration item {0}={1}".Fmt(key, value));
            }

            return configuration;
        }

        private void Add(string key, string value, bool envOverride)
        {
            key = key.ToLower();

            if (!envOverride && _items.ContainsKey(key))
            {
                throw new Exception("Key has already been added: {0}".Fmt(key));
            }

            _items[key] = value;
        }

        private string TryResolveString(string key)
        {
            key = key.ToLower();

            string value;
            if (!_items.TryGetValue(key, out value))
            {
                return null;
            }

            return value;
        }

        public string ResolveString(string key)
        {
            var val = TryResolveString(key);

            if (val == null)
            {
                var message = "Unable to locate {0}".Fmt(key);
                Log.Warn(message);
                throw new Exception(message);
            }

            return val;
        }
    }
}