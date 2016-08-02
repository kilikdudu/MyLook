using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyLook.Models
{
    public class InfoUsuarioACS
    {
        public string id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string password_confirmation { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public CustomFieldsUsuarioACS custom_fields { get; set; }
        public byte[] photo { get; set; }
    }
    public class CustomFieldsUsuarioACS
    {
        public string IdIUGU { get; set; }
    }
}