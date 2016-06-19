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
    builder.EntitySet<sys_comments>("Comments");
    config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
    */
    public class CommentsController : ODataController
    {
        private StockManDBEntities db = new StockManDBEntities();

        // GET: odata/Comments
        [EnableQuery]
        public IQueryable<sys_comments> GetComments()
        {
            return db.sys_comments;
        }

        // GET: odata/Comments(5)
        [EnableQuery]
        public SingleResult<sys_comments> Getsys_comments([FromODataUri] int key)
        {
            return SingleResult.Create(db.sys_comments.Where(sys_comments => sys_comments.code == key));
        }

        // PUT: odata/Comments(5)
        public IHttpActionResult Put([FromODataUri] int key, Delta<sys_comments> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            sys_comments sys_comments = db.sys_comments.Find(key);
            if (sys_comments == null)
            {
                return NotFound();
            }

            patch.Put(sys_comments);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!sys_commentsExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(sys_comments);
        }

        // POST: odata/Comments
        public IHttpActionResult Post(sys_comments sys_comments)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.sys_comments.Add(sys_comments);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (sys_commentsExists(sys_comments.code))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Created(sys_comments);
        }

        // PATCH: odata/Comments(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public IHttpActionResult Patch([FromODataUri] int key, Delta<sys_comments> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            sys_comments sys_comments = db.sys_comments.Find(key);
            if (sys_comments == null)
            {
                return NotFound();
            }

            patch.Patch(sys_comments);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!sys_commentsExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(sys_comments);
        }

        // DELETE: odata/Comments(5)
        public IHttpActionResult Delete([FromODataUri] int key)
        {
            sys_comments sys_comments = db.sys_comments.Find(key);
            if (sys_comments == null)
            {
                return NotFound();
            }

            db.sys_comments.Remove(sys_comments);
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

        private bool sys_commentsExists(int key)
        {
            return db.sys_comments.Count(e => e.code == key) > 0;
        }
    }
}
