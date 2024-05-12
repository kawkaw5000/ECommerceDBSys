using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EcommerceShop.Repository;
using EcommerceShop.DAL;

namespace EcommerceShop.Controllers
{

    public class BaseController : Controller
    {
        public String ErrorMessage;
        public UserManager _userManager;
        public dbMyOnlineShoppingEntities _db;
        public BaseRepository<Tbl_Members> _userRepo;
       

        public String Username { get { return User.Identity.Name; } }
        public String UserId { get { return _userManager.GetUserByUsername(Username).userId; } }
        public BaseController()
        {
            _db = new dbMyOnlineShoppingEntities();
            _userRepo = new BaseRepository<Tbl_Members>();
            _userManager = new UserManager();
        }     
    }
}