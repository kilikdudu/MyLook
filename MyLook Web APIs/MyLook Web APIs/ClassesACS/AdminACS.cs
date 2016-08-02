
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Text;
using System.Web;
using System.Configuration;
using System.Net.Http;
using MyLook.Models;

public static class AdminACS
{

    //Chaves do APP no ACS.
    private static string ChaveACS;

    private static bool emProducao;
    //Cookies container.
    private static CookieContainer CC { get; set; }

    //Indica se se está logado no ACS.
    private static bool statusLogin { get; set; }

    //Indica o número de tentativas de login executadas.
    private static int contadortentativa { get; set; }

    /// <summary>
    /// Faço login ROOT no ACS para o banco de dados UAU Corporativo.
    /// </summary>
    /// <remarks>
    /// Criação    : Carlos Eduardo Santos Alves Domingos        Data: 03/08/2015
    /// Projeto    : 183484 - Projeto
    /// </remarks>
    public static void realizaLogin()
    {
        try
        {
            ChaveACS = (ConfigurationManager.AppSettings["emProducao"].ToString() == "1" ? ConfigurationManager.AppSettings["APPKeyProd"].ToString() : ConfigurationManager.AppSettings["APPKeyDev"].ToString());

            emProducao = (ConfigurationManager.AppSettings["emProducao"].ToString() == "1" ? true : false);

            string login = null;
            string senha = null;

            if (emProducao)
            {
                login = ConfigurationManager.AppSettings["loginAdmProd"].ToString();
                senha = ConfigurationManager.AppSettings["senhaAdmProd"].ToString();
            }
            else
            {
                login = ConfigurationManager.AppSettings["loginAdmDev"].ToString();
                senha = ConfigurationManager.AppSettings["senhaAdmDev"].ToString();
            }
            CC = new CookieContainer();
            dynamic status = realizaLogin(ChaveACS, login, senha);

            if (status.sucesso == false)
            {
                //Tenta novamente
                contadortentativa = contadortentativa + 1;
                dynamic t = new Thread(TentativaLogin);
                t.Start();
            }
            else
            {
                //Indico que o login foi feito.
                statusLogin = true;
            }

        }
        catch (Exception)
        {
            throw;
        }

    }

