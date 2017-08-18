using EmotionPlatzi.Web.Models;
using EmotionPlatzi.Web.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EmotionPlatzi.Web.Controllers
{
    public class EmoUploaderController : Controller
    {
        string serverFolderPath;
        EmotionHelper emoHelper;
        string key;
        EmotionPlatziWebContext db = new EmotionPlatziWebContext();

        public EmoUploaderController()
        {
            serverFolderPath = ConfigurationManager.AppSettings["UPLOAD_DIR"];
            key = ConfigurationManager.AppSettings["EMOTION_KEY"];
            emoHelper = new EmotionHelper(key);
        }

        public object Pathfile { get; private set; }

        // GET: EmoUploader
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(HttpPostedFileBase file)
        {
            if (file?.ContentLength > 0)
            {
                //Genero una cadena aleatoria de texto
                var pictureName = Guid.NewGuid().ToString();
                //Concateno el nombre y la extension
                pictureName += Path.GetExtension(file.FileName);

                //Agregamos la ruta
                //Server.MapPath genera una ruta en disco de mi servidor
                var route = Server.MapPath(serverFolderPath);
                route = route +"/"+ pictureName;

                //Lo guardamos
                file.SaveAs(route);

                //Llama el metodo del Helper
                var emoPicture = await emoHelper.DetectAndExtractFacesAsync(file.InputStream);

                //Agregamos el nombre y la ruta
                emoPicture.Name = file.FileName;
                emoPicture.Path = serverFolderPath + "/" + pictureName;

                //Se agrega
                db.EmoPictures.Add(emoPicture);

                //Y asincronamente se guarda en la Base de Datos
                await db.SaveChangesAsync();

                //Retorna hacia el detalle de la Pintura
                return RedirectToAction("Details", "EmoPictures", new { Id = emoPicture.Id});
            }

            return View();
        }
    }
}