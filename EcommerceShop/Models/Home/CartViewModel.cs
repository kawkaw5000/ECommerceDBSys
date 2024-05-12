using EcommerceShop.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EcommerceShop.Models.Home
{
    public class CartViewModel
    {
        public List<Tbl_Cart> CartItems { get; set; }
        // You can add more properties here if needed
    }
}