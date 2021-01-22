using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buildersoft.Andy.X.Storage.Data.Model;
using Buildersoft.Andy.X.Storage.Extensions;
using Buildersoft.Andy.X.Storage.IO.Configurations;
using Buildersoft.Andy.X.Storage.Logic.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Buildersoft.Andy.X.Storage.Controllers
{
    [Route("andyx")]
    [ApiController]
    public class ConnectionsController : ControllerBase
    {
        private readonly ILogger<ConnectionsController> _logger;
        private readonly ConnectionService _connectionService;
        private readonly IServiceProvider _serviceProvider;

        public ConnectionsController(ILogger<ConnectionsController> logger, ConnectionService connectionService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _connectionService = connectionService;
            _serviceProvider = serviceProvider;
        }

        [HttpPost("remote/register")]
        public async Task<ActionResult<string>> RegisterAndyX([FromBody] AndyXProperty andyX)
        {
            if (andyX == null)
                return BadRequest("Please provide Andy X Remote data");

            DataStorage dataStorage = ConfigFile.GetDataStorageSettings();
            if (dataStorage.DataStorageName == "")
                return BadRequest("First, you have to configure Andy X Storage");

            if (await _connectionService.RegisterToAndyXAsync(dataStorage, andyX) != true)
                return BadRequest("Can not connect to Andy X, try different url");

            if (ConfigFile.UpdateAndyXSettings(andyX))
                return Ok("Andy X has been updated, now you can try to connect to the remote, by calling the /connect endpoint");

            return BadRequest("Please provide Andy X Remote data");
        }

        [HttpPost("remote/connect")]
        public ActionResult<string> ConnectToAndyX()
        {
            AndyXProperty andyX = ConfigFile.GetAndyXSettings();
            if (andyX.Name == "")
                return BadRequest("First, you have to register Andy X Remote.");

            // TODO... Connect to Andy X, try a Ping.
            if (_connectionService.ConnectToAndyX(andyX))
            {
                SignalR.InitializeSignalREventHandlers(null, _serviceProvider);
            }

            return Ok("Linked");
        }

        [HttpPost("remote/connect/test")]
        public ActionResult<string> TestConnectionWithAndyX()
        {
            AndyXProperty andyX = ConfigFile.GetAndyXSettings();
            if (andyX.Name == "")
                return BadRequest("First, you have to register Andy X Remote.");

            // TODO... Connect to Andy X, try a Ping.
            if (_connectionService.ConnectToAndyX(andyX))
            {
                return Ok("Connection exists between Andy X Storage and Andy X");
            }
            return BadRequest("Connection does not exists between Andy X Storage and Andy X");
        }

        [HttpGet("remote/download/currentstate")]
        public async Task<ActionResult<string>> GetCurrentState()
        {
            AndyXProperty andyX = ConfigFile.GetAndyXSettings();
            if (andyX.Name == "")
                return BadRequest("First, you have to register Andy X Remote.");

            if (await _connectionService.GetAndyXCurrentStateAsync(andyX) != true)
                return BadRequest("Can not connect to Andy X, try different url");

            return Ok("Updated");
        }
    }
}
