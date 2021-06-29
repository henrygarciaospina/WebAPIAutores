using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;

namespace WebAPIAutores.Controllers
{
    [ApiController]
    [Route("api/libros/{libroId:int}/comentarios")]
    public class ComentariosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public ComentariosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(libroBD => libroBD.Id == libroId);

            if (!existeLibro)
            {
                return NotFound($"El libro de id : {libroId} no existe");
            }

            var comentarios = await context.Comentarios
                .Where(comentarioBD => comentarioBD.LibroId == libroId).ToListAsync();

            return mapper.Map<List<ComentarioDTO>>(comentarios);
        }

        [HttpGet("{id:int}", Name ="obtenerComentario")]
        public async Task<ActionResult<ComentarioDTO>> GetbyId(int id) 
        {
            var comentario = await context.Comentarios.FirstOrDefaultAsync(comentarioBD => comentarioBD.Id == id);

            if (comentario == null)
            {
                return NotFound($"Comentario de Id {id} no existe");
            }

            return mapper.Map<ComentarioDTO>(comentario);
        }

        [HttpPost]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(libroBD => libroBD.Id == libroId);

            if (!existeLibro)
            {
                return NotFound($"El libro de id : {libroId} no existe");
            }

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.LibroId = libroId;
            context.Add(comentario);
            await context.SaveChangesAsync();

            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);
            
            return CreatedAtRoute("obtenerComentario", new { id = comentario.Id, libroId = libroId}, comentarioDTO );
        }
    }
}
