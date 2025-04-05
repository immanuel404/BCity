using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using BCity.Models;
using System.Text.RegularExpressions;
using BCity.Models.ViewModels;

namespace BCity.Controllers
{
    public class ContactController : Controller
    {

        private readonly MyDbContext _context;

        public ContactController(MyDbContext context)
        {
            _context = context;
        }


        // get -> contact
        public async Task<IActionResult> Index()
        {
            var contacts = await _context.Contacts.OrderBy(c => c.Surname).ThenBy(c => c.Name).ToListAsync();
            return View(contacts);
        }


        // post: create contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create()
        {
            var name = Request.Form["Name"].ToString().Trim();
            var surname = Request.Form["Surname"].ToString().Trim();
            var email = Request.Form["Email"].ToString().Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["NameField_Error"] = "Name is required!";
                ModelState.AddModelError("Name", "Name is required!");
                return RedirectToAction(nameof(Index));
            }
            if (string.IsNullOrWhiteSpace(surname))
            {
                TempData["SurnameField_Error"] = "Surname is required!";
                ModelState.AddModelError("Surname", "Surname is required!");
                return RedirectToAction(nameof(Index));
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["EmailField_Error"] = "Email is required!";
                ModelState.AddModelError("Email", "Email is required!");
                return RedirectToAction(nameof(Index));
            }
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                TempData["EmailField_Error"] = "Invalid email format!";
                ModelState.AddModelError("Email", "Invalid email format!");
                return RedirectToAction(nameof(Index));
            }
            var emailExists = await _context.Contacts.FirstOrDefaultAsync(c => c.Email == email);
            if (emailExists != null)
            {
                TempData["EmailField_Error"] = "Email is already taken!";
                ModelState.AddModelError("Email", "Email is already taken!");
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                var contact = new Contact
                {
                    Name = name,
                    Surname = surname,
                    Email = email,
                    NoOfLinkedClients = 0
                };
                _context.Add(contact);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        //
        // get: contact details page
        public async Task<IActionResult> Details(int id)
        {
            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id);

            if (contact == null)
            {
                return NotFound();
            }

            // first get all clients id's linked to this contact
            var linkedClientIds = await _context.ClientContactLinks
                .Where(link => link.ContactId == id)
                .Select(link => link.ClientId)
                .ToListAsync();

            // get clients linked to this contact
            var linkedClients = await _context.Clients
                .Where(c => linkedClientIds.Contains(c.Id))
                .OrderBy(c => c.Name)
                .ToListAsync();

            // get client not linked to this contacts
            var unlinkedClients = await _context.Clients
                .Where(c => !linkedClientIds.Contains(c.Id))
                .OrderBy(c => c.Name)
                .ToListAsync();

            var viewModel = new ContactDetailsViewModel
            {
                Contact = contact,
                LinkedClients = linkedClients,
                UnlinkedClients = unlinkedClients
            };

            return View(viewModel);
        }



        // post: link a client
        [HttpPost]
        public async Task<IActionResult> LinkClient(int clientId, int contactId)
        {
            var linkExists = await _context.ClientContactLinks
                .AnyAsync(l => l.ClientId == clientId && l.ContactId == contactId);

            if (!linkExists)
            {
                _context.ClientContactLinks.Add(new ContactClientLink
                {
                    ClientId = clientId,
                    ContactId = contactId,
                    LinkedDate = DateTime.UtcNow
                });

                // update client's no_of_linked_contacts
                var client = await _context.Clients.FindAsync(clientId);
                var contact = await _context.Contacts.FindAsync(contactId);
                if (client != null)
                {
                    client.NoOfLinkedContacts += 1;
                }
                if (contact != null)
                {
                    contact.NoOfLinkedClients += 1;
                }
                TempData["Msg"] = $"Contact <strong>{contact?.Name} {contact?.Surname}</strong> was linked to client <strong>{client.Name}</strong>.";

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = contactId });
        }



        // post: unlink a client
        [HttpPost]
        public async Task<IActionResult> UnlinkClient(int clientId, int contactId)
        {
            var linkToRemove = await _context.ClientContactLinks
                .FirstOrDefaultAsync(l => l.ClientId == clientId && l.ContactId == contactId);

            if (linkToRemove != null)
            {
                _context.ClientContactLinks.Remove(linkToRemove);

                // update client's no_of_linked_contacts
                var client = await _context.Clients.FindAsync(clientId);
                var contact = await _context.Contacts.FindAsync(contactId);
                if (client != null)
                {
                    client.NoOfLinkedContacts -= 1;
                }
                if (contact != null)
                {
                    contact.NoOfLinkedClients -= 1;
                }
                TempData["Msg"] = $"Contact <strong>{contact?.Name} {contact?.Surname}</strong> was unlinked from client <strong>{client.Name}</strong>.";

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = contactId });
        }


    }
}
