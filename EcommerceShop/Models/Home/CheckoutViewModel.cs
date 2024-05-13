using EcommerceShop.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EcommerceShop.Models.Home
{
    public class CheckoutViewModel
    {
        public List<Tbl_Cart> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
        public string ErrorMessage { get; set; }
    }
}