    private static StatusRequisicao realizaLogin(string ChaveACS, string loginACS, string senhaACS)
    {
        try
        {
            //Url de login no ACS. 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.cloud.appcelerator.com/v1/users/login.json?key=" + ChaveACS + "&pretty_json=true");
            //O método de login é POST de acordo a documentação da appcelerator.
            request.Method = "POST";
            //Seto o cookie container para manter o valor da sessão.
            request.CookieContainer = CC;
            //Serializo os dados do login em um json.
            ParansLogin info = new ParansLogin();
            info.login = loginACS;
            info.password = senhaACS;
            string postData = JsonConvert.SerializeObject(info);
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // Seto o ContentType para json, já que os dados no POST estão no formato JSON.
            request.ContentType = "application/json";

            //Preparo os dados
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //Obtenho a resposta da requisição
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            dataStream = default(Stream);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            //Leio a resposta.
            string responseFromServer = reader.ReadToEnd();
            //Deserializo a resposta.
            rootACSResponseUsuario resposta = JsonConvert.DeserializeObject<rootACSResponseUsuario>(responseFromServer);
            var ccs = CC.GetCookies(new Uri("https://api.cloud.appcelerator.com"));
            //Fecho os streans.
            reader.Close();
            dataStream.Close();

            response.Close();

            //Retorno sucesso
            StatusRequisicao ret = new StatusRequisicao();
            ret.sucesso = true;
            return ret;
        }
        catch (WebException ex)
        {
            StatusRequisicao ret = new StatusRequisicao();
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                ret.sucesso = false;
                ret.mensagem = "Erro de protocolo;Cod-1";
                return ret;
            }
            else if (ex.Status == WebExceptionStatus.Timeout)
            {
                ret.sucesso = false;
                ret.mensagem = "Time out;Cod-2";
                return ret;
            }
            else
            {
                ret.sucesso = false;
                ret.mensagem = "Desconhecido;Cod-3";
                return ret;
            }
            throw;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    private static void TentativaLogin()
    {
        Thread.Sleep(60000);
        realizaLogin();
    }

    public static rootACSResponseNotificacao EnviarNotificacao(string canal, object Dados, string clientes = "everyone", string Mensagem = "", string Servico = "")
    {
        try
        {
            rootACSResponseNotificacao resposta = new rootACSResponseNotificacao();
            rootACSResponse aux = resposta;
            //Verifico se o admin está logado.
            checkLogin(ref aux);
            resposta = (rootACSResponseNotificacao)aux;
            //Se não estiver retorno o código de erro -1.
            if (resposta.meta.code != 0)
            {
                return resposta;
            }

            HttpWebRequest request = default(HttpWebRequest);


            //Url de notificação no ACS. 
            request = (HttpWebRequest)WebRequest.Create("https://api.cloud.appcelerator.com/v1/push_notification/notify.json?key=" + ChaveACS);
            //Seto o cookie container para manter o valor da sessão.
            request.CookieContainer = CC;

            //O método de notificação é POST de acordo a documentação da appcelerator.
            request.Method = "POST";

            //Serializo os dados da notificacao em um json.
            ParametrosNotificacao info = new ParametrosNotificacao();
            //Seto o canal da notificação.
            info.channel = canal;
            info.options = new OptionsNotificacao();
            //A notificação expira após 1 dia.
            info.options.expire_after_seconds = 86400;
            //Envio os dados para os usuários selecionados, por padrão envia para todo mundo.
            info.to_ids = clientes;
            //Monto os dados de  configuração da notificação.
            dynamic payload = new PayLoad();
            payload.alert = Mensagem;
            payload.dados = Dados;
            payload.mensagem = Mensagem;
            if (!string.IsNullOrEmpty(Servico))
            {
                payload.servico = Servico;
            }
            payload.title = "Britec";

            info.payload = payload;

            string postData = JsonConvert.SerializeObject(info);
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // Seto o ContentType para json, já que os dados no POST estão no formato JSON.
            request.ContentType = "application/json";

            //Preparo os dados
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //Obtenho a resposta da requisição
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            //Leio a resposta.
            string responseFromServer = reader.ReadToEnd();
            //Deserializo a resposta.
            resposta = JsonConvert.DeserializeObject<rootACSResponseNotificacao>(responseFromServer);
            //Fecho os streans.
            reader.Close();
            dataStream.Close();
            response.Close();

            //Retorno a resposta do serviço
            return resposta;
        }
        catch (WebException ex)
        {
            rootACSResponseNotificacao resposta = new rootACSResponseNotificacao();
            rootACSResponse aux = resposta;
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                MontaException400(ref aux);
            }
            else
            {
                MontaExceptionDesconhecido(ref aux);
            }
            resposta = (rootACSResponseNotificacao)aux;
            return resposta;
        }
        catch (Exception)
        {
            throw;
        }

    }

    private static void checkLogin(ref rootACSResponse root)
    {
        root.meta = new InfoBasicoACS();
        root.meta.code = -1;
        root.meta.method_name = "checkLogin";
        root.meta.status = "Fail";


        if (!statusLogin)
        {
            contadortentativa = contadortentativa + 1;
            dynamic t = new Thread(TentativaLogin);
            t.Start();
        }
        else
        {
            root.meta.code = 0;
            root.meta.status = "OK";
        }


    }

    /*private System.IO.Stream Upload(string url, string filename, Stream fileStream, byte[] fileBytes)
    {
        // Convert each of the three inputs into HttpContent objects

        HttpContent stringContent = new StringContent(filename);
        // examples of converting both Stream and byte [] to HttpContent objects
        // representing input type file
        HttpContent fileStreamContent = new StreamContent(fileStream);
        HttpContent bytesContent = new ByteArrayContent(fileBytes);

        // Submit the form using HttpClient and 
        // create form data as Multipart (enctype="multipart/form-data")
        var client = new HttpClient();

        using (var formData = new MultipartFormDataContent())
        {
            // Add the HttpContent objects to the form data

            // <input type="text" name="filename" />
            formData.Add(stringContent, "filename", "filename");
            // <input type="file" name="file1" />
            formData.Add(fileStreamContent, "file1", "file1");
            // <input type="file" name="file2" />
            formData.Add(bytesContent, "file2", "file2");

            // Actually invoke the request to the server

            // equivalent to (action="{url}" method="post")
            var response = client.PostAsync(url, formData).Result;

            // equivalent of pressing the submit button on the form
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return response.Content.ReadAsStreamAsync().Result;
        }
    }*/

