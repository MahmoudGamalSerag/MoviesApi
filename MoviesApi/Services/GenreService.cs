
using MoviesApi.Models;

namespace MoviesApi.Services
{
    public class GenreService : IGenreService
    {
        private readonly ApplicationDbContext _context;

        public GenreService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Genre> Create(GenreDto createGenreDto)
        {
            var genre = new Genre
            {
                Name = createGenreDto.Name
            };
            await _context.Genres.AddAsync(genre);
            await _context.SaveChangesAsync();
            return genre;
        }

        public Genre Delete(Genre genre)
        {
            _context.Genres.Remove(genre);
             _context.SaveChanges();
            return genre;
        }

        public async Task<IEnumerable<Genre>> GetAll()
        {
            var genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            return genres;
        }

        public async Task<Genre> GetById(byte id)
        {
            return  await _context.Genres.SingleOrDefaultAsync(g => g.Id == id);
        }

        public Genre Update(Genre genre)
        {
            _context.Genres.Update(genre);
             _context.SaveChanges();
            return genre;
        }
    }
}
