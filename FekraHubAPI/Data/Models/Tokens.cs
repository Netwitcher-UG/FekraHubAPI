using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FekraHubAPI.Data.Models
{
    public class Tokens
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        public string? UserId { get; set; }
    }

}

