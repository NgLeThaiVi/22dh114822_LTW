using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _22dh114822_LTW.Models.ViewModel
{
    public class SearchProductVM
    {
        public string SearchTerm { get; set; }
        public List<Product> Products { get; set; }
    }
}