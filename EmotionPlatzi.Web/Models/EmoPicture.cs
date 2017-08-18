using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace EmotionPlatzi.Web.Models
{
    public class EmoPicture
    {
        //Clase Maestro
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        //Relacion con EmoFace
        public virtual ObservableCollection<EmoFace> Faces { get; set; }
    }
}