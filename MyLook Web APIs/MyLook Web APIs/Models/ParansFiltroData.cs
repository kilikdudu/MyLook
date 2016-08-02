using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyLook.Models
{
    public class ParansFiltroData<T> : ParansLista
    {
        public DateTime lastSync { get; set; }
        public List<String> identificadores { get; set; }
        public int modoOperacao { get; set; }
        public T parans { get; set; }
    }
}