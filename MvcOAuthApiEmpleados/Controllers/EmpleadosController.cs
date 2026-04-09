using Microsoft.AspNetCore.Mvc;
using MvcOAuthApiEmpleados.Filters;
using MvcOAuthApiEmpleados.Models;
using MvcOAuthApiEmpleados.Services;
using System.Security.Claims;

namespace MvcOAuthApiEmpleados.Controllers
{
    public class EmpleadosController : Controller
    {
        private ServiceEmpleados service;
        public EmpleadosController(ServiceEmpleados service)
        {
            this.service = service;
        }
        public async Task<IActionResult> EmpleadosOficios()
        {
            List<string> oficios = await this.service.GetOficiosAsync();
            ViewData["OFICIOS"] = oficios;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EmpleadosOficios(int? incremento, List<string> oficios, string? accion)
        {
            List<string> oficiosLayout = await this.service.GetOficiosAsync();
            ViewData["OFICIOS"] = oficiosLayout;
            if (accion != null && accion.ToLower() == "update")
            {
                await this.service.IncrementarSalarioOficios(incremento.Value, oficios);
            }
            List<Empleado> empleados = await this.service.GetEmpleadosOficioAsync(oficios);
            return View(empleados);
        }

        public async Task<IActionResult> Index()
        {
            List<Empleado> empleados =
                await this.service.GetEmpleadosAsync();
            return View(empleados);
        }

        [AuthorizeEmpleados]
        public async Task<IActionResult> Details(int idempleado)
        {
            
            Empleado empleado =
                await this.service.FindEmpleadoAsync(idempleado);
            return View(empleado);
            
        }

        [AuthorizeEmpleados]
        public async Task<IActionResult> Perfil()
        {
            Empleado empleado = await this.service.GetPerfilAsync();
            return View(empleado);
        }

        [AuthorizeEmpleados]
        public async Task<IActionResult> Compis()
        {
            List<Empleado> empleados = await this.service.GetCompisAsync();
            return View(empleados);
        }

    }
}
