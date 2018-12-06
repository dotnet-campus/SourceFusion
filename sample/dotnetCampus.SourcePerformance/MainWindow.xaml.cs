using System;
using System.Windows;
using dotnetCampus.SourcePerformance.Framework;

namespace dotnetCampus.SourcePerformance
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            _counter = Services.PerformanceCounter;
            InitializeComponent();
            ContentRendered += OnContentRendered;
        }

        private void OnContentRendered(object sender, EventArgs e)
        {
            _counter.Complete();
            FrameworkRun.Text = _counter.FrameworkLoaded.ToString();
            ExtensionRun.Text = (_counter.ExtensionFound - _counter.FrameworkLoaded).ToString();
            CompletedRun.Text = (_counter.Completed - _counter.ExtensionFound).ToString();
        }

        private readonly PerformanceCounter _counter;
    }
}