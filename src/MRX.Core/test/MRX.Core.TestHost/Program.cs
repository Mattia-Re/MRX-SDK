using System.Text.Json;
using MRX.Core.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddMrxModelBindingValidation(options =>
{
    options.ModelBindingOptions = bindingOptions =>
    {
        bindingOptions.DefaultNamingPolicy = JsonNamingPolicy.SnakeCaseUpper;
    };
});

WebApplication app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program
{
}