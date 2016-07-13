using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmartTalk.Models
{
    public class Group
    {
        public Group() {
            this.members = new List<User>();
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual User GroupLeader { get; set; }

        public virtual List<User> Members
        {
            get { return members; }
            set { members = value; }
        }

        public virtual List<User> MemberRequests { get; set; }

        private List<User> members;

        private List<User> memberRequests;
    }
}