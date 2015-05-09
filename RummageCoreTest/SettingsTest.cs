using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RummageCore;

namespace RummageCoreTest
{
    [TestClass]
    public class SettingsTest
    {
        [TestMethod]
        public void Can_create_text_setting()
        {
            var setting = new Setting("TestSetting1", "TestSetting1_Description", "category1", SettingType.text, "test_setting_value");

            Assert.AreEqual(SettingType.text, setting.Type);
            Assert.AreEqual("TestSetting1", setting.Name);
            Assert.AreEqual("TestSetting1_Description", setting.Description);
            Assert.AreEqual("category1", setting.Category);
            Assert.AreEqual("test_setting_value", setting.Value);
        }


        [TestMethod]
        public void Can_create_bool_setting()
        {
            var setting = new Setting("TestSetting2", "TestSetting2_Description", "category2", SettingType.boolean, true);

            Assert.AreEqual(SettingType.boolean, setting.Type);
            Assert.AreEqual("TestSetting2", setting.Name);
            Assert.AreEqual("TestSetting2_Description", setting.Description);
            Assert.AreEqual("category2", setting.Category);
            Assert.AreEqual(true, setting.Value);
        }

        [TestMethod]
        public void Can_create_integer_setting()
        {
            var setting = new Setting("TestSetting3", "TestSetting3_Description", "category3", SettingType.integer, 4456);

            Assert.AreEqual(SettingType.integer, setting.Type);
            Assert.AreEqual("TestSetting3", setting.Name);
            Assert.AreEqual("TestSetting3_Description", setting.Description);
            Assert.AreEqual("category3", setting.Category);
            Assert.AreEqual(4456, setting.Value);
        }

        [TestMethod]
        public void Can_create_collection_setting()
        {
            var collectionInit = new List<string>();
            collectionInit.Add("Entry1");
            collectionInit.Add("Entry2");

            var setting = new Setting("TestSetting4", "TestSetting4_Description", "category4", SettingType.collection, collectionInit);

            Assert.AreEqual(SettingType.collection, setting.Type);
            Assert.AreEqual("TestSetting4", setting.Name);
            Assert.AreEqual("TestSetting4_Description", setting.Description);
            Assert.AreEqual("category4", setting.Category);
            
            List<string> testColl = setting.Value as List<string>;
            Assert.AreEqual(2, testColl.Count);
            Assert.AreEqual("Entry1", testColl[0]);
            Assert.AreEqual("Entry2", testColl[1]);
        }

        [TestMethod]
        public void Can_retrieve_setting_by_name()
        {
            Settings settings = new Settings();

            settings.AddSetting(new Setting("Alpha", "Alpha setting", "Cat1", SettingType.integer, 111));
            settings.AddSetting(new Setting("Beta", "Beta setting", "Cat1", SettingType.integer, 222));

            var retrievedSetting = settings.GetSettingByName("Beta").Value;
            Assert.AreEqual(222, retrievedSetting);
        }


        [TestMethod]
        public void Retrieve_setting_by_name_where_name_doesnt_exist_returns_null()
        {
            Settings settings = new Settings();

            settings.AddSetting(new Setting("Alpha", "Alpha setting", "Cat1", SettingType.integer, 111));
            settings.AddSetting(new Setting("Beta", "Beta setting", "Cat1", SettingType.integer, 222));

            var retrievedSetting = settings.GetSettingByName("Delta");

            Assert.AreEqual(null, retrievedSetting);
        }

