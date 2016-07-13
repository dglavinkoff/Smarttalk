using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmartTalk.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public string ActionLink { get; set; }

        public string Message { get; set; }

    }
}