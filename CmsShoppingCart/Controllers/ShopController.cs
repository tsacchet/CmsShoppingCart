using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index","Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            // declare  list of categoryVM
            List<CategoryVM> categoryVMList;

            // init the list
            using(Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }
            // return partial with list

            return PartialView(categoryVMList);
        }

        // GET : /shop/category/name
        public ActionResult Category(string name)
        {
            // Declare a list of ProductVM
            List<ProductVM> productVMList;

            
            using(Db db = new Db())
            {
                // Get category Id
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;

                //Init the list
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                // Get category name
                var productCat = db.Products.ToArray().Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.CategoryName;
            }



            // return view with list
            return View(productVMList);
        }

        // GET : /shop/Product-Details/name
        [ActionName("product-details")]
        public ActionResult ProductDetails (string name)
        {
            //Declare the VM and DTO
            ProductVM model;
            ProductDTO dto;

            // Init prooduct id
            int id = 0;

            using (Db db = new Db())
            {
                // check id product exists
                if(! db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                // Init productDTO
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                // get id
                id = dto.Id;

                // init the model
                model = new ProductVM(dto);
            }

            // Get all gallery images
            //Get all gallery images
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));

            return View("ProductDetails", model);
        }

    }
}