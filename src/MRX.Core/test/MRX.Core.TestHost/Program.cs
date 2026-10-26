using System.Text.Json;
using MRX.Core.DependencyInjection;
using MRX.Core.Routing;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddMrxModelBindingValidation(options =>
{
    options.ModelBindingOptions = bindingOptions =>
    {
        bindingOptions.DefaultNamingPolicy = JsonNamingPolicy.SnakeCaseUpper;
    };
});

builder.Services.AddExecutionPipelineRecording();

WebApplication app = builder.Build();

app.UseHttpsRedirection();

app.UseExecutionPipelineRecording();

app.MapControllers();

app.Run();

public partial class Program
{
}