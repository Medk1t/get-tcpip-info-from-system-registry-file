namespace GetCompInfo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string hiveFilePath = string.Empty;
            if (args.Length == 1)
            {
                hiveFilePath = args[0];
            }
            else
            {
                hiveFilePath = @"C:\Windows\System32\config\SYSTEM";
            }
            RegistryLoader.LoadAndReadSystemHive(hiveFilePath);
        }
    }
}
