using KrkaWeb.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace KrkaWeb.Controllers
{
    public class HomeController : Controller
    {

        // GET: Home
        public ActionResult Index()
        {
            ViewBag.LoggedIn = Session["CurrentUser"] != null;
            if (ViewBag.LoggedIn)
            {
                ViewBag.IsAdmin = (string)Session["CurrentUserRole"] == "Administrator";
                ViewBag.IsNormal = (string)Session["CurrentUserRole"] == "NormalUser";
                ViewBag.IsStore = (string)Session["CurrentUserRole"] == "Storekeeper";
                
                if (ViewBag.IsStore)
                {
                    Int32 war_id = (Int32)Session["StoreWarehouse"];
                    var deliveries = Globals.DeliveryContext.Deliveries.Where(s => s.DeliveryWarehouse == war_id);
                    if (deliveries == null)
                    {
                        ViewBag.CurrentTitle = "Prazno";
                        ViewBag.CurrentMessage = "Trenutno ni nobenih pošiljk v tem skladišču.";
                        return View("Message");
                    }
                    deliveries = deliveries.OrderBy(s => s.DeliveryDone);
                    ViewBag.Deliveries = deliveries;
                    ViewBag.UserWar = war_id;
                }
                if (ViewBag.IsAdmin)
                {
                    ViewBag.Deliveries = Globals.DeliveryContext.Deliveries;
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult GenerateReport(int delivery_id)
        {
            if (Session["CurrentUser"] == null)
            {
                return Redirect("/AccountAction/login");
            }
            if (delivery_id < 0)
            {
                ViewBag.CurrentMessage = "Indeks dostave mora biti pozitiven!";
                ViewBag.CurrentTitle = "Napaka";
                return View("Message");
            }
            var del = Globals.DeliveryContext.Deliveries.Where(s => s.DeliveryDone && s.DeliveryID == delivery_id);
            if (del == null || !del.Any())
            {
                ViewBag.CurrentMessage = "Pošiljka ni bila najdena!";
                ViewBag.CurrentTitle = "Napaka";
                return View("Message");
            }
            ApplicationDelivery delivery = del.First();
            if ((string)Session["CurrentUserRole"] != "Administrator" && (string)Session["CurrentUser"] != delivery.DeliveryUser)
            {
                ViewBag.CurrentMessage = "Dostop zavrnjen";
                ViewBag.CurrentTitle = "Napaka";
                return View("Message");
            }

            ViewBag.ReportDeliveryUser = delivery.DeliveryUserName;
            ViewBag.ReportDeliveryNumber = delivery.DeliveryNumber;
            ViewBag.ReportDeliveryDate = delivery.DeliveryDate;
            ViewBag.ReportDeliveryPlace = delivery.DeliveryPlace;
            ViewBag.ReportDeliveryWarehause = delivery.DeliveryWarehouse;
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> ConfirmDelivery(int delivery_number)
        {
            if (Session["CurrentUser"] == null)
            {
                return Redirect("/AccountAction/login");
            }
            if ((string)Session["CurrentUserRole"] != "Storekeeper")
            {
                ViewBag.CurrentMessage = "Samo skladiščnik lahko potrdi dostavo!";
                ViewBag.CurrentTitle = "Napaka";
                return View("Message");
            }
            if (delivery_number < 0)
            {
                ViewBag.CurrentMessage = "Številka dostave mora biti pozitivna!";
                ViewBag.CurrentTitle = "Napaka";
                return View("Message");
            }
            var del = Globals.DeliveryContext.Deliveries.Where(s => !s.DeliveryDone && s.DeliveryNumber == delivery_number);
            if (del == null || !del.Any())
            {
                ViewBag.CurrentMessage = "Pošiljka ni bila najdena!";
                ViewBag.CurrentTitle = "Napaka";
                return View("Message");
            }
            ApplicationDelivery delivery = del.First();
            delivery.DeliveryDone = true;
            await Globals.DeliveryContext.SaveChangesAsync();
            var controller = DependencyResolver.Current.GetService<AccountActionController>();
            controller.ControllerContext = new ControllerContext(this.Request.RequestContext, controller);
            controller.SendNotificationEmail(delivery.DeliveryNumber, delivery.DeliveryUser, false);
            ViewBag.CurrentMessage = "Pošiljka uspešno potrjena!";
            ViewBag.CurrentTitle = "Uspeh";
            return View("Message");
        }

        [HttpGet]
        public ActionResult AddDelivery ()
        {
            if (Session["CurrentUser"] == null)
            {
                return Redirect("/AccountAction/login");
            }
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddDelivery (DeliveryInfoModel model)
        {
            if (Session["CurrentUser"] == null)
            {
                return Redirect("/AccountAction/login");
            }
            if (ModelState.IsValid)
            {
                if ((string)Session["CurrentUserRole"] != "Administrator" && (string)Session["CurrentUserRole"] != "NormalUser")
                {
                    ModelState.AddModelError("", "Samo uporabnik in administrator lahko dodajata dostave");
                    return View();
                }

                if (model.date < 0 || model.date > 6)
                {
                    ModelState.AddModelError("", "Napaka pri izbiri dneva!");
                    return View();
                }

                DateTime del_date = DateTime.Now.Date.AddDays(model.date);

                if (model.place < 1 || model.place > 5)
                {
                    ModelState.AddModelError("", "Dostavna točka mora biti med 1 in 5!");
                    return View();
                }
                if (model.number < 0)
                {
                    ModelState.AddModelError("", "Številka dostave mora biti pozitivna!");
                    return View();
                }
                var data = Globals.DeliveryContext.Deliveries.Where(s => s.DeliveryNumber == model.number && !s.DeliveryDone);
                if (data != null && data.Any())
                {
                    ModelState.AddModelError("", "Ta številka dostave že obstaja!");
                    return View();
                }
                ApplicationDelivery delivery = new ApplicationDelivery();
                delivery.DeliveryDate = del_date;
                delivery.DeliveryID = 0;
                delivery.DeliveryPlace = model.place;
                delivery.DeliveryNumber = model.number;
                delivery.DeliveryWarehouse = delivery.DeliveryNumber / 1000;

                delivery.DeliveryDone = false;
                delivery.DeliveryUser = Session["CurrentUser"] as string;
                delivery.DeliveryUserName = Session["CurrentUserName"] as string;

                data = Globals.DeliveryContext.Deliveries.Where(s => s.DeliveryDate == delivery.DeliveryDate && s.DeliveryWarehouse == delivery.DeliveryWarehouse && delivery.DeliveryPlace == s.DeliveryPlace);
                if (data != null && data.Any())
                {
                    ModelState.AddModelError("", "Ta točka je že zasedena ob tem času!");
                    return View();
                }

                Globals.DeliveryContext.Deliveries.Add(delivery);


                await Globals.DeliveryContext.SaveChangesAsync();

                ViewBag.CurrentTitle = "Success";
                ViewBag.CurrentMessage = "Your delivery was added!";
                return View("Message");
            }
            return View();
        }

        [HttpGet]
        public ActionResult EditDelivery (int delivery_number)
        {
            if (Session["CurrentUser"] == null)
            {
                return Redirect("/AccountAction/login");
            }
            ViewBag.DeliveryNumber = delivery_number;
            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDelivery(DeliveryInfoModel model)
        {
            if (Session["CurrentUser"] == null)
            {
                return Redirect("/AccountAction/login");
            }
            if (ModelState.IsValid)
            {
                var delivery = Globals.DeliveryContext.Deliveries.Where(s => !s.DeliveryDone && model.number == s.DeliveryNumber).First();
                if (delivery == null)
                {
                    ModelState.AddModelError("", "Dostava ni bila najdena!");
                    return View();
                }
                

                if ((string)Session["CurrentUserRole"] != "Administrator" && ((string)Session["CurrentUserRole"] != "NormalUser" || (string)Session["CurrentUser"] != delivery.DeliveryUser))
                {
                    ModelState.AddModelError("", "Nimate pravic za popravljanje te pošiljke. " + Session["CurrentUserRole"] + "|" + Session["CurrentUser"] + "|" + delivery.DeliveryUser);
                    return View();
                }

                if (model.date < 0 || model.date > 6)
                {
                    ModelState.AddModelError("", "Napaka pri izbiri dneva!");
                    return View();
                }

                DateTime del_date = DateTime.Now.Date.AddDays(model.date);

                if (model.place < 1 || model.place > 5)
                {
                    ModelState.AddModelError("", "Dostavna točka mora biti med 1 in 5!");
                    return View();
                }
                if (model.number < 0)
                {
                    ModelState.AddModelError("", "Številka dostave mora biti pozitivna!");
                    return View();
                }
                var data = Globals.DeliveryContext.Deliveries.Where(s => s.DeliveryDate == del_date && s.DeliveryWarehouse == model.number / 1000 && model.place == s.DeliveryPlace);
                if (data != null && data.Any())
                {
                    ModelState.AddModelError("", "Ta točka je že zasedena ob tem času!");
                    return View();
                }
                delivery.DeliveryDate = del_date;
                delivery.DeliveryPlace = model.place;


                await Globals.DeliveryContext.SaveChangesAsync();

                if ((string)Session["CurrentUser"] != delivery.DeliveryUser)
                {
                    // Send an email
                    var controller = DependencyResolver.Current.GetService<AccountActionController>();
                    controller.ControllerContext = new ControllerContext(this.Request.RequestContext, controller);
                    controller.SendNotificationEmail(delivery.DeliveryNumber, delivery.DeliveryUser, true);
                }

                ViewBag.CurrentTitle = "Uspeh";
                ViewBag.CurrentMessage = "Pošiljka je bila spremenjena!";
                return View("Message");
            }
            return View();
        }

        [HttpGet]
        public string DeliveryNumberCheck (int delivery_number)
        {
            if (delivery_number < 0)
                return "0";
            var del = Globals.DeliveryContext.Deliveries.Where(s => s.DeliveryNumber == delivery_number && !s.DeliveryDone);
            if (del != null && del.Any())
            {
                return "0";
            }
            return "1";
        }

        [HttpGet]
        public string DateCheck (int day_number, int delivery_number)
        {
            if (day_number < 0 || day_number > 6)
                day_number = 0;
            DateTime dt = DateTime.Now.Date.AddDays(day_number);
            if (delivery_number < 0)
                delivery_number = 0;
            int wh = delivery_number / 1000;
 
            bool[] tocke = new bool[] { true, true, true, true, true };
            try
            {
                var data = Globals.DeliveryContext.Deliveries.Where(s => s.DeliveryDate == dt && s.DeliveryWarehouse == wh && !s.DeliveryDone);
                foreach (ApplicationDelivery d in data)
                {
                    tocke[d.DeliveryPlace - 1] = false;
                }
            }
            catch (Exception) { }
            JavaScriptSerializer seri = new JavaScriptSerializer();
            return seri.Serialize(tocke);
        }

    }
}