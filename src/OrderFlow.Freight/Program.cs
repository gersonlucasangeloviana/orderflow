using Grpc.Core;
using OrderFlow.Contracts.Freight;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc(); builder.Services.AddHealthChecks();
var app = builder.Build(); app.MapGrpcService<FreightGrpcService>(); app.MapHealthChecks("/health"); app.Run();
public sealed class FreightGrpcService : FreightService.FreightServiceBase
{
    public override Task<FreightQuoteResponse> CalculateQuote(FreightQuoteRequest request, ServerCallContext context)
    {
        var amount = Math.Max(10d, request.TotalWeight * 2d + request.CartValue * .01d);
        return Task.FromResult(new FreightQuoteResponse { Amount = amount, Service = "Standard" });
    }
}
