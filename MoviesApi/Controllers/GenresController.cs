using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


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
        // GET: api/Genres
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var genres = await _context.Genres.OrderBy(g=>g.Name).ToListAsync();
            return Ok(genres);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] GenreDto createGenreDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var genre = new Genre
            {
                Name = createGenreDto.Name
            };
            await _context.Genres.AddAsync(genre);
            await _context.SaveChangesAsync();
            return Ok(genre);
        }
        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateAsync(int Id,[FromBody] GenreDto genre)
        {
            var existingGenre = await _context.Genres.SingleOrDefaultAsync(g => g.Id == Id);
            if (existingGenre == null)
            {
                return NotFound();
            }
            existingGenre.Name = genre.Name;
            _context.Genres.Update(existingGenre);
            await _context.SaveChangesAsync();
            return Ok(existingGenre);
        }
        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteAsync(int Id)
        {
            var genre = await _context.Genres.SingleOrDefaultAsync(g => g.Id == Id);
            if (genre == null)
            {
                return NotFound();
            }
            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();
            return Ok(genre);
        }

    }
}
