using EcommerceShop.DAL;
using EcommerceShop.Models.Home;
using EcommerceShop.Repository;
using EcommerceShop.Utils;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EcommerceShop.Controllers
{
    public class HomeController : BaseController
    {
        dbMyOnlineShoppingEntities ctx = new dbMyOnlineShoppingEntities();
        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();
        private readonly MailManager _mailManager;

        public HomeController()
        {
         
            _mailManager = new MailManager();
        }

        public List<SelectListItem> GetRoles()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var roles = _unitOfWork.GetRepositoryInstance<Tbl_Roles>().GetAllRecords();

            foreach (var role in roles)
            {
                list.Add(new SelectListItem { Value = role.id.ToString(), Text = role.RoleName });
            }

            return list;
        }

        private IEnumerable<SelectListItem> GetRolesAsSelectList()
        {
            List<SelectListItem> roleList = new List<SelectListItem>();

            // Retrieve roles from the database using Entity Framework
            using (var dbContext = new dbMyOnlineShoppingEntities()) // Replace YourDbContext with your actual DbContext class
            {
                var roles = dbContext.Tbl_Roles.ToList(); // Assuming Tbl_Roles is a DbSet in your DbContext representing roles

                // Convert roles to SelectListItem objects
                roleList = roles.Select(role => new SelectListItem
                {
                    Text = role.RoleName, // Assuming RoleName is the property representing the role name
                    Value = role.id.ToString() // Assuming id is the property representing the role ID
                }).ToList();
            }

            return roleList;
        }




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
        [Authorize(Roles = "User")]
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

        [Authorize(Roles = "User")]
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

        [Authorize(Roles = "User")]
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

        [Authorize(Roles = "User")]
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
                        _unitOfWork.GetRepositoryInstance<Tbl_Cart>().Remove(existingCartItem);
                        _unitOfWork.SaveChanges();
                    }
                }
                return RedirectToAction("ViewCart");
            }

            ModelState.AddModelError("", "User not found or not authenticated.");
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "User")]
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


        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(Tbl_Members u, string SelectedRole, string otp)
        {
            var storedOTP = Session["GeneratedOTP"]?.ToString();
            u.IsActive = true;
            u.IsDelete = false;
            u.CreatedOn = DateTime.Now;

            if (string.IsNullOrEmpty(storedOTP))
            {
                ModelState.AddModelError("", "OTP expired or not found. Please try signing up again.");
                return View(u);
            }

            if (otp != storedOTP)
            {
                ModelState.AddModelError("", "Incorrect OTP. Please try again.");
                return View(u);
            }

            // OTP is correct, proceed with user creation
            var existingUser = _userRepo.Table.FirstOrDefault(m => m.Username == u.Username);

            if (existingUser != null)
            {
                TempData["ErrorMsg"] = "Username already exists. Please choose a different username.";
                return RedirectToAction("SignUp");
            }

            // Proceed with user creation if the username is unique
            if (string.IsNullOrEmpty(SelectedRole))
            {
                ModelState.AddModelError("", "Role not selected.");
                return View(u);
            }

            var role = _db.Tbl_Roles.FirstOrDefault(r => r.RoleName == SelectedRole);

            if (role == null)
            {
                ModelState.AddModelError("", "Invalid role selected.");
                return View(u);
            }

            u.roleId = role.id; // Assign the roleId to the user

            _userRepo.Create(u);

            var userAdded = _userRepo.Table.FirstOrDefault(m => m.Username == u.Username);

            if (userAdded == null)
            {
                ModelState.AddModelError("", "Failed to create user.");
                return View(u);
            }

            var userRole = new Tbl_UserRole
            {
                MemberId = userAdded.id,
                RoleId = role.id
            };

            _userRole.Create(userRole);

            Session.Remove("GeneratedOTP");

            TempData["SuccessMsg"] = $"User {u.Username} added!";
            return RedirectToAction("SuccessRegister"); // Redirect to SuccessRegister action
        }

        public ActionResult SuccessRegister()
        {
            // Check if SuccessMsg exists in TempData
            if (TempData["SuccessMsg"] == null)
            {
                // If SuccessMsg doesn't exist, redirect to an error page or another appropriate action
                return RedirectToAction("Error");
            }
            else
            {
                // If SuccessMsg exists, render the SuccessRegister view
                return View();
            }
        }


        [HttpPost]
        public ActionResult GenerateOTP(string email)
        {

            string generatedOTP = "";
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[6];
                rng.GetBytes(tokenData);
                generatedOTP = string.Join("", tokenData.Select(b => (b % 10).ToString()));
            }


            Session["GeneratedOTP"] = generatedOTP;


            string errResponse = "";
            bool emailSent = _mailManager.SendEmail(email, "Your OTP", $"Your sign up OTP is: {generatedOTP}", generatedOTP, ref errResponse);


            if (emailSent)
            {
                return Json(new { success = true, message = "OTP sent successfully!" });
            }
            else
            {
                return Json(new { success = false, message = $"Failed to send OTP: {errResponse}" });
            }
        }


        //[AllowAnonymous]
        //[HttpPost]
        //public ActionResult SignUp(Tbl_Members ua, String ConfirmPass, int roleId, string otp)
        //{
        //    var storedOTP = Session["GeneratedOTP"]?.ToString();

        //    if (string.IsNullOrEmpty(storedOTP) || otp != storedOTP)
        //    {
        //        ModelState.AddModelError("", "Incorrect OTP. Please try again."); // Add model error
        //        ViewBag.Roles = new SelectList(ctx.Tbl_Roles, "id", "RoleName");
        //        return View(ua); // Return the view with the error message
        //    }

        //    if (string.IsNullOrEmpty(storedOTP) || otp != storedOTP)
        //    {
        //        TempData["error"] = "Incorrect OTP"; // Set error message in TempData
        //        return View(ua); // Return the view with the error message
        //    }

        //    ua.roleId = roleId;

        //    if (_userManager.SignUp(ua, ref ErrorMessage) != Contracts.ErrorCode.Success)
        //    {
        //        ModelState.AddModelError(String.Empty, ErrorMessage);
        //        ViewBag.Roles = new SelectList(ctx.Tbl_Roles, "id", "RoleName");
        //        return View(ua);
        //    }       

        //    if (ua.IsOTPGenerated)
        //    {
        //        TempData["error"] = "Incorrect Code";
        //        return View(ua);
        //    }

        //    if (string.IsNullOrEmpty(storedOTP))
        //    {
        //       TempData["error"] = "Incorrect Code";
        //        return View(ua);
        //    }

        //    if (otp != storedOTP)
        //    {
        //        TempData["error"] = "Incorrect Code";
        //        return View(ua);
        //    }

        //    int memberId = ua.id;
        //    Session.Remove("GeneratedOTP");
        //    TempData["username"] = ua.Username;
        //    return RedirectToAction("Login", "Account");
        //}



        //[HttpPost]
        //public ActionResult GenerateOTP(string email)
        //{
        //    Check if there's already an OTP stored in session
        //    if (Session["GeneratedOTP"] != null)
        //    {
        //        return Json(new { success = false, message = "You already have an active OTP. Please use it to sign up." });
        //    }

        //    string generatedOTP = "";
        //    using (var rng = new RNGCryptoServiceProvider())
        //    {
        //        byte[] tokenData = new byte[6];
        //        rng.GetBytes(tokenData);
        //        generatedOTP = string.Join("", tokenData.Select(b => (b % 10).ToString()));
        //    }

        //    Session["GeneratedOTP"] = generatedOTP;

        //    string errResponse = "";
        //    bool emailSent = _mailManager.SendEmail(email, "Your OTP", $"Your sign up OTP is: {generatedOTP}", generatedOTP, ref errResponse);

        //    if (emailSent)
        //    {
        //        return Json(new { success = true, message = "OTP sent successfully!" });
        //    }
        //    else
        //    {
        //        return Json(new { success = false, message = $"Failed to send OTP: {errResponse}" });
        //    }
        //}

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

                    if (product != null && product.Quantity >= cartItem.Quantity)
                    {
                        product.Quantity -= cartItem.Quantity;
                        _unitOfWork.GetRepositoryInstance<Tbl_Product>().Update(product);
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Insufficient stock for product '{product.ProductName}'.");
                        return RedirectToAction("ViewCart");
                    }

                    var transaction = new Tbl_Transaction
                    {
                        MemberId = user.id,
                        StoreId = product.StoreId,
                        ProductId = product.ProductId,
                        Quantity = cartItem.Quantity,
                        TotalAmount = product.Price * cartItem.Quantity,
                        TimeStamp = DateTime.Now
                    };

                    _unitOfWork.GetRepositoryInstance<Tbl_Transaction>().Add(transaction);

                    _unitOfWork.GetRepositoryInstance<Tbl_Cart>().Remove(cartItem);
                }

                _unitOfWork.SaveChanges();

                TempData["CheckoutSuccess"] = true;

                return RedirectToAction("CheckoutSuccess", "Home");
            }

            ModelState.AddModelError("", "User not found or not authenticated.");
            return RedirectToAction("Index");
        }


        public ActionResult CheckoutSuccess()
        {
            if (TempData["CheckoutSuccess"] != null && (bool)TempData["CheckoutSuccess"] == true)
            {
                TempData.Remove("CheckoutSuccess");
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}