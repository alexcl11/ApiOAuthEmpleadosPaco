using ApiOAuthEmpleados.Models;
using ApiOAuthEmpleados.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
// IMPORTANTE: No olvides el using de tus helpers
using ApiOAuthEmpleados.Helpers;
using Microsoft.AspNetCore.Components;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace ApiOAuthEmpleados.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpleadosController : ControllerBase
    {
        private RepositoryHospital repo;
        private HelperCifrado helperCifrado; // 1. Declaramos nuestro helper

        // 2. Añadimos IConfiguration al constructor para leer el secreto
        public EmpleadosController(RepositoryHospital repo, IConfiguration config)
        {
            this.repo = repo;
            // Leemos el secreto del appsettings y montamos el helper
            string secretoCifrado = config.GetValue<string>("CifradoDatos:SecretKey");
            this.helperCifrado = new HelperCifrado(secretoCifrado);
        }

        [HttpGet]
        public async Task<ActionResult<List<Empleado>>> GetEmpleados()
        {
            return await this.repo.GetEmpleadosAsync();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Empleado>> FindEmpleado(int id)
        {
            return await this.repo.FindEmpleadoAsync(id);
        }

        [Authorize]
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<Empleado>> Perfil()
        {
            Claim claim = HttpContext.User.FindFirst(z => z.Type == "UserData");
            
            // 3. El valor que viene del claim ahora está encriptado
            string jsonEncriptado = claim.Value; 
            
            // 4. Lo desencriptamos con el helper
            string jsonEmpleado = this.helperCifrado.DecryptString(jsonEncriptado);
            
            // 5. Ahora sí, deserializamos el JSON en texto plano
            Empleado empleado = JsonConvert.DeserializeObject<Empleado>(jsonEmpleado);
            return await this.repo.FindEmpleadoAsync(empleado.IdEmpleado);
        }

        [Authorize]
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<List<Empleado>>> Compis()
        {
            Claim claim = HttpContext.User.FindFirst(z => z.Type == "UserData");
            
            // Repetimos el mismo proceso aquí
            string jsonEncriptado = claim.Value; 
            
            // Desencriptamos
            string jsonEmpleado = this.helperCifrado.DecryptString(jsonEncriptado);
            
            Empleado empleado = JsonConvert.DeserializeObject<Empleado>(jsonEmpleado);
            return await this.repo.GetCompisAsync(empleado.IdDepartamento);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<List<string>>> Oficios()
        {
            return await this.repo.GetOficiosAsync();
        }

        //?oficio=ANALISTA&oficio=DIRECTOR
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<List<Empleado>>> EmpleadosOficio([FromQuery] List<string> oficios)
        {
            return await this.repo.GetEmpleadosOficiosAsync(oficios);
        }
        [HttpPut]
        [Route("[action]/{incremento}")]
        public async Task<ActionResult> IncrementarSalarios(int incremento, [FromQuery] List<string> oficios)
        {
            await this.repo.IncrementarSalario(incremento, oficios);
            return Ok();
        }
    }
}