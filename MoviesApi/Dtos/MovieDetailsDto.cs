﻿namespace MoviesApi.Dtos
{
    public class MovieDetailsDto
    {
        public int Id { get; set; }
       
        public string Title { get; set; }
        
        public string StoryLine { get; set; }
        public int Year { get; set; }
        public float Rate { get; set; }
        public byte[] Poster { get; set; }
        public byte GenreId { get; set; }
        public string GenreName { get; set; }
    }
}
