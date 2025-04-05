using System.ComponentModel.DataAnnotations;

namespace BCity.Models
{
    public class ContactClientLink
    {
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public int ContactId { get; set; }
        public Contact Contact { get; set; } = null!;

        public DateTime LinkedDate { get; set; } = DateTime.UtcNow;
    }
}
