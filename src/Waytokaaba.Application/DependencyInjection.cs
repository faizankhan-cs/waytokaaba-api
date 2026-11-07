using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Waytokaaba.Application.Auth;
using Waytokaaba.Application.Hotels;
using Waytokaaba.Application.Inquiries;
using Waytokaaba.Application.Packages;

namespace Waytokaaba.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Singleton);
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IHotelService, HotelService>();
        services.AddScoped<IPackageService, UmrahPackageService>();
        services.AddScoped<IInquiryService, InquiryService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
