using System.Windows;
using dotnetCampus.SourcePerformance.Framework;

namespace dotnetCampus.SourcePerformance
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Services.PerformanceCounter.Framework();
            Services.ExtensionManager.LoadExtensions();
            base.OnStartup(e);
        }
    }
}
