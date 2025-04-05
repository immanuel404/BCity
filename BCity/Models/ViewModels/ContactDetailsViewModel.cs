namespace BCity.Models.ViewModels
{
    public class ContactDetailsViewModel
    {
        public Contact Contact { get; set; } = null!;
        public List<Client> LinkedClients { get; set; } = new();
        public List<Client> UnlinkedClients { get; set; } = new();
    }
}