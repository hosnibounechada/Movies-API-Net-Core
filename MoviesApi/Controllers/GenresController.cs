﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GenresController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();

            return Ok(genres);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(byte id)
        {
            var genre = await _context.Genres.FindAsync(id);

            if (genre == null) return NotFound();

            return Ok(genre);
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] GenreDTO dto)
        {
            var genre = new Genre { Name = dto.Name };

            await _context.AddAsync(genre);

            _context.SaveChanges();

            return Ok(genre);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] GenreDTO dto)
        {
            var genre = await _context.Genres.SingleOrDefaultAsync(g => g.Id == id);

            if (genre == null) return NotFound($"No Genre was found with ID: {id}");

            genre.Name = dto.Name;

            _context.SaveChanges();

            return Ok(genre);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var genre = await _context.Genres.SingleOrDefaultAsync(g =>g.Id == id);

            if(genre == null) return NotFound($"No Genre was found with ID: {id}");

            _context.Remove(genre);

            _context.SaveChanges();

            return Ok(genre);
        }
    }
}
