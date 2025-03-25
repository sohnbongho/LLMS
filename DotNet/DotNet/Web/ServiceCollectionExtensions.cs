using DotNetLLM.Repo;
using DotNetLLM.Service;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetLLM.Web;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddControllerServices(this IServiceCollection services)
    {
        services.AddScoped<ChatService>();
        services.AddScoped<IChatRepo, ChatRepo>();

        services.AddHttpClient(); // ✅ IHttpClientFactory 사용하기 위해

        return services;
    }
}
