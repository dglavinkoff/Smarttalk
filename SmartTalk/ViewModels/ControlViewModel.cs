using SmartTalk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartTalk.ViewModels
{
    public class ControlViewModel
    {
        public List<Question> ReportedQuestions;
        public List<Answer> ReportedAnswers;
        public List<Comment> ReportedComments;
        public List<AccountListSimpleViewModel> Moderators;
        public List<CategoriesListSimpleViewModel> RequestedCategories;
    }
}