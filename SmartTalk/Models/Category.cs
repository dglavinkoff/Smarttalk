using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmartTalk.Models
{
    public class Category
    {

        public Category() {
            this.questions = new List<Question>();
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        private ICollection<Question> questions;

        public virtual ICollection<Question> Questions
        {
            get { return questions; }
            set { questions = value; }
        }
    }
}