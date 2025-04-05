
using System.ComponentModel.DataAnnotations;

namespace BCity.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Surname { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        public int NoOfLinkedClients { get; set; }

        // join entity
        public ICollection<ContactClientLink> ClientLinks { get; set; } = new List<ContactClientLink>();
    }
}