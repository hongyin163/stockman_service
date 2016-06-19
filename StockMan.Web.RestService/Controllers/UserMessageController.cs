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
    builder.EntitySet<user_message>("UserMessage");
    config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
    */
    public class UserMessageController : ODataController
    {
        private StockManDBEntities db = new StockManDBEntities();

        // GET: odata/UserMessage
        [EnableQuery]
        public IQueryable<user_message> GetUserMessage()
        {
            return db.user_message;
        }

        // GET: odata/UserMessage(5)
        [EnableQuery]
        public SingleResult<user_message> Getuser_message([FromODataUri] string key)
        {
            return SingleResult.Create(db.user_message.Where(user_message => user_message.code == key));
        }

        // PUT: odata/UserMessage(5)
        public IHttpActionResult Put([FromODataUri] string key, Delta<user_message> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            user_message user_message = db.user_message.Find(key);
            if (user_message == null)
            {
                return NotFound();
            }

            patch.Put(user_message);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!user_messageExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(user_message);
        }

        // POST: odata/UserMessage
        public IHttpActionResult Post(user_message user_message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.user_message.Add(user_message);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (user_messageExists(user_message.code))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Created(user_message);
        }

        // PATCH: odata/UserMessage(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public IHttpActionResult Patch([FromODataUri] string key, Delta<user_message> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            user_message user_message = db.user_message.Find(key);
            if (user_message == null)
            {
                return NotFound();
            }

            patch.Patch(user_message);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!user_messageExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(user_message);
        }

        // DELETE: odata/UserMessage(5)
        public IHttpActionResult Delete([FromODataUri] string key)
        {
            user_message user_message = db.user_message.Find(key);
            if (user_message == null)
            {
                return NotFound();
            }

            db.user_message.Remove(user_message);
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

        private bool user_messageExists(string key)
        {
            return db.user_message.Count(e => e.code == key) > 0;
        }
    }
}
