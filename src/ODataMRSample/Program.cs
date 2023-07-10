using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using ODataMRSample.Models;

var builder = WebApplication.CreateBuilder(args);

var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<Asset>("Assets");

builder.Services.AddControllers().AddOData(
    options => options.EnableQueryFeatures().AddRouteComponents(
        model: modelBuilder.GetEdmModel()));

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());

app.Run();
