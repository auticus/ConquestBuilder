using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ConquestBuilder.ViewModels;

namespace ConquestBuilder.Views
{
    /// <summary>
    /// Interaction logic for ArmyBuilderWindow.xaml
    /// </summary>
    public partial class ArmyBuilderWindow : Window, IView
    {
        private readonly ArmyBuilderViewModel _vm;

        public ArmyBuilderWindow(ArmyBuilderViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            _vm = vm;
        }

        private void ArmyBuilderWindow_OnClosed(object? sender, EventArgs e)
        {
            _vm.OnWindowClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}
