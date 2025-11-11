using _22dh114822_LTW.Models.ViewModel;
using _22dh114822_LTW.Models;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _22dh114822_LTW.Controllers
{
    public class PayPalController : Controller
    {
        // GET: PayPal
        public ActionResult Index()
        {
            return View();
        }
        private MyStoreEntities db = new MyStoreEntities();

        private Payment CreatePayment(APIContext apiContext, string redirectUrl, CheckoutVM model)
        {
            var itemList = new ItemList() { items = new List<Item>() };

            foreach (var item in model.CartItems)
            {
                itemList.items.Add(new Item()
                {
                    name = item.ProductName,
                    currency = "USD",
                    price = item.UnitPrice.ToString(),
                    quantity = item.Quantity.ToString(),
                    sku = item.ProductID.ToString()
                });
            }

            var payer = new Payer() { payment_method = "paypal" };

            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl
            };

            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = model.TotalAmount.ToString()
            };

            var amount = new Amount()
            {
                currency = "USD",
                total = model.TotalAmount.ToString(),
                details = details
            };

            var transactionList = new List<Transaction>();
            transactionList.Add(new Transaction()
            {
                description = "Transaction description",
                invoice_number = Convert.ToString((new Random()).Next(100000)),
                amount = amount,
                item_list = itemList
            });

            var payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            return payment.Create(apiContext);
        }

        private APIContext GetAPIContext()
        {
            var config = ConfigManager.Instance.GetProperties();
            var accessToken = new OAuthTokenCredential(config).GetAccessToken();
            return new APIContext(accessToken);
        }

        public ActionResult PaymentWithPaypal(CheckoutVM model)
        {
            APIContext apiContext = GetAPIContext();

            try
            {
                string payerId = Request.Params["PayerID"];

                if (string.IsNullOrEmpty(payerId))
                {
                    string baseURL = Request.Url.Scheme + "://" + Request.Url.Authority + "/PayPal/PaymentWithPaypal?";
                    var guid = Convert.ToString((new Random()).Next(100000));
                    var createdPayment = CreatePayment(apiContext, baseURL + "guid=" + guid, model);

                    string paypalRedirectUrl = null;
                    var links = createdPayment.links.GetEnumerator();

                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = lnk.href;
                        }
                    }

                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);

                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }
                    // Logic thành công sẽ nằm ở đây, sau khi kiểm tra trạng thái "approved"
                }
            }
            catch (Exception ex)
            {
                return View("FailureView");
            }

            return RedirectToAction("CreateOrder", "Order", model);
        }

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            var payment = new Payment() { id = paymentId };
            return payment.Execute(apiContext, paymentExecution);
        }
    }
}