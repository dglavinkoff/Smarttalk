using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmartTalk.Models
{
    public class Question
    {

        public Question() {
            this.answers = new List<Answer>();
        }

        [Key]
        public int Id { get; set; }

        public virtual Category Category { get; set; }

        public virtual User Author { get; set; }

        public virtual Group Group { get; set; }

        [Required]
        [StringLength(100)]
        public string QuestionBrief { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

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

        public virtual List<Answer> Answers
        {
            get
            {
                return this.answers;
            }
            set
            {
                this.answers = value;
            }
        }

        private List<Answer> answers;

        private List<Comment> comments;
    }
}