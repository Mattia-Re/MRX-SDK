using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using MRX.UI.ErrorsAutoWiring.Http;

namespace MRX.UI.ErrorsAutoWiring.DependencyInjection;

public class ProblemDetailsHttpClientOptionsSetup : IConfigureOptions<ProblemDetailsHttpClientOptions>
{
    public void Configure(ProblemDetailsHttpClientOptions options)
    {
        options.ErrorFactory = (editContext, key, errors) =>
        {
            ValidationMessageStore msgStore = new(editContext);
            msgStore.Add(
                editContext.Field(key), string.Join('\n', errors.Select(e => e.Description)));
        };
    }
}