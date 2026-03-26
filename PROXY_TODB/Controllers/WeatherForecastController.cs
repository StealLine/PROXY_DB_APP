// <copyright file="WeatherForecastController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PROXY_TODB.Controllers
{
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.AspNetCore.Mvc;
    using PROXY_TODB.DBInterfaces;
    using PROXY_TODB.DBmethods;
    using PROXY_TODB.Models;

    [ApiController]
    [Route("[controller]/[action]")]
    public class HomeController : ControllerBase
    {
        private readonly IcreateDB createDB;
        private readonly ILogger<HomeController> logger;
        private readonly IremoveDB removeDB;
        private readonly IexecScript iexecScript;

        public HomeController(ILogger<HomeController> logger, IcreateDB icreateDB, IremoveDB iremoveDB, IexecScript iexecScript)
        {
            this.logger = logger;
            this.createDB = icreateDB;
            this.removeDB = iremoveDB;
            this.iexecScript = iexecScript;
        }

        [HttpGet]
        public async Task<IActionResult> CreateDB()
        {
            (string hash, string resp) = await this.createDB.Create();

            var re = new
            {
                Message = resp,
                Hash = hash,
            };

            return this.Ok(re);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDB(string uniquehash)
        {
            string response = await this.removeDB.Delete(uniquehash);
            return this.Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteScript([FromBody] ExecuteScriptRequest request)
        {
            var (scriptResponse, success) = await this.iexecScript.ExecuteScriptByHash(request.Hash, request.Script);

            var resp = new
            {
                Message = scriptResponse,
                Success = success,
            };

            return this.Ok(resp);
        }
        [HttpGet]
        public async Task<IActionResult> Hello()
        { 

            return Ok("I am working");

		}
    }
}
