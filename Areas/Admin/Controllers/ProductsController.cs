using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _22dh114822_LTW.Models;
using _22dh114822_LTW.Models.ViewModel;
using PagedList;

namespace _22dh114822_LTW.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Products
        public ActionResult Index(string searchString, int? page)
        {
            // 1. Logic Tìm kiếm
            var products = db.Products.Include(p => p.Category).AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                // Lọc sản phẩm theo ProductName (hoặc ProductDescription)
                products = products.Where(s => s.ProductName.Contains(searchString) || s.ProductDescription.Contains(searchString));
            }

            ViewBag.SearchString = searchString; // Gửi chuỗi tìm kiếm hiện tại sang View để giữ lại trên ô tìm kiếm

            // 2. Logic Phân trang
            int pageSize = 10; // Số lượng sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Nếu 'page' là null, đặt mặc định là trang 1

            // Phân trang kết quả và gửi sang View
            return View(products.ToList().ToPagedList(pageNumber, pageSize));
        }

        // GET: Home/ProductDetails/5
        public ActionResult ProductDetails(int? id, int? quantity, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product pro = db.Products.Find(id);
            if (pro == null)
            {
                return HttpNotFound();
            }

            // Lấy tất cả các sản phẩm cùng danh mục
            var products = db.Products.Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID).AsQueryable();

            ProductDetailVM model = new ProductDetailVM();

            // Đoạn code liên quan tới phân trang
            // Lấy số trang hiện tại (mặc định là trang 1 nếu không có giá trị)
            int pageNumber = page ?? 1;
            int pageSize = model.PageSize; // Số sản phẩm mỗi trang

            model.product = pro;
            model.RelatedProducts = products.OrderBy(p => p.ProductID).Take(8).ToPagedList(pageNumber, pageSize);
            model.TopProducts = products.OrderByDescending(p => p.OrderDetails.Count()).Take(8).ToPagedList(pageNumber, pageSize);

            if (quantity.HasValue)
            {
                model.quantity = quantity.Value;
            }

            return View(model);
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription," + "ProductPrice,ProductImage,UploadImg")] Product product)
        {
            // Kiểm tra file trước khi kiểm tra ModelState.IsValid (nếu bạn muốn hiển thị lỗi ngay lập tức)
            if (product.UploadImg != null && product.UploadImg.ContentLength > 0)
            {
                string fileExtension = Path.GetExtension(product.UploadImg.FileName).ToLower();

                // Kiểm tra định dạng file thủ công
                if (fileExtension != ".png" && fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".gif")
                {
                    // Thêm lỗi vào ModelState
                    ModelState.AddModelError("UploadImg", "Chỉ chấp nhận các định dạng ảnh PNG, JPG và GIF!");
                }
            }

            if (ModelState.IsValid)
            {

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription," + "ProductPrice,ProductImage,UploadImg")] Product product)
        {
            if (ModelState.IsValid)
            {
                // Thêm đoạn code để lấy đường link của ảnh đã upload và lưu ảnh vào thư mục Content/images
                if (product.UploadImg != null)
                {
                    string filename = Path.GetFileName(product.UploadImg.FileName);
                    string savePath = "~/Content/images/";
                    product.ProductImage = savePath + filename;
                    product.UploadImg.SaveAs(Path.Combine(Server.MapPath(savePath), filename));
                }

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
