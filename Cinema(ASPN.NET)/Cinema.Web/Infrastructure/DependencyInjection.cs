using AutoMapper;

namespace ELTE.Cinema.Web.Infrastructure
{
    public static class WebDependencyInjection
    {
        public static IServiceCollection AddWebAutomapper(this IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile(new WebMappingProfile()));
            mapperConfig.AssertConfigurationIsValid();

            services.AddAutoMapper(typeof(WebMappingProfile));

            return services;
        }
    }
}
