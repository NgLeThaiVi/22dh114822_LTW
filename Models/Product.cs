namespace _22dh114822_LTW.Models
{
    using System;
    using System.Collections.Generic;
    using System.Net.NetworkInformation;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.ComponentModel.DataAnnotations;
    using System.Web;

    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            this.OrderDetails = new HashSet<OrderDetail>();
            ProductImage = "~/Content/images/default_img.png";
        }

        //scala properties
        public int ProductID { get; set; }
        public int CategoryID { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductImage { get; set; }

        //reference properties
        public virtual Category Category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        // Thêm trường dữ liệu UploadImg kiểu HttpPostedFile để upload file ảnh
        // Dùng thuộc tính [NotMapped] để đánh dấu đây là trường dữ liệu không lưu vào DB
        [RegularExpression(@"^([a-zA-Z0-9\s_\\.\-:])+(.jpg|.jpeg|.gif|.png|.JPG|.JPEG|.GIF|.PNG)$",
            ErrorMessage = "Chỉ chấp nhận các định dạng ảnh PNG, JPG và GIF!")]
        [NotMapped]
        public HttpPostedFileBase UploadImg { get; set; }
    }
}
