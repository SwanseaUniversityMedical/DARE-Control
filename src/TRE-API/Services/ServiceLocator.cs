namespace TRE_API.Services
{
    public static class ServiceLocator
    {
        public static IServiceProvider Current { get; private set; }

        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            Current = serviceProvider;
        }
    }
}
