using Microsoft.AspNetCore.Http;

namespace MRX.Core.Tests.Setup;

public class EndpointCapture
{
    public Endpoint? Last { get; set; }
}