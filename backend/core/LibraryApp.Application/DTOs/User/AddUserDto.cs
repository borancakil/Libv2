﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryApp.Domain.Enums;

namespace LibraryApp.Application.DTOs.User
{
    public class AddUserDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } 
    }
}
