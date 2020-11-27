using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ConquestBuilder.ViewModels;

namespace ConquestBuilder.Views
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLongPtr(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        #region Constructors
        public InputBox(Window owner, InputBoxViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            this.Owner = owner;
        }
        #endregion Constructors

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper((Window)sender).Handle;
            var value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLongPtr(hwnd, GWL_STYLE, (int)(value & ~WS_SYSMENU));
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}