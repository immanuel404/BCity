namespace BCity.Models.ViewModels
{
    public class ClientDetailsViewModel
    {
        public Client Client { get; set; } = null!;
        public List<Contact> LinkedContacts { get; set; } = new();
        public List<Contact> UnlinkedContacts { get; set; } = new();
    }
}
