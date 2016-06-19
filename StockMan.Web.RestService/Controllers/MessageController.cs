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

namespace StockMan.Web.RestService.Controllers
{
    public class MessageController : ApiController
    {
        private StockManDBEntities db = new StockManDBEntities();

        // GET: api/Message
        public IQueryable<user_message> Getuser_message()
        {
            return db.user_message;
        }

        // GET: api/Message/5
        [ResponseType(typeof(user_message))]
        public IHttpActionResult Getuser_message(string id)
        {
            user_message user_message = db.user_message.Find(id);
            if (user_message == null)
            {
                return NotFound();
            }

            return Ok(user_message);
        }

        // PUT: api/Message/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putuser_message(string id, user_message user_message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user_message.code)
            {
                return BadRequest();
            }

            db.Entry(user_message).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!user_messageExists(id))
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

        // POST: api/Message
        [ResponseType(typeof(user_message))]
        public IHttpActionResult Postuser_message(user_message user_message)
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

            return CreatedAtRoute("DefaultApi", new { id = user_message.code }, user_message);
        }

        // DELETE: api/Message/5
        [ResponseType(typeof(user_message))]
        public IHttpActionResult Deleteuser_message(string id)
        {
            user_message user_message = db.user_message.Find(id);
            if (user_message == null)
            {
                return NotFound();
            }

            db.user_message.Remove(user_message);
            db.SaveChanges();

            return Ok(user_message);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool user_messageExists(string id)
        {
            return db.user_message.Count(e => e.code == id) > 0;
        }
        [HttpGet]
        public IHttpActionResult GetMyMessage(string id)
        {
            //var list = db.user_message
            //      .Where(p => p.user_id == id)
            //      .OrderByDescending(p => p.createtime)
            //      .Take(1)
            //      .ToList();
            string sql = @"SELECT code,
                            user_id,
                            content,
                            createtime,
                            hint,
                            title,
                            state,
                            type,
                            notice_state
                        FROM `user_message` where user_id=@p0 and state='0' order by createtime desc limit 5;
                        update `user_message` set state='1' where user_id=@p0 ;";
            var list = db.Database.SqlQuery<user_message>(sql, id).ToList();


            return Ok(list);
        }

    }
}