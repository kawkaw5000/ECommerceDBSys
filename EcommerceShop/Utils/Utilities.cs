using EcommerceShop.DAL;
using EcommerceShop.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EcommerceShop.Utils
{
    public enum ErrorCode
    {
        Success,
        Error
    }

    public enum RoleType
    {
        User,
        Manager
    }

    public class Constant
    {
        public const string Role_Customer = "User";
        public const string Role_Staff = "Manager";

        public const int ERROR = 1;
        public const int SUCCESS = 0;
    }
    public class Utilities
    {
        public static String gUid
        {
            get
            {
                return Guid.NewGuid().ToString();
            }
        }
        public static List<SelectListItem> ListRole
        {
            get
            {
                BaseRepository<Tbl_Roles> role = new BaseRepository<Tbl_Roles>();
                var list = new List<SelectListItem>();
                foreach (var item in role.GetAll())
                {
                    var r = new SelectListItem
                    {
                        Text = item.RoleName,
                        Value = item.id.ToString()
                    };

                    list.Add(r);
                }

                return list;
            }
        }
    }
}