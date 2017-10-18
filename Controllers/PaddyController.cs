using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Data.Linq.SqlClient;

namespace youtube.Controllers
{
    public class PaddyController : Controller
    {
        // GET: Paddy
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Items()
        {
            return View();
        }

        public ActionResult search()
        {
            return View();
        }

        public ActionResult csp()
        {
            return View();
        }

        public ActionResult csp2()
        {
            return View();
        }

        public ActionResult loaddata()
        {
          using(CSPEntities dc = new CSPEntities())
                {
                var data = dc.Items.OrderBy(a => a.Description).Select(p => new { p.Description, p.Supermarket, p.Price }).Take(50).ToList();

                //var query = from p in dc.Items
                //            orderby p.Description
                //            select p.Description
                //            select p.Supermarket, p.Price;


                return Json(new { data = data }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult LoadItems()
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
             var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][data]").FirstOrDefault();
        //  sortColumn = "Price";
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            using (CSPEntities dc = new CSPEntities())
            {
                var v = (from a in dc.Items.Select(a => new { a.Description, a.Supermarket, a.Price }) select a);
                // Sorting
                if(!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.OrderBy(sortColumn + " " + sortColumnDir);
                }
                totalRecords = v.Count();
                var data = v.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords,  data = data }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult LoadCSP(string search)
        {
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
            var description = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
            var Supermarket = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();



            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt16(start) : 0;
            int recordsTotal = 0;

            using (CSPEntities dc = new CSPEntities())
            {
                // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
                var v = (from a in dc.Items select a);

                //SEARCHING...
                if (!string.IsNullOrEmpty(description))
                {
                    v = v.Where(a => a.Description.Contains(description) || a.Offers.Contains(description));
                    //v = from a in dc.Items
                    //    where SqlMethods.Like(a.Description, description) 
                    //select new { a.Description };
                }
                if (!string.IsNullOrEmpty(Supermarket))
                {
                    v = v.Where(a => a.Supermarket == Supermarket);
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.OrderBy(sortColumn + " " + sortColumnDir);
                }

                recordsTotal = v.Count();
                var data = v.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult LoadCSP_auto(string id)
        {
            // Is there a query string?
            var test = Request.QueryString.Get("search");

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
            var searchTerm  = Request.Form.GetValues("search[value]").FirstOrDefault();
            string[] searchTermSplit = searchTerm.Split(' ');

            var tmpSearchTerm = " " + searchTerm;

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
                            v = v.Where(a => a.Description.Contains(searchTerm));
                        }
                        else
                            v = tmp;
                    }
                }
                else
                {
                    string s1 = searchTermSplit[0];
                    string s2 = searchTermSplit[1];
                    tmp = tmp.Where(a => a.Description.Contains(s1) && a.Description.Contains(s2));
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
                var data = v.Skip(skip).Take(pageSize).ToList();
             //   data.Add(Google);
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
        }


    }
}