using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using MvcMusicStore.Models;
using Nest;

namespace MvcMusicStore.Controllers
{
    public class StoreController : Controller
    {
        MusicStoreEntities storeDB = new MusicStoreEntities();

        private static ElasticClient elasticClient
        {
            get
            {
                var setting = new ConnectionSettings(new Uri("http://localhost:9200"));
                setting.SetDefaultIndex("musicstore");
                return new ElasticClient(setting);
            }
        }

        public ActionResult Search(string q)
        {
            var result = elasticClient.Search<Album>(body => body.Query(query => query.QueryString(qs => qs.Query(q))));

            var genre = new Genre { Description = "Search results for " + q, Albums = result.Documents.ToList() };

            return View("Browse", genre);
        }


        //
        // GET: /Store/

        public ActionResult Index()
        {
            var genres = storeDB.Genres.OrderBy(o => o.Name).ToList();

            return View(genres);
        }

        //
        // GET: /Store/Browse?genre=Disco

        public ActionResult Browse(string genre)
        {
            // Retrieve Genre and its Associated Albums from database
            var genreModel = storeDB.Genres.Include("Albums")
                .Single(g => g.Name == genre);

            return View(genreModel);
        }

        //
        // GET: /Store/Details/5

        public ActionResult Details(int id)
        {
            var album = storeDB.Albums.Find(id);

            return View(album);
        }

        //
        // GET: /Store/GenreMenu

        [ChildActionOnly]
        public ActionResult GenreMenu()
        {
            var genres = storeDB.Genres.OrderBy(o => o.Name).ToList();

            return PartialView(genres);
        }

        [HttpPost]
        public JsonResult Like(int id)
        {
            Album album = storeDB.Albums.Find(id);
            album.Likes = album.Likes + 1;
            storeDB.Entry(album).State = EntityState.Modified;
            storeDB.SaveChanges();

            return Json(album.Likes);
        }
    }
}