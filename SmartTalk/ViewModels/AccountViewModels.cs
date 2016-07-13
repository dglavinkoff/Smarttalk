using SmartTalk.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmartTalk.ViewModels
{

    public class AccountRegisterViewModel 
    {
        [Required(ErrorMessage="Username is required.")]
        [StringLength(20, ErrorMessage="Username must be between 3 and 20 characters.", MinimumLength = 3)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Firstname is required.")]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Lastname is required.")]
        public string Lastname { get; set; }
        
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(20, ErrorMessage = "Password should be between 4 and 25 characters.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

    public class AccountLoginViewModel 
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class AccountDetailsViewModel {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public List<AccountListSimpleViewModel> Followers { get; set; }

        public List<AccountListSimpleViewModel> Favorites { get; set; }

        public List<AccountListSimpleViewModel> Banned { get; set; }

        public List<QuestionListViewModel> Questions { get; set; }

        public bool IsFollowedByLoggedUser { get; set; }

        public bool isBannedByLoggedUser { get; set; }

        public bool isLoggedUserBanned { get; set; }
    }

    public class SearchUsersViewModel {
        public List<AccountListViewModel> users { get; set; }
        public int NumberOfPages { get; set; }
        public int ActivePage { get; set; }
    }

    public class AccountHomeViewModel {
        public List<Notification> Notifications { get; set; }
        public List<FeedItemViewModel> News { get; set; }
    }

    public class AccountListViewModel {
        public int Id { get; set; }

        public string Username { get; set; }

        public int FollowersCount { get; set; }

        public bool IsFollowedByLoggedUser { get; set; }

        public bool isBannedByLoggedUser { get; set; }

        public bool isLoggedUserBanned { get; set; }

    }

    public class AccountListSimpleViewModel
    {
        public int Id { get; set; }

        public string Username { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(20, ErrorMessage = "Password should be between 4 and 25 characters.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [System.Web.Mvc.HiddenInput]
        public int UserId { get; set; }
    }
}