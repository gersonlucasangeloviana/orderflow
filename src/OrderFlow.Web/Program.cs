using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<OrderFlow.Web.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
var apiBaseAddress = builder.Configuration["OrderFlowApi"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });
builder.Services.AddScoped<OrderFlow.Web.Services.ProductCatalogClient>();
await builder.Build().RunAsync();
