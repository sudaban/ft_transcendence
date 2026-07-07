using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Backend.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // AutoMapper vb. kayıtları buraya eklenebilir.
            
            // Tüm FluentValidation sınıflarını (IValidator<T> türevleri) IoC container'a kaydet.
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
