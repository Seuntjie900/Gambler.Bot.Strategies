using DoormatBot;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoormatControllers.Controllers
{
    [Route("api/[controller]")]
    public class InitController : Controller
    {        
        [HttpGet]
        public IActionResult Get()
        {           
            Program.DiceBot = new Doormat();
            return Ok(Program.DiceBot);
        }
    }
}
