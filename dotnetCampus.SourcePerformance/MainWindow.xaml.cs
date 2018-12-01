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
            var elapsed = _counter.Elapsed;
            StartupTimeRun.Text = elapsed.ToString();
        }

        private readonly PerformanceCounter _counter;
    }
}