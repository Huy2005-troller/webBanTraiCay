using Fruitables.Models;
using Microsoft.AspNetCore.Http;

namespace Fruitables.Services.Interfaces;

public interface IVNPayService
{
    string CreatePaymentUrl(Order order, HttpContext context);
    bool PaymentExecute(IQueryCollection collections, out int orderId);
}
