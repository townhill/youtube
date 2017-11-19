using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Data.Linq.SqlClient;
using System.Text.RegularExpressions;

namespace youtube.Controllers
{
   
    public class SupermarketsController : Controller
    {
 string[] keyWords = new string[] { @"Chicken","Whisky","Christmas","Pork","Heinz","Tassimo","Lamb","Merlot","Chardonnay","Pinot","Beef","Turkey","Pasta",
                                    "Alpro","Coffee","Cigarettes","Soup","Actimel","Yoghurt","Weetabix","Cereal","Biscuit","Bread","Birthday","Milk",
                                    "Gin", "Brandy", "Butter","Vodka","Dog","Cat","Garlic","McCain","Chocolate","Sugar","Mince","Bleach", "Bitter","Lager" };


        // GET: Supermarkets
        public ActionResult Search(string search)
        {
            // Nov 16th 2017 - check to see if we're clicked a brand link
            var brand = Request.QueryString["brand"];
            if (!string.IsNullOrEmpty(brand))
                ViewBag.brand = brand;
            else
                ViewBag.brand = "";
            // Also added a section
            var section = Request.QueryString["section"];
            if (!string.IsNullOrEmpty(section))
                ViewBag.section = section;
            else
                ViewBag.section = "";

            //  Removed default search 25/10/17
            //if (string.IsNullOrEmpty(search))
            //    search = "Stella";
            var pageLength = Request.QueryString["pageLength"];
            if (string.IsNullOrEmpty(pageLength))
                pageLength = "25";
            ViewBag.search = search;
            ViewBag.pageLength = pageLength;
            return View();
        }

        public ActionResult Tesco()
        {
            return View();
        }


