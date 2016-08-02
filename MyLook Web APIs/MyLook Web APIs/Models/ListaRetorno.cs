using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyLook.Models
{
    public class ListaRetorno<T>
    {
        public List<T> inserir { get; set; }
        public List<String> deletar { get; set; }
    }
}