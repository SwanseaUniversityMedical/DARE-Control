﻿using Microsoft.AspNetCore.Mvc;
using BL;
using System.Reflection.Metadata.Ecma335;

namespace TRE_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HasuraController : Controller
    {
        [HttpGet("RunQuery/{token}")]
        public string RunQuery(string token)
        {

           
            return "";

        }

    }
}