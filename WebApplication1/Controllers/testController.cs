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
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class testController : ApiController
    {
        private goDbEntities db = new goDbEntities();

        // GET: api/test
        public IQueryable<Territory> GetTerritories()
        {
            return db.Territories;
        }

        // GET: api/test/5
        [ResponseType(typeof(Territory))]
        public IHttpActionResult GetTerritory(Guid id)
        {
            Territory territory = db.Territories.Find(id);
            if (territory == null)
            {
                return NotFound();
            }

            return Ok(territory);
        }

        // PUT: api/test/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutTerritory(Guid id, Territory territory)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != territory.TerritoryId)
            {
                return BadRequest();
            }

            db.Entry(territory).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TerritoryExists(id))
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

        // POST: api/test
        [ResponseType(typeof(Territory))]
        public IHttpActionResult PostTerritory(Territory territory)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Territories.Add(territory);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (TerritoryExists(territory.TerritoryId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = territory.TerritoryId }, territory);
        }

        // DELETE: api/test/5
        [ResponseType(typeof(Territory))]
        public IHttpActionResult DeleteTerritory(Guid id)
        {
            Territory territory = db.Territories.Find(id);
            if (territory == null)
            {
                return NotFound();
            }

            db.Territories.Remove(territory);
            db.SaveChanges();

            return Ok(territory);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TerritoryExists(Guid id)
        {
            return db.Territories.Count(e => e.TerritoryId == id) > 0;
        }
    }
}