using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyLook.Models
{
    public class RespostaValidacao : IDisposable
    {
        public Boolean ok { get; set; }
        public String campo { get; set; }
        public string mensagem { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}