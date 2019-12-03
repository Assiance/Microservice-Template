using System;
using System.Collections.Generic;
using System.Text;
using Omni.BuildingBlocks.Shared;

namespace EfMicroservice.Application.Orders.Queries
{
    public class OrderModel : IVersionInfoModel
    {
        public int Id { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
        public byte[] RowVersion { get; set; }
    }
}