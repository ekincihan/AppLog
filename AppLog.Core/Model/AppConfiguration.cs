using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppLog.Core.Model
{
    public class AppConfiguration : SingletonBase<AppConfiguration>
    {
        public IConfiguration Configuration { get;}
        public AppConfiguration()
        {
            string envc = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
               .SetBasePath(System.Environment.CurrentDirectory)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
               
            if (envc != null)
                builder.AddJsonFile($"appsettings.{envc}.json", optional: true);

            Configuration = builder.Build();
        }
    }
}
