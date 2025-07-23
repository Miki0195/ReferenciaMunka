using ELTE.TravelAgency.Admin.Blazor;
using ELTE.TravelAgency.Admin.Model;
using ELTE.TravelAgency.Admin.Persistence;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["BaseAddress"] ?? builder.HostEnvironment.BaseAddress),
});
builder.Services.AddScoped<ITravelAgencyPersistence, TravelAgencyServicePersistence>();
builder.Services.AddScoped<ITravelAgencyModel, TravelAgencyModel>();

await builder.Build().RunAsync();
