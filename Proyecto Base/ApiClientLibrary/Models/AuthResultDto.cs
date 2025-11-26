using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiClientLibrary.Models
{
    public class AuthResult
    {
        public string? Token { get; set; }
        public UserDetails? UserDetails { get; set; }
        public string? LastUsage { get; set; }
    }
    public class UserDetails
    {
        public string? nombre { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
        public string? empresa { get; set; }
        public string? idEmpresa { get; set; }
        public string? rfc { get; set; }
        public string? rol { get; set; }
        public int maximoIntentosPruebas { get; set; }
        public string[]? ip { get; set; }
    }
}
