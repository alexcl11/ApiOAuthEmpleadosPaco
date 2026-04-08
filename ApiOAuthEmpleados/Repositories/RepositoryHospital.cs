using ApiOAuthEmpleados.Data;
using ApiOAuthEmpleados.Models;
using Microsoft.EntityFrameworkCore;

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
    }
}
