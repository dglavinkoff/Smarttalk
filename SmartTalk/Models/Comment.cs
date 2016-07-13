using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmartTalk.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        public string CommentBody { get; set; }

        public virtual User Author { get; set; }

        public bool IsReported { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }
    }
}