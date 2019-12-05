﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vts.Api.Models;
using Vts.Api.Services;

namespace Vts.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IForwardSolverService _forwardSolverService;

        public ValuesController(IForwardSolverService forwardSolverService)
        {
            _forwardSolverService = forwardSolverService;
        }

        // GET api/v1/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/v1/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/v1/values
        [HttpPost]
        [Authorize(Policy = "ApiKeyPolicy")]
        public string Post([FromBody] SolutionDomainPlotParameters plotParameters)
        {
            return _forwardSolverService.GetPlotData(plotParameters);
        }
    }
}
