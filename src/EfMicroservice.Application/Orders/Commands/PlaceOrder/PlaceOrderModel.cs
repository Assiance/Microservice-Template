using System;
using System.ComponentModel.DataAnnotations;

namespace EfMicroservice.Application.Orders.Commands.PlaceOrder
{
    public class PlaceOrderModel
    {
        [Required]
        public Guid ProductId { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}