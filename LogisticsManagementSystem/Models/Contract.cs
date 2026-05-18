using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogisticsManagementSystem.Models
{
    public class Contract
    {
        [Key]
        public int ContractID { get; set; }

        [Required(ErrorMessage = "Please select a Client")]
        [Display(Name = "Client")]
        public int ClientID { get; set; }

        public Client? Client { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "Please select a Status")]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Draft";

        [Required(ErrorMessage = "Service Level is required")]
        [StringLength(50, ErrorMessage = "Service Level cannot exceed 50 characters")]
        [Display(Name = "Service Level")]
        public string ServiceLevel { get; set; } = string.Empty;

        public string? SignedAgreement { get; set; }

        public List<ServiceRequest> ServiceRequest { get; set; } = new();
    }
}