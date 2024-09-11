using BeadandoSajatWPF.ViewModel;
using BombazoSajat.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BeadandoSajatWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow _view = null!;
        private BombazoViewModel? _viewModel;

        public App()
        {
            this.Startup += OnStartup;
        }

        private void OnStartup(object o, StartupEventArgs e)
        {
            _viewModel = new BombazoViewModel(new BombazoGameModel());

            _view = new MainWindow();
            _view.DataContext = _viewModel;
            _view.Show();
            _view.KeyDown += _viewModel.KeyDownFunction;
        }
    }
}
