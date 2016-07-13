using SmartTalk.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace SmartTalk.ViewModels
{
    public class QuestionAskViewModel
    {
        
        public SelectList Categories { get; set; }

        [Required]
        public int Category { get; set; }

        [Required]
        [StringLength(50)]
        public string QuestionBrief { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [HiddenInput]
        public int GroupId { get; set; }

        [HiddenInput]
        public string UrlReferrer { get; set; }

    }

    public class QuestionTenMostRecentViewModel {
        public List<QuestionListViewModel> Questions { get; set; }

        public int NumberOfPages { get; set; }

        public int ActivePage { get; set; }
    }


    public class QuestionListViewModel {
        public int Id { get; set; }

        public string QuestionBrief { get; set; }

        public string AuthorName { get; set; }

        public string Category { get; set; }

    }

    public class QuestionDetailsViewModel {

        public int Id { get; set; }

        public string QuestionBrief { get; set; }

        public string Description { get; set; }

        public string AuthorName { get; set; }

        public DateTime DateAsked { get; set; }

        public bool IsReported { get; set; }

        public List<Answer> Answers { get; set; }

        public List<Comment> Comments { get; set; }

    }

    public class SearchQuestionsViewModel {

        public List<QuestionListViewModel> Questions { get; set; }

        public int NumberOfPages { get; set; }

        public int ActivePage { get; set; }
    }
}