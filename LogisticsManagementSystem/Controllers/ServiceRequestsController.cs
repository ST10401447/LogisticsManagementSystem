using LogisticsManagementSystem.Data;
using LogisticsManagementSystem.Models;
using LogisticsManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogisticsManagementSystem.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly LogisticDbContext _context;
        private readonly CurrencyService _currencyService;

        public ServiceRequestsController(LogisticDbContext context, CurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index()
        {
            var logisticDbContext = _context.ServiceRequests.Include(s => s.Contract);
            return View(await logisticDbContext.ToListAsync());
        }

        // GET: ServiceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(m => m.ServiceID == id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        // GET: ServiceRequests/Create
        public IActionResult Create()
        {
            ViewData["ContractID"] = new SelectList(_context.Contracts, "ContractID", "ServiceLevel");
            return View();

        }

        // POST: ServiceRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServiceID,ContractID,Description,Cost,Status")] ServiceRequest serviceRequest)
        {

            // -------------------- WORKFLOW LOGIC --------------------
            if (serviceRequest.ContractID > 0)
            {
                var contract = await _context.Contracts.FindAsync(serviceRequest.ContractID);
                if (contract != null)
                {
                    if (contract.Status == "Expired" || contract.Status == "On Hold")
                    {
                        ModelState.AddModelError("", "Cannot create Service Request: Parent Contract is Expired or On Hold.");
                        ViewData["ContractID"] = new SelectList(_context.Contracts, "ContractID", "ServiceLevel", serviceRequest.ContractID);
                        return View(serviceRequest);
                    }
                }
            }
           

            //-------------------- CURRENCY CONVERSION --------------------
            if (!string.IsNullOrEmpty(serviceRequest.Cost) && decimal.TryParse(serviceRequest.Cost, out decimal usdAmount))
            {
                try
                {
                    decimal rate = await _currencyService.GetUsdToZarRateAsync();
                    decimal zarAmount = Math.Round(usdAmount * rate, 2);

                    serviceRequest.Cost = $"USD {usdAmount} = ZAR {zarAmount}";
                }
                catch
                {
                    // Fallback if API fails
                    serviceRequest.Cost += " (ZAR conversion failed)";
                }
            }
            



            if (ModelState.IsValid)
            {
                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ContractID"] = new SelectList(_context.Contracts, "ContractID", "ServiceLevel", serviceRequest.ContractID);
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
            {
                return NotFound();
            }
            ViewData["ContractID"] = new SelectList(_context.Contracts, "ContractID", "ServiceLevel", serviceRequest.ContractID);
            return View(serviceRequest);
        }

        // POST: ServiceRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceID,ContractID,Description,Cost,Status")] ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.ServiceID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(serviceRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceRequestExists(serviceRequest.ServiceID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ContractID"] = new SelectList(_context.Contracts, "ContractID", "ServiceLevel", serviceRequest.ContractID);
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(m => m.ServiceID == id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        // POST: ServiceRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest != null)
            {
                _context.ServiceRequests.Remove(serviceRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceRequestExists(int id)
        {
            return _context.ServiceRequests.Any(e => e.ServiceID == id);
        }
    }
}
