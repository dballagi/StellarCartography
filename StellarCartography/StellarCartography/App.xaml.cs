using StellarCartography.Model;
using StellarCartography.View;
using StellarCartography.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

        public App()
        {
            this.model = new StellarCartographyModel();

            this.mainViewModel = new MainViewModel(this.model);
            this.mainViewModel.Quit += OnMainWindowQuit;

            this.mainWindow = new MainWindow();
            this.mainWindow.DataContext = this.mainViewModel;
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
