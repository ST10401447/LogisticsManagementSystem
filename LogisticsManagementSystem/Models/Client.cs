using System.ComponentModel.DataAnnotations;

namespace LogisticsManagementSystem.Models
{
    public class Client
    {
        [Key]
        public int ClientID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ContactNo { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Region { get; set; }

        public List<Contract> Contracts { get; set; } = new();
    }
}
