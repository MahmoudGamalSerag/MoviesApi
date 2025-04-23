using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesApi.Services;


namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly IGenreService _genreService;

        public GenresController(IGenreService genreService)
        {
            _genreService = genreService;
        }

        // GET: api/Genres
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var genres = await _genreService.GetAll();
            return Ok(genres);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] GenreDto createGenreDto)
        {
           
            var genre = await _genreService.Create(createGenreDto);
            return Ok(genre);
        }
        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateAsync(byte Id,[FromBody] GenreDto genre)
        {
            var existingGenre = await _genreService.GetById(Id);
            if (existingGenre == null)
            {
                return NotFound();
            }
            existingGenre.Name = genre.Name;
            var updatedGenre=_genreService.Update(existingGenre);

            return Ok(existingGenre);
        }
        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteAsync(byte Id)
        {
            var genre = await _genreService.GetById(Id);
            if (genre == null)
            {
                return NotFound();
            }
            
            var deletedGenre= _genreService.Delete(genre);
            return Ok(genre);
        }

    }
}
