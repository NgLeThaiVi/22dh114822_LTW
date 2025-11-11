using _22dh114822_LTW.Models.ViewModel;
using _22dh114822_LTW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace _22dh114822_LTW.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                //kiểm tra xem tên đăng nhập đã tồn tại chưa
                var existingUser = db.Users.SingleOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                    return View(model);
                }

                //nếu chưa tồn tại thì tạo bản ghi thông tin tài khoản trong bảng User
                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password, // Lưu ý: Nên mã hóa mật khẩu trước khi lưu
                    UserRole = "Customer"
                };
                db.Users.Add(user);

                // và tạo bản ghi thông tin khách hàng trong bảng Customer
                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    CustomerPhone = model.CustomerPhone,
                    CustomerAddress = model.CustomerAddress,
                    Username = model.Username,
                };
                db.Customers.Add(customer);

                //lưu thông tin tài khoản và thông tin khách hàng vào CSDL
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    // *** Quan trọng: Xuất lỗi ra cửa sổ Output/Debug trong Visual Studio ***
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            // Kiểm tra cửa sổ Output/Debug trong VS để xem thông báo lỗi cụ thể!
                            System.Diagnostics.Debug.WriteLine($"LỖI TRƯỜNG {validationError.PropertyName}: {validationError.ErrorMessage}");
                        }
                    }
                    // (Tùy chọn: Thêm lỗi chung vào ModelState để hiển thị trên View)
                    ModelState.AddModelError("", "Đăng ký thất bại do dữ liệu không hợp lệ. Vui lòng kiểm tra lại.");

                    return View(model); // Quay lại View để người dùng sửa lỗi
                }
                catch (Exception ex)
                {
                    // Lỗi khác (ví dụ: lỗi kết nối DB, lỗi khóa chính/ngoại)
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                    return View(model);
                }
            }

            return View(model);
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.SingleOrDefault(u => u.Username == model.Username
                    && u.Password == model.Password
                    && u.UserRole == "Customer");

                if (user != null)
                {
                    // Lưu trạng thái đăng nhập vào session
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole;

                    // Lưu thông tin xác thực người dùng vào cookie
                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            return View(model);
        }
    }
}