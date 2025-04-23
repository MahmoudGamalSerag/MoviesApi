using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private new List<string> _allowedExtensions = new List<string> { ".jpg", ".png", ".jpeg" };
        private const int _maxPosterSize = 2 * 1024 * 1024; // 2 MB

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies = await _context.Movies.Include(m=>m.Genre )
                .OrderByDescending(m=>m.Rate)
                .Select(m=>new MovieDetailsDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    StoryLine = m.StoryLine,
                    Year = m.Year,
                    Rate = m.Rate,
                    Poster = m.Poster,
                    GenreId = m.GenreId,
                    GenreName = m.Genre.Name
                })
                .ToListAsync();
            return Ok(movies);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie = await _context.Movies.Include(m=>m.Genre).SingleOrDefaultAsync(m=>m.Id==id);
            if (movie == null)
            {
                return NotFound();
            }
            var movieDetails = new MovieDetailsDto
            {
                Id = movie.Id,
                Title = movie.Title,
                StoryLine = movie.StoryLine,
                Year = movie.Year,
                Rate = movie.Rate,
                Poster = movie.Poster,
                GenreId = movie.GenreId,
                GenreName = movie.Genre.Name
            };
            return Ok(movie);
        }
        [HttpGet("genre")]
        public async Task<IActionResult> GetByGenreIdAsync(int id)
        {
            var movies = await _context.Movies.Include(m => m.Genre).Where(m => m.GenreId == id)
                .Select(m => new MovieDetailsDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    StoryLine = m.StoryLine,
                    Year = m.Year,
                    Rate = m.Rate,
                    Poster = m.Poster,
                    GenreId = m.GenreId,
                    GenreName = m.Genre.Name
                })
                .ToListAsync();
            return Ok(movies);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] MovieDto createMovieDto)
        {
            if(!_allowedExtensions.Contains(Path.GetExtension(createMovieDto.Poster.FileName).ToLower()) )
            {
                return BadRequest("Invalid file type");
            }
            if (createMovieDto.Poster.Length > _maxPosterSize)
            {
                return BadRequest("File size exceeds the limit");
            }
            var ValidGenre =  _context.Genres.Any(g => g.Id == createMovieDto.GenreId);
            if (!ValidGenre)
            {
                return BadRequest("Invalid Genre");
            }
            using var stream=new MemoryStream();
            await createMovieDto.Poster.CopyToAsync(stream);
            var movie = new Movie
            {
                Title = createMovieDto.Title,
                StoryLine = createMovieDto.StoryLine,
                Year = createMovieDto.Year,
                Rate = createMovieDto.Rate,
                Poster = stream.ToArray(),
                GenreId = createMovieDto.GenreId
            };

            await _context.Movies.AddAsync(movie);
            await _context.SaveChangesAsync();
            return Ok(movie);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] MovieDto updateMovieDto)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound($"No Movie was found with the id {id} ");
            }
            if (updateMovieDto.Poster is not null)
            {
                if (!_allowedExtensions.Contains(Path.GetExtension(updateMovieDto.Poster.FileName).ToLower()))
                {
                    return BadRequest("Invalid file type");
                }
                if (updateMovieDto.Poster.Length > _maxPosterSize)
                {
                    return BadRequest("File size exceeds the limit");
                }
                using var stream = new MemoryStream();
                await updateMovieDto.Poster.CopyToAsync(stream);
                movie.Poster = stream.ToArray();
            }
            var ValidGenre = _context.Genres.Any(g => g.Id == updateMovieDto.GenreId);
            if (!ValidGenre)
            {
                return BadRequest("Invalid Genre");
            }
           
            movie.Title = updateMovieDto.Title;
            movie.StoryLine = updateMovieDto.StoryLine;
            movie.Year = updateMovieDto.Year;
            movie.Rate = updateMovieDto.Rate;
            movie.GenreId = updateMovieDto.GenreId;
            _context.Update(movie);
            await _context.SaveChangesAsync();
            return Ok(movie);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound($"No Movie was found with the id {id} ");
            }
            _context.Remove(movie);
            await _context.SaveChangesAsync();
            return Ok(movie);
        }
    }
}
