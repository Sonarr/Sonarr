using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Controllers;

namespace NzbDrone.Web.Controllers
{
    public class SeriesController : Controller
    {
        private readonly ISeriesController _seriesController;
        //
        // GET: /Series/

        public SeriesController(ISeriesController seriesController)
        {
            _seriesController = seriesController;
        }

        public ActionResult Index()
        {
            ViewData.Model = _seriesController.GetSeries().ToList();
            return View();
        }


        public ActionResult Sync()
        {
            _seriesController.SyncSeriesWithDisk();
            return RedirectToAction("Index");
        }


        //
        // GET: /Series/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Series/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Series/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Series/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Series/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Series/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Series/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
