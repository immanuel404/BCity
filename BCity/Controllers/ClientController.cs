using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using BCity.MyServices;
using BCity.Models;
using BCity.Models.ViewModels;

namespace BCity.Controllers
{
    public class ClientController : Controller
    {

        private readonly MyDbContext _context;
        private readonly ClientCodeGenerator _clientCodeGenerator;

        public ClientController(MyDbContext context, ClientCodeGenerator clientCodeGenerator)
        {
            _context = context;
            _clientCodeGenerator = clientCodeGenerator;
        }


        //
        public async Task<IActionResult> Index()
        {
            // get -> clients
            var clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
            return View(clients);
        }


        // post: generate client code
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateClientCode()
        {
            var name = Request.Form["Name"].ToString().Trim(); 
            
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["NameField_Error"] = "Name is required!";
                return RedirectToAction(nameof(Index));
            }

            // generate client code
            var clientcode = _clientCodeGenerator.GenerateClientCode(name);

            // pass generated client code to the view
            TempData["Name"] = name;
            TempData["ClientCode"] = clientcode;

            return RedirectToAction(nameof(Index));
        }



        // post: create client
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create()
        {
            var name = Request.Form["Name"].ToString().Trim();
            var clientcode = Request.Form["ClientCode"].ToString().Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["NameField_Error"] = "Name is required!";
                ModelState.AddModelError("Name", "Name is required!");
                return RedirectToAction(nameof(Index));
            }
            if (string.IsNullOrWhiteSpace(clientcode))
            {
                TempData["NameField_Error"] = "Client Code is required!";
                ModelState.AddModelError("ClientCode", "Client Code is required!");
                return RedirectToAction(nameof(Index));
            }
            if (clientcode.Length != 6)
            {
                TempData["NameField_Error"] = "Client Code must be exactly 6 characters in length!";
                ModelState.AddModelError("ClientCode", "Client Code must be exactly 6 characters in length!");
                return RedirectToAction(nameof(Index));
            }
            var clientExists = await _context.Clients.FirstOrDefaultAsync(c => c.ClientCode == clientcode);
            if (clientExists != null)
            {
                TempData["NameField_Error"] = "Client Code is already taken!";
                ModelState.AddModelError("ClientCode", "Client Code is already taken!");
                return RedirectToAction(nameof(Index));
            }

            // save client
            if (ModelState.IsValid)
            {
                var client = new Client
                {
                    Name = name,
                    ClientCode = clientcode,
                    NoOfLinkedContacts = 0
                };
                _context.Add(client);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }



        // get: client details page
        public async Task<IActionResult> Details(int id)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            // first get all contact id's linked to this client
            var linkedContactIds = await _context.ClientContactLinks
                .Where(link => link.ClientId == id)
                .Select(link => link.ContactId)
                .ToListAsync();

            // get contacts linked to this client
            var linkedContacts = await _context.Contacts
                .Where(c => linkedContactIds.Contains(c.Id))
                .OrderBy(c => c.Surname).ThenBy(c => c.Name)
                .ToListAsync();

            // get contacts not linked to this client
            var unlinkedContacts = await _context.Contacts
                .Where(c => !linkedContactIds.Contains(c.Id))
                .OrderBy(c => c.Surname).ThenBy(c => c.Name)
                .ToListAsync();

            var viewModel = new ClientDetailsViewModel
            {
                Client = client,
                LinkedContacts = linkedContacts,
                UnlinkedContacts = unlinkedContacts
            };

            return View(viewModel);
        }



        // post: link a contact
        [HttpPost]
        public async Task<IActionResult> LinkContact(int clientId, int contactId)
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
                TempData["Msg"] = $"Client <strong>{client.Name}</strong> was linked to <strong>{contact?.Name} {contact?.Surname}</strong>.";

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = clientId });
        }



        // post: unlink a contact
        [HttpPost]
        public async Task<IActionResult> UnlinkContact(int clientId, int contactId)
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
                TempData["Msg"] = $"Client <strong>{client?.Name}</strong> was unlinked from <strong>{contact?.Name} {contact?.Surname}</strong>.";

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = clientId });
        }

    }
}
