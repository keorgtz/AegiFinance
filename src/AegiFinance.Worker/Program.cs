using AegiFinance.Application;
using AegiFinance.Infrastructure;
using AegiFinance.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<BillingGenerationHostedService>();

var host = builder.Build();
host.Run();
