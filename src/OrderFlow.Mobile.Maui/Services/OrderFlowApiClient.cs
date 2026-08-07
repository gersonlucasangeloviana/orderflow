using System.Net.Http.Json;
using OrderFlow.Mobile.Maui.Models;

namespace OrderFlow.Mobile.Maui.Services;

public sealed class OrderFlowApiClient(HttpClient http)
{
    // Cliente isolado permite testes e evita que as páginas conheçam detalhes HTTP.
    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<List<ProductDto>>("api/products", cancellationToken) ?? [];
}
