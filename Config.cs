﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP.IO;

namespace kOS
{
    public class Config : SpecialValue
    {
        private static Config _instance = null;
        private Dictionary<string, ConfigKey> _keys;
        private Dictionary<string, ConfigKey> _alias;
        private Dictionary<PropId, ConfigKey> _properties;

        public int InstructionsPerUpdate { get { return GetPropValue<int>(PropId.InstructionsPerUpdate); } set { SetPropValue(PropId.InstructionsPerUpdate, value); } }
        public bool UseNewPersistenceFormat { get { return GetPropValue<bool>(PropId.UseNewPersistenceFormat); } set { SetPropValue(PropId.UseNewPersistenceFormat, value); } }
        public bool ShowStatistics { get { return GetPropValue<bool>(PropId.ShowStatistics); } set { SetPropValue(PropId.ShowStatistics, value); } }

        private Config()
        {
            _keys = new Dictionary<string, ConfigKey>();
            _alias = new Dictionary<string, ConfigKey>();
            _properties = new Dictionary<PropId, ConfigKey>();
            BuildValuesDictionary();
            LoadConfig();
        }

        private void BuildValuesDictionary()
        {
            AddConfigKey(PropId.InstructionsPerUpdate, new ConfigKey("InstructionsPerUpdate", "IPU", "Instructions per update", 40, typeof(int)));
            AddConfigKey(PropId.UseNewPersistenceFormat, new ConfigKey("UseNewPersistenceFormat", "NPF", "Use new persistence format", false, typeof(bool)));
            AddConfigKey(PropId.ShowStatistics, new ConfigKey("ShowStatistics", "STAT", "Show execution statistics", false, typeof(bool)));
        }

        private void AddConfigKey(PropId id, ConfigKey key)
        {
            _keys.Add(key.StringKey.ToUpper(), key);
            _alias.Add(key.Alias.ToUpper(), key);
            _properties.Add(id, key);
        }

        private void LoadConfig()
        {
            try
            {
                PluginConfiguration config = PluginConfiguration.CreateForType<Config>();
                config.load();

                foreach (ConfigKey key in _keys.Values)
                {
                    object value = config[key.StringKey];
                    if (value != null)
                    {
                        key.Value = value;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static Config GetInstance()
        {
            if (_instance == null) _instance = new Config();
            return _instance;
        }

        private T GetPropValue<T>(PropId id)
        {
            return (T)_properties[id].Value;
        }

        private void SetPropValue(PropId id, object value)
        {
            _properties[id].Value = value;
        }

        public void SaveConfig()
        {
            PluginConfiguration config = PluginConfiguration.CreateForType<Config>();
            config.load();

            foreach (ConfigKey key in _keys.Values)
            {
                SaveConfigKey(key, config);
            }

            config.save();
        }

        private void SaveConfigKey(ConfigKey key)
        {
            PluginConfiguration config = PluginConfiguration.CreateForType<Config>();
            config.load();
            SaveConfigKey(key, config);
            config.save();
        }

        private void SaveConfigKey(ConfigKey key, PluginConfiguration config)
        {
            config.SetValue(key.StringKey, _keys[key.StringKey.ToUpper()].Value);
        }

        public override object GetSuffix(String suffixName)
        {
            ConfigKey key = null;

            if (_keys.ContainsKey(suffixName))
            {
                key = _keys[suffixName];
            }
            else if (_alias.ContainsKey(suffixName))
            {
                key = _alias[suffixName];
            }

            if (key != null)
            {
                return key.Value;
            }
            else
            {
                return base.GetSuffix(suffixName);
            }
        }

        public override bool SetSuffix(String suffixName, object value)
        {
            ConfigKey key = null;

            if (_keys.ContainsKey(suffixName))
            {
                key = _keys[suffixName];
            }
            else if (_alias.ContainsKey(suffixName))
            {
                key = _alias[suffixName];
            }

            if (key != null)
            {
                if (value.GetType() == key.Type)
                {
                    key.Value = value;
                    SaveConfigKey(key);
                    return true;
                }
                else
                {
                    throw new Exception(string.Format("The value of the configuration key '{0}' has to be of type '{1}'", key.Name, key.Type));
                }
            }
            else
            {
                return false;
            }
        }

        public List<ConfigKey> GetConfigKeys()
        {
            return _keys.Values.ToList<ConfigKey>();
        }

        public override string ToString()
        {
            return "Use \"list config.\" to view all configurations";
        }

        private enum PropId
        {
            InstructionsPerUpdate = 1,
            UseNewPersistenceFormat = 2,
            ShowStatistics = 3
        }
    }

    public class ConfigKey
    {
        public string StringKey;
        public string Alias;
        public string Name;
        public Type Type;
        public object Value;

        public ConfigKey(string stringKey, string alias, string name, object defaultValue, Type type)
        {
            this.StringKey = stringKey;
            this.Alias = alias;
            this.Name = name;
            this.Value = defaultValue;
            this.Type = type;
        }
    }

    [kOSBinding("ksp")]
    public class BindingConfig : Binding
    {
        public override void AddTo(SharedObjects shared)
        {
            _shared = shared;
            _shared.BindingMgr.AddGetter("CONFIG", delegate(CPU cpu) { return Config.GetInstance(); });
        }
    }
}
