using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using StockMan.EntityModel;
using StockMan.MySqlAccess;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using Newtonsoft.Json;
using System.Web.Mvc;

namespace StockMan.Web.RestService.Controllers
{
    public class RelatedObjectController : ApiController
    {
        private StockManDBEntities db = new StockManDBEntities();
        public IRelatedDataService service = new RelatedDataService();
        // GET: api/RelatedObject
        public IQueryable<related_object_define> Getrelated_object_define()
        {
            return db.related_object_define;
        }

        // GET: api/RelatedObject/5
        [ResponseType(typeof(related_object_define))]
        public IHttpActionResult Getrelated_object_define(string id)
        {
            related_object_define related_object_define = db.related_object_define.Find(id);
            if (related_object_define == null)
            {
                return NotFound();
            }

            return Ok(related_object_define);
        }

        // PUT: api/RelatedObject/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putrelated_object_define(string id, related_object_define related_object_define)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != related_object_define.code)
            {
                return BadRequest();
            }

            db.Entry(related_object_define).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!related_object_defineExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/RelatedObject
        [ResponseType(typeof(related_object_define))]
        public IHttpActionResult Postrelated_object_define(related_object_define related_object_define)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.related_object_define.Add(related_object_define);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (related_object_defineExists(related_object_define.code))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = related_object_define.code }, related_object_define);
        }

        // DELETE: api/RelatedObject/5
        [ResponseType(typeof(related_object_define))]
        public IHttpActionResult Deleterelated_object_define(string id)
        {
            related_object_define related_object_define = db.related_object_define.Find(id);
            if (related_object_define == null)
            {
                return NotFound();
            }

            db.related_object_define.Remove(related_object_define);
            db.SaveChanges();

            return Ok(related_object_define);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool related_object_defineExists(string id)
        {
            return db.related_object_define.Count(e => e.code == id) > 0;
        }

        /// <summary>
        /// {
        ///     field:['','']
        ///     data:[[]]
        /// 
        /// }
        ///  //api/RelateObject/GetData/R001
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        public IHttpActionResult GetData(string id)
        {
            string[] ids = id.Split(',');
            string str = "";
            foreach (string i in ids)
            {
                if (str.Length == 0)
                {
                    str = service.GetData(i);
                }
                else
                {
                    str += "," + service.GetData(i);
                }
            }
            if (ids.Length == 1)
            {

                return Ok(str);
            }
            else
            {
                return Ok("[" + str + "]");
            }
        }
        public IHttpActionResult PostData(string id, [FromBody] string data)
        {
            try
            {
                service.InsertData(id, data);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}