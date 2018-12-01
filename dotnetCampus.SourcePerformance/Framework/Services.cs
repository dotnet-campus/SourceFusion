namespace dotnetCampus.SourcePerformance.Framework
{
    public static class Services
    {
        public static PerformanceCounter PerformanceCounter { get; } = new PerformanceCounter(false);
    }
}
