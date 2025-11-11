using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _22dh114822_LTW.Models.ViewModel
{
    public class HomeProductVM
    {
        public string SearchTerm {  get; set; }

        public int PagedNumber { get; set; }
        public int PageSize { get; set; } = 10;

        public List<Product> FeatureProducts { get; set; }

        public PagedList.IPagedList<Product> NewProducts { get; set; }
    }
}