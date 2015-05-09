using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace RummageCore
{
    public enum SettingType { boolean, text, integer, collection }; 

    /// <summary>
    /// Top-level settings object for Rummage
    /// </summary>
    public class Settings
    {
        public string SettingsFile = string.Empty;

        public List<Setting> settings;

        public Settings()
        {
            settings = new List<Setting>();
        }

        /// <summary>
        /// Add a new setting to the Settings collection
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddSetting(Setting value)
        {
            settings.Add(value);
        }

        /// <summary>
        /// Get the setting with the name given.
        /// </summary>
        /// <param name="name">Returns null if there is no setting with this name</param>
        /// <returns></returns>
        public Setting GetSettingByName(string name)
        {
            foreach (var s in settings)
            {
                if (s.Name == name)
                {
                    return s;
                }
            }

            return null;
        }

        /// <summary>
        /// Save the settings to the file given
        /// </summary>
        /// <param name="file">File to save the settings to. If none is given and these settings have previously been loaded from a file then save back to that one. Otherwise return false</param>
        /// <returns>True if the save was successful, otherwise false</returns>
        public bool SaveSettings(string file = "")
        {
            if (file.Length == 0)
            {
                if (SettingsFile.Length > 0)
                {
                    file = SettingsFile;
                }
                else
                {
                    return false;
                }
            }
            try
            {
                using (StreamWriter sw = new StreamWriter(file))
                {
                    string line = Newtonsoft.Json.JsonConvert.SerializeObject(this);
                    sw.WriteLine(line);

                    sw.Close();
                }

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Load a settings object from the file specified
        /// </summary>
        /// <param name="file"></param>
        /// <returns>The loaded settings object, or an empty one if the file can't be found</returns>
        public static Settings LoadSettings(string file)
        {
            Settings settings = new Settings();

            if (File.Exists(file))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Trim().Length > 0)
                        {
                            try
                            {
                                settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(line);

                            }
                            catch (JsonReaderException)
                            {
                                
                                throw;
                            }
                        }
                    }
                }

                settings.SettingsFile = file;
            }

            return settings;
        }
    }
}
