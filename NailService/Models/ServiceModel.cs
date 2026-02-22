using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace NailService
{
    public class ServiceModel
    {
        public int IDServices { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public int Category { get; set; }
        public string CategoryName { get; set; }
        public string Photo { get; set; } // Имя файла изображения
        public bool IsActive { get; set; }
        public Image ServiceImage { get; set; } // Объект Image для отображения

        // Конструктор
        public ServiceModel()
        {
            ServiceName = string.Empty;
            Description = string.Empty;
            Photo = string.Empty;
            CategoryName = string.Empty;
            ServiceImage = null;
        }
    }
}
