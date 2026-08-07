using System.Net.Http.Json;
using OrderFlow.Web.Models;

namespace OrderFlow.Web.Services;
public sealed class ProductCatalogClient(HttpClient http)
{
    public async Task<IReadOnlyList<ProductDto>> GetAsync(CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<List<ProductDto>>("api/products", cancellationToken) ?? [];
}
