using OrderFlow.Mobile.Maui.Services;

namespace OrderFlow.Mobile.Maui;

public partial class MainPage(OrderFlowApiClient api) : ContentPage
{
    private readonly OrderFlowApiClient _api = api;
    public async void OnLoadProductsClicked(object? sender, EventArgs args) => Products.ItemsSource = await _api.GetProductsAsync();
}
