using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIAutores.DTOs
{
    public class LibroPatchDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Titulo { get; set; }
        public DateTime FechaPublicacion { get; set; }
    }
}
