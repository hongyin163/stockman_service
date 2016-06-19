using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData;
using System.Web.Http.OData.Routing;
using StockMan.EntityModel;
using StockMan.MySqlAccess;

namespace StockMan.Web.RestService.Controllers
{
    /*
    The WebApiConfig class may require additional changes to add a route for this controller. Merge these statements into the Register method of the WebApiConfig class as applicable. Note that OData URLs are case sensitive.

    using System.Web.Http.OData.Builder;
    using System.Web.Http.OData.Extensions;
    using StockMan.EntityModel;
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntitySet<reco_stock_category_index>("Recommend");
    config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
    */
    public class RecommendController : ODataController
    {
        private StockManDBEntities db = new StockManDBEntities();

        // GET: odata/Recommend
        [EnableQuery]
        public IQueryable<reco_stock_category_index> GetRecommend()
        {
            return db.reco_stock_category_index;
        }

        // GET: odata/Recommend(5)
        [EnableQuery]
        public SingleResult<reco_stock_category_index> Getreco_stock_category_index([FromODataUri] string key)
        {
            return SingleResult.Create(db.reco_stock_category_index.Where(reco_stock_category_index => reco_stock_category_index.code == key));
        }

        // PUT: odata/Recommend(5)
        public IHttpActionResult Put([FromODataUri] string key, Delta<reco_stock_category_index> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            reco_stock_category_index reco_stock_category_index = db.reco_stock_category_index.Find(key);
            if (reco_stock_category_index == null)
            {
                return NotFound();
            }

            patch.Put(reco_stock_category_index);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!reco_stock_category_indexExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(reco_stock_category_index);
        }

        // POST: odata/Recommend
        public IHttpActionResult Post(reco_stock_category_index reco_stock_category_index)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.reco_stock_category_index.Add(reco_stock_category_index);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (reco_stock_category_indexExists(reco_stock_category_index.code))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Created(reco_stock_category_index);
        }

        // PATCH: odata/Recommend(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public IHttpActionResult Patch([FromODataUri] string key, Delta<reco_stock_category_index> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            reco_stock_category_index reco_stock_category_index = db.reco_stock_category_index.Find(key);
            if (reco_stock_category_index == null)
            {
                return NotFound();
            }

            patch.Patch(reco_stock_category_index);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!reco_stock_category_indexExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(reco_stock_category_index);
        }

        // DELETE: odata/Recommend(5)
        public IHttpActionResult Delete([FromODataUri] string key)
        {
            reco_stock_category_index reco_stock_category_index = db.reco_stock_category_index.Find(key);
            if (reco_stock_category_index == null)
            {
                return NotFound();
            }

            db.reco_stock_category_index.Remove(reco_stock_category_index);
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool reco_stock_category_indexExists(string key)
        {
            return db.reco_stock_category_index.Count(e => e.code == key) > 0;
        }

        
    }
}
