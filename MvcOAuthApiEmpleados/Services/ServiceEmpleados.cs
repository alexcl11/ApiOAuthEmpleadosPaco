using Microsoft.AspNetCore.Mvc;
using MvcOAuthApiEmpleados.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace MvcOAuthApiEmpleados.Services
{
    public class ServiceEmpleados
    {
        private string UrlApi;
        private MediaTypeWithQualityHeaderValue header;
        private IHttpContextAccessor contextAccessor;
        public ServiceEmpleados(IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            this.UrlApi = configuration.GetValue<string>
                ("ApiUrls:ApiEmpleados");
            this.header = new 
                MediaTypeWithQualityHeaderValue("application/json");
            this.contextAccessor = contextAccessor;
        }

        public async Task<string> LogInAsync
            (string user, string pass)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/auth/login";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                LoginModel model = new LoginModel
                {
                    UserName = user, Password = pass
                };
                string json = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent
                    (json, Encoding.UTF8, this.header);
                HttpResponseMessage response =
                    await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode == true)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject keys = JObject.Parse(data);
                    string token = keys.GetValue("response").ToString();
                    return token;
                }
                else
                {
                    return null;
                }
            }
        }

        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                HttpResponseMessage response =
                    await client.GetAsync(request);
                if (response.IsSuccessStatusCode == true)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        //REALIZAMOS UNA SOBRECARGA DEL METODO
        private async Task<T> CallApiAsync<T>
            (string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Add
                    ("Authorization", "bearer " + token);
                HttpResponseMessage response =
                    await client.GetAsync(request);
                if (response.IsSuccessStatusCode == true)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        public async Task<List<Empleado>> GetEmpleadosAsync()
        {
            string request = "api/empleados";
            List<Empleado> empleados = await
                this.CallApiAsync<List<Empleado>>(request);
            return empleados;
        }

        //POR AHORA, RECIBIREMOS EL TOKEN EN EL METODO
        public async Task<Empleado> FindEmpleadoAsync
            (int idEmpleado)
        {
            string token = this.contextAccessor.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;
            string request = "api/empleados/" + idEmpleado;
            Empleado empleado = await
                this.CallApiAsync<Empleado>(request, token);
            return empleado;
        }

        public async Task<Empleado> GetPerfilAsync()
        {
            string token = this.contextAccessor.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;
            string request = "api/empleados/perfil";
            Empleado empleado = await this.CallApiAsync<Empleado>(request, token);
            return empleado;
        }
        public async Task<List<Empleado>> GetCompisAsync()
        {
            string token = this.contextAccessor.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;
            string request = "api/empleados/compis";
            List<Empleado> empleados = await this.CallApiAsync<List<Empleado>>(request, token);
            return empleados;
        }
    }
}
