﻿using System.ComponentModel.DataAnnotations;

namespace NungSue.Areas.Admin.ViewModels.Account
{
    public class SignInViewModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}