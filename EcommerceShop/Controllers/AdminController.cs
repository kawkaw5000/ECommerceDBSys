using EcommerceShop.DAL;
using EcommerceShop.Models;
using EcommerceShop.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.IO;
namespace EcommerceShop.Controllers
{
    [Authorize(Roles = "Manager, Admin")]
    public class AdminController : BaseController
    { 
        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();

        public List<SelectListItem> GetMembers()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var mem = _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetAllRecords();
            foreach (var item in mem)
            {
                list.Add(new SelectListItem { Value = item.id.ToString(), Text = item.Username });
            }
            return list;
        }

        public List<Tbl_Category> GetCategories()
        {
         
            int memberId = GetCurrentMemberId(); 

     
            var categories = _unitOfWork.GetRepositoryInstance<Tbl_Category>()
                .GetAllRecords()
                .Where(c => c.MemberId == memberId && !c.IsDelete.GetValueOrDefault())
                .ToList();

            return categories;
        }
        public List<Tbl_Brand> GetBrand()
        {

            int memberId = GetCurrentMemberId();


            var brand = _unitOfWork.GetRepositoryInstance<Tbl_Brand>()
                .GetAllRecords()
                .Where(c => c.MemberId == memberId && !c.IsDelete.GetValueOrDefault())
                .ToList();

            return brand;
        }

        
        public ActionResult Dashboard()
        {
            return View();
        }
      
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

