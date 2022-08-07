using KrkaWeb.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Web;


//Initial Catalog=user_database;

namespace KrkaWeb
{



    public class Globals
    {
        // Emails
        public static string email_from = "info@test.si";
        public static string smtp_server = "smtp.amis.net";
        public static ushort smtp_port = 25;

        // Email content
        public static string confirm_email_subject = "Potrdite vaš E-Mail naslov";
        public static string confirm_email_body = "Pozdravljeni, {0}\r\nZahvaljujemo se vam za registracijo.\r\nObiščite link spodaj in potrdite vaš E-Mail račun.\r\n\r\n{1}\r\n";
        public static string delivery_confirmed_email_subject = "[INFO] Vaša pošiljka je bila potrjena";
        public static string delivery_confirmed_email_body = "Pozdravljeni, {0}\r\n\r\nObveščamo vas, da je skladiščnik potrdil vašo pošiljko.\r\n\r\nLep dan še naprej.";
        public static string delivery_changed_email_subject = "[INFO] Administrator je spremenil vašo pošiljko";
        public static string delivery_changed_email_body = "Pozdravljeni, {0}\r\n\r\nObveščamo vas, da je administrator spremenil vašo pošiljko ob spodaj navedenem času:\r\n{1}\r\n\r\nLep dan še naprej.";
        public static string change_password_body = "Pozdravljeni, {0}\r\nPred kratkim ste nam sporočili, da ste pozabili vaše geslo.\r\nObiščite link spodaj da spremenite geslo.\r\n\r\n{1}\r\n";


        // To je SHA256
        public static string warehouse_password = "5F39E7EADF6C32C1834ABEFFB804F5CA6727E3F664864AADDFC887CA15C8A7FF";


        // Dostave
        public static DeliveryDbContext DeliveryContext = new DeliveryDbContext();

        public static string GetSHA256(string input)
        {
            SHA256Managed sha = new SHA256Managed();
            byte[] hash_bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash_bytes).Replace("-", "");
        }

        public static string GetSlovenianDate(int day_index)
        {
            CultureInfo culture = new CultureInfo("sl-SI");
            return DateTime.Now.Date.AddDays(day_index).ToString(culture.DateTimeFormat.LongDatePattern, culture);
        }

        public static string ConvertToSlovenianDate(DateTime _dt)
        {
            CultureInfo culture = new CultureInfo("sl-SI");
            return _dt.ToString(culture.DateTimeFormat.LongDatePattern, culture);
        }

        public static string ConvertToSlovenianDateTime(DateTime _dt)
        {
            CultureInfo culture = new CultureInfo("sl-SI");
            return _dt.ToString(culture.DateTimeFormat.LongDatePattern, culture) +
                " " + _dt.ToString(culture.DateTimeFormat.LongTimePattern, culture);
        }
    }
}