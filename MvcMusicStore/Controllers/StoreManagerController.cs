using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using MvcMusicStore.Models;
using Nest;

namespace MvcMusicStore.Controllers
{
    //[Authorize(Roles = "Administrator")]
    public class StoreManagerController : Controller
    {
        private MusicStoreEntities db = new MusicStoreEntities();

        private static ElasticClient elasticClient
        {
            get
            {
                var setting = new ConnectionSettings(new Uri("http://localhost:9200"));
                setting.SetDefaultIndex("musicstore");
                return new ElasticClient(setting);
            }
        }

        public ActionResult Reindex()
        {
            var client = elasticClient;

            foreach (var album in db.Albums)
            {
                client.Index(album, "musicstore", "albums", album.AlbumId);
            }

            return RedirectToAction("Index");
        }

        //
        // GET: /StoreManager/

        public ViewResult Index()
        {
            var albums = db.Albums.Include(a => a.Genre).Include(a => a.Artist);
            return View(albums.OrderBy(a => a.Title).ToList());
        }

        //
        // GET: /StoreManager/Details/5

        public ViewResult Details(int id)
        {
            Album album = db.Albums.Find(id);
            return View(album);
        }

        //
        // GET: /StoreManager/Create

        public ActionResult Create()
        {
            ViewBag.GenreId = new SelectList(db.Genres.OrderBy(o => o.Name), "GenreId", "Name");
            ViewBag.ArtistId = new SelectList(db.Artists.OrderBy(o => o.Name), "ArtistId", "Name");
            return View();
        }

        //
        // POST: /StoreManager/Create

        [HttpPost]
        public ActionResult Create(Album album)
        {
            if (ModelState.IsValid)
            {
                db.Albums.Add(album);
                db.SaveChanges();

                elasticClient.Index(album, "musicstore", "albums", album.AlbumId);

                return RedirectToAction("Index");
            }

            ViewBag.GenreId = new SelectList(db.Genres.OrderBy(o => o.Name), "GenreId", "Name", album.GenreId);
            ViewBag.ArtistId = new SelectList(db.Artists.OrderBy(a => a.Name), "ArtistId", "Name", album.ArtistId);
            return View(album);
        }

        //
        // GET: /StoreManager/Edit/5

        public ActionResult Edit(int id)
        {
            Album album = db.Albums.Find(id);
            ViewBag.GenreId = new SelectList(db.Genres.OrderBy(o => o.Name), "GenreId", "Name", album.GenreId);
            ViewBag.ArtistId = new SelectList(db.Artists.OrderBy(o => o.Name), "ArtistId", "Name", album.ArtistId);
            return View(album);
        }

        //
        // POST: /StoreManager/Edit/5

        [HttpPost]
        public ActionResult Edit(Album album)
        {
            if (ModelState.IsValid)
            {
                db.Entry(album).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.GenreId = new SelectList(db.Genres.OrderBy(o => o.Name), "GenreId", "Name", album.GenreId);
            ViewBag.ArtistId = new SelectList(db.Artists.OrderBy(o => o.Name), "ArtistId", "Name", album.ArtistId);
            return View(album);
        }

        //
        // GET: /StoreManager/Delete/5

        public ActionResult Delete(int id)
        {
            Album album = db.Albums.Find(id);
            return View(album);
        }

        //
        // POST: /StoreManager/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Album album = db.Albums.Find(id);
            db.Albums.Remove(album);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}