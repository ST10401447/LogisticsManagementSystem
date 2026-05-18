using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogisticsManagementSystem.Models
{
    public class ServiceRequest
    {
        [Key]
        public int ServiceID { get; set; }

        [ForeignKey("Contract")]
        [Required(ErrorMessage = "Please select a Contract")]
        [Display(Name = "Contract")]
        public int ContractID { get; set; }

        public Contract? Contract { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cost is required")]
        [Display(Name = "Cost (USD)")]
        public string Cost { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a Status")]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";
    }
}