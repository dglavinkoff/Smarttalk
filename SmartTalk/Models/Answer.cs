using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmartTalk.Models
{
    public class Answer
    {
        public Answer() {
            this.comments = new List<Comment>();
        }

        [Key]
        public int Id { get; set; }

        public virtual Question Question { get; set; }

        public virtual User Author { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        public string AnswerBody { get; set; }

        public bool IsReported { get; set; }

        public virtual List<Comment> Comments
        {
            get
            {
                return this.comments;
            }
            set
            {
                this.comments = value;
            }
        }

        private List<Comment> comments;
    }
}