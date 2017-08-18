using EmotionPlatzi.Web.Models;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Emotion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;

namespace EmotionPlatzi.Web.Util
{
    public class EmotionHelper
    {
        public EmotionServiceClient emoCliente;

        public EmotionHelper(string key)
        {
            //Inicializamos el cliente de Emotion que recibe una Llave para usarse
            emoCliente = new EmotionServiceClient(key);
        }

        //Metodo que se ejecuta en un hilo asincrono
        public async Task<EmoPicture> DetectAndExtractFacesAsync(Stream imageStream)
        {
            //Recibe un array de Emociones
            Emotion[] emotions  = await emoCliente.RecognizeAsync(imageStream);

            //Variable e instancia del modelo EmoPicture
            var emoPicture = new EmoPicture();

            /*Es un metodo que devuelve una lista, que recibe el Array de emociones, y la fotografia
            extrae las caras de la pintura*/
            emoPicture.Faces = ExtractFaces(emotions, emoPicture);

            //Retornamos la Pintura
            return emoPicture;
        }

        //ObservableCollection son colecciones de datos que reciben notificacion al insertar o eliminar
        private ObservableCollection<EmoFace> ExtractFaces(Emotion[] emotions, EmoPicture emoPicture)
        {
            //Crea una lista observable de caras
            var facesList = new ObservableCollection<EmoFace>();

            //Emotions es una lista, asi que se itera para agregar datos de la cara
            foreach (var emotion in emotions)
            {
                var emoface = new EmoFace()
                {
                    X = emotion.FaceRectangle.Left,
                    Y = emotion.FaceRectangle.Top,
                    Width = emotion.FaceRectangle.Width,
                    Height = emotion.FaceRectangle.Height,
                    Picture = emoPicture,
                };
                //Aqui se devuelve la lista con los Scores de las emociones de dicha cara
                emoface.Emotions = ProcessEmotions(emotion.Scores, emoface);

                //Se agrega los datos
                facesList.Add(emoface);
            }

            //Retornamos la lista de Caras
            return facesList;
        }

        private ObservableCollection<EmoEmotion> ProcessEmotions(EmotionScores scores, EmoFace emoface)
        {
            //Creamos una lista observable de emociones
            var emotionList = new ObservableCollection<EmoEmotion>();

            //Devuelve el tipo y las propiedades publicas y que son parte de una instancia
            var properties = scores.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            //Las propiedades son generias, las filtramos por el tipo Float que es el tipo 
            //de dato que necesitamos de Scores
            var filterProperties = properties.Where(p => p.PropertyType == typeof(float));

            //Incia la iteracion
            var emotype = EmoEmotionEnum.Undetermined;
            foreach (var prop in filterProperties)
            {
                //este metodo recibe dos parametros, 1 un string de valor, y otro un valor de salida del Enum
                if (!Enum.TryParse<EmoEmotionEnum>(prop.Name, out emotype))
                    emotype = EmoEmotionEnum.Undetermined;

                //Instancia del EmoEmotion
                var emoEmotion = new EmoEmotion();
                //Agregamos el valor que se itera de las filtraciones
                emoEmotion.Score = (float)prop.GetValue(scores);
                emoEmotion.EmotionType = emotype;
                emoEmotion.Face = emoface;

                //Lo agregamos 
                emotionList.Add(emoEmotion);
            }

            //Retornamos la lista
            return emotionList;
        }

        
    }
}