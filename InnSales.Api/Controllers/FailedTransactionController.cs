using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using InnSales.DataBase;
using InnSales.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InnSales.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/failed-transactions")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Authorize(Roles = "Employee")] 
        public class FailedTransactionController : ControllerBase
    {
        private readonly InnSalesDbContext _context;

        public FailedTransactionController(InnSalesDbContext context)
        {
            _context = context;
        }

  
        [HttpGet]
        public async Task<ActionResult<List<FailedTransaction>>> GetAllFailedTransactions()
        {
            var failedTransactions = await _context.FailedTransactions
                .OrderByDescending(ft => ft.LoggedDate)
                .ToListAsync();

            return Ok(failedTransactions);
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<FailedTransaction>> GetFailedTransactionById(Guid id)
        {
            var transaction = await _context.FailedTransactions.FindAsync(id);
            if (transaction == null)
                return NotFound();

            return Ok(transaction);
        }
    }
}