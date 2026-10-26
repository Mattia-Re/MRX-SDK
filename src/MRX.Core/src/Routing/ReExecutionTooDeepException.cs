namespace MRX.Core.Routing;

/// <summary>
///     The exception thrown when a request has been re-executed more times than allowed, which usually means two
///     rewrites are sending the request back and forth.
/// </summary>
/// <param name="message">The message that describes the error.</param>
public class ReExecutionTooDeepException(string message) : Exception(message);