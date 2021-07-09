using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;

namespace WebAPIAutores.Controllers
{
    [ApiController]
    [Route("api/libros")]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public LibrosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("{id:int}", Name = "obtenerLibro")]
        public async Task<ActionResult<LibroDTOConAutores>> Get(int id)
        {
            var libro = await context.Libros
                .Include(libroBD => libroBD.AutoresLibros)
                //Ingresa a AutoresLibros e incluye a Autor (es una de las propiedades de navegación de AutorLibro)
                .ThenInclude(autorLibroBD => autorLibroBD.Autor)
                .FirstOrDefaultAsync(libroBD => libroBD.Id == id);

            if (libro == null)
            {
                return NotFound($"El libro de Id: {id} no existe");
            }

            libro.AutoresLibros = libro.AutoresLibros.OrderBy(al => al.Orden).ToList();
            return mapper.Map<LibroDTOConAutores>(libro);
        }

        [HttpPost]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresIds == null)
            {
                return BadRequest("No se puede crear un libro sin autores");
            }

            var autoresIds = await context.Autores
                .Where(autorBD => libroCreacionDTO.AutoresIds.Contains(autorBD.Id))
                  .Select(a => a.Id).ToListAsync();

            if (libroCreacionDTO.AutoresIds.Count != autoresIds.Count)
            {
                return BadRequest("No existe uno de los autores enviados");
            }

            var libro = mapper.Map<Libro>(libroCreacionDTO);
            AsignarOrdenAutores(libro);

            context.Add(libro);
            await context.SaveChangesAsync();

            var libroDTO = mapper.Map<LibroDTO>(libro);

            return CreatedAtRoute("obtenerLibro", new { id = libro.Id }, libroDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, LibroCreacionDTO librocreacionDTO)
        {
            var libroBD = await context.Libros
                .Include(al => al.AutoresLibros)
                .FirstOrDefaultAsync(al => al.Id == id);

            if (libroBD == null)
            {
                return NotFound($"No existe el libro de Id: {id}");
            }

            libroBD = mapper.Map(librocreacionDTO, libroBD);

            AsignarOrdenAutores(libroBD);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<LibroPatchDTO> patchDocument) 
        {
            if (patchDocument == null)
            {
                return BadRequest("Error em el envío de los datos");
            }

            var libroBD = await context.Libros.FirstOrDefaultAsync(l => l.Id == id);

            if (libroBD == null)
            {
                return NotFound($"No existe el libro de id: {id}");
            }

            var libroDTO = mapper.Map<LibroPatchDTO>(libroBD);

            patchDocument.ApplyTo(libroDTO, ModelState);

            var esValido = TryValidateModel(libroDTO);


            if (!esValido)
            {
                return BadRequest(ModelState);
            }

            mapper.Map(libroDTO, libroBD);

            await context.SaveChangesAsync();
            return NoContent();
        }

        // /api/libros/1
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existeLibro = await context.Libros.AnyAsync(l => l.Id == id);

            if (!existeLibro)
            {
                return NotFound($"Libro de Id: {id} no existe");
            }

            context.Remove(new Libro() { Id = id });
            await context.SaveChangesAsync();

            return Ok($"Libro de Id: {id} eliminado exitosamente");
        }

        private static void AsignarOrdenAutores(Libro libro) 
        {
            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }
        }
    }
}