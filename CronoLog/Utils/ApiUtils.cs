namespace CronoLog.Utils
{
    public static class ApiUtils
    {
#if DEBUG
        public const string API_URL = "https://localhost:5001";
#else
        // public const string API_URL = "https://trellotemporizador.herokuapp.com";
        public const string API_URL = "https://trello-timer.herokuapp.com";
#endif
    }
}
