using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Reflection;

namespace DotNet;

#pragma warning disable CS8602, CS8604 // null 가능 참조에 대한 역참조입니다.			
public class Program
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var port = 6001;
        var usedHttps = false;

        return Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>()
            .UseKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB로 설정

                if (usedHttps)
                {
                    // HTTPS 엔드포인트 추가
                    options.Listen(IPAddress.Any, port, listenOptions =>
                    {
                        listenOptions.UseHttps("certificate.pfx", "1111");
                    });
                }
                else
                {
                    // HTTP 엔드포인트에서 리스닝
                    options.Listen(IPAddress.Any, port);
                }


            })
            .UseUrls($"https://0.0.0.0:{port}"); // HTTPS URL로 변경
        });
    }
    static async Task Main(string[] args)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
        {
            var logRepo = LogManager.GetRepository(entryAssembly);
            XmlConfigurator.Configure(logRepo, new FileInfo("log4net.config"));


            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }
        else
        {
            _logger.Error("Error: Entry assembly is null. Logging is not configured.");
        }
    }
}
#pragma warning restore CS8602, CS8604 // null 가능 참조에 대한 역참조입니다.