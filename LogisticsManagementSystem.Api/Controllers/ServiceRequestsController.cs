using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticsManagementSystem.Api.Data;
using LogisticsManagementSystem.Api.Models;

namespace LogisticsManagementSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly LogisticDbContext _context;

        public ServiceRequestsController(LogisticDbContext context)
        {
            _context = context;
        }

        // GET: api/ServiceRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetServiceRequests()
        {
            return await _context.ServiceRequests
                .Include(s => s.Contract)
                .ToListAsync();
        }

        // GET: api/ServiceRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceRequest>> GetServiceRequest(int id)
        {
            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(s => s.ServiceID == id);
            if (serviceRequest == null)
            {
                return NotFound();
            }
            return serviceRequest;
        }

        // PUT: api/ServiceRequests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceRequest(int id, ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.ServiceID)
            {
                return BadRequest();
            }
            _context.Entry(serviceRequest).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceRequestExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // POST: api/ServiceRequests
        [HttpPost]
        public async Task<ActionResult<ServiceRequest>> PostServiceRequest(ServiceRequest serviceRequest)
        {
            var contract = await _context.Contracts.FindAsync(serviceRequest.ContractID);
            if (contract == null)
            {
                return BadRequest("Contract not found.");
            }

            if (contract.EndDate < DateOnly.FromDateTime(DateTime.Today)
                && contract.Status != "On Hold")
            {
                contract.Status = "Expired";
                _context.Entry(contract).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            if (contract.Status == "Expired" || contract.Status == "On Hold")
            {
                return BadRequest("Cannot create Service Request: Parent Contract is Expired or On Hold.");
            }

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetServiceRequest", new { id = serviceRequest.ServiceID }, serviceRequest);
        }

        // DELETE: api/ServiceRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceRequest(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
            {
                return NotFound();
            }
            _context.ServiceRequests.Remove(serviceRequest);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ServiceRequestExists(int id)
        {
            return _context.ServiceRequests.Any(e => e.ServiceID == id);
        }
    }
}