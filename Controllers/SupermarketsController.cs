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
        // GET: Supermarkets
        public ActionResult Search(string search)
        {
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
          //  var Paddy = RouteData.Values["search"];
            //jQuery DataTables Param
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //Find paging info
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find order columns info
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                    + "][data]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            //find search columns info
            //var description = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
            //var Supermarket = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();

            // Reads the built in search
            var searchTerm = Request.Form.GetValues("search[value]").FirstOrDefault();
   // Trim off any spaces fro the end and remove common words like ' and ' , ' or ' , etc
            var modifiedSearch = searchTerm.Replace(" and ", " ").Replace("&", "").Replace(" or ", " ").Replace(",", " ").Replace(".", " ");

            // Replace multiple spaces with just one
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            modifiedSearch = regex.Replace(modifiedSearch, " ");

            string[] searchTermSplit = modifiedSearch.Split(' ');
            var tmpSearchTerm = " " + searchTerm;
            Models.LogWriter log = new Models.LogWriter(searchTerm + "   Returns " + "Here!");
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt16(start) : 0;
            int recordsTotal = 0;

            using (CSPEntities dc = new CSPEntities())
            {
                // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
                var v = (from a in dc.Items select a);
                var tmp = (from a in dc.Items select a);
                //SEARCHING...
                if (searchTermSplit.Count() == 1)
                {
                    if (!string.IsNullOrEmpty(searchTermSplit[0]))
                    {
                        //
                        // #1st try
                        //
                        tmp = tmp.Where(a => a.Description.Contains(tmpSearchTerm) || a.Description.StartsWith(searchTerm));
                        if (tmp.Count() == 0)
                        {
                            //
                            // #2nd try
                            //
                            v = v.Where(a => a.Description.Contains(searchTerm) || a.Offers.Contains(searchTerm) || a.Section.Contains(searchTerm));
                        }
                        else
                            v = tmp;
                    }
                }
                else if (searchTermSplit.Count() == 2)
                {
                    string s1 = searchTermSplit[0];
                    string s2 = searchTermSplit[1];
                    tmp = tmp.Where( a => (a.Description.Contains(s1) && a.Description.Contains(s2)) || (a.Section.Contains(s1) && a.Section.Contains(s2)));
                    v = tmp;
                }
                else // search contains three words
                {
                    string s1 = searchTermSplit[0];
                    string s2 = searchTermSplit[1];
                    string s3 = searchTermSplit[2];
                    tmp = tmp.Where(a => (a.Description.Contains(s1) && a.Description.Contains(s2) && a.Description.Contains(s3)) || (a.Section.Contains(s1) && a.Section.Contains(s2) && a.Section.Contains(s3)));
                    v = tmp;
                }

                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.OrderBy(sortColumn + " " + sortColumnDir);
                }

                //youtube.Item Google = new youtube.Item();
                //Google.Description = "test";



                recordsTotal = v.Count();

                Models.LogWriter logDebug = new Models.LogWriter(searchTerm + "   Returns " + recordsTotal.ToString());
                var data = v.Skip(skip).Take(pageSize).ToList();
                //   data.Add(Google);
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