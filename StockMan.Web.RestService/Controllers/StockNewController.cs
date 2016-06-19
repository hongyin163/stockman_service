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
using StockMan.MySqlAccess;
using StockMan.EntityModel;
using StockMan.Web.RestService.Filters;

namespace StockMan.Web.RestService.Controllers
{
    /*
    The WebApiConfig class may require additional changes to add a route for this controller. Merge these statements into the Register method of the WebApiConfig class as applicable. Note that OData URLs are case sensitive.

    using System.Web.Http.OData.Builder;
    using System.Web.Http.OData.Extensions;
    using StockMan.EntityModel;
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntitySet<stock_new>("StockNew");
    config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
    */
     [IdentityBasicAuthentication]
    public class StockNewController : ODataController
    {
        private StockManDBEntities db = new StockManDBEntities();

        // GET: odata/StockNew
        [EnableQuery]
        public IQueryable<stock_new> GetStockNew()
        {
            return db.stock_new;
        }

        // GET: odata/StockNew(5)
        [EnableQuery]
        public SingleResult<stock_new> GetStock_New([FromODataUri] string key)
        {
            return SingleResult.Create(db.stock_new.Where(stock_New => stock_New.code == key));
        }

        // PUT: odata/StockNew(5)
        public IHttpActionResult Put([FromODataUri] string key, Delta<stock_new> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            stock_new stock_New = db.stock_new.Find(key);
            if (stock_New == null)
            {
                return NotFound();
            }

            patch.Put(stock_New);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Stock_NewExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(stock_New);
        }

        // POST: odata/StockNew
        public IHttpActionResult Post(stock_new stock_New)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.stock_new.Add(stock_New);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (Stock_NewExists(stock_New.code))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Created(stock_New);
        }

        // PATCH: odata/StockNew(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public IHttpActionResult Patch([FromODataUri] string key, Delta<stock_new> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            stock_new stock_New = db.stock_new.Find(key);
            if (stock_New == null)
            {
                return NotFound();
            }

            patch.Patch(stock_New);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Stock_NewExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(stock_New);
        }

        // DELETE: odata/StockNew(5)
        public IHttpActionResult Delete([FromODataUri] string key)
        {
            stock_new stock_New = db.stock_new.Find(key);
            if (stock_New == null)
            {
                return NotFound();
            }

            db.stock_new.Remove(stock_New);
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

        private bool Stock_NewExists(string key)
        {
            return db.stock_new.Count(e => e.code == key) > 0;
        }
    }
}
