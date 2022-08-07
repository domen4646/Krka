using KrkaWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using System.Net.Mail;
using Microsoft.AspNet.Identity.Owin;
using KrkaWeb.App_Start;

namespace KrkaWeb.Controllers
{
    public class AccountActionController : Controller
    {

        /*

        //private SignInManager<ApplicationUser, string> _signInManager;
        public UserManager<ApplicationUser> UserManager { get; private set; }
        /*public SignInManager<ApplicationUser, string> SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<SignInManager<ApplicationUser, string>>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        //SignInManager { get; private set; }
        */
        public AccountActionController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }

        public AccountActionController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
            //SignInManager = HttpContext.GetOwinContext().GetUserManager<ApplicationSignInManager>();
            //SignInManager = ApplicationSignInManager.Create()
            //SignInManager = new SignInManager<ApplicationUser, string>(UserManager, AuthenticationManager);
        }

       /* public AccountActionController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser, string> signInManager)
        {
            UserManager = userManager;
            //SignInManager = signInManager;
        }*/

        //public ApplicationSignInManager SignInManager { get; private set; }
        public UserManager<ApplicationUser> UserManager { get; private set; }


        /*public AccountActionController(UserManager<ApplicationUser> userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            //SignInManager = signInManager;
        }*/



        // GET: AccoutAction
        public ActionResult Index()
        {
            /*if (Request.IsAuthenticated)
            {
                return "I AM AUTHENTICATED!!!";
            }*/
            ViewBag.CurrentTitle = "Error";
            ViewBag.CurrentMessage = "No account action specified!";
            return View("Message");
            //return "No account action specified!";
        }

        [ChildActionOnly]
        public string GetUsername ()
        {
            if (Session["CurrentUserName"] == null)
            {
                return "Prijava";
            }
            return Session["CurrentUserName"] as string;
        }

        [ChildActionOnly]
        public void SendNotificationEmail (int delivery_number, string userid, bool was_changed)
        {
            // was_changed -> če ni bila spremenjena, je bila potrjena.
            var user = UserManager.FindById(userid);
            if (user == null)
            {
                return;
            }
            try
            {
                MailMessage mm = new MailMessage(Globals.email_from, user.Email);
                mm.Subject = Globals.delivery_confirmed_email_subject;
                if (was_changed)
                {
                    mm.Subject = Globals.delivery_changed_email_subject;
                }
                mm.Body = String.Format(Globals.delivery_changed_email_body, user.UserName);
                if (was_changed)
                {
                    mm.Body = String.Format(Globals.delivery_changed_email_body, user.UserName, Globals.ConvertToSlovenianDateTime(DateTime.Now));
                }
                mm.IsBodyHtml = false;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = Globals.smtp_server;
                smtp.Port = Globals.smtp_port;
                smtp.UseDefaultCredentials = false;
                smtp.Send(mm);
            }
            catch (Exception)
            {

            } 
        }

        [ChildActionOnly]
        public string GetActionUrl ()
        {
            if (Session["CurrentUser"] == null)
            {
                return "Login";
            }
            return "UserInfo?userid=" + Session["CurrentUser"];
        }

        public async Task<ActionResult> UserInfo(string userid)
        {
            ApplicationUser user = null;
            if (userid != null)
            {
                user = await UserManager.FindByIdAsync(userid);
            }
            /*ViewBag.SiteUserName = user.UserName;
            ViewBag.SiteUserEmail = user.Email;
            ViewBag.LastLoginDate = user.LastLogin.ToShortDateString();*/
            if (user == null)
            {
                ViewBag.CurrentTitle = "Napaka";
                ViewBag.CurrentMessage = "Uporabnik ni bil najden!";
                return View("Message");
            }

            var user_del = Globals.DeliveryContext.Deliveries.Where(s => s.DeliveryUser == user.Id);
            ViewBag.SiteUser = user;
            ViewBag.UserDeliveries = user_del;
            ViewBag.AnyDeliveries = (user_del == null || user_del.Any());
            ViewBag.IsCurrentUser = (Session["CurrentUser"] != null && (string)Session["CurrentUser"] == userid);
            ViewBag.IsAdmin = (Session["CurrentUserRole"] != null && (string)Session["CurrentUserRole"] == "Administrator");
            return View();
        }

        [HttpPost]
        public ActionResult Logoff()
        {
            if (Session["CurrentUser"] == null)
            {
                ViewBag.CurrentTitle = "Napaka";
                ViewBag.CurrentMessage = "Da se odjavite, morate biti prijavljeni!";
                return View("Message");
            }
            Session["CurrentUser"] = null;
            Session["CurrentUserName"] = null;
            Session["CurrentUserRole"] = null;
            Session["StoreWarehouse"] = null;
            return Redirect("/");
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> Login(Models.AccountLoginModel model)
        {
            if (Session["CurrentUser"] != null)
            {
                ModelState.AddModelError("", "Trenutno ste že prijavljeni");
                return View();
            }
            if (ModelState.IsValid)
            {
                string username = model.username_or_email;
                if (username.Contains("@"))
                {
                    var use = await UserManager.FindByEmailAsync(username);
                    if (use != null)
                    {
                        username = use.UserName;
                    }
                }
                var user = await UserManager.FindAsync(model.username_or_email, model.password);
                if (user != null)
                {

                    if (user.EmailConfirmed)
                    {
                        // Sign in
                        Session["CurrentUser"] = user.Id;
                        Session["CurrentUserName"] = user.UserName;
                        Session["CurrentUserRole"] = user.UserRole;
                        if (user.UserRole == "Storekeeper")
                        {
                            Session["StoreWarehouse"] = user.WarehouseNumber;
                        }

                        user.LastLogin = DateTime.Now;
                        await UserManager.UpdateAsync(user);

                        return Redirect("/");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Vaš E-Mail rečun ni potrjen");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Nepravilno uporabniško ime ali geslo");
                }
            }

            return View(model);
        }


        // Prikaže seznam uporabnikov (Samo administrator)
        [HttpGet]
        public ActionResult Users()
        {
            if (Session["CurrentUser"] == null)
            {
                return Redirect("/AccountAction/Login");
            }
            if ((string)Session["CurrentUserRole"] != "Administrator")
            {
                ViewBag.CurrentTitle = "Dostop Zavrnjen";
                ViewBag.CurrentMessage = "Samo administrator lahko vidi seznam uporabnikov";
                return View("Message");
            }
            ViewBag.AllUsers = UserManager.Users;
            return View();
        }



        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        [HttpGet]
        public async Task<ActionResult> Forgot(string userid = "", string token = "")
        {
            var user = await UserManager.FindByIdAsync(userid);
            if (user == null)
            {
                ViewBag.CurrentTitle = "Napaka";
                ViewBag.CurrentMessage = "Neprepoznan uporabnik!";
                return View("Message");
                //return new ContentResult { Content = "Unknown user or invalid token!" };
            }
            ViewBag.PasswordResetUserID = userid;
            ViewBag.PasswordResetToken = token;
            ViewBag.CurrentUserName = user.UserName;
            return View();
            //return "Password cannot be forgotten!\r\nUsername: " + userid + "\r\nHash: " + token + "\r\n";
        }


        // Za pozabljeno geslo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Forgot(AccountForgotModel model)
        {
            
            if (model.confirm_new_password != model.new_password)
            {
                ModelState.AddModelError("", "Gesli se ne ujemata!");
                return View();
            }
            var user = await UserManager.FindByIdAsync(model.__userid);
            if (user == null || user.UniqueToken != model.__token)
            {
                ViewBag.CurrentTitle = "Napaka";
                ViewBag.CurrentMessage = "Neprepoznan uporabnik ali neveljaven žeton!";
                return View();
            }

            await UserManager.RemovePasswordAsync(model.__userid);
            string new_hash = UserManager.PasswordHasher.HashPassword(model.new_password);
            user.PasswordHash = new_hash;
            await UserManager.UpdateAsync(user);

            ViewBag.CurrentTitle = "Uspeh";
            ViewBag.CurrentMessage = "Vaše geslo je bilo spremenjeno!";
            return View("Message");
        }


        // Pozabljeno geslo
        // Uporabnik dobi E-Mail, ki vsebuje link s katerim lahko spremeni geslo
        [HttpGet]
        public ActionResult ForgotPassword ()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword (AccountMailModel model)
        {
            if (Session["CurrentUser"] != null)
            {
                ModelState.AddModelError("", "Trenutno ste že prijavljeni");
                return View();
            }
            if (ModelState.IsValid)
            {
                string username = model.username_or_email;
                if (username.Contains("@"))
                {
                    var use = await UserManager.FindByEmailAsync(username);
                    if (use != null)
                    {
                        username = use.UserName;
                    }
                }
                var user = await UserManager.FindByNameAsync(username);
                if (user == null)
                {
                    ModelState.AddModelError("", "Uporabnik ni bil najden!");
                }
                if (!user.EmailConfirmed)
                {
                    ModelState.AddModelError("", "Vaš E-Mail rečun ni potrjen");
                }
                string token = user.UniqueToken;//await UserManager.GeneratePasswordResetTokenAsync(user.Id);

                try
                {
                    MailMessage mm = new MailMessage(Globals.email_from, user.Email);
                    mm.Subject = "Spremenite pozabljeno geslo";
                    mm.Body = String.Format(Globals.change_password_body, user.UserName, Url.Action("Forgot", "AccountAction", new { userid = user.Id, token = token }, Request.Url.Scheme));
                    mm.IsBodyHtml = false;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = Globals.smtp_server;
                    smtp.Port = Globals.smtp_port;
                    smtp.UseDefaultCredentials = false;
                    smtp.Send(mm);
                    ViewBag.CurrentTitle = "Uspeh";
                    ViewBag.CurrentMessage = "Vaše geslo je spremenjeno!";
                    return View("Message");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "E-Maila ni bilo mogoče poslati!");
                    return View();
                }

            }
            return View(model);
        }



        // Potrditev E-Naslova
        public async Task<ActionResult> ConfirmEmail(string userid = "", string token = "")
        {
            ApplicationUser user = await UserManager.FindByIdAsync(userid);
            if (user != null)
            {
                if (user.EmailConfirmed)
                {
                    ViewBag.CurrentTitle = "Napaka";
                    ViewBag.CurrentMessage = "Vaš E-Mail je že potrjen!";
                    return View("Message");
                    //return "Your email is already confirmed!";
                }
                if (user.UniqueToken == token)
                {
                    user.EmailConfirmed = true;
                    await UserManager.UpdateAsync(user);
                    ViewBag.CurrentTitle = "Uspeh";
                    ViewBag.CurrentMessage = "Vaš E-Mail je uspešno potrjen!";
                    return View("Message");
                }
                else
                {
                    ViewBag.CurrentTitle = "Napaka";
                    ViewBag.CurrentMessage = "Neveljaven žeton!";
                    return View("Message");
                }
            }
            ViewBag.CurrentTitle = "Napaka";
            ViewBag.CurrentMessage = "Neprepoznan uporabnik!";
            return View("Message");
        }

        //[AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> Register(AccountRegisterModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.confirm_password == model.password)
                {
                    if (model.is_store)
                    {
                        string hash = Globals.GetSHA256(model.store_password);
                        if (hash != Globals.warehouse_password)
                        {
                            ModelState.AddModelError("", "Skladiščniško geslo ni pravilno!");
                            return View();
                        }
                        if (model.store_number < 0)
                        {
                            ModelState.AddModelError("", "Številka skladišča ne sme biti negativna!");
                            return View();
                        }
                    }
                    
                    ApplicationUser new_user = new ApplicationUser();
                    new_user.UserName = model.username;
                    new_user.Email = model.email;
                    new_user.EmailConfirmed = false;
                    new_user.DateCreated = DateTime.Now;
                    new_user.LastLogin = new_user.DateCreated;
                    new_user.UserRole = "NormalUser";
                    if (model.is_store)
                    {
                        new_user.UserRole = "Storekeeper";
                    }
                    if (new_user.UserName == "Admin")
                    {
                        new_user.UserRole = "Administrator";
                    }
                    

                    var existing_user = await UserManager.FindByEmailAsync(new_user.Email);
                    if (existing_user != null)
                    {
                        ModelState.AddModelError("", "That email already exists!");
                        return View();
                    }
                    existing_user = await UserManager.FindByNameAsync(new_user.UserName);
                    if (existing_user != null)
                    {
                        ModelState.AddModelError("", "That username already exists!");
                        return View();
                    }

                    Random r = new Random();
                    string unique_token_plaintext = r.Next().ToString() + new_user.DateCreated.ToLongDateString() + new_user.PasswordHash + new_user.Email + r.Next().ToString();
                    new_user.UniqueToken = Globals.GetSHA256(unique_token_plaintext); // Used for email
                    
                    //new_user.SecurityStamp
                    var result = await UserManager.CreateAsync(new_user, model.password);
                    if (result.Succeeded)
                    {
                        try
                        {
                            MailMessage mm = new MailMessage(Globals.email_from, new_user.Email);
                            mm.Subject = "Potrdite vaš E-Mail naslov";
                            mm.Body = String.Format(Globals.confirm_email_body, new_user.UserName, Url.Action("ConfirmEmail", "AccountAction", new { userid = new_user.Id, token = new_user.UniqueToken }, Request.Url.Scheme));
                            mm.IsBodyHtml = false;
                            SmtpClient smtp = new SmtpClient();
                            smtp.Host = Globals.smtp_server;
                            smtp.Port = Globals.smtp_port;
                            smtp.UseDefaultCredentials = false;
                            smtp.Send(mm);
                            ViewBag.CurrentTitle = "Uspeh";
                            ViewBag.CurrentMessage = "Vaša registracija je uspešna!";
                            return View("Message");
                        }
                        catch (Exception)
                        {
                            ModelState.AddModelError("", "Failed to send conformation email!");
                            await UserManager.DeleteAsync(new_user);
                        }
                        
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
                ModelState.AddModelError("", "Passwords must match!");
            }
            return View();
        }

        private void AddErrors (IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}