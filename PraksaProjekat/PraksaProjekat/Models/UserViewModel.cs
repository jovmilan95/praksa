using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PraksaProjekat.Models
{
    public class UserViewModel
    {
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        public bool AdminRole { get; set; }
        [Display(Name = "This month hours")]
        public int HoursThisMonth { get; set; }

        [Display(Name = "Previous month hours")]
        public int HoursPrevMonth { get; set; }

        public List<MonthlyHours> MonthlyHoursList { get; set; }
        public ContractViewModel LastContract { get; set; }


    }
}