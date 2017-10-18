using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PraksaProjekat.Models
{
    public class ContractViewModel
    {

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Expiry Date")]
        public DateTime ExpiryDate { get; set; }
        // TipUgovora = "O radu" || TipUgovora = "Honorarni"
        [Required]
        public string TipUgovora { get; set; }

        public UserBasicViewModel User { get; set; }
    }
}