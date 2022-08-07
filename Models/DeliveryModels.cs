using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KrkaWeb.Models
{

    public class ApplicationDelivery
    {
        [Key]
        public Int32 DeliveryID { get; set; }
        public Int32 DeliveryNumber { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryUser { get; set; }
        public string DeliveryUserName { get; set; }
        public Int32 DeliveryPlace { get; set; }
        public Int32 DeliveryWarehouse { get; set; }
        public bool DeliveryDone { get; set; }
    }

    public class DeliveryInfoModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Delivery number")]
        public Int32 number { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Delivery date")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public Int32 date { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Delivery place")]
        //[Range(1, 5, ErrorMessage = "Točke dostave so lahko med 1 in 5!")]
        public Int32 place { get; set; }

    }

    public class DeliveryEditModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Delivery date")]
        public Int32 date { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Delivery place")]
        public Int32 place { get; set; }
    }
}