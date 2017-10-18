using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PraksaProjekat.Models
{
    public class MonthlyHours
    {
        //Date odredjuje godinu i mesec za koju su vezani radni sati

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime Date { get; set; }

        public int Hours { get; set; }
    }
}