        //Bool tests
        [TestMethod]
        public void Can_retrieve_boolean_setting_value()
        {
            var setting = new Setting("Alpha", "A boolean setting", "Ints", SettingType.boolean, true);

            Assert.AreEqual(true, setting.ValueAsBoolean);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Attempt_to_retrieve_non_boolean_setting_as_boolean_throws_exception()
        {
            var setting = new Setting("Beta", "An integer setting", "Testing", SettingType.integer, 77);

            Assert.AreEqual(77, setting.ValueAsBoolean);
        }

        //integer tests
        [TestMethod]
        public void Can_retrieve_integer_setting_value()
        {
            var setting = new Setting("Alpha", "An integer setting", "Ints", SettingType.integer, 555);

            Assert.AreEqual(555, setting.ValueAsInteger);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Attempt_to_retrieve_non_integer_setting_as_integer_throws_exception()
        {
            var setting = new Setting("Beta", "A text setting", "Testing", SettingType.text, "This is a text setting value");

            Assert.AreEqual(555, setting.ValueAsInteger);
        }

        //text tests
        [TestMethod]
        public void Can_retrieve_text_setting_value()
        {
            var setting = new Setting("Alpha", "A text setting", "Tests", SettingType.text, "Fate Amenable To Change");

            Assert.AreEqual("Fate Amenable To Change", setting.ValueAsText);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Attempt_to_retrieve_non_text_setting_as_text_throws_exception()
        {
            var setting = new Setting("Beta", "A bool setting", "Testing", SettingType.boolean, true);

            Assert.AreEqual(false, setting.ValueAsText);
        }

        //collection tests
        [TestMethod]
        public void Can_retrieve_collection_setting_value()
        {

            var collectionInit = new List<string>();
            collectionInit.Add("Entry1");
            collectionInit.Add("Entry2");

            var setting = new Setting("Alpha", "A collection setting", "Tests", SettingType.collection, collectionInit);

            var retrievedColl = setting.ValueAsCollection;
            Assert.AreEqual(2, retrievedColl.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Attempt_to_retrieve_non_collection_setting_as_collection_throws_exception()
        {
            var setting = new Setting("Delta", "A text setting", "Testing", SettingType.text, "Sleeper Service");

            List<string> retrievedColl = setting.ValueAsCollection;
        }

        [TestMethod]
        public void Can_save_some_settings()
        {
            var file = "TestSettings.json";

            save_settings(file);

            Assert.AreEqual(true, File.Exists(file));
        }

        [TestMethod] 
        public void Can_save_and_reload_settings()
        {
            var file = "TestSettings.json";

            save_settings(file);

            var loadedSettings = Settings.LoadSettings(file);

            Assert.AreEqual(3, loadedSettings.settings.Count);

            var secondSetting = loadedSettings.GetSettingByName("Two");
            Assert.IsNotNull(secondSetting);
            Assert.AreEqual(22, secondSetting.ValueAsInteger);
        }

        private void save_settings(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }

            var settings = new Settings();
            var setting1 = new Setting("One", "First setting", "Test", SettingType.text, "Attitude Adjuster");
            var setting2 = new Setting("Two", "Second setting", "Test", SettingType.integer, 22);
            var setting3 = new Setting("Three", "Third setting", "Test", SettingType.collection, new List<string>());
            setting3.ValueAsCollection.Add("Setting String A");
            setting3.ValueAsCollection.Add("Setting String B");

            settings.AddSetting(setting1);
            settings.AddSetting(setting2);
            settings.AddSetting(setting3);

            Setting.AllowSerialize = true;

            settings.SaveSettings(file);
        }

        [TestMethod] 
        public void create_settings()
        {
            string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), "Rummage_Settings.prf");

            if (File.Exists(file))
            {
                File.Delete(file);
            }

            var settings = new Settings();
            var setting1 = new Setting("SubFolders", "Search Sub Folders (recursively)", "System", SettingType.boolean, true);
            var setting2 = new Setting("Binaries", "Search Binary files", "System", SettingType.boolean, false);
            var setting3 = new Setting("CaseSensitive", "Make the search case-sensitive", "System", SettingType.boolean, false);
            var setting4 = new Setting("FolderExclusions", "Names of Folders to ignore when searching", "System", SettingType.collection, new List<string>());
            setting4.ValueAsCollection.Add(".svn");
            setting4.ValueAsCollection.Add("bin");
            setting4.ValueAsCollection.Add("obj");
            var setting5 = new Setting("Editor", "Editor to open files with", "System", SettingType.text, "Notepad.exe");
            var setting6 = new Setting("EditorArgs", "Arguments to pass to the selected editor", "System", SettingType.text, "");
            settings.AddSetting(setting1);
            settings.AddSetting(setting2);
            settings.AddSetting(setting3);
            settings.AddSetting(setting4);
            settings.AddSetting(setting5);
            settings.AddSetting(setting6);

            Setting.AllowSerialize = true;

            settings.SaveSettings(file);

            Assert.AreEqual(6, settings.settings.Count);
        }
    }
}
