using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailService
{
    public class MasterModel
    {
        public int IDMasters { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }

        // Дополнительные свойства для отображения
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }

        public string FullName
        {
            get
            {
                return $"{LastName} {FirstName} {MiddleName}".Trim();
            }
        }

        public string ShortName
        {
            get
            {
                if (string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(FirstName))
                    return "";

                string result = $"{LastName} {FirstName[0]}.";

                if (!string.IsNullOrEmpty(MiddleName))
                {
                    result += $"{MiddleName[0]}.";
                }

                return result;
            }
        }
    }
}
