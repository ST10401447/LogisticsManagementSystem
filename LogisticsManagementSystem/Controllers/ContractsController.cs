using LogisticsManagementSystem.Models;
using LogisticsManagementSystem.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;

namespace LogisticsManagementSystem.Controllers
{
    public class ContractsController : Controller
    {
        private readonly FileValidationService _fileValidationService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly HttpClient _httpClient;


        public ContractsController(
    IHttpClientFactory httpClientFactory,
    FileValidationService fileValidationService,
    IWebHostEnvironment webHostEnvironment)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://logistics-api:8080/api/");
            _fileValidationService = fileValidationService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Contracts
        public async Task<IActionResult> Index(string? status, DateOnly? startDate, DateOnly? endDate)
        {
            var response = await _httpClient.GetAsync("contracts");
            if (!response.IsSuccessStatusCode)
                return View(new List<Contract>());

            var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>();

            if (!string.IsNullOrEmpty(status))
                contracts = contracts?.Where(c => c.Status == status).ToList();
            if (startDate.HasValue)
                contracts = contracts?.Where(c => c.StartDate >= startDate.Value).ToList();
            if (endDate.HasValue)
                contracts = contracts?.Where(c => c.EndDate <= endDate.Value).ToList();

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentStartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentEndDate = endDate?.ToString("yyyy-MM-dd");

            return View(contracts);
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var response = await _httpClient.GetAsync($"contracts/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();
            var contract = await response.Content.ReadFromJsonAsync<Contract>();
            return View(contract);
        }

        // GET: Contracts/Create
        public async Task<IActionResult> Create()
        {
            var clientsResponse = await _httpClient.GetAsync("Clients");
            var clients = await clientsResponse.Content.ReadFromJsonAsync<List<Client>>() ?? new List<Client>();
            ViewData["ClientID"] = new SelectList(clients, "ClientID", "Name");
            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ClientID,StartDate,EndDate,Status,ServiceLevel")]
            Contract contract,
            IFormFile? SignedAgreement)
        {
            if (SignedAgreement == null || SignedAgreement.Length == 0)
                ModelState.AddModelError("SignedAgreement", "Signed Agreement PDF is required.");
            else
            {
                try { _fileValidationService.ValidatePdf(SignedAgreement.FileName); }
                catch (Exception ex) { ModelState.AddModelError("SignedAgreement", ex.Message); }
            }

            if (contract.EndDate < contract.StartDate)
                ModelState.AddModelError("EndDate", "End Date cannot be earlier than Start Date.");

            if (ModelState.IsValid)
            {
                if (SignedAgreement != null)
                {
                    var uploadsFolder = Path.Combine(
                        _webHostEnvironment.WebRootPath, "Uploads", "Contracts");
                    Directory.CreateDirectory(uploadsFolder);
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(SignedAgreement.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await SignedAgreement.CopyToAsync(stream);
                    contract.SignedAgreement = $"/Uploads/Contracts/{fileName}";
                }

                var response = await _httpClient.PostAsJsonAsync("contracts", contract);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
                ModelState.AddModelError("", "Failed to create contract.");
            }

            var clientsRes = await _httpClient.GetAsync("Clients");
            var clients = await clientsRes.Content.ReadFromJsonAsync<List<Client>>() ?? new List<Client>();
            ViewData["ClientID"] = new SelectList(clients, "ClientID", "Name", contract.ClientID);
            return View(contract);
        }

        // DOWNLOAD PDF
        public async Task<IActionResult> Download(int id)
        {
            var response = await _httpClient.GetAsync($"contracts/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();
            var contract = await response.Content.ReadFromJsonAsync<Contract>();
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreement))
                return NotFound("File not found.");
            var filePath = Path.Combine(
                _webHostEnvironment.WebRootPath,
                contract.SignedAgreement.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found on server.");
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", "SignedAgreement.pdf");
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var response = await _httpClient.GetAsync($"contracts/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();
            var contract = await response.Content.ReadFromJsonAsync<Contract>();
            if (contract == null) return NotFound();

            var clientsResponse = await _httpClient.GetAsync("Clients");
            var clients = await clientsResponse.Content.ReadFromJsonAsync<List<Client>>() ?? new List<Client>();
            ViewData["ClientID"] = new SelectList(clients, "ClientID", "Name", contract.ClientID);
            return View(contract);
        }

        // POST: Contracts/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ContractID,ClientID,StartDate,EndDate,Status,ServiceLevel")]
            Contract contract,
            IFormFile? SignedAgreement)
        {
            if (id != contract.ContractID) return NotFound();

            var existingResponse = await _httpClient.GetAsync($"contracts/{id}");
            if (!existingResponse.IsSuccessStatusCode) return NotFound();
            var existingContract = await existingResponse.Content.ReadFromJsonAsync<Contract>();
            if (existingContract == null) return NotFound();
            contract.SignedAgreement = existingContract.SignedAgreement;

            if (SignedAgreement != null && SignedAgreement.Length > 0)
            {
                try
                {
                    _fileValidationService.ValidatePdf(SignedAgreement.FileName);
                    var uploadsFolder = Path.Combine(
                        _webHostEnvironment.WebRootPath, "Uploads", "Contracts");
                    Directory.CreateDirectory(uploadsFolder);
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(SignedAgreement.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await SignedAgreement.CopyToAsync(stream);
                    contract.SignedAgreement = $"/Uploads/Contracts/{fileName}";
                }
                catch (Exception ex) { ModelState.AddModelError("SignedAgreement", ex.Message); }
            }

            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync(
                    $"contracts/{contract.ContractID}", contract);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
                ModelState.AddModelError("", "Failed to update contract.");
            }

            var clientsRes = await _httpClient.GetAsync("Clients");
            var clientsList = await clientsRes.Content.ReadFromJsonAsync<List<Client>>() ?? new List<Client>();
            ViewData["ClientID"] = new SelectList(clientsList, "ClientID", "Name", contract.ClientID);
            return View(contract);
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var response = await _httpClient.GetAsync($"contracts/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();
            var contract = await response.Content.ReadFromJsonAsync<Contract>();
            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"contracts/{id}");
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to delete contract.");
                return View();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}