using EcommerceShop.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EcommerceShop.Models.Home
{
    public class ProductDetailsViewModel
    {
        public Tbl_Product Product { get; set; }
        public Tbl_Store Seller { get; set; }
        public Tbl_Brand Brand { get; set; }
    }
}