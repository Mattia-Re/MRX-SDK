using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using MRX.Core.Common.ModelBinding;
using MRX.UI.ErrorsAutoWiring.Http.Interop;

namespace MRX.UI.ErrorsAutoWiring.Http;

/// <summary>
/// An HTTP client that sends requests and maps any RFC 7807 problem details error response onto the validation
/// messages of an <see cref="EditContext"/>.
/// </summary>
/// <param name="httpClient">The underlying HTTP client used to send requests.</param>
/// <param name="options">The options controlling how errors are mapped to fields.</param>
public class ProblemDetailsHttpClient(HttpClient httpClient, IOptions<ProblemDetailsHttpClientOptions> options)
{
    private readonly ProblemDetailsHttpClientOptions _options = options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Sends an HTTP request and, if the response is a problem details error, maps its errors onto
    /// <paramref name="editContext"/>'s validation message store.
    /// </summary>
    /// <param name="editContext">The edit context whose model must be a <see cref="DataBindingSources"/> instance.</param>
    /// <param name="request">The HTTP request to send.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The HTTP response returned by the underlying client.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="editContext"/>'s model is not a <see cref="DataBindingSources"/>.</exception>
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
        catch (HttpCodedErrorsProblemDetailsException problem)
        {
            ValidationMessageStore msgStore = new(editContext);

            // Add root ($) errors if any
            if (problem.Errors.TryGetValue("$", out CodedError[]? errors))
            {
                foreach (CodedError err in errors)
                {
                    msgStore.Add(FieldIdentifier.Create(() => sources.RootError), err.Description);
                }
            }

            // Map the error keys to field identifiers and then add their errors
            Dictionary<FieldIdentifier, List<CodedError>> errorMap =
                _options.ErrorMappingFactory(editContext, problem.Errors, res);
            foreach ((FieldIdentifier fi, List<CodedError> fieldErrors) in errorMap)
            {
                msgStore.Add(fi, fieldErrors.Select(e => e.Description));
            }

            return res;
        }
        catch (HttpProblemDetailsException problem)
        {
            ValidationMessageStore msgStore = new(editContext);

            // If the HTTP response provided no MRX coded errors, the best we can do is display the Detail member
            // or a generic root error
            string rootErr = sources.RootError = problem.Detail ?? "An unexpected error occurred.";
            msgStore.Add(
                FieldIdentifier.Create(() => sources.RootError),
                rootErr);

            return res;
        }
        finally
        {
            editContext.NotifyValidationStateChanged();
        }
    }

    /// <summary>
    /// Ensures <paramref name="response"/> was successful, throwing an <see cref="HttpProblemDetailsException"/> (or the more
    /// specific <see cref="HttpCodedErrorsProblemDetailsException"/>) built from the response body otherwise.
    /// </summary>
    /// <param name="response">The HTTP response to check.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <exception cref="HttpProblemDetailsException">Thrown when the response is not successful.</exception>
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