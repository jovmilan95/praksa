using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace PraksaProjekat.Models
{
    public class EmailJob : IJob
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public void Execute(IJobExecutionContext context)
        {
            
            var users = db.Users.Select(x => new
            {
                LastContract = x.ContractList.OrderByDescending(s => s.Id)
                                 .FirstOrDefault(),
                Ime = x.FirstName,
                Prezime = x.LastName
            }).ToList();
            var emails = db.NotificationEmails.ToList();
            foreach (var item in users)
            {
                if (item.LastContract != null)
                {
                    TimeSpan remtime = item.LastContract.ExpiryDate - DateTime.Now;
                    if (remtime.Days == 30 || (remtime.Days >= 0 && remtime.Days <= 3))
                    {
                        foreach (var email in emails)
                        {
                            using (var message = new MailMessage("email@gmail.com", email.Email))
                            {
                                message.Subject = "Istice ugovor " + item.Ime + " " + item.Prezime;
                                message.Body = "Pozdrav,\n\n Istice ugovor korisnika " + item.Ime + " " + item.Prezime + " dana " + item.LastContract.ExpiryDate.ToString("dd.MMM.yyyy.") + "god";
                                using (SmtpClient client = new SmtpClient
                                {
                                    EnableSsl = true,
                                    Host = "smtp.gmail.com",
                                    Port = 587,
                                    Credentials = new NetworkCredential("email@gmail.com", "sifra")
                                })
                                {
                                    client.Send(message);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}