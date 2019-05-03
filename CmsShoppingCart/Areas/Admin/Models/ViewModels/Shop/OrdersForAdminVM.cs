using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CmsShoppingCart.Areas.Admin.Models.ViewModels.Shop
{
    public class OrdersForAdminVM
    {
        [Display(Name = "Order Number")]
        public int OrderNumber { get; set; }
        public string Username { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, int> ProductsAndQty { get; set; }
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }
        public bool Completed { get; set; }
    }
}