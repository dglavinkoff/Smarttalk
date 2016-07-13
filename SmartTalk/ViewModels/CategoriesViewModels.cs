using SmartTalk.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmartTalk.ViewModels
{
    public class CategoriesShowViewModel
    {
        [Required]
        public List<CategoriesListViewModel> Categories { get; set; }
    }

    public class CategoriesShowQuestionsViewModel
    {
        public int Id { get; set; }

        public List<QuestionListViewModel> Questions { get; set; }

        public int NumberOfPages { get; set; }

        public int ActivePage { get; set; }
    }

    public class CategoriesListViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int NumberOfQuestions { get; set; }

    }

    public class CategoriesListSimpleViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class CategoriesCreateViewModel
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }

    public class CategoriesRequestViewModel {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}