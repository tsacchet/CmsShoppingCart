using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PagedList;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {


        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {

            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                categoryVMList = db.Categories
                                .ToArray()
                                .OrderBy(x => x.Sorting)
                                .Select(x => new CategoryVM(x))
                                .ToList();
            }

            return View(categoryVMList);
        }

        [HttpPost]
        public string AddNewCategory(string catName)
        {
            // Declare id
            string id;

            using (Db db = new Db())
            {
                // Check that the category name is unique
                if (db.Categories.Any(x => x.Name == catName))
                {
                    return "titletaken";
                }
                // Init DTO
                CategoryDTO dto = new CategoryDTO();

                // Add to DTO
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                // Save DTO
                db.Categories.Add(dto);
                db.SaveChanges();

                // Get the id
                id = dto.Id.ToString();
            }


            // return id
            return id;
        }

        [HttpPost]
        public ActionResult DeleteCategory()
        {

            return View();
        }

        // GET: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                // Set initiall count
                int count = 1;

                // Declare CategoryDTO
                CategoryDTO dto;

                // Set sorting for each category
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        // GET: Admin/Shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {

            using (Db db = new Db())
            {
                // Get the category
                CategoryDTO dto = db.Categories.Find(id);

                // Remove the category
                db.Categories.Remove(dto);

                // Save
                db.SaveChanges();
            }

            // Redirect
            return RedirectToAction("Categories");
        }

        // GET: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                // check category is unique
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                // Get DTO
                CategoryDTO dto = db.Categories.Find(id);

                // Edit DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                // Save DTO
                db.SaveChanges();

            }

            return "ok";
        }

        // GET: Admin/Shop/AddProduct/
        [HttpGet]
        public ActionResult AddProduct()
        {
            // Init the model
            ProductVM model = new ProductVM();

            // Add select list of categories to model
            using(Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            // return view with model
            return View(model);
        }

        // POST: Admin/Shop/AddProduct/
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            // Check model state
            if(!ModelState.IsValid)
            {
                using(Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            // Make sure pruct name is unique
            using (Db db = new Db())
            {
                if(db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
                
            }

            // Declare product id
            int id;

            // Init and save productDTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ","-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                
                product.CategoryId = model.CategoryId;
                //product.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                //product.CategoryName = model.CategoryName;

                // Get inserted id
                id = product.Id;

            }

            // Set TempData message
            TempData["SM"] = "You have added a product!";

            #region Upload Image

            // create necessary directories
            var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
          
            var pathsString1 = Path.Combine(orginalDirectory.ToString(), "Products");
            var pathsString2 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString());
            var pathsString3 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathsString4 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathsString5 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathsString1))
                Directory.CreateDirectory(pathsString1);

            if (!Directory.Exists(pathsString2))
                Directory.CreateDirectory(pathsString2);

            if (!Directory.Exists(pathsString3))
                Directory.CreateDirectory(pathsString3);

            if (!Directory.Exists(pathsString4))
                Directory.CreateDirectory(pathsString4);

            if (!Directory.Exists(pathsString5))
                Directory.CreateDirectory(pathsString5);

            // check if a file was uploaded

            if (file != null && file.ContentLength > 0)
            {
                // get file extension
                string ext = file.ContentType.ToLower();

                // verify extension
                if(ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" && 
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension.");
                        return View(model);
                    }
                }

                // init image name
                string imagename = file.FileName;

                // save image name to DTO
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imagename;
                    db.SaveChanges();
                }

                // set orginal and thumb image paths
                var path = string.Format("{0}\\{1}", pathsString2, imagename);
                var path2 = string.Format("{0}\\{1}", pathsString3, imagename);

                // save orginal
                file.SaveAs(path);

                // create and save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            // redirect
            return RedirectToAction("AddProduct");
        }

        // GET: Admin/Shop/Products/
        public ActionResult Products(int? page, int? catId)
        {
            // Declare a list of ProductVM
            List<ProductVM> listOfProductVM;

            // set page number
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)

            using (Db db = new Db())
            {
                // init the list
                listOfProductVM = db.Products.ToArray()
                                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                    .Select(x => new ProductVM(x))
                                    .ToList();
                // Populate categories select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Set selected category
                ViewBag.SelectedCat = catId.ToString();
            }

            // set pagination
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3); // will only contain 25 products max because of the pageSize
            ViewBag.OnePageOfProducts = onePageOfProducts;

            // return view with list

            return View(listOfProductVM);
        }

        // GET: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            // Declare productVM
            ProductVM model;

            using (Db db = new Db())
            {
                // Get the Product
                ProductDTO dto = db.Products.Find(id);

                // Make sure product exists
                if(dto == null)
                {
                    return Content("That product does not exist.");
                }

                // init model
                model = new ProductVM(dto);

                // Make a select list
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Get all Gallery images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }


            // Return view with model
            return View(model);
        }

        // POST: Admin/Shop/EditProduct/id
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            // Get product id
            int id = model.Id;

            // populate categories select list and gallery images
            using(Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));

            // check model state
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {                    
                    return View(model);
                }
            }

            // make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name ))
                {               
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }

            }

            // Update product
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }
            // Set TemData message
            TempData["SM"] = "You have edited the product!";

            #region Image Upload

            // Get extension
            if (file != null && file.ContentLength > 0)
            {
                // get file extension
                string ext = file.ContentType.ToLower();

                // verify extension
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension.");
                        return View(model);
                    }
                }

                // set upload directory paths
                var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                var pathsString1 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString());
                var pathsString2 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                // Delete files from directories
                DirectoryInfo di1 = new DirectoryInfo(pathsString1);
                DirectoryInfo di2 = new DirectoryInfo(pathsString2);

                foreach(FileInfo file2 in di1.GetFiles())
                {
                    file2.Delete();
                }

                foreach (FileInfo file3 in di2.GetFiles())
                {
                    file3.Delete();
                }

                // Save images name
                string imagename = file.FileName;

                using(Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imagename;
                    db.SaveChanges();
                }

                // Save original and thumbs images
                var path = string.Format("{0}\\{1}", pathsString1, imagename);
                var path2 = string.Format("{0}\\{1}", pathsString2, imagename);

                // save orginal
                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }
                #endregion

                // redirect
                return RedirectToAction("EditProduct");
        }

        // GET: Admin/Shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            // Delete product from DB
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }

            // Delete product Folders
            var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathsString = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString());

            if(Directory.Exists(pathsString))
            {
                Directory.Delete(pathsString, true);
            }

            // Redirect
            return RedirectToAction("Products");
        }

        // POST: Admin/Shop/SaveGalleryImages
        [HttpPost]
        public void SaveGalleryImages( int id)
        {
            // loop through files
            foreach(string filename in Request.Files)
            {
                // init the file
                HttpPostedFileBase file = Request.Files[filename];

                // Check its not null
                if (file != null && file.ContentLength > 0)
                {

                    // set directory paths
                    var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    var pathsString1 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    var pathsString2 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    // set images paths
                    var path = string.Format("{0}\\{1}", pathsString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathsString2, file.FileName);

                    // save original and thumbs
                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);

                }
            }
        }

        // POST: Admin/Shop/DeleteImage
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }
    }
}