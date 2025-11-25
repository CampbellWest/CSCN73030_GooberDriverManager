using System;
using DotNetEnv; 

namespace DriverManagement
{
    public static class Config
    {
        static Config()
        {
            // Load environment variables from .env file at project root
            Env.Load(); 
        }

        public static readonly string SupabaseApiKey = Environment.GetEnvironmentVariable("SUPABASE_API_KEY");
        public static readonly string DriverEndpoint = Environment.GetEnvironmentVariable("DRIVER_ENDPOINT");
        public static readonly string TripEndpoint = Environment.GetEnvironmentVariable("TRIP_ENDPOINT");
    }
}
