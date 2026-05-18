using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogisticsManagementSystem.Models
{
    public class Contract
    {
        [Key]
        public int ContractID { get; set; }

        [ForeignKey("Client")]
        public int ClientID { get; set; }
        public Client? Client { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }
        [Required]

        public string Status { get; set; }
        [Required]
        public string ServiceLevel { get; set; }

        public string? SignedAgreement { get; set; }

        public List<ServiceRequest> ServiceRequest { get; set; } = new();
    }
}

