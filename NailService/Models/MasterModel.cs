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
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string FullName
        {
            get
            {
                string full = LastName + " " + FirstName;
                if (!string.IsNullOrWhiteSpace(MiddleName))
                    full += " " + MiddleName;
                return full;
            }
        }
        public string Description { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
    }
}