        // ADMIN USER EDIT------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public ActionResult Members()
        {
            return View(_unitOfWork.GetRepositoryInstance<Tbl_Members>().GetMembers());
        }
        public ActionResult MembersEdit(int memberId)
        {
           
            ViewBag.MembersList = GetMembers();
            return View(_unitOfWork.GetRepositoryInstance<Tbl_Members>().GetFirstorDefault(memberId));
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult MembersEdit(Tbl_Members model)
        {
            if (ModelState.IsValid)
            {
                var member = _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetFirstorDefault(model.id);
                if (member != null)
                {
                    member.IsActive = model.IsActive;
                    member.IsDelete = model.IsDelete;
                    

                    _unitOfWork.GetRepositoryInstance<Tbl_Members>().Update(member);
                    _unitOfWork.SaveChanges();
                    return RedirectToAction("Members"); 
                }
                else
                {
                    ModelState.AddModelError("", "Member not found.");
                }
            }

     
            ViewBag.MembersList = GetMembers();
            return View(model);
        }

        // ADMIN CATEGORIES EDIT------------------------------------------------------------
        public ActionResult Categories()
        {
        
            int memberId = GetCurrentMemberId(); 

        
            List<Tbl_Category> userCategories = _unitOfWork.GetRepositoryInstance<Tbl_Category>()
                .GetAllRecords()
                .Where(c => c.MemberId == memberId && !c.IsDelete.GetValueOrDefault())
                .ToList();

            return View(userCategories);
        }
        

        public ActionResult AddCategory()
        {
            var categories = GetCategories();

            ViewBag.CategoryList = new SelectList(categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        public ActionResult AddCategory(Tbl_Category tbl)
        {
       
            int memberId = GetCurrentMemberId(); 

        
            tbl.MemberId = memberId;
            tbl.IsActive = true;
            tbl.IsDelete = false;
            _unitOfWork.GetRepositoryInstance<Tbl_Category>().Add(tbl);
            _unitOfWork.SaveChanges();
            return RedirectToAction("Categories");
        }

        public ActionResult CategoryEdit(int catId)
        {
            int memberId = GetCurrentMemberId();

            var category = _unitOfWork.GetRepositoryInstance<Tbl_Category>().GetAllRecords().FirstOrDefault(p => p.CategoryId == catId && p.MemberId == memberId);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        [HttpPost]
        public ActionResult CategoryEdit(Tbl_Category tbl)
        {
            int memberId = GetCurrentMemberId();

        
            var existingCategory = _unitOfWork.GetRepositoryInstance<Tbl_Category>().GetFirstorDefault(tbl.CategoryId);
            if (existingCategory == null || existingCategory.MemberId != memberId)
            {
                return HttpNotFound(); 
            }

            existingCategory.CategoryName = tbl.CategoryName;
            existingCategory.IsActive = tbl.IsActive;
            existingCategory.IsDelete = tbl.IsDelete;


            _unitOfWork.GetRepositoryInstance<Tbl_Category>().Update(existingCategory);
            _unitOfWork.SaveChanges(); 

            return RedirectToAction("Categories");
        }

        

        private int GetCurrentMemberId()
        {    
            string loggedInUserEmail = User.Identity.Name;       
            var user = _unitOfWork.GetRepositoryInstance<Tbl_Members>()
                                  .GetAllRecords()
                                  .FirstOrDefault(m => m.Username == loggedInUserEmail);

            if (user != null)
            {
         
                return user.id;
            }
            else
            {  
                throw new InvalidOperationException("Member not found for the logged-in user.");
            }
        }

        private int GetCurrentStoreId()
        {
            string loggedInUserEmail = User.Identity.Name;
            var user = _unitOfWork.GetRepositoryInstance<Tbl_Members>()
                                  .GetAllRecords()
                                  .FirstOrDefault(m => m.Username == loggedInUserEmail);

            if (user != null)
            {
               
                return user.StoreId ?? 0; 
            }
            else
            {
                throw new InvalidOperationException("Store not found for the logged-in user.");
            }
        }




        public ActionResult Brand()
        {

            int memberId = GetCurrentMemberId();


            List<Tbl_Brand> userBrand = _unitOfWork.GetRepositoryInstance<Tbl_Brand>()
                .GetAllRecords()
                .Where(c => c.MemberId == memberId && !c.IsDelete.GetValueOrDefault())
                .ToList();

            return View(userBrand);
        }

        public ActionResult AddBrand()
        {
            var brand = GetBrand();

            ViewBag.BrandList = new SelectList(brand, "BrandId", "BrandName");
            return View();
        }

        [HttpPost]
        public ActionResult AddBrand(Tbl_Brand tbl)
        {
            int memberId = GetCurrentMemberId();

            tbl.MemberId = memberId;
            tbl.IsActive = true;
            tbl.IsDelete = false;
            _unitOfWork.GetRepositoryInstance<Tbl_Brand>().Add(tbl);
            _unitOfWork.SaveChanges();
            return RedirectToAction("Brand");
        }

        public ActionResult BrandEdit(int brandId)
        {
            int memberId = GetCurrentMemberId(); 

            var brand = _unitOfWork.GetRepositoryInstance<Tbl_Brand>().GetAllRecords().FirstOrDefault(p => p.BrandId == brandId && p.MemberId == memberId);
            if (brand == null)
            {
                return HttpNotFound();
            }
            return View(brand);
        }

        [HttpPost]
        public ActionResult BrandEdit(Tbl_Brand tbl)
        {
            int memberId = GetCurrentMemberId();
     
            var existingBrand = _unitOfWork.GetRepositoryInstance<Tbl_Brand>().GetFirstorDefault(tbl.BrandId);
            if (existingBrand == null || existingBrand.MemberId != memberId)
            {
                return HttpNotFound(); 
            }

            existingBrand.BrandName = tbl.BrandName;
            existingBrand.IsActive = tbl.IsActive;
            existingBrand.IsDelete = tbl.IsDelete;
    
      
            _unitOfWork.GetRepositoryInstance<Tbl_Brand>().Update(existingBrand);
            _unitOfWork.SaveChanges();

            return RedirectToAction("Brand");
        }
        // ADMIN PRODUCT EDIT------------------------------------------------------------
        public ActionResult Product()
        {
            int memberId = GetCurrentStoreId();

            var products = _unitOfWork.GetRepositoryInstance<Tbl_Product>()
                .GetProduct()
                .Where(p => !(p.IsDelete ?? false) && p.StoreId == memberId);

            return View(products);
        }

        public ActionResult ProductEdit(int productId)
        {
            int memberId = GetCurrentStoreId();

            ViewBag.CategoryList = new SelectList(GetCategories(), "CategoryId", "CategoryName");
            ViewBag.BrandList = new SelectList(GetBrand(), "BrandId", "BrandName");
            var product = _unitOfWork.GetRepositoryInstance<Tbl_Product>().GetAllRecords().FirstOrDefault(p => p.ProductId == productId && p.StoreId == memberId);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        [HttpPost]
        public ActionResult ProductEdit(Tbl_Product tbl, HttpPostedFileBase file)
        {
            int memberId = GetCurrentStoreId();
       
            ViewBag.CategoryList = new SelectList(GetCategories(), "CategoryId", "CategoryName");   
            ViewBag.BrandList = new SelectList(GetBrand(), "BrandId", "BrandName");

            var existingProduct = _unitOfWork.GetRepositoryInstance<Tbl_Product>().GetFirstorDefault(tbl.ProductId);
            if (existingProduct == null || existingProduct.StoreId != memberId)
            {
                return HttpNotFound();
            }

            string pic = null;
            if (file != null)
            {
                
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

               
                if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png")
                {
                   
                    pic = Guid.NewGuid().ToString() + fileExtension;

                 
                    var path = Path.Combine(Server.MapPath("~/ProductImg/"), pic);

                    file.SaveAs(path);
                }
                else
                {
                  
                    ModelState.AddModelError("", "Only JPEG and PNG images are allowed.");
                    return View(tbl);
                }
            }
            existingProduct.ProductName = tbl.ProductName;
            existingProduct.Description = tbl.Description;
            existingProduct.Price = tbl.Price;
            existingProduct.BrandId = tbl.BrandId;
            existingProduct.Quantity = tbl.Quantity;
            existingProduct.CategoryId = tbl.CategoryId;
            existingProduct.IsFeatured = tbl.IsFeatured;
            existingProduct.IsActive = tbl.IsActive;
            existingProduct.IsDelete = tbl.IsDelete;

            if (file != null)
            {
                existingProduct.ProductImage = pic;
            }

            existingProduct.ModifiedDate = DateTime.Now;
            _unitOfWork.GetRepositoryInstance<Tbl_Product>().Update(existingProduct);
            _unitOfWork.SaveChanges();

            return RedirectToAction("Product");
        }

        public ActionResult ProductAdd()
        {
            ViewBag.CategoryList = new SelectList(GetCategories(), "CategoryId", "CategoryName");
            ViewBag.BrandList = new SelectList(GetBrand(), "BrandId", "BrandName");
            return View();
        }

        [HttpPost]
        public ActionResult ProductAdd(Tbl_Product tbl, HttpPostedFileBase file)
        {
            int memberId = GetCurrentStoreId();
         
            ViewBag.CategoryList = new SelectList(GetCategories(), "CategoryId", "CategoryName");
            ViewBag.BrandList = new SelectList(GetBrand(), "BrandId", "BrandName");

        
            if (file != null && file.ContentLength > 0)
            {
           
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
            
                if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png")
                {
                   
                    var fileName = Guid.NewGuid().ToString() + fileExtension;               
                    var path = Path.Combine(Server.MapPath("~/ProductImg/"), fileName);
                 
                    file.SaveAs(path);                
                    tbl.ProductImage = fileName;
                }
                else
                {                  
                    ModelState.AddModelError("", "Only JPEG and PNG images are allowed.");
                    return View(tbl);
                }
            }
            else
            {              
                ModelState.AddModelError("", "Please upload an image.");
                return View(tbl);
            }   
            tbl.CreatedDate = DateTime.Now;
            tbl.StoreId = memberId;
            tbl.IsActive = true;
            tbl.IsDelete = false;
          
            _unitOfWork.GetRepositoryInstance<Tbl_Product>().Add(tbl);
     
            return RedirectToAction("Product");
        }

        

        public ActionResult AddStore()
        {
            try
            {               
                int storeId = GetCurrentStoreId();

                if (storeId != 0)
                {                 
                    var userStore = _unitOfWork.GetRepositoryInstance<Tbl_Store>()
                        .GetAllRecords()
                        .FirstOrDefault(s => s.StoreId == storeId);
                    if (userStore != null)
                    {                
                        return View(userStore);
                    }
                    else
                    {                   
                        return HttpNotFound();
                    }
                }
                else
                {    
                    return View();
                }
            }
            catch (Exception ex)
            {          
                ModelState.AddModelError("", "Error retrieving store data: " + ex.Message);
                return View();
            }
        }

        [HttpPost]
        public ActionResult AddStore(Tbl_Store tbl)
        {
            try
            {           
                int memberId = GetCurrentMemberId();
  
                var member = _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetFirstorDefault(memberId);
                if (member != null && member.StoreId.HasValue)
                {                      
                    var existingStore = _unitOfWork.GetRepositoryInstance<Tbl_Store>()
                        .GetAllRecords()
                        .FirstOrDefault(s => s.StoreId == member.StoreId.Value);

                    if (existingStore != null)
                    {
                 
                        existingStore.storeName = tbl.storeName; 

                        _unitOfWork.GetRepositoryInstance<Tbl_Store>().Update(existingStore);
                        _unitOfWork.SaveChanges();

                        return RedirectToAction("Dashboard");
                    }
                    else
                    {             
                        return HttpNotFound();
                    }
                }
                else
                {                              
                    tbl.IsActive = true;
                    tbl.IsDelete = false;
             
                    _unitOfWork.GetRepositoryInstance<Tbl_Store>().Add(tbl);
                    _unitOfWork.SaveChanges();
                  
                    if (member != null)
                    {
                        member.StoreId = tbl.StoreId;
                        _unitOfWork.GetRepositoryInstance<Tbl_Members>().Update(member);
                        _unitOfWork.SaveChanges();
                    }

                    return RedirectToAction("Dashboard");
                }
            }
            catch (Exception ex)
            {
            
                ModelState.AddModelError("", "Error adding/updating store: " + ex.Message);
                return View(tbl); 
            }
        }

        public ActionResult StoreTransactions()
        {
            try
            {           
                int storeId = GetCurrentStoreId();          
                var transactions = _unitOfWork.GetRepositoryInstance<Tbl_Transaction>()
                    .GetAllRecords()
                    .Where(t => t.StoreId == storeId)
                    .ToList();

                return View(transactions);
            }
            catch (Exception ex)
            {          
                ModelState.AddModelError("", "Error retrieving store transactions: " + ex.Message);
                return View();
            }
        }   

    }
}