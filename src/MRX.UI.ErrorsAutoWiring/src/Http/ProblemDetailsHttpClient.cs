using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using MRX.Core.ModelBinding;
using MRX.UI.ErrorsAutoWiring.Http.Interop;

namespace MRX.UI.ErrorsAutoWiring.Http;

public class ProblemDetailsHttpClient(HttpClient httpClient, IOptions<ProblemDetailsHttpClientOptions> options)
{
    private readonly ProblemDetailsHttpClientOptions _options = options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<HttpResponseMessage> SendContextAwareAsync(EditContext editContext, HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (editContext.Model is not DataBindingSources sources)
            throw new InvalidOperationException("The attached EditContext model is not a binding sources object");

        HttpResponseMessage res = await httpClient.SendAsync(request, cancellationToken);

        try
        {
            await HandleProblemAsync(res, cancellationToken);
            return res;
        }
        catch (HttpProblemDetailsException ex)
        {
            ValidationMessageStore msgStore = new(editContext);

            // If the HTTP response provided no MRX coded errors, the best we can do is display the Detail member
            // or a generic root error
            if (ex is not HttpCodedErrorsProblemDetailsException problem)
            {
                string rootErr = sources.RootError = ex.Detail ?? "An unexpected error occurred.";
                msgStore.Add(
                    FieldIdentifier.Create(() => sources.RootError),
                    rootErr);
                return res;
            }

            foreach (KeyValuePair<string, CodedError[]> err in problem.Errors)
            {
                _options.ErrorFactory(editContext, err.Key, err.Value);
            }

            return res;
        }
    }

    private static async Task HandleProblemAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException)
        {
            // Deserialize the minimal version of the problem details object. Saving all extensions as generic
            // JsonElement dictionary
            Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            HttpProblemDetailsException ex = (await JsonSerializer.DeserializeAsync<HttpProblemDetailsException>(
                contentStream,
                JsonOptions,
                cancellationToken))!;

            try
            {
                // If the error response included an "errors" extension that is compatible with MRX coded errors
                // upgrade the generic ex to attach the errors.
                if (ex.TryGetExtension("errors", out Dictionary<string, CodedError[]>? errors))
                {
                    ex = new HttpCodedErrorsProblemDetailsException
                    {
                        Type = ex.Type,
                        Status = ex.Status,
                        Title = ex.Title,
                        Detail = ex.Detail,
                        Instance = ex.Instance,
                        Errors = errors,
                        Extensions = ex.Extensions
                    };
                }
            }
            catch (JsonException)
            {
                // It is possible that the response includes an "errors" extension which does not contain
                // an MRX coded errors dictionary, in this case we just skip attaching errors.
            }

            throw ex;
        }
    }
}