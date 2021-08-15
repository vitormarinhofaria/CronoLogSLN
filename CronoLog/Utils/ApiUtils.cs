namespace CronoLog.Utils
{
    public static class ApiUtils
    {

        public static string API_URL
        {
            get
            {
                var env_addr = System.Environment.GetEnvironmentVariable("API_URL");
                if (env_addr == null)
                {
                    return "https://localhost:5001";
                }
                else
                {
                    return env_addr;
                }
            }
        }
    }
}
