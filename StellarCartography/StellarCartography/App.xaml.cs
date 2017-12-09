using Newtonsoft.Json.Linq;
using StellarCartography.Model;
using StellarCartography.View;
using StellarCartography.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StellarCartography
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private StellarCartographyModel model;

        private MainViewModel mainViewModel;

        private MainWindow mainWindow;

        private Int32 resolutionX, resolutionY, rounds, gameFieldRows, gameFieldColumns, roundTime;

        public App()
        {
            this.ReadConfig();

            this.model = new StellarCartographyModel(this.resolutionX, this.resolutionY, this.rounds, this.gameFieldRows, this.gameFieldColumns);

            this.mainViewModel = new MainViewModel(this.model, this.roundTime);
            this.mainViewModel.Quit += OnMainWindowQuit;

            this.mainWindow = new MainWindow();
            this.mainWindow.DataContext = this.mainViewModel;
        }

        private void ReadConfig()
        {
            string contents = File.ReadAllText(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Resources\config.json");

            JObject obj = JObject.Parse(contents);

            Int32.TryParse(obj["resolutionX"].ToString(), out this.resolutionX);
            Int32.TryParse(obj["resolutionY"].ToString(), out this.resolutionY);
            Int32.TryParse(obj["rounds"].ToString(), out this.rounds);
            Int32.TryParse(obj["gameFieldRows"].ToString(), out this.gameFieldRows);
            Int32.TryParse(obj["gameFieldColumns"].ToString(), out this.gameFieldColumns);
            Int32.TryParse(obj["roundTime"].ToString(), out this.roundTime);
        }

        private void OnMainWindowQuit(object sender, EventArgs e)
        {
            this.mainWindow.Close();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.mainWindow.WindowState = WindowState.Maximized;
            this.mainWindow.WindowStyle = WindowStyle.None;
            this.mainWindow.Show();
        }
    }
}
