using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Account;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CmsShoppingCart.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        // GET: account//Login
        [HttpGet]
        public ActionResult Login()
        {
            // confirm user is not logged in
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("user-profile");
            }


            return View();
        }

        // POST: account//Login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if user is valid
            bool isValid = false;

            using (Db db = new Db())
            {
                if(db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if( ! isValid)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
            }
                
        }


        // GET: account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {

            return View("CreateAccount");
        }

        // POST: account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            // check model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            // check if password match
            if(!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                // Make sure username is unique
                if(db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", "Usrename " + model.Username + " is taken.");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                // create userDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    Username = model.Username,
                    Password = model.Password,

                    PA_Address = model.PA_Address,
                    PA_Suburb = model.PA_Suburb,
                    PA_State = model.PA_State,
                    PA_PostCode = model.PA_PostCode,
                    PA_Country = model.PA_Country,

                    DA_Address = model.DA_Address,
                    DA_Suburb = model.DA_Suburb,
                    DA_State = model.DA_State,
                    DA_PostCode = model.DA_PostCode,
                    DA_Country = model.DA_Country
                };


                // add the DTO
                db.Users.Add(userDTO);

                // save
                db.SaveChanges();

                // Add to userRoleDTO
                int id = userDTO.Id;

                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }

            // Create a TempData message
            TempData["SM"] = "You are now registered and can login.";

            // redirect
            return Redirect("~/account/login");
        }

        // GET: account/logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }


        [Authorize]
        public ActionResult UserNavPartial()
        {
            // Get username
            string username = User.Identity.Name;

            // declare model
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                // Get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                // Build the model
                model = new UserNavPartialVM()
                {
                    Firstname = dto.FirstName,
                    Lasttname = dto.LastName
                };
            }

            // return partial view with model
            return PartialView(model);
        }

        // GET: account/UserProfile
        [HttpGet]
        [Authorize]
        [ActionName("user-profile")]
        public ActionResult UserProfile()
        {
            // Get username
            string username = User.Identity.Name;

            // Declare model
            UserProfileVM model;

            using (Db db = new Db())
            {
                // Get user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                // Build model
                model = new UserProfileVM(dto);
            }

            // reurn view with model
            return View("UserProfile", model);
        }

        // POST: account/UserProfile
        [HttpPost]
        [Authorize]
        [ActionName("user-profile")]
        public ActionResult UserProfile(UserProfileVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            // Check if the passwords match if need be
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not match.");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                // Get username
                string username = User.Identity.Name;

                // Make sure username is unique
                if(db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == username))
                {
                    ModelState.AddModelError("", "Username " + model.Username +" already exists.");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                // Edit DTO
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.Username = model.Username;

                dto.PA_Address = model.PA_Address;
                dto.PA_Suburb = model.PA_Suburb;
                dto.PA_State = model.PA_State;
                dto.PA_PostCode = model.PA_PostCode;
                dto.PA_Country = model.PA_Country;

                dto.DA_Address = model.DA_Address;
                dto.DA_Suburb = model.DA_Suburb;
                dto.DA_State = model.DA_State;
                dto.DA_PostCode = model.DA_PostCode;
                dto.DA_Country = model.DA_Country;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                // Save
                db.SaveChanges();
            }

            // Set TempData message
            TempData["SM"] = "You have edited your profile.";

            // redirect
            return Redirect("~/account/user-profile");
        }

        // GET: account/Orders
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            //Init list if OrderForUserVM
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {
                // Get UserId
                UserDTO user = db.Users.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;

                // Init list of OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();
                
                // Loop through list orderVM
                foreach(var order in orders)
                {
                    // init products dict
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    // Declare total
                    decimal total = 0m;

                    // init list of orderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    // Loop through list of OrderDetailsDTO
                    foreach(var orderDetails in orderDetailsDTO)
                    {
                        // Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // Get product price
                        decimal price = product.Price;

                        // Get product name
                        string productName = product.Name;

                        // Add to product dict
                        productAndQty.Add(productName, orderDetails.Quantity);

                        // Get total
                        total += orderDetails.Quantity * price;

                    }

                    // Add to orderForUserVm list
                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productAndQty,
                        CreatedAt = order.CreatedAt,
                        Completed = order.Completed
                    });
                }
            }

            // return view with list of OrdersForUserVM
            return View(ordersForUser);
        }

       
    }
}