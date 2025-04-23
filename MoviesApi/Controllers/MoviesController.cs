using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IGenreService _genreService;
        private readonly IMapper _mapper;

        public MoviesController(IMovieService movieService, IGenreService genreService, IMapper mapper)
        {
            _movieService = movieService;
            _genreService = genreService;
            _mapper = mapper;
        }

        private new List<string> _allowedExtensions = new List<string> { ".jpg", ".png", ".jpeg" };
        private const int _maxPosterSize = 2 * 1024 * 1024; // 2 MB

       
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies = await _movieService.GetAll();
            var data=_mapper.Map<IEnumerable<MovieDetailsDto>>(movies);
            return Ok(data);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie = await _movieService.GetById(id);
            if (movie == null)
            {
                return NotFound();
            }
            var movieDetails = _mapper.Map<MovieDetailsDto>(movie);
            return Ok(movie);
        }
        [HttpGet("genre")]
        public async Task<IActionResult> GetByGenreIdAsync(byte id)
        {
            var movies = await _movieService.GetAll(id);
            var data = _mapper.Map<IEnumerable<MovieDetailsDto>>(movies);
            return Ok(data);
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
            var ValidGenre =await  _genreService.GetById(createMovieDto.GenreId);
            if (ValidGenre is null)
            {
                return BadRequest("Invalid Genre");
            }
            using var stream=new MemoryStream();
            await createMovieDto.Poster.CopyToAsync(stream);
            var movie = _mapper.Map<Movie>(createMovieDto);
            movie.Poster = stream.ToArray();

            _movieService.Create(movie);
            return Ok(movie);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] MovieDto updateMovieDto)
        {
            var movie = await _movieService.GetById(id);
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
            var ValidGenre = await _genreService.GetById(updateMovieDto.GenreId);
            if (ValidGenre is null)
            {
                return BadRequest("Invalid Genre");
            }

            movie.Title = updateMovieDto.Title;
            movie.StoryLine = updateMovieDto.StoryLine;
            movie.Year = updateMovieDto.Year;
            movie.Rate = updateMovieDto.Rate;
            movie.GenreId = updateMovieDto.GenreId;
            var updatedMovie = _movieService.Update(movie);
            return Ok(updatedMovie);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _movieService.GetById(id);
            if (movie == null)
            {
                return NotFound($"No Movie was found with the id {id} ");
            }
            var DeletedMovie = _movieService.Delete(movie);
            return Ok(DeletedMovie);
        }
    }
}
