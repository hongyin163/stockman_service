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
using Microsoft.Data.Edm;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Properties;
//using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Atom;
using StockMan.Web.RestService.Filters;
namespace StockMan.Web.RestService.Controllers
{
    /*
    The WebApiConfig class may require additional changes to add a route for this controller. Merge these statements into the Register method of the WebApiConfig class as applicable. Note that OData URLs are case sensitive.

    using System.Web.Http.OData.Builder;
    using System.Web.Http.OData.Extensions;
    using StockMan.EntityModel;
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntitySet<sys_goodidea>("GoodIdea");
    config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
    */
    [IdentityBasicAuthentication]
    public class GoodIdeaController : ODataController
    {
        private StockManDBEntities db = new StockManDBEntities();

        // GET: odata/GoodIdea
        //[HttpOptions]
        [HttpGet]
        [EnableQuery]
        public IQueryable<sys_goodidea> GetGoodIdea()
        {
            return db.sys_goodidea;
        }

        // GET: odata/GoodIdea(5)
        [EnableQuery]
        public SingleResult<sys_goodidea> GetSys_GoodIdea([FromODataUri] string key)
        {
            return SingleResult.Create(db.sys_goodidea.Where(sys_GoodIdea => sys_GoodIdea.code == key));
        }

        // PUT: odata/GoodIdea(5)
        public IHttpActionResult Put([FromODataUri] string key, Delta<sys_goodidea> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            sys_goodidea sys_GoodIdea = db.sys_goodidea.Find(key);
            if (sys_GoodIdea == null)
            {
                return NotFound();
            }

            patch.Put(sys_GoodIdea);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Sys_GoodIdeaExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(sys_GoodIdea);
        }

        // POST: odata/GoodIdea
        [HttpPost]
        public IHttpActionResult Post(sys_goodidea sys_GoodIdea)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var item = db.sys_goodidea.FirstOrDefault(p => p.code == sys_GoodIdea.code);
            if (item == null)
            {
                db.sys_goodidea.Add(sys_GoodIdea);
            }
            else
            {
                item.description = sys_GoodIdea.description;
            }


            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (Sys_GoodIdeaExists(sys_GoodIdea.code))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Created(sys_GoodIdea);
        }

        // PATCH: odata/GoodIdea(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public IHttpActionResult Patch([FromODataUri] string key, Delta<sys_goodidea> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            sys_goodidea sys_GoodIdea = db.sys_goodidea.Find(key);
            if (sys_GoodIdea == null)
            {
                return NotFound();
            }

            patch.Patch(sys_GoodIdea);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Sys_GoodIdeaExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(sys_GoodIdea);
        }

        // DELETE: odata/GoodIdea(5)
        public IHttpActionResult Delete([FromODataUri] string key)
        {
            sys_goodidea sys_GoodIdea = db.sys_goodidea.Find(key);
            if (sys_GoodIdea == null)
            {
                return NotFound();
            }

            db.sys_goodidea.Remove(sys_GoodIdea);
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


        private bool Sys_GoodIdeaExists(string key)
        {
            return db.sys_goodidea.Count(e => e.code == key) > 0;
        }
        [HttpPost]
        public IHttpActionResult Up(string id)
        {
            string sql = @"update sys_goodidea set up=up+1 where code=@p0";
            db.Database.ExecuteSqlCommand(sql, id);
            return Ok();
        }
        [HttpPost]
        public IHttpActionResult Down(string id)
        {
            string sql = @"update sys_goodidea set down=down+1 where code=@p0";
            db.Database.ExecuteSqlCommand(sql, id);
            return Ok();
        }
        [HttpGet]
        public IHttpActionResult GetMetaData(string id)
        {

            return Ok("1234");
        }
    }
}
