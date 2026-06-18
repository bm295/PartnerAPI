using Shipping.Partner.Integration.Api.Endpoints;
using Shipping.Partner.Integration.Application.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddShippingPartnerIntegration(builder.Configuration);

var app = builder.Build();

app.UseShippingPartnerIntegration();
app.Run();
