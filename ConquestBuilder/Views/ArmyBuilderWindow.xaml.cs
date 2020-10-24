using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;

        public ArmyBuilderWindow(ArmyBuilderViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            _vm = vm;
        }

        private void ArmyBuilderWindow_OnClosed(object sender, EventArgs e)
        {
            _vm.OnWindowClosed?.Invoke(this, EventArgs.Empty);
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper((Window)sender).Handle;
            var value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (int) (value & ~WS_MAXIMIZEBOX));
        }
    }
}
