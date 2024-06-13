using System.ComponentModel.DataAnnotations;

namespace StoreAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public List<Order> Orders { get; set; }
    }
}
