using Microsoft.AspNetCore.Mvc;
namespace OrderFlow.Admin.Mvc;
public sealed class OrdersController : Controller { public IActionResult Index() => Content("Relatório administrativo de pedidos — requer policy Admin."); }
