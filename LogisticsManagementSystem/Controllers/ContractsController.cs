using LogisticsManagementSystem.Data;
using LogisticsManagementSystem.Models;
using LogisticsManagementSystem.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LogisticsManagementSystem.Controllers
{
    public class ContractsController : Controller
    {
        private readonly LogisticDbContext _context;
        private readonly FileValidationService _fileValidationService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ContractsController(LogisticDbContext context,
                                  FileValidationService fileValidationService,
                                  IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _fileValidationService = fileValidationService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Contracts (with Search/Filter)
        public async Task<IActionResult> Index(string? status, DateOnly? startDate, DateOnly? endDate)
        {
            var query = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            if (startDate.HasValue)
                query = query.Where(c => c.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.EndDate <= endDate.Value);

            var contracts = await query.ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentStartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentEndDate = endDate?.ToString("yyyy-MM-dd");

            return View(contracts);
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.ContractID == id);

            return contract == null ? NotFound() : View(contract);
        }

        // GET: Contracts/Create
        public IActionResult Create()
        {
            ViewData["ClientID"] = new SelectList(_context.Clients, "ClientID", "Name");
            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientID,StartDate,EndDate,Status,ServiceLevel")] Contract contract, IFormFile? SignedAgreement)
        {
            // Manual validation for file
            if (SignedAgreement == null || SignedAgreement.Length == 0)
            {
                ModelState.AddModelError("SignedAgreement", "Signed Agreement PDF is required.");
            }
            else
            {
                try
                {
                    _fileValidationService.ValidatePdf(SignedAgreement.FileName);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("SignedAgreement", ex.Message);
                }
            }

            // Additional business validation
            if (contract.EndDate < contract.StartDate)
            {
                ModelState.AddModelError("EndDate", "End Date cannot be earlier than Start Date.");
            }

            if (ModelState.IsValid)
            {
                // File Upload
                if (SignedAgreement != null)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "Contracts");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(SignedAgreement.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await SignedAgreement.CopyToAsync(stream);
                    }

                    contract.SignedAgreement = $"/Uploads/Contracts/{fileName}";
                }

                _context.Add(contract);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdown if validation fails
            ViewData["ClientID"] = new SelectList(_context.Clients, "ClientID", "Name", contract.ClientID);
            return View(contract);
        }

        // Download Action
        public async Task<IActionResult> Download(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreement))
                return NotFound("File not found.");

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, contract.SignedAgreement.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found on server.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", "SignedAgreement.pdf");
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            ViewData["ClientID"] = new SelectList(_context.Clients, "ClientID", "Name", contract.ClientID);
            return View(contract);
        }

        // POST: Contracts/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContractID,ClientID,StartDate,EndDate,Status,ServiceLevel")] Contract contract, IFormFile? SignedAgreement)
        {
            if (id != contract.ContractID) return NotFound();

            if (SignedAgreement != null && SignedAgreement.Length > 0)
            {
                try
                {
                    _fileValidationService.ValidatePdf(SignedAgreement.FileName);

                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "Contracts");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(SignedAgreement.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await SignedAgreement.CopyToAsync(stream);
                    }

                    contract.SignedAgreement = $"/Uploads/Contracts/{fileName}";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("SignedAgreement", ex.Message);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.ContractID))
                        return NotFound();
                    throw;
                }
            }

            ViewData["ClientID"] = new SelectList(_context.Clients, "ClientID", "Name", contract.ClientID);
            return View(contract);
        }

        // Delete Actions (unchanged)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.ContractID == id);

            return contract == null ? NotFound() : View(contract);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.ContractID == id);
        }
    }
}