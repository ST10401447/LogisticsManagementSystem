using LogisticsManagementSystem.Models;
using LogisticsManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;

namespace LogisticsManagementSystem.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly CurrencyService _currencyService;
        private readonly HttpClient _httpClient;


        public ServiceRequestsController(
      IHttpClientFactory httpClientFactory,
      CurrencyService currencyService)
        {     _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://logistics-api:8080/api/");
            _currencyService = currencyService;
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("servicerequests");
            if (!response.IsSuccessStatusCode)
                return View(new List<ServiceRequest>());
            var serviceRequests = await response.Content.ReadFromJsonAsync<List<ServiceRequest>>();
            return View(serviceRequests);
        }

        // GET: ServiceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var response = await _httpClient.GetAsync($"servicerequests/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();
            var serviceRequest = await response.Content.ReadFromJsonAsync<ServiceRequest>();
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create()
        {
            var contractsResponse = await _httpClient.GetAsync("Contracts");
            var contracts = await contractsResponse.Content.ReadFromJsonAsync<List<Contract>>() ?? new List<Contract>();
            ViewData["ContractID"] = new SelectList(contracts, "ContractID", "ServiceLevel");
            return View();
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ServiceID,ContractID,Description,Cost,Status")]
            ServiceRequest serviceRequest)
        {
            if (serviceRequest.ContractID > 0)
            {
                var contractRes = await _httpClient.GetAsync($"Contracts/{serviceRequest.ContractID}");
                var contract = await contractRes.Content.ReadFromJsonAsync<Contract>();

                if (contract != null)
                {
                    if (contract.Status == "Expired" || contract.Status == "On Hold")
                    {
                        ModelState.AddModelError("",
                            "Cannot create Service Request: Parent Contract is Expired or On Hold.");

                        var contractsRes = await _httpClient.GetAsync("Contracts");
                        var contractsList = await contractsRes.Content.ReadFromJsonAsync<List<Contract>>() ?? new List<Contract>();
                        ViewData["ContractID"] = new SelectList(contractsList, "ContractID", "ServiceLevel", serviceRequest.ContractID);
                        return View(serviceRequest);
                    }
                }
            }

            if (!string.IsNullOrEmpty(serviceRequest.Cost)
                && decimal.TryParse(serviceRequest.Cost, out decimal usdAmount))
            {
                try
                {
                    decimal rate = await _currencyService.GetUsdToZarRateAsync();
                    decimal zarAmount = Math.Round(usdAmount * rate, 2);
                    serviceRequest.Cost = $"USD {usdAmount} = ZAR {zarAmount}";
                }
                catch
                {
                    serviceRequest.Cost += " (ZAR conversion failed)";
                }
            }

            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync("servicerequests", serviceRequest);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
                ModelState.AddModelError("", "Failed to create Service Request.");
            }

            var contractsResponse2 = await _httpClient.GetAsync("Contracts");
            var contracts2 = await contractsResponse2.Content.ReadFromJsonAsync<List<Contract>>() ?? new List<Contract>();
            ViewData["ContractID"] = new SelectList(contracts2, "ContractID", "ServiceLevel", serviceRequest.ContractID);
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var response = await _httpClient.GetAsync($"servicerequests/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();
            var serviceRequest = await response.Content.ReadFromJsonAsync<ServiceRequest>();
            if (serviceRequest == null) return NotFound();

            var contractsResponse = await _httpClient.GetAsync("Contracts");
            var contracts = await contractsResponse.Content.ReadFromJsonAsync<List<Contract>>() ?? new List<Contract>();
            ViewData["ContractID"] = new SelectList(contracts, "ContractID", "ServiceLevel", serviceRequest.ContractID);
            return View(serviceRequest);
        }

        // POST: ServiceRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ServiceID,ContractID,Description,Cost,Status")]
            ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.ServiceID) return NotFound();

            if (serviceRequest.ContractID > 0)
            {
                var contractRes = await _httpClient.GetAsync($"Contracts/{serviceRequest.ContractID}");
                var contract = await contractRes.Content.ReadFromJsonAsync<Contract>();

                if (contract != null)
                {
                    if (contract.Status == "Expired" || contract.Status == "On Hold")
                        ModelState.AddModelError("",
                            "Cannot update Service Request: Parent Contract is Expired or On Hold.");
                }
            }

            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync(
                    $"servicerequests/{serviceRequest.ServiceID}", serviceRequest);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
                ModelState.AddModelError("", "Failed to update Service Request.");
            }

            var contractsResponse2 = await _httpClient.GetAsync("Contracts");
            var contracts2 = await contractsResponse2.Content.ReadFromJsonAsync<List<Contract>>() ?? new List<Contract>();
            ViewData["ContractID"] = new SelectList(contracts2, "ContractID", "ServiceLevel", serviceRequest.ContractID);
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var response = await _httpClient.GetAsync($"servicerequests/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();
            var serviceRequest = await response.Content.ReadFromJsonAsync<ServiceRequest>();
            if (serviceRequest == null) return NotFound();
            return View(serviceRequest);
        }

        // POST: ServiceRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"servicerequests/{id}");
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to delete Service Request.");
                return View();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}