using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using MyLook_Web_APIs;
using MyLook.Models;
using MyLook_Web_APIs.Models.Seguidor;
using MyLook_Web_APIs.Models;

namespace MyLook_Web_APIs.Controllers
{
    public class seguidoresController : ApiController
    {
        private mylookEntities db = new mylookEntities();

        [HttpPost]
        public ListaRetorno<InfoSeguidor> getSeguidores(ParansFiltroData<ParansByUsuario> parans)
        {
            var lstRetorno = new ListaRetorno<InfoSeguidor>();
            var lstSeguidor = new List<InfoSeguidor>();
            var rowsSeguidores = db.seguidores.Where(s => s.Destino == parans.parans.UsuarioId)
                .OrderBy(s => s.usuario1.Nome + " " + s.usuario1.Sobrenome)
                .Skip(parans.cursor)
                .Take(parans.limite)
                .ToList();
            foreach (var seguidorRow in rowsSeguidores)
            {
                var seg = new InfoSeguidor();
                seg.Nome = seguidorRow.usuario1.Nome + " " + seguidorRow.usuario1.Sobrenome;
                seg.UrlFoto = seguidorRow.usuario1.Foto;
                seg.UsuarioId = seguidorRow.Destino;
                lstSeguidor.Add(seg);
            }
            lstRetorno.inserir = lstSeguidor;
            return lstRetorno;
        }

        [HttpPost]
        public ListaRetorno<InfoSeguidor> getSeguindo(ParansFiltroData<ParansByUsuario> parans)
        {
            var lstRetorno = new ListaRetorno<InfoSeguidor>();
            var lstSeguindo = new List<InfoSeguidor>();
            var rowsSeguindo = db.seguidores.Where(s => s.Origem == parans.parans.UsuarioId)
                .OrderBy(s => s.usuario.Nome + " " + s.usuario.Sobrenome)
                .Skip(parans.cursor)
                .Take(parans.limite)
                .ToList()
                ;


            foreach (var seguindoRow in rowsSeguindo)



            {
                var seg = new InfoSeguidor();
                seg.Nome = seguindoRow.usuario.Nome + " " + seguindoRow.usuario.Sobrenome;
                seg.UrlFoto = seguindoRow.usuario.Foto;
                seg.UsuarioId = seguindoRow.Origem;
                lstSeguindo.Add(seg);

            }
            lstRetorno.inserir = lstSeguindo;
            return lstRetorno;
        }
    }
}

