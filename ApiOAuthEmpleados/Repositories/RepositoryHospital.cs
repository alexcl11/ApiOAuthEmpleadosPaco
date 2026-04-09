using ApiOAuthEmpleados.Data;
using ApiOAuthEmpleados.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ApiOAuthEmpleados.Repositories
{
    public class RepositoryHospital
    {
        private HospitalContext context;

        public RepositoryHospital(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<List<Empleado>> GetEmpleadosAsync()
        {
            return await this.context.Empleados.ToListAsync();
        }

        public async Task<Empleado> FindEmpleadoAsync(int idEmpleado)
        {
            return await this.context.Empleados.FirstOrDefaultAsync
                (x => x.IdEmpleado == idEmpleado);
        }
        public async Task<List<Empleado>> GetCompisAsync(int idDepartamento)
        {
            return await this.context.Empleados.Where(e => e.IdDepartamento == idDepartamento).ToListAsync();
        }
        public async Task<Empleado> LogInEmpleadoAsync
            (string apellido, int idEmpleado)
        {
            return await this.context.Empleados
                .Where(z => z.Apellido == apellido
                && z.IdEmpleado == idEmpleado)
                .FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetOficiosAsync()
        {
            var consulta = (from datos in this.context.Empleados
                            select datos.Oficio).Distinct();
            return await consulta.ToListAsync();
        }
        public async Task<List<Empleado>> GetEmpleadosOficiosAsync(List<string> oficios)
        {
            var consulta = from datos in this.context.Empleados
                           where oficios.Contains(datos.Oficio)
                           select datos;
            return await consulta.ToListAsync();
        }
        public async Task IncrementarSalario(int incremento, List<string> oficios)
        {
            List<Empleado> empleados = await this.GetEmpleadosOficiosAsync(oficios);
            foreach (var empleado in empleados)
            {
                empleado.Salario += incremento;
            }
            await this.context.SaveChangesAsync();
        }
    }
}
