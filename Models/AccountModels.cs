using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KrkaWeb.Models
{
    public class AccountRegisterModel
    {

        [Required]
        [Display(Name = "Uporabniško ime")]
        public string username { get; set; }

        [Required]
        [Display(Name = "E-Mail")]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Geslo")]
        public string password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Potrditev Gesla")]
        public string confirm_password { get; set; }

        [Display(Name = "Skladiščnik")]
        public bool is_store { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Skladiščniško geslo")]
        public string store_password { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Številka skladišča")]
        public int? store_number { get; set; }
    }


    public class AccountLoginModel
    {

        [Required]
        [Display(Name = "Username or Email")]
        public string username_or_email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }

    }



    public class AccountForgotModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Novo geslo")]
        public string new_password;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Potrditev gesla")]
        public string confirm_new_password;

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "UserID")]
        public string __userid;

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Token")]
        public string __token;
    }

    public class AccountMailModel
    {
        [Required]
        [Display(Name = "Username or Email")]
        public string username_or_email { get; set; }
    }
}