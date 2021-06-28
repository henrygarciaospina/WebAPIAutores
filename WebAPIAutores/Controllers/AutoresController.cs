using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;

namespace WebAPIAutores.Controllers
{
    [ApiController]
    [Route("api/autores")]
    public class AutoresController: ControllerBase 
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public AutoresController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        // /api/autores
        [HttpGet]
        public async Task<ActionResult<List<AutorDTO>>> Get() 
        {
            var autores =  await context.Autores.ToListAsync();
            return mapper.Map<List<AutorDTO>>(autores);                
        }

        // /api/autores/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AutorDTOConLibros>> Get(int id)
        {
            var autor = await context.Autores
                .Include(AutorBD => AutorBD.AutoresLibros)
                //Ingresa a AutoresLibros e incluye a Libro (es una de las propiedades de navegación de AutorLibro)
                .ThenInclude(autorLibroBD => autorLibroBD.Libro)
                .FirstOrDefaultAsync(autorBD => autorBD.Id == id);

            if (autor == null)
            {
                return NotFound($"Autor de Id: {id} no existe");
            }


            return mapper.Map<AutorDTOConLibros>(autor);
        }

        // /api/autores/Gabriel
        [HttpGet("{nombre}")]
        public async Task<ActionResult<List<AutorDTO>>> Get([FromRoute] string nombre)
        {
            var autores = await context.Autores.Where(autorBD => autorBD.Nombre.Contains(nombre)).ToListAsync();

            return mapper.Map<List<AutorDTO>>(autores);
        }

        // /api/autores
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO autorCreacionDTO) 
        {
            var existeNombreAutor = await context.Autores.AnyAsync(n => n.Nombre == autorCreacionDTO.Nombre);

            if (existeNombreAutor)
            {
               return  BadRequest($"Ya existe un autor con el nombre {autorCreacionDTO.Nombre}");
            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);
           
            context.Add(autor);
            await context.SaveChangesAsync();
            return Ok("Autor creado exitosamente");
        }

        // /api/autores/1
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(Autor autor, int id) 
        {
            if (autor.Id != id)
            {
                return BadRequest($"El Id: {id} del autor no coincide con el de la URL");
            }

            var existeAutor = await context.Autores.AnyAsync(a => a.Id == id);

            if (!existeAutor)
            {
                return NotFound($"Autor de Id: {id} no existe");
            }

            context.Update(autor);
            await context.SaveChangesAsync();
            return Ok($"Autor de Id: {id} actualizado exitosamente");
        }

        // /api/autores/1
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id) 
        {
            var existeAutor = await context.Autores.AnyAsync(a => a.Id == id);

            if (!existeAutor)
            {
                return NotFound($"Autor de Id: {id} no existe");
            }

            context.Remove(new Autor() { Id = id });
            await context.SaveChangesAsync();
            
            return Ok($"Autor de Id: {id} eliminado exitosamente");
        }

    }
}
