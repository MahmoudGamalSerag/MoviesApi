namespace MoviesApi.Dtos
{
    public class GenreDto
    {
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
