using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using RummageCore;

namespace Rummage
{
    /// <summary>
    /// Interaction logic for SettingScreen.xaml
    /// </summary>
    public partial class SettingScreen : Window
    {
        private Control currentControl;

        public SettingScreen(Settings settings)
        {
            this.settings = settings;
            InitializeComponent();
            loadScreen();
        }

        public Settings settings { get; set; }

        private void loadScreen()
        {
            this.ShowInTaskbar = false;

            foreach (Setting s in settings.settings)
            {
                Button b = new Button();
                b.Name = s.Name.Replace(' ', '_');
                b.Content = s.Name;
                b.ToolTip = s.Description;
                b.Click += BrowserButton_Click;
                b.Tag = s;
                settingButtons.Children.Add(b);
            }
        }

        /// <summary>
        /// Unload the current control and load the selected setting ready for editing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowserButton_Click(object sender, RoutedEventArgs e)
        {
            unloadCurrentControl();

            var setting = ((Button)sender).Tag as Setting;

            currentControl = createControlFromSetting(setting);
            if (currentControl != null)
            {
                DetailPanel.Children.Add(currentControl);
                SettingLabel.Content = setting.Description;
            }
        }

        /// <summary>
        /// Unload the currently selected setting control and ensure any changes made by the user are moved back into the setting
        /// </summary>
        private void unloadCurrentControl()
        {
            if (currentControl == null) { return; }

            Setting currentSetting = currentControl.Tag as Setting;

            if (currentSetting == null) { return; }
            
            switch(currentSetting.Type)
            {
                case SettingType.text:
                    TextBox textControl = currentControl as TextBox;
                    currentSetting.ValueAsText = textControl.Text;
                    break;

                case SettingType.boolean:
                    CheckBox boolControl = currentControl as CheckBox;
                    currentSetting.ValueAsBoolean = (bool)boolControl.IsChecked;
                    break;

                case SettingType.collection:
                    currentSetting.ValueAsCollection.Clear();
                    TextBox collectionControl = currentControl as TextBox;
                    string[] lines = collectionControl.Text.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
                    foreach(var line in lines)
                    {
                        currentSetting.ValueAsCollection.Add(line);
                    }
                    break;

            }

            DetailPanel.Children.Remove(currentControl);
            currentControl = null;

        }

        /// <summary>
        /// Returns the correct control for the type of setting passed in, initialised with the settings contents
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        private Control createControlFromSetting(Setting setting)
        {
            switch (setting.Type)
            {
                case SettingType.text:
                    TextBox textControl = new TextBox();
                    textControl.Text = setting.ValueAsText;
                    textControl.Tag = setting;
                    textControl.Margin = new Thickness(5, 0, 0, 0);
                    return textControl;

                case SettingType.boolean:
                    CheckBox boolControl = new CheckBox();
                    boolControl.IsChecked = setting.ValueAsBoolean;
                    boolControl.Tag = setting;
                    boolControl.Margin = new Thickness(5, 0, 0, 0);
                    return boolControl;

                case SettingType.integer:
                    //Not currently implemented as there is no need for it at this time
                    return null;

                case SettingType.collection:
                    TextBox collectionControl = new TextBox();
                    collectionControl.TextWrapping = TextWrapping.NoWrap;
                    collectionControl.AcceptsReturn = true;
                    foreach(var item in setting.ValueAsCollection)
                    {
                        collectionControl.Text += item.ToString() + Environment.NewLine;
                    }
                    
                    collectionControl.Tag = setting;
                    collectionControl.Margin = new Thickness(5, 0, 0, 0);
                    return collectionControl;

            }

            return null;
        }

        /// <summary>
        /// Save the settings and close the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            unloadCurrentControl();
            this.settings.SaveSettings();
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Reload the original settings (to discard any modifications) and then close the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            restoreSettingsToUnmodifiedVersion();
            this.Close();
        }

        /// <summary>
        /// Reloads the settings from scratch, effectively throwing away any changes the user has made
        /// </summary>
        private void restoreSettingsToUnmodifiedVersion()
        {
            this.settings = Settings.LoadSettings(this.settings.SettingsFile);
            this.DialogResult = false;
        }

        /// <summary>
        /// Make the close button act as a cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            restoreSettingsToUnmodifiedVersion();
        }
    }
}
