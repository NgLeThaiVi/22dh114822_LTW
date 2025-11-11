using _22dh114822_LTW.Models.ViewModel;
using _22dh114822_LTW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _22dh114822_LTW.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Order
        public ActionResult Index()
        {
            return View();
        }

        // GET: Order/Checkout
        [Authorize]
        public ActionResult Checkout()
        {
            //Kiểm tra giỏ hàng trong session,
            //nếu giỏ hàng rỗng hoặc không có sản phẩm thì chuyển hướng về trang chủ
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            //Xác thực người dùng đã đăng nhập chưa, nếu chưa thì chuyển hướng tới trang đăng nhập
            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //Lấy thông tin khách hàng từ CSDL, nếu chưa có thì chuyển hướng tới trang Đăng nhập
            //nếu có rồi thì lấy địa chỉ chỉ của khách hàng và gán vào ShippingAddress của CheckoutVM
            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //Tạo dữ liệu hiển thị cho CheckoutVM
            var model = new CheckoutVM
            {
                CartItems = cart, //Lấy danh sách các sản phẩm trong giỏ hàng
                TotalAmount = cart.Sum(item => item.TotalPrice), //Tổng giá trị của các mặt hàng trong giỏ
                OrderDate = DateTime.Now, //Mặc định lấy bằng thời điểm đặt hàng
                ShippingAddress = customer.CustomerAddress, //Lấy địa chỉ mặc định từ bảng Customer
                CustomerID = customer.CustomerID, //Lấy mã khách hàng từ bảng Customer
                Username = customer.Username //Lấy tên đăng nhập từ bảng Customer
            };

            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Checkout(CheckoutVM model)
        {
            if (ModelState.IsValid)
            {
                //Nếu giỏ hàng rỗng, chuyển hướng tới trang Home
                var cart = Session["Cart"] as List<CartItem>;
                if (cart == null || !cart.Any())
                {
                    return RedirectToAction("Index", "Home");
                }

                //Nếu người dùng chưa đăng nhập, chuyển hướng tới trang Login
                var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                //Nếu khách hàng không khớp với tên đăng nhập, chuyển hướng tới trang Login
                var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
                if (customer == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                //Nếu người dùng chọn thanh toán bằng PayPal, sẽ điều hướng tới trang PaymentWithPaypal
                if (model.PaymentMethod == "PayPal")
                {
                    return RedirectToAction("PaymentWithPaypal", "PayPal", model);
                }

                // Thiết lập PaymentStatus dựa trên PaymentMethod
                string paymentStatus = "Chưa thanh toán";
                switch (model.PaymentMethod)
                {
                    case "Tiền mặt":
                        paymentStatus = "Thanh toán tiền mặt";
                        break;
                    case "PayPal":
                        paymentStatus = "Thanh toán PayPal";
                        break;
                    case "Mua trước trả sau":
                        paymentStatus = "Trả góp";
                        break;
                    default:
                        paymentStatus = "Chưa thanh toán";
                        break;
                }

                //Tạo đơn hàng và chi tiết đơn hàng liên quan
                var order = new Order
                {
                    CustomerID = customer.CustomerID,
                    OrderDate = model.OrderDate,
                    TotalAmount = model.TotalAmount,
                    PaymentStatus = paymentStatus,
                    PaymentMethod = model.PaymentMethod,
                    DeliveryMethod = model.ShippingMethod,
                    ShippingAddress = model.ShippingAddress,
                    OrderDetails = cart.Select(item => new OrderDetail
                    {
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    }).ToList()
                };

                //Lưu đơn hàng vào CSDL
                db.Orders.Add(order);
                db.SaveChanges();

                //Xóa giỏ hàng sau khi đặt hàng thành công
                Session["Cart"] = null;

                //Điều hướng tới trang xác nhận đơn hàng
                return RedirectToAction("OrderSuccess", new { id = order.OrderID });
            }

            return View(model);
        }

        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Include("OrderDetails").SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }
    }
}