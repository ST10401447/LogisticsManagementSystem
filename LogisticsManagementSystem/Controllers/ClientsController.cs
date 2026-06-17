using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using LogisticsManagementSystem.Models;

namespace LogisticsManagementSystem.Controllers
{
    public class ClientsController : Controller
    {
        private readonly HttpClient _httpClient;

        public ClientsController(IHttpClientFactory httpClientFactory)
        {
            // CHANGED: was "_httpClient = httpClient; _httpClient.BaseAddress = new Uri
            _httpClient = httpClientFactory.CreateClient("LogisticsApi");
        }
        // ==========================
        // GET: Clients
        // ==========================
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("clients");

            if (!response.IsSuccessStatusCode)
            {
                return View(new List<Client>());
            }

            var clients = await response.Content.ReadFromJsonAsync<List<Client>>();

            return View(clients);
        }

        // ==========================
        // GET: Clients/Details/5
        // ==========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var response = await _httpClient.GetAsync($"clients/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var client = await response.Content.ReadFromJsonAsync<Client>();

            return View(client);
        }

        // ==========================
        // GET: Clients/Create
        // ==========================
        public IActionResult Create()
        {
            return View();
        }

        // ==========================
        // POST: Clients/Create
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ClientID,Name,ContactNo,Email,Region")] Client client)
        {
            if (!ModelState.IsValid)
                return View(client);

            var response = await _httpClient.PostAsJsonAsync("clients", client);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Unable to create client.");
            return View(client);
        }

        // ==========================
        // GET: Clients/Edit/5
        // ==========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var response = await _httpClient.GetAsync($"clients/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var client = await response.Content.ReadFromJsonAsync<Client>();

            return View(client);
        }

        // ==========================
        // POST: Clients/Edit/5
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ClientID,Name,ContactNo,Email,Region")] Client client)
        {
            if (id != client.ClientID)
                return NotFound();

            if (!ModelState.IsValid)
                return View(client);

            var response = await _httpClient.PutAsJsonAsync(
                $"clients/{id}",
                client);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Unable to update client.");
            return View(client);
        }

        // ==========================
        // GET: Clients/Delete/5
        // ==========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var response = await _httpClient.GetAsync($"clients/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var client = await response.Content.ReadFromJsonAsync<Client>();

            return View(client);
        }

        // ==========================
        // POST: Clients/Delete/5
        // ==========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"clients/{id}");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Unable to delete client.");
                return View();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}