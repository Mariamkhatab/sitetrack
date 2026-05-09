using System.Windows;

namespace SnaggingTracker.Views
{
    /// <summary>
    /// Code-behind for MainView.
    /// Per strict MVVM: this file contains ONLY the constructor with InitializeComponent().
    /// All logic, commands, and state live in MainViewModel.
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }
    }
}
