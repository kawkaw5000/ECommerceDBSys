using EcommerceShop.DAL;
using EcommerceShop.Repository;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;

namespace EcommerceShop.Models.Home
{
    public class HomeIndexViewModel
    {
        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();

        dbMyOnlineShoppingEntities context = new dbMyOnlineShoppingEntities();

        public List<Tbl_Cart> CartItems { get; internal set; }
        public IPagedList<Tbl_Product> ListOfProducts { get; set; }

        public HomeIndexViewModel CreateModel(string search, int pageSize, int? page)
        {
            SqlParameter[] param = new SqlParameter[]
            {
        new SqlParameter("@search", search ?? (object)DBNull.Value)
            };

            IEnumerable<Tbl_Product> data = context.Database.SqlQuery<Tbl_Product>("GetBySearch @search", param);

            IPagedList<Tbl_Product> pagedData = data.ToPagedList(page ?? 1, pageSize);

            return new HomeIndexViewModel
            {
                ListOfProducts = pagedData
            };
        }


        //public HomeIndexViewModel CreateModel(string search,int pageSize, int? page)
        //{
        //    SqlParameter[] param = new SqlParameter[]
        //    {
        //        new SqlParameter("@search", search??(object)DBNull.Value)
        //    };
        //    IPagedList<Tbl_Product> data = context.Database.SqlQuery<Tbl_Product>("GetBySearch @search", param).ToList().ToPagedList(page ?? 1, pageSize);
        //    return new HomeIndexViewModel
        //    {
        //        ListOfProducts = data
        //    }; 
        //}
    }
}