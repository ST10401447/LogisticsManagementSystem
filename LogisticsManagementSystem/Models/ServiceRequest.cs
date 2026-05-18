using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogisticsManagementSystem.Models
{
    public class ServiceRequest
    {
        [Key]
        public int ServiceID { get; set; }

        [ForeignKey ("Contract")]
        public int ContractID { get; set; }
        public Contract? Contract { get; set; }

        [Required]
        public string Description { get; set; }
        [Required]
        public string Cost { get; set; }
        [Required]
        public string Status { get; set; }

    }
}
