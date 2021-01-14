using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhereTheFile.Database;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WhereTheFile.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScanController : ControllerBase
    {
        private readonly ILogger<ScanController> _logger;
        private readonly IWTFContext _context;
        private readonly IFileScanner _scanner;
        public ScanController(ILogger<ScanController> logger, IWTFContext context, IFileScanner scanner)
        {
            _logger = logger;
            _context = context;
            _scanner = scanner;
        }

        [HttpGet()]
        [Route("toplevel")]
        public ActionResult<string[]> ShowTopLevelScanPaths(string? start = null)
        {
            return _scanner.GetStartPaths(start);
        }


        // GET api/<ValuesController>/5
        [HttpGet()]

        //todo: WTF do you do when there's over Int32.Max files in the database?
        public IActionResult Get(int start = 0, int stop = 99)
        {
            return Ok(_context.FilePaths.Skip(start).Take(stop).ToList());
        }

        [HttpGet()]
        [Route("count")]
        public IActionResult Count()
        {
            return Ok(_context.FilePaths.Count());
        }

        // POST api/<ValuesController>
        [HttpPost]
        [Route("startscan")]
        public IActionResult StartScan([FromServices] IServiceScopeFactory serviceScopeFactory, [FromBody] string value)
        {
            Task.Run(async () =>
            {
                try
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    await _scanner.ScanFiles(value);
                }
                catch(Exception e)
                {
                    _logger.LogError(e.ToString());
                }
                }
            );

            return Ok($"Scanning {value}");
                   
        }



        //// PUT api/<ValuesController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<ValuesController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
