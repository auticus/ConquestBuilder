using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ConquestBuilder.Models;
using ConquestBuilder.ViewModels;

namespace ConquestBuilder.Views
{
    /// <summary>
    /// Interaction logic for FactionPickerWindow.xaml
    /// </summary>
    public partial class FactionPickerWindow : Window, IView
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        private FactionPickerViewModel _vm;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public FactionPickerWindow()
        {
            InitializeComponent();
        }

        public FactionPickerWindow(FactionPickerViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            _vm = vm;
        }

        private void FactionPickerWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU); //get rid of the X in the upper right corner
        }

        private void Carousel_OnSelectionClicked(FactionCarouselCard selectedcard)
        {
            _vm.FinalizeSelection(this);
        }
    }
}
