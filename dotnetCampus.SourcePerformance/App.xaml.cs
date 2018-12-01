using System.Windows;

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
            var assembly = typeof(Foo).Assembly;


            base.OnStartup(e);
        }
    }
}
