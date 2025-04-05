using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BCity.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string ClientCode { get; set; } = null!;

        public int NoOfLinkedContacts { get; set; }

        // join entity
        public ICollection<ContactClientLink> ContactLinks { get; set; } = new List<ContactClientLink>();
    }
}