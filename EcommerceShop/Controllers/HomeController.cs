using EcommerceShop.DAL;
using EcommerceShop.Models.Home;
using EcommerceShop.Repository;
using EcommerceShop.Utils;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EcommerceShop.Controllers
{
    public class HomeController : BaseController
    {
        dbMyOnlineShoppingEntities ctx = new dbMyOnlineShoppingEntities();
        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();

        [AllowAnonymous]
        public ActionResult Index(string search, int? page)
        {
            var products = _unitOfWork.GetRepositoryInstance<Tbl_Product>().GetProduct().Where(p => !(p.IsDelete ?? false));


         
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search));
            }

         
            int pageSize = 4; 
            int pageNumber = (page ?? 1);
            var paginatedProducts = products.ToPagedList(pageNumber, pageSize);

       
            var viewModel = new HomeIndexViewModel
            {
                ListOfProducts = paginatedProducts,
              
            };

            ViewBag.CartItemCount = GetCartItemCount();
            ViewBag.TestMessage = $"Cart item count: {ViewBag.CartItemCount}";

            return View(viewModel);
        }


        [Authorize(Roles = "User, Manager")]
        public ActionResult UserIndex(string search, int? page)
        {

            var products = _unitOfWork.GetRepositoryInstance<Tbl_Product>().GetProduct().Where(p => !(p.IsDelete ?? false));


         
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search));
            }

          
            int pageSize = 4; 
            int pageNumber = (page ?? 1);
            var paginatedProducts = products.ToPagedList(pageNumber, pageSize);

        
            var viewModel = new HomeIndexViewModel
            {
                ListOfProducts = paginatedProducts,
              
            };

            ViewBag.CartItemCount = GetCartItemCount();
            ViewBag.TestMessage = $"Cart item count: {ViewBag.CartItemCount}";

            return View(viewModel);
        }

        private int GetCartItemCount()
        {
            string loggedInUserEmail = User.Identity.Name;
            var user = _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetAllRecords()
                           .FirstOrDefault(m => m.Username == loggedInUserEmail);

            if (user != null)
            {
                var cartItemCount = _unitOfWork.GetRepositoryInstance<Tbl_Cart>()
                                        .GetAllRecords()
                                        .Count(c => c.MemberId == user.id);
                return cartItemCount;
            }
            return 0;
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("UserIndex");
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Login(Tbl_Members u)
        {
            var user = _userRepo.Table.FirstOrDefault(m => m.Username == u.Username && m.Password == u.Password);
            if (user != null)
            {
                if (user.IsDelete == true)
                {
                    ModelState.AddModelError("", "Your account has set to isInactive wait for Admin Aproval.");
                    return View();
                }

                if (!user.IsActive == true)
                {
                    ModelState.AddModelError("", "Your account is not active. Please contact support.");
                    return View();
                }

                FormsAuthentication.SetAuthCookie(u.Username, false);
                return RedirectToAction("UserIndex");
            }
            ModelState.AddModelError("", "Username does not Exist or Incorrect Password");

            return View();
        }
        [Authorize(Roles = "User, Manager")]
        public ActionResult ViewCart()
        {
            string loggedInUserEmail = User.Identity.Name;
            var user = _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetAllRecords()
                                .FirstOrDefault(m => m.Username == loggedInUserEmail);

            if (user != null)
            {
                var cartItems = _unitOfWork.GetRepositoryInstance<Tbl_Cart>()
                                    .GetAllRecords()
                                    .Where(c => c.MemberId == user.id)
                                    .ToList();

                return View(cartItems);
            }

            ModelState.AddModelError("", "User not found or not authenticated.");
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "User, Manager")]
        [HttpPost]
        public ActionResult AddToCart(int productId)
        {
            string loggedInUserEmail = User.Identity.Name;
            var user = _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetAllRecords()
                            .FirstOrDefault(m => m.Username == loggedInUserEmail);

            if (user != null)
            {
                var existingCartItems = _unitOfWork.GetRepositoryInstance<Tbl_Cart>()
                                            .GetAllRecords()
                                            .Where(c => c.MemberId == user.id)
                                            .ToList();

                var existingCartItem = existingCartItems.FirstOrDefault(c => c.ProductId == productId);
           
                if (existingCartItem != null)
                {
                    existingCartItem.Quantity++;
                    _unitOfWork.GetRepositoryInstance<Tbl_Cart>().Update(existingCartItem);
                }
                else
                {
                    _unitOfWork.GetRepositoryInstance<Tbl_Cart>().Add(new Tbl_Cart
                    {
                        ProductId = productId,
                        MemberId = user.id,
                        CartStatusId = 1,
                        Quantity = 1
                        
                    });
                }

                _unitOfWork.SaveChanges();

        
                int cartItemCount = existingCartItems.Sum(c => c.Quantity ?? 0);

           
                ViewBag.CartItemCount = cartItemCount;

                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "User not found or not authenticated.");
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "User, Manager")]
        [HttpPost]
        public ActionResult IncreaseProductQuant(int productId)
        {
            string loggedInUserEmail = User.Identity.Name;
            var user = _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetAllRecords()
                            .FirstOrDefault(m => m.Username == loggedInUserEmail);

            if (user != null)
            {
                var existingCartItem = _unitOfWork.GetRepositoryInstance<Tbl_Cart>()
                                        .GetAllRecords()
                                        .FirstOrDefault(c => c.MemberId == user.id && c.ProductId == productId);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity++;
                    _unitOfWork.GetRepositoryInstance<Tbl_Cart>().Update(existingCartItem);
                    _unitOfWork.SaveChanges();
                }
                return RedirectToAction("ViewCart");
            }

            ModelState.AddModelError("", "User not found or not authenticated.");
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "User, Manager")]
        [HttpPost]
        public ActionResult DecreaseProductQuant(int productId)
        {
            string loggedInUserEmail = User.Identity.Name;
            var user = _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetAllRecords()
                            .FirstOrDefault(m => m.Username == loggedInUserEmail);

            if (user != null)
            {
                var existingCartItem = _unitOfWork.GetRepositoryInstance<Tbl_Cart>()
                                        .GetAllRecords()
                                        .FirstOrDefault(c => c.MemberId == user.id && c.ProductId == productId);

                if (existingCartItem != null)
                {
                    if (existingCartItem.Quantity > 1)
                    {
                        existingCartItem.Quantity--;
                        _unitOfWork.GetRepositoryInstance<Tbl_Cart>().Update(existingCartItem);
                        _unitOfWork.SaveChanges();
                    }
                    else
                    {
                        // If quantity is already 1, remove the item from the cart
                        _unitOfWork.GetRepositoryInstance<Tbl_Cart>().Remove(existingCartItem);
                        _unitOfWork.SaveChanges();
                    }
                }
                return RedirectToAction("ViewCart");
            }

            ModelState.AddModelError("", "User not found or not authenticated.");
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "User, Manager")]
        [HttpPost]
        public ActionResult RemoveProduct(int productId)
        {
            string loggedInUserEmail = User.Identity.Name;
            var user = _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetAllRecords()
                            .FirstOrDefault(m => m.Username == loggedInUserEmail);

            if (user != null)
            {
                var existingCartItem = _unitOfWork.GetRepositoryInstance<Tbl_Cart>()
                                        .GetAllRecords()
                                        .FirstOrDefault(c => c.MemberId == user.id && c.ProductId == productId);

                if (existingCartItem != null)
                {
                    _unitOfWork.GetRepositoryInstance<Tbl_Cart>().Remove(existingCartItem);
                    _unitOfWork.SaveChanges();
                }
                return RedirectToAction("ViewCart");
            }

            ModelState.AddModelError("", "User not found or not authenticated.");
            return RedirectToAction("Index");
        }



        public List<SelectListItem> GetMembers(string loggedInUserId)
        {
            List<SelectListItem> list = new List<SelectListItem>();


            var mem = _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetAllRecords()
                      .Where(m => m.Username == loggedInUserId);

            foreach (var item in mem)
            {
                list.Add(new SelectListItem { Value = item.id.ToString(), Text = item.Username });
            }

            return list;
        }

        public ActionResult AccountInfo()
        {
            return View(_unitOfWork.GetRepositoryInstance<Tbl_Members>().GetMembers());
        }
        public ActionResult AccountEdit(int memberId)
        {
            string loggedInUserId = User.Identity.Name;

            Tbl_Members member = _unitOfWork.GetRepositoryInstance<Tbl_Members>()
                                  .GetAllRecords()
                                  .FirstOrDefault(m => m.Username == loggedInUserId && m.id == memberId);

            if (member == null)
            {
                return RedirectToAction("AccessDenied");
            }

            ViewBag.MembersList = GetMembers(loggedInUserId);
            return View(member);
        }

        [HttpPost]
        public ActionResult AccountEdit(Tbl_Members tbl)
        {

            tbl.ModifiedOn = DateTime.Now;
            _unitOfWork.GetRepositoryInstance<Tbl_Members>().Update(tbl);
            ViewBag.MembersList = GetMembers(User.Identity.Name);
            return RedirectToAction("UserIndex");
        }




        public List<SelectListItem> GetMembersInfo()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var mem = _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetAllRecords();
            foreach (var item in mem)
            {
                list.Add(new SelectListItem { Value = item.id.ToString(), Text = item.Username });
            }
            return list;
        }

        [AllowAnonymous]
        public ActionResult SignUp()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("UserIndex");

            ViewBag.Roles = new SelectList(ctx.Tbl_Roles, "id", "RoleName");

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult SignUp(Tbl_Members ua, String ConfirmPass, int roleId)
        {
            if (!ua.Password.Equals(ConfirmPass))
            {
                ModelState.AddModelError(String.Empty, "Password not match");
                ViewBag.Roles = new SelectList(ctx.Tbl_Roles, "id", "RoleName");
                return View(ua);
            }

        
            ua.roleId = roleId;

            if (_userManager.SignUp(ua, ref ErrorMessage) != Contracts.ErrorCode.Success)
            {
                ModelState.AddModelError(String.Empty, ErrorMessage);
                ViewBag.Roles = new SelectList(ctx.Tbl_Roles, "id", "RoleName");
                return View(ua);
            }

            int memberId = ua.id;      
            TempData["username"] = ua.Username;
            return RedirectToAction("AddUserInfo", new { memberId = memberId }); ;
        }

        public ActionResult MemberInfo()
        {
            return View(_unitOfWork.GetRepositoryInstance<Tbl_MemberInfo>().GetMemberInfo());
        }
        public ActionResult AddUserInfo(int memberId)
        {
            ViewBag.MemberList = GetMembersInfo();
       
            ViewBag.MemberId = memberId;
  
            return View();
        }
        [HttpPost]
        public ActionResult AddUserInfo(Tbl_MemberInfo tbl, HttpPostedFileBase file)
        {

            int memberId = Convert.ToInt32(Request.Form["MemberId"]);
            tbl.MemberId = memberId;
            string pic = null;
            if (file != null)
            {
                pic = System.IO.Path.GetFileName(file.FileName);
                string path = System.IO.Path.Combine(Server.MapPath("~/UserImg/"), pic);
                file.SaveAs(path);
            }
            tbl.UserImage = pic;
            _unitOfWork.GetRepositoryInstance<Tbl_MemberInfo>().Add(tbl);
            return RedirectToAction("Index");
        }

        public ActionResult ViewProductDetails(int productId)
        {
       
            var product = _unitOfWork.GetRepositoryInstance<Tbl_Product>().GetFirstorDefault(productId);

       
            Tbl_Store seller = null;
            Tbl_Brand brand = null;
          
            if (product.StoreId != null)
            {
                seller = _unitOfWork.GetRepositoryInstance<Tbl_Store>().GetFirstorDefault(product.StoreId.Value);
            }

            if (product.BrandId != null)
            {
                brand = _unitOfWork.GetRepositoryInstance<Tbl_Brand>().GetFirstorDefault(product.BrandId.Value);
            }
      
            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                Seller = seller,
                Brand = brand
            };
    
            return View(viewModel);
        }

        [Authorize(Roles = "User, Manager")]
        public ActionResult Checkout()
        {
            string loggedInUserEmail = User.Identity.Name;
            var user = _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetAllRecords()
                                .FirstOrDefault(m => m.Username == loggedInUserEmail);

            if (user != null)
            {
                var cartItems = _unitOfWork.GetRepositoryInstance<Tbl_Cart>()
                                    .GetAllRecords()
                                    .Where(c => c.MemberId == user.id)
                                    .ToList();

                foreach (var cartItem in cartItems)
                {
                    var product = _unitOfWork.GetRepositoryInstance<Tbl_Product>().GetFirstorDefault(cartItem.ProductId ?? 0);
                    var storeId = product.StoreId; 

                
                    var transaction = new Tbl_Transaction
                    {
                        MemberId = user.id,
                        StoreId = storeId,
                        ProductId = product.ProductId,
                        Quantity = cartItem.Quantity,
                        TotalAmount = product.Price * cartItem.Quantity, 
                        TimeStamp = DateTime.Now
                    };

                    _unitOfWork.GetRepositoryInstance<Tbl_Transaction>().Add(transaction);
                }

         
                foreach (var cartItem in cartItems)
                {
                    _unitOfWork.GetRepositoryInstance<Tbl_Cart>().Remove(cartItem);
                }
                _unitOfWork.SaveChanges();

                return RedirectToAction("Index", "Home"); 
            }

            ModelState.AddModelError("", "User not found or not authenticated.");
            return RedirectToAction("Index");
        }


    }
}