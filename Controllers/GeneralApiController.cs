using GCTConnect.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GCTConnect.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralApiController : ControllerBase
    {
        private readonly GctConnectContext _context;

        public GeneralApiController(GctConnectContext context)
        {
            _context = context;
        }

        // GET: api/generalapi/departments
        [HttpGet("departments")]
        public IActionResult GetDepartments()
        {
            var departments = _context.Departments
                .Select(d => new
                {
                    departmentId = d.DepartmentId,
                    name = d.Name
                })
                .ToList();

            return Ok(departments);
        }

        // GET: api/generalapi/batches?departmentId=#
        [HttpGet("batches")]
        public IActionResult GetBatches([FromQuery] int? departmentId)
        {
            var query = _context.Batches.AsQueryable();

            if (departmentId.HasValue)
            {
                query = query.Where(b => b.DepartmentId == departmentId.Value);
            }

            var batches = query
                .Select(b => new
                {
                    batchId = b.BatchId,
                    batchYear = b.BatchYear
                })
                .ToList();

            return Ok(batches);
        }
    }
}
