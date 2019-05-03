using CmsShoppingCart.Models;
using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Cart;
using CmsShoppingCart.Models.ViewModels.Shop;
using PayPal.Api;
using PayPalTest.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            // Init the cart list
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // check if cart is empty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty.";
                return View();
            }

            // calculate total and save to ViewBag
            decimal total = 0;

            foreach(var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            // return view with list
            return View(cart);
        }



        public ActionResult CartPartial()
        {
            // init Cart VM
            CartVM model = new CartVM();

            // Init Quantity
            int qty = 0;

            // Init Price
            decimal price = 0m;

            // Check for cart session
            if (Session["cart"] != null)
            {
                //Get total qty and price
                var list = (List<CartVM>)Session["cart"];

                foreach( var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                // or set qty and price to 0
                model.Quantity = 0;
                model.Price = 0;
            }

            // return partial view with model
            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            // Init CartVM List
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Init CartVM
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                // Get the product
                ProductDTO product = db.Products.Find(id);

                // Check if the product is already in cart
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                // if not, add new
                if(productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                else
                {
                    // if it is, increment
                    productInCart.Quantity++;
                }

            }

            // Get total qty and price and add to model
            int qty = 0;
            decimal price = 0m;

            foreach(var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            // Save cart back to session
            Session["cart"] = cart;

            // return partial view with model
            return PartialView(model);
        }

        public JsonResult IncrementProduct(int productId)
        {
            // Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Get cartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // increment qty
                model.Quantity++;

                // store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                // return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DecrementProduct(int productId)
        {
            // Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using(Db db = new Db())
            {
                // Get cartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // decrement qty
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                // store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                // return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        // Get: /cart/RemoveProduct
        public void RemoveProduct(int productId)
        {
            // Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Get cartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //Remove moel from cart
                cart.Remove(model);
            }
        }

        public ActionResult PayPalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            return PartialView(cart);
        }


        // PlaceOrder
        public string PlaceOrder()
        {
            // Get cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // init OrderId
            int orderId = 0;

            if (cart.Count > 0)
            {
                // Get username
                string username = User.Identity.Name;
              
                using (Db db = new Db())
                {
                    // Init orderDTO
                    OrderDTO orderDTO = new OrderDTO();

                    // get user id
                    var q = db.Users.FirstOrDefault(x => x.Username == username);
                    int userId = q.Id;

                    // Add to OrderDTO and save
                    orderDTO.UserId = userId;
                    orderDTO.CreatedAt = DateTime.Now;
                    orderDTO.Completed = false;

                    db.Orders.Add(orderDTO);

                    db.SaveChanges();

                    // Get inserted id
                    orderId = orderDTO.OrderId;

                    // Set Session OrerId
                    Session["orderId"] = orderId;

                    // Init OrderDetailsDTO
                    OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                    // Add to OrderDetailsDTO
                    foreach (var item in cart)
                    {
                        orderDetailsDTO.OrderId = orderId;
                        orderDetailsDTO.UserId = userId;
                        orderDetailsDTO.ProductId = item.ProductId;
                        orderDetailsDTO.Quantity = item.Quantity;

                        db.OrderDetails.Add(orderDetailsDTO);

                        db.SaveChanges();
                    }
                }


                // Email admin
                //SendEmail(orderId);

                // reset session
                Session["cart"] = null;


                return orderId.ToString();
            }

            return orderId.ToString();

        }

        public void UpdateOrderCompleted(int OrdId, bool flag)
        {
            if (OrdId > 0)
            {
                using (Db db = new Db())
                {
                    var tbl = db.Orders.Find(OrdId);
                    tbl.Completed = flag;
                    db.SaveChanges();
                }
            }
        }

        public void SendEmail(int orderId)
        {
            var EmailFrom = ConfigurationManager.AppSettings["FromEmail"];
            var EmailTo = ConfigurationManager.AppSettings["ToEmail"];





            using (MailMessage mm = new MailMessage(EmailFrom, EmailTo))
            {
                mm.Subject = "New Order";
                string body = "You Have a new order. " + orderId + "<br />";
                body += "<br />";

                using (Db db = new Db())
                {
                    // Get UserId
                    UserDTO user = db.Users.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
                    int userId = user.Id;

                    body += "Customer First Name: " + user.FirstName + "<br />";
                    body += "Customer Last Name: " + user.LastName + "<br />";
                    body += "<br />";
                    body += "Postal Address: " + user.PA_Address + "<br />";
                    body += "Postal Suburb: " + user.PA_Suburb + "<br />";
                    body += "Postal PostCode: " + user.PA_PostCode + "<br />";
                    body += "Postal State: " + user.PA_State + "<br />";
                    body += "Postal Country: " + user.PA_Country + "<br />";
                    body += "<br />";

                    decimal total = 0;

                    // Init list of OrderVM
                    OrderDTO orders = db.Orders.Where(x => x.OrderId == orderId).FirstOrDefault();                     
                    body += "Order Creation Date: " + orders.CreatedAt + "<br />";
                    body += "<br />";

                    // init list of orderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == orderId).ToList();

                    // Set up Table
                    body += "<table class=*table table-bordered table-striped*>";
                    body += "<thead>";
                    body += "<tr>";
                    body += "<th>ID</th>";
                    body += "<th>Product </th>";
                    body += "<th>Price</th>";
                    body += "<th>Qty</th>";
                    body += "<th>Total</th>";
                    body += "</tr>";
                    body += "</thead>";
                    body += "<tbody>";

                    // Loop through list of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        // Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        body += "<tr>";

                        // Get product id
                        body += "<td>" + product.Id + "</td>";

                        // Get product name
                        body += "<td>" + product.Name + "</td>";

                        // Get product price
                        body += "<td>" + product.Price + "</td>";

                        // Get product Qty
                        body += "<td>" + orderDetails.Quantity + "</td>";

                        // Get total
                        body += "<td>" + orderDetails.Quantity * product.Price + "</td>";

                        total += orderDetails.Quantity * product.Price;

                        body += "</tr>";
                    }

                    body += "<tr>";
                    body += "<td></td><td></td><td></td>";
                    body += "<td>Total Price:</td> <td><strong>" + total + "</strong></td>";
                    body += "</tr>";

                    body += "</tbody >";
                    body += "</table>";
                }


                body = body.Replace('*', '"');
                mm.Body = body;
                mm.IsBodyHtml = true;

                //Attachment attachment;
                //attachment = new Attachment(FilesFolder + "\\" + id + "_Order.pdf");
                //mm.Attachments.Add(attachment);

                // Send Mail
                using (var smtp = new SmtpClient())
                {
                    smtp.Send(mm);
                    Console.Write("Mail Sent: EMail sent Sucessfully: Function Mail() :");
                }
            }

        }




        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            //getting the apiContext  
            APIContext apiContext = PayPalConfiguration.GetAPIContext();
            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath   +"/Cart/PaymentWithPayPal?";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    var guid = Convert.ToString((new Random()).Next(100000));
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }

                    // saving the paymentID in the key guid  
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        Session["orderId"] = 0;
                        return View("FailureView");
                    }
                }
            }
            catch (Exception ex)
            {
                Session["orderId"] = 0;
                return View("FailureView");
            }
            //on successful payment, show success page to user.  
            UpdateOrderCompleted((int)Session["orderId"], true);
            SendEmail((int)Session["orderId"]);
            Session["orderId"] = 0;
            return View("SuccessView");
        }





        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }


        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            //create itemlist and add item objects to it  
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };

            //Adding Item Details like name, currency, price etc  
            //itemList.items.Add(new Item()
            //{
            //    name = "Apples",
            //    currency = "AUD",
            //    price = "1",
            //    quantity = "1",
            //    sku = "sku"
            //});

            // My routine to get items from cart
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            decimal totalItemCost = 0;

            foreach (var item in cart)
            {
                itemList.items.Add(new Item()
                {
                    name = item.ProductName,
                    currency = "AUD",
                    price = item.Price.ToString(),
                    quantity = item.Quantity.ToString(),
                    sku = item.ProductId.ToString()
                });

                totalItemCost += item.Total;
            }
            


            var payer = new Payer()
            {
                payment_method = "paypal"
            };

            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };

            // Adding Tax, shipping and Subtotal details  
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = totalItemCost.ToString()
            };

            //Final amount with details  
            var amount = new Amount()
            {
                currency = "AUD",
                total = (decimal.Parse(details.shipping) + decimal.Parse(details.subtotal) + decimal.Parse(details.tax)).ToString(), // Total must be equal to sum of tax, shipping and subtotal.  
                details = details
            };

            // My Code Place Order
            string orderid = PlaceOrder();

            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            transactionList.Add(new Transaction()
            {
                description = "Order for Shopping Cart.",
                invoice_number = orderid, //Generate an Invoice No  
                amount = amount,
                item_list = itemList
            });

            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }







    }

    
}