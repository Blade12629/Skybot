﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Skybot.Web
{
    public class IndexModel : PageModel
    {
        public string BotName => "Skybot";

        public void OnGet()
        {

        }
    }
}