using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly List<string> _allowedExtensions = new() { ".jpg",".png"};

        private readonly long _maxAllowedPosterSize = 1048576;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies = await _context.Movies
                .OrderByDescending(m => m.Rate)
                .Include(m => m.Genre)
                .Select(m => new { m.Id, m.Title, m.Year, m.StoryLine, m.Rate, GenreName=m.Genre.Name})
                .ToListAsync();

            return Ok(movies);
        }
        [HttpGet("GetByGenreId")]
        public async Task<IActionResult> GetByGenreIdAsync(byte genreId)
        {
            var movie = await _context.Movies
                .Where(m => m.GenreId == genreId)
                .OrderByDescending(m => m.Rate)
                .Include(m => m.Genre)
                .Select(m => new { m.Id, m.Title, m.Year, m.StoryLine, m.Rate, GenreName = m.Genre.Name })
                .ToListAsync();

            if (movie == null) return NotFound("");

            return Ok(movie);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.Genre)
                .Select(m => new { m.Id, m.Title, m.Year, m.StoryLine, m.Rate, GenreName = m.Genre.Name })
                .SingleOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound("");

            return Ok(movie);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] MovieDTO dto)
        {
            if (!_allowedExtensions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                return BadRequest("Only PNG and JPG are allowed");

            if (dto.Poster.Length > _maxAllowedPosterSize) return BadRequest("Max allowed size is 1MB");

            var isValidGenre = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);

            if (!isValidGenre) return BadRequest("Invalid Genre ID");

            using var dataStrim = new MemoryStream();

            await dto.Poster.CopyToAsync(dataStrim);

            var movie = new Movie
            {
                GenreId = dto.GenreId,
                Title = dto.Title,
                Year = dto.Year,
                StoryLine = dto.StoryLine,
                Poster = dataStrim.ToArray(),
                Rate= dto.Rate

            };

            await _context.AddAsync(movie);

            _context.SaveChanges();

            return Ok(movie);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] MovieDTO dto)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null) return NotFound($"No movie found with ID: {id}");

            var isValidGenre = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);

            if (!isValidGenre) return BadRequest("Invalid Genre ID");

            if(dto.Poster != null)
            {
                if (!_allowedExtensions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                    return BadRequest("Only PNG and JPG are allowed");

                if (dto.Poster.Length > _maxAllowedPosterSize) return BadRequest("Max allowed size is 1MB");

                using var dataStrim = new MemoryStream();

                await dto.Poster.CopyToAsync(dataStrim);

                movie.Poster = dataStrim.ToArray();
            }

            movie.GenreId = dto.GenreId;
            movie.Title = dto.Title;
            movie.Year = dto.Year;
            movie.StoryLine = dto.StoryLine;
            movie.Rate = dto.Rate;

            _context.SaveChanges();

            return Ok(movie);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null) return NotFound($"No movie found with ID: {id}");

            _context.Remove(movie);

            _context.SaveChanges();

            return Ok(movie);
        }
    }
}