        public ActionResult LoadCSP_auto(string search)
        {
            //
            //  Check to see if we're searching for a brand or a section
            //
            var Brand = Request.Form.GetValues("brand").FirstOrDefault();
            var Section = Request.Form.GetValues("section").FirstOrDefault();

            //jQuery DataTables Param
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //Find paging info
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //
            //  Get the ORDER columns info
            //
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                    + "][data]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            // Reads the built in search
            var searchTerm = Request.Form.GetValues("search[value]").FirstOrDefault();
            // Trim off any spaces fro the end and remove common words like ' and ' , ' or ' , etc
            var modifiedSearch = searchTerm.Replace(" and ", " ").Replace("&", "").Replace(" or ", " ").Replace(",", " ").Replace(".", " ").Replace("the ", "").Replace("-","").Replace("'","");
            // Replace multiple spaces with just one
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            modifiedSearch = regex.Replace(modifiedSearch, " ");

            string[] searchTermSplit = modifiedSearch.Split(' ');
            var tmpSearchTerm = " " + searchTerm;

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt16(start) : 0;
            int recordsTotal = 0;

            using (CSPEntities dc = new CSPEntities())
            {
                // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
                var v = (from a in dc.Items select a);
                var tmp = (from a in dc.Items select a);
                //
                //SEARCHING...
                //
                if (searchTerm == Brand)
                {
                    if (!string.IsNullOrEmpty(Brand))
                    {
                        // Check t see if we did click a brand hyperlink but are now just using the normal search

                        tmp = tmp.Where(a => a.Brand.StartsWith(searchTerm));
                        v = tmp;

                    }
                }
                else if (searchTerm == Section)
                {
                    if (!string.IsNullOrEmpty(Section))
                    {
                        searchTerm = searchTerm.Replace("and", "&");
                        tmp = tmp.Where(a => a.Section.StartsWith(searchTerm));
                        v = tmp;
                    }
                }
                else if (searchTermSplit.Count() == 1)
                {
                    if (!string.IsNullOrEmpty(searchTermSplit[0]))
                    {
                        //
                        // #1st try
                        //
                        tmp = tmp.Where(a => a.Description.Contains(tmpSearchTerm) || a.Description.StartsWith(modifiedSearch) || a.Supermarket.Contains(modifiedSearch) || a.Section.Equals(modifiedSearch));
                        if (tmp.Count() == 0)
                        {
                            //
                            // #2nd try
                            //
                            v = v.Where(a => a.Description.Contains(modifiedSearch) || a.Supermarket.Contains(modifiedSearch) || a.Offers.Contains(modifiedSearch) || a.Section.Contains(modifiedSearch) || a.Offers.Contains(modifiedSearch));
                        }
                        else
                            v = tmp;
                    }
                }
                else if (searchTermSplit.Count() == 2)
                {
                    string s1 = searchTermSplit[0];
                    string s2 = searchTermSplit[1];
                    tmp = tmp.Where(a => (a.Description.Contains(s1) && a.Description.Contains(s2)) || (a.Section.Contains(s1) && a.Section.Contains(s2) || (a.Offers.Contains(s1) && a.Offers.Contains(s2))));
                    v = tmp;
                }
                else // search contains three words
                {
                    string s1 = searchTermSplit[0];
                    string s2 = searchTermSplit[1];
                    string s3 = searchTermSplit[2];
                    tmp = tmp.Where(a => (a.Description.Contains(s1) && a.Description.Contains(s2) && a.Description.Contains(s3)) || (a.Section.Contains(s1) && a.Section.Contains(s2) && a.Section.Contains(s3) || (a.Offers.Contains(s1) && a.Offers.Contains(s2) && a.Offers.Contains(s3))));
                    v = tmp;
                }

                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (sortColumn == "Price")
                        sortColumn = "iPrice";
                    v = v.OrderBy(sortColumn + " " + sortColumnDir);
                }

                recordsTotal = v.Count();

                // Just get the data we need
                var data = v.Skip(skip).Take(pageSize).ToList();

                //  Transform some columns and keyworkds into hyperlinks
                foreach (var rec in data)
                {
                    rec.Supermarket = string.Format("<a href='http://christownhill.com/supermarkets/search/{0}'>{0}</a>", rec.Supermarket);
                    //rec.Supermarket = string.Format("<a href='http://comparesupermarketprices.co.uk/supermarkets/search/{0}'>{0}</a>", rec.Supermarket);
                    rec.Price = string.Format("{0:C}", rec.iPrice);
                    foreach (string keyword in keyWords)
                    {
                        var tmpStr = string.Format(" {0} ", keyword);
                        if (rec.Description.Contains(tmpStr))
                        {
                            rec.Description = rec.Description.Replace(tmpStr, string.Format("<a href='http://christownhill.com/supermarkets/search/{0}'>{1}</a>", keyword, tmpStr));
                            //rec.Description = rec.Description.Replace(tmpStr, string.Format("<a href='http://comparesupermarketprices.co.uk/supermarkets/search/{0}'>{1}</a>", keyword, tmpStr));
                        }
                        else
                        {
                            var tmpStrSecond = string.Format("{0} ", keyword);
                            if (rec.Description.Contains(tmpStrSecond))
                            {
                                rec.Description = rec.Description.Replace(tmpStr, string.Format("<a href='http://christownhill.com/supermarkets/search/{0}'>{1}</a>", keyword, tmpStr));
                                //rec.Description = rec.Description.Replace(tmpStrSecond, string.Format("<a href='http://comparesupermarketprices.co.uk/supermarkets/search/{0}'>{1}</a>", keyword, tmpStrSecond));
                            }
                        }
                    }
                }
                //
                //  Log the search
                //
                Models.LogWriter log = new Models.LogWriter(searchTerm + " " + recordsTotal.ToString());
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Get_5000_Tesco()
        {
            //jQuery DataTables Param
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //Find paging info

            int recordsTotal = 0;

            using (CSPEntities dc = new CSPEntities())
            {
                // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
                var v = (from a in dc.Items select a);

                v = v.Where(a => a.Supermarket.StartsWith("Tesco"));
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)

                v = v.OrderBy("Description" + " " + "asc");

                recordsTotal = v.Count();
                var data = v.Take(500).ToList();
                //   data.Add(Google);
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
        }
    }
}