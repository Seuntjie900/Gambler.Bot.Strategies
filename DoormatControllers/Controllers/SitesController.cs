using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoormatControllers.Controllers
{
    [Route("api/[controller]")]
    public class SitesController:Controller
    {
        // GET api/contacts
        [HttpGet]
        public IActionResult Get()
        {
            if (Program.DiceBot != null)
            {
                Program.DiceBot.CompileSites();
                return Ok(Program.DiceBot.Sites);
            }
            return this.Forbid();
        }
    }
}