    public static async System.Threading.Tasks.Task<rootACSResponseUsuario> AddUser(InfoUsuarioACS parans)
    {
        try
        {
            rootACSResponseUsuario resposta = new rootACSResponseUsuario();
            rootACSResponse aux = resposta;
            //Verifico se o admin está logado.
            checkLogin(ref aux);
            resposta = (rootACSResponseUsuario)aux;
            //Se não estiver retorno o código de erro -1.
            if (resposta.meta.code != 0)
            {
                return resposta;
            }
            HttpContent stringUsername = new ByteArrayContent(Encoding.UTF8.GetBytes(parans.email));
            HttpContent stringNome = new ByteArrayContent(Encoding.UTF8.GetBytes(parans.first_name));
            HttpContent stringSobrenome = new ByteArrayContent(Encoding.UTF8.GetBytes(parans.last_name));
            HttpContent stringSenha= new ByteArrayContent(Encoding.UTF8.GetBytes(parans.password));
            HttpContent stringSenha02 = new ByteArrayContent(Encoding.UTF8.GetBytes(parans.password_confirmation));
            HttpContent stringCustomFields = new ByteArrayContent(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parans.custom_fields)));
            HttpContent photoContent = null;
            if (parans.photo != null)
            {
                photoContent = new ByteArrayContent(parans.photo);
                photoContent.Headers.Add("Content-Type", "image/png");
                photoContent.Headers.Add("Content-Disposition", "form-data; name=\"photo\"; filename=\"imagem.png\"");
            }

