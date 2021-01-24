using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buildersoft.Andy.X.Storage.Data.Model;
using Buildersoft.Andy.X.Storage.Data.Model.Enums;
using Buildersoft.Andy.X.Storage.IO.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Buildersoft.Andy.X.Storage.Controllers
{
    [Route("settings")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ILogger<SettingsController> logger)
        {
            _logger = logger;
        }

        [HttpGet("about")]
        public ActionResult<DataStorage> GetDataStorage()
        {
            return Ok(ConfigFile.GetDataStorageSettings());
        }

        [HttpPost("changes/storagename")]
        public ActionResult ChangeStorageName([FromQuery] string name)
        {
            if (name != null && name != "")
            {
                DataStorage dataStorage = ConfigFile.GetDataStorageSettings();
                dataStorage.DataStorageName = name;
                if (ConfigFile.UpdateDataStorageSettings(dataStorage) == true)
                    return Ok("Data Storage has been updated");
            }
            return BadRequest("Please provide the name for this data storage server");
        }
        [HttpPost("changes/storageenvironmant")]
        public ActionResult ChangeStorageEnvironment([FromQuery] string environment)
        {
            if (environment != null && environment != "")
            {
                DataStorage dataStorage = ConfigFile.GetDataStorageSettings();
                dataStorage.DataStorageEnvironment = (DataStorageEnvironment)Enum.Parse(typeof(DataStorageEnvironment), environment);
                if (ConfigFile.UpdateDataStorageSettings(dataStorage) == true)
                    return Ok("Data Storage has been updated");
            }
            return BadRequest("Please provide the environment for this data storage server");
        }

        [HttpPost("changes/storagetype")]
        public ActionResult ChangeStorageType([FromQuery] string type)
        {
            if (type != null && type != "")
            {
                DataStorage dataStorage = ConfigFile.GetDataStorageSettings();
                dataStorage.DataStorageType = (DataStorageType)Enum.Parse(typeof(DataStorageType), type);
                if (ConfigFile.UpdateDataStorageSettings(dataStorage) == true)
                    return Ok("Data Storage has been updated");
            }
            return BadRequest("Please provide the type for this data storage server");
        }

        [HttpPost("changes/storagestatus")]
        public ActionResult ChangeStorageStatus([FromQuery] string status)
        {
            if (status != null && status != "")
            {
                DataStorage dataStorage = ConfigFile.GetDataStorageSettings();
                dataStorage.DataStorageStatus = (DataStorageStatus)Enum.Parse(typeof(DataStorageStatus), status);
                if (ConfigFile.UpdateDataStorageSettings(dataStorage) == true)
                    return Ok("Data Storage has been updated");
            }
            return BadRequest("Please provide the status for this data storage server");
        }

        [HttpPost("changes/storageconfig")]
        public ActionResult ChangeStorageStatus([FromBody] DataStorage storage)
        {
            if (storage != null)
            {
                if (ConfigFile.UpdateDataStorageSettings(storage) == true)
                    return Ok("Data Storage has been updated");
            }
            return BadRequest("Please provide the data storage info for this data storage server");
        }
    }
}
