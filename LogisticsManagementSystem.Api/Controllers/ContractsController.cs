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
    public class ContractsController : ControllerBase
    {
        private readonly LogisticDbContext _context;

        public ContractsController(LogisticDbContext context)
        {
            _context = context;
        }

        // GET: api/Contracts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contract>>> GetContracts()
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .ToListAsync();
        }

        // GET: api/Contracts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Contract>> GetContract(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.ContractID == id);
            if (contract == null)
            {
                return NotFound();
            }
            return contract;
        }

        // PUT: api/Contracts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContract(int id, Contract contract)
        {
            if (id != contract.ContractID)
            {
                return BadRequest();
            }
            _context.Entry(contract).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContractExists(id))
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

        // POST: api/Contracts
        [HttpPost]
        public async Task<ActionResult<Contract>> PostContract(Contract contract)
        {
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetContract", new { id = contract.ContractID }, contract);
        }

        // DELETE: api/Contracts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound();
            }
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.ContractID == id);
        }
    }
}