using ApiOAuthEmpleados.Helpers;
using ApiOAuthEmpleados.Models;
using ApiOAuthEmpleados.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Diagnostics.Eventing.Reader;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ApiOAuthEmpleados.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private RepositoryHospital repo;
        private HelperActionOAuthService helper;
        private HelperCifrado helperCifrado;
        public AuthController(RepositoryHospital repo
            , HelperActionOAuthService helper, IConfiguration config)
        {
            this.repo = repo;
            this.helper = helper;
            string secretoCifrado = config.GetValue<string>("CifradoDatos:SecretKey");

            // Inicializamos el helper con el nuevo secreto
            this.helperCifrado = new HelperCifrado(secretoCifrado);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult>
            Login(LoginModel model)
        {
            Empleado empleado = await
               this.repo.LogInEmpleadoAsync
               (model.UserName, int.Parse(model.Password));
            if (empleado == null)
            {
                return Unauthorized();
            }
            else
            {
                //DEBEMOS CREAR UNAS CREDENCIALES CON NUESTRO 
                //TOKEN
                SigningCredentials credentials =
                    new SigningCredentials
                    (this.helper.GetKeyToken(),
                    SecurityAlgorithms.HmacSha256);

                // 1. Serializamos el objeto
                string jsonEmpleado = JsonConvert.SerializeObject(empleado);

                // 2. ENCRIPTAMOS el JSON
                string jsonEncriptado = this.helperCifrado.EncryptString(jsonEmpleado);


                // 3. Guardamos el dato encriptado en el Claim
                Claim[] informacion = new[]
                {
                    new Claim("UserData", jsonEncriptado)
                };

                //EL TOKEN SE GENERA CON UNA CLASE Y DEBEMOS 
                //ALMACENAR LOS DATOS DE ISSUER, CREDENTIALS...
                JwtSecurityToken token =
                    new JwtSecurityToken(
                        claims: informacion,
                        issuer: this.helper.Issuer,
                        audience: this.helper.Audience,
                        signingCredentials: credentials,
                        expires: DateTime.UtcNow.AddMinutes(20),
                        notBefore: DateTime.UtcNow
                        );
                //POR ULTIMO, DEVOLVEMOS LA RESPUESTA AFIRMATIVA 
                //CON EL TOKEN
                return Ok(new
                {
                    response = 
                    new JwtSecurityTokenHandler()
                    .WriteToken(token)
                });
            }
        }
    }
}
