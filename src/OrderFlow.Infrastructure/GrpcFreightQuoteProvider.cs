using Grpc.Core;
using OrderFlow.Application;
using OrderFlow.Contracts.Freight;

namespace OrderFlow.Infrastructure;

public sealed class GrpcFreightQuoteProvider(FreightService.FreightServiceClient client) : IFreightQuoteProvider
{
    public async Task<decimal> CalculateAsync(string postalCode, decimal weight, decimal cartValue, CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.CalculateQuoteAsync(new FreightQuoteRequest { PostalCode = postalCode, TotalWeight = (double)weight, CartValue = (double)cartValue }, deadline: DateTime.UtcNow.AddSeconds(3), cancellationToken: cancellationToken);
            return Convert.ToDecimal(response.Amount);
        }
        catch (RpcException exception) when (exception.StatusCode is StatusCode.DeadlineExceeded or StatusCode.Unavailable)
        {
            throw new FreightUnavailableException("Serviço de frete indisponível.", exception);
        }
    }
}

public sealed class FreightUnavailableException(string message, Exception innerException) : Exception(message, innerException);