            using (var client = new HttpClient())
            {
                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(stringUsername, string.Format("\"{0}\"", "email"));
                    formData.Add(stringNome, string.Format("\"{0}\"", "first_name"));
                    formData.Add(stringSobrenome, string.Format("\"{0}\"", "last_name"));
                    formData.Add(stringSenha, string.Format("\"{0}\"", "password_confirmation"));
                    formData.Add(stringSenha02, string.Format("\"{0}\"", "password"));
                    formData.Add(stringCustomFields, string.Format("\"{0}\"", "custom_fields"));
                    if (photoContent != null)
                    {
                        formData.Add(photoContent, string.Format("\"{0}\"", "photo"), string.Format("\"{0}\"", "imagem.png"));
                    }
                    
                    HttpResponseMessage response = await client.PostAsync("https://api.cloud.appcelerator.com/v1/users/create.json?key=" + ChaveACS + "&pretty_json=true", formData);
                    response.EnsureSuccessStatusCode();

                    resposta = JsonConvert.DeserializeObject<rootACSResponseUsuario>(await response.Content.ReadAsStringAsync());
                }
            }


            //Devolvo o Id do usuário da resposta.
            return resposta;
        }
        catch (WebException ex)
        {
            rootACSResponseUsuario resposta = new rootACSResponseUsuario();
            rootACSResponse aux = resposta;
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                MontaException400(ref aux);
            }
            else
            {
                MontaExceptionDesconhecido(ref aux);
            }
            resposta = (rootACSResponseUsuario)aux;
            return resposta;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static async System.Threading.Tasks.Task<rootACSResponseUsuario> AlteraUser(InfoUsuarioACS parans)
    {
        try
        {
            rootACSResponseUsuario resposta = new rootACSResponseUsuario();
            rootACSResponse aux = resposta;
            //Verifico se o admin está logado.
            checkLogin(ref aux);
            resposta = (rootACSResponseUsuario)aux;
            //Se não estiver retorno o código de erro -1.
            if (resposta.meta.code != 0)
            {
                return resposta;
            }
            HttpContent stringUsername = null;
            if (parans.email != null)
            {
                stringUsername = new ByteArrayContent(Encoding.UTF8.GetBytes(parans.email));
            }
            HttpContent stringNome = new ByteArrayContent(Encoding.UTF8.GetBytes(parans.first_name));
            HttpContent stringSobrenome = new ByteArrayContent(Encoding.UTF8.GetBytes(parans.last_name));
            HttpContent stringSenha = null;
            HttpContent stringSenha02 = null;
            if (parans.password != null)
            {
                stringSenha = new ByteArrayContent(Encoding.UTF8.GetBytes(parans.password));
                stringSenha02 = new ByteArrayContent(Encoding.UTF8.GetBytes(parans.password_confirmation));
            }
            HttpContent stringId = new ByteArrayContent(Encoding.UTF8.GetBytes(parans.id));
            HttpContent photoContent = null;
            if (parans.photo != null)
            {
                photoContent = new ByteArrayContent(parans.photo);
                photoContent.Headers.Add("Content-Type", "image/png");
                photoContent.Headers.Add("Content-Disposition", "form-data; name=\"photo\"; filename=\"imagem.png\"");
            }
            using (var handler = new HttpClientHandler() { CookieContainer = CC })
            using (var client = new HttpClient(handler))
            {
                using (var formData = new MultipartFormDataContent())
                {
                    if(stringUsername != null)
                    {
                        formData.Add(stringUsername, string.Format("\"{0}\"", "email"));
                    }
                    formData.Add(stringNome, string.Format("\"{0}\"", "first_name"));
                    formData.Add(stringSobrenome, string.Format("\"{0}\"", "last_name"));
                    if (parans.password != null)
                    {
                        formData.Add(stringSenha, string.Format("\"{0}\"", "password_confirmation"));
                        formData.Add(stringSenha02, string.Format("\"{0}\"", "password"));
                    } 
                    formData.Add(stringId, string.Format("\"{0}\"", "su_id"));
                    if (photoContent != null)
                    {
                        formData.Add(photoContent, string.Format("\"{0}\"", "photo"), string.Format("\"{0}\"", "imagem.png"));
                    }

                    HttpResponseMessage response = await client.PostAsync("https://api.cloud.appcelerator.com/v1/users/update.json?key=" + ChaveACS + "&pretty_json=true", formData);
                    response.EnsureSuccessStatusCode();

                    resposta = JsonConvert.DeserializeObject<rootACSResponseUsuario>(await response.Content.ReadAsStringAsync());
                }
            }


            //Devolvo o Id do usuário da resposta.
            return resposta;
        }
        catch (WebException ex)
        {
            rootACSResponseUsuario resposta = new rootACSResponseUsuario();
            rootACSResponse aux = resposta;
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                MontaException400(ref aux);
            }
            else
            {
                MontaExceptionDesconhecido(ref aux);
            }
            resposta = (rootACSResponseUsuario)aux;
            return resposta;
        }
        catch (Exception)
        {
            throw;
        }
    }


    public static rootACSResponseUsuario DeleteUser(string idUsuario)
    {
        try
        {
            rootACSResponseUsuario resposta = new rootACSResponseUsuario();
            rootACSResponse aux = resposta;
            //Verifico se o admin está logado.
            checkLogin(ref aux);
            resposta = (rootACSResponseUsuario)aux;
            //Se não estiver retorno o código de erro -1.
            if (resposta.meta.code != 0)
            {
                return resposta;
            }

            HttpWebRequest request = default(HttpWebRequest);

            
            //Url de busca de usuário no ACS. 
            request = (HttpWebRequest)WebRequest.Create("https://api.cloud.appcelerator.com/v1/users/delete.json?key=" + ChaveACS + "&pretty_json=true" + "&su_id=" + idUsuario);

            //Seto o cookie container para manter o valor da sessão.
            request.CookieContainer = CC;

            //O método de busca é GET de acordo a documentação da appcelerator.
            request.Method = "GET";

            // Seto o ContentType para json, já que os dados no POST estão no formato JSON.
            request.ContentType = "application/json";

            //Obtenho a resposta da requisição
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream dataStream = default(Stream);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            //Leio a resposta.
            string responseFromServer = reader.ReadToEnd();
            //Deserializo a resposta.
            resposta = JsonConvert.DeserializeObject<rootACSResponseUsuario>(responseFromServer);
            //Fecho os streans.
            reader.Close();
            dataStream.Close();
            response.Close();

            //Devolvo o Id do usuário da resposta.
            return resposta;
        }
        catch (WebException ex)
        {
            rootACSResponseUsuario resposta = new rootACSResponseUsuario();
            rootACSResponse aux = resposta;
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                MontaException400(ref aux);
            }
            else
            {
                MontaExceptionDesconhecido(ref aux);
            }
            resposta = (rootACSResponseUsuario)aux;
            return resposta;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static async System.Threading.Tasks.Task<rootACSResponseUsuario> getUser(string idUsuario)
    {
        try
        {
            try
            {
                var resposta = new rootACSResponseUsuario();
                using (var handler = new HttpClientHandler() { CookieContainer = CC })
                using (var client = new HttpClient(handler))
                {
                    HttpResponseMessage response = await client.GetAsync("https://api.cloud.appcelerator.com/v1/users/show.json?key=" + ChaveACS + "&pretty_json=true&response_json_depth=2&user_id=" + idUsuario);
                    response.EnsureSuccessStatusCode();
                    resposta = JsonConvert.DeserializeObject<rootACSResponseUsuario>(await response.Content.ReadAsStringAsync());
                }
                //Devolvo o Id do usuário da resposta.
                return resposta;
            }
            catch (Exception e)
            {
                throw;
            }


        }
        catch (WebException ex)
        {
            rootACSResponseUsuario resposta = new rootACSResponseUsuario();
            rootACSResponse aux = resposta;
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                MontaException400(ref aux);
            }
            else
            {
                MontaExceptionDesconhecido(ref aux);
            }
            resposta = (rootACSResponseUsuario)aux;
            return resposta;
        }
        catch (Exception)
        {
            throw;
        }
    }


    public static async System.Threading.Tasks.Task<rootACSResponseUsuario> checkExists(string email)
    {
        try
        {
            try
            {
                var resposta = new rootACSResponseUsuario();
                using (var handler = new HttpClientHandler() { CookieContainer = CC })
                using (var client = new HttpClient(handler))
                {
                    HttpResponseMessage response = await client.GetAsync("https://api.cloud.appcelerator.com/v1/users/query.json?key=" + 
                        ChaveACS + "&pretty_json=true" + "&limit=1&skip=0&where=" + HttpUtility.UrlEncode("{\"email\":\"" + email + "\"}"));
                    response.EnsureSuccessStatusCode();
                    resposta = JsonConvert.DeserializeObject<rootACSResponseUsuario>(await response.Content.ReadAsStringAsync());
                }
                return resposta;
            }
            catch (Exception e)
            {
                throw;
            }


        }
        catch (WebException ex)
        {
            rootACSResponseUsuario resposta = new rootACSResponseUsuario();
            rootACSResponse aux = resposta;
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                MontaException400(ref aux);
            }
            else
            {
                MontaExceptionDesconhecido(ref aux);
            }
            resposta = (rootACSResponseUsuario)aux;
            return resposta;
        }
        catch (Exception)
        {
            throw;
        }
    }


    private static void MontaException400(ref rootACSResponse resposta)
    {
        resposta.meta = new InfoBasicoACS();
        resposta.meta.code = -2;
        resposta.meta.status = "Fail";
        resposta.meta.method_name = "Exception400";
    }

    private static void MontaExceptionDesconhecido(ref rootACSResponse resposta)
    {
        resposta.meta = new InfoBasicoACS();
        resposta.meta.code = -3;
        resposta.meta.status = "Fail";
        resposta.meta.method_name = "Desconhecido";
    }

    #region "Classes de mapeamento"
    private class ParansLogin
    {
        public string login { get; set; }
        public string password { get; set; }
    }
    private class Parametrosbasicos
    {
        public int limit { get; set; }
        public int skip { get; set; }
        public string sel { get; set; }
    }
    public abstract class rootACSResponse
    {
        public InfoBasicoACS meta { get; set; }
    }
    public class InfoBasicoACS
    {
        public int code { get; set; }
        public string status { get; set; }
        public string method_name { get; set; }
    }
    public class rootACSResponseUsuario : rootACSResponse
    {
        public User response { get; set; }
    }
    public class User
    {
        public List<InfoUsuario> users { get; set; }
    }
    public class InfoUsuario
    {
        public string id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime confirmed_at { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public customFieldsUsuario custom_fields { get; set;}
        public bool admin { get; set; }
        public PhotoUser photo { get; set; }
    }
    public class PhotoUser
    {
        public string id { get; set; }
        public string filename { get; set; }
        public bool processed { get; set; }
        public string content_type { get; set; }
        public PhotoUrls urls { get; set; }
    }
    public class PhotoUrls
    {
        public string original { get; set; }
    }
    public class customFieldsUsuario
    {
        public string IdIUGU { get; set; }
    }
    public class rootACSResponseNotificacao : rootACSResponse
    {
        public PayLoad response { get; set; }
    }
    private class ParametrosNotificacao
    {
        public string channel { get; set; }
        public string to_ids { get; set; }
        public PayLoad payload { get; set; }
        public OptionsNotificacao options { get; set; }
        public bool pretty_json { get; set; }
    }
    public class PayLoad
    {
        public string alert { get; set; }
        public string icon { get; set; }
        public string sound { get; set; }
        public string title { get; set; }
        public string vibrate { get; set; }
        public string servico { get; set; }
        public string mensagem { get; set; }
        public object dados { get; set; }
    }
    private class OptionsNotificacao
    {
        public long expire_after_seconds { get; set; }
    }
    #endregion

}
