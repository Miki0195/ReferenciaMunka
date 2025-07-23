using AutoMapper;

namespace ELTE.Cinema.WebAPI.Infrastructure
{
    /// <summary>
    /// DependencyInjection
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// AddAutomapper
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAutomapper(this IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
            mapperConfig.AssertConfigurationIsValid();

            services.AddAutoMapper(typeof(MappingProfile));

            return services;
        }
    }
}
