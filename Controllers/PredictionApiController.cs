using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cadar_Raul_Lab4.Data;
using Cadar_Raul_Lab4.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cadar_Raul_Lab4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PredictionApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PredictionHistory>>> GetAll()
        {
            var list = await _context.PredictionHistories
                                      .OrderByDescending(p => p.CreatedAt)
                                      .ToListAsync();

            return Ok(list);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var prediction = await _context.PredictionHistories.FindAsync(id);

            if (prediction == null)
                return NotFound(new { message = "Prediction not found." });

            _context.PredictionHistories.Remove(prediction);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Prediction deleted successfully." });
        }

    }
}
