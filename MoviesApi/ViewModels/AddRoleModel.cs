namespace MoviesApi.ViewModels
{
    public class AddRoleModel
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Role { get; set; }
    }
}
