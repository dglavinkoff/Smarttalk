using SmartTalk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartTalk.Services
{
    public class QuestionsService
    {
        public QuestionsService()
        {
            this.db = new AppContext();
            this.accountService = new AccountsService();
        }

        private AppContext db;
        private AccountsService accountService;

        /// <summary>
        /// Gets the ten most recent questions in the datanbase.
        /// </summary>
        /// <returns></returns>
        public List<Question> GetTenMostRecentQuestions()
        {
            return new List<Question>((from q in db.Questions
                                       orderby q.Date descending
                                       where q.Group == null
                                       select q).Take(10));
        }

        public List<Question> GetQuestionsForUser(int id)
        {
            if (!db.Users.Any(x => x.Id == id))
            {
                throw new ArgumentException("User does not exist.");
            }
            else
            {
                return db.Questions.Where(x => x.Author.Id == id).ToList();
            }
        }

        /// <summary>
        /// Adds question to the database and return it's Id.
        /// </summary>
        /// <param name="questionBrief"></param>
        /// <param name="description"></param>
        /// <param name="category"></param>
        /// <param name="author"></param>
        public int AddQuestion(string questionBrief, string description, Category category, User author, Group group = null)
        {
            if (category.IsActive == false)
            {
                throw new ArgumentException("Category does not exist");
            }
            if (group == null)
            {
                var question = new Question
                {
                    QuestionBrief = questionBrief,
                    Description = description,
                    Category = category,
                    Author = author,
                    Date = (int)DateTime.Now.Ticks,
                    IsReported = false
                };
                db.Questions.Add(question);
                db.SaveChanges();
                foreach (var user in accountService.GetUserById(author.Id).MyFollowers)
                {
                    user.Notifications.Add(new Notification
                    {
                        ActionLink = "/Questions/Details/" + question.Id,
                        Message = author.Username + " asked a question."
                    });
                }
                db.SaveChanges();
                return question.Id;
            }
            else
            {
                if (!group.Members.Any(x => x.Id == (int)HttpContext.Current.Session["Id"]))
                {
                    throw new ArgumentException("You are not member of this group.");
                }
                else
                {
                    var question = new Question
                    {
                        QuestionBrief = questionBrief,
                        Description = description,
                        Category = category,
                        Author = author,
                        Date = (int)DateTime.Now.Ticks,
                        IsReported = false,
                        Group = group
                    };
                    db.Questions.Add(question);
                    db.SaveChanges();
                    foreach (var member in group.Members)
                    {
                        if (member.Id != (int)HttpContext.Current.Session["Id"])
                        {
                            member.Notifications.Add(new Notification
                            {
                                ActionLink = "Questions/Details/" + question.Id,
                                Message = author.Username + " asked a question in " + group.Name + "."
                            });
                        }
                    }
                    db.SaveChanges();
                    return question.Id;
                }
            }
        }

        /// <summary>
        /// Searches for questions in the database whose QuestionBrief contains the given substring.
        /// </summary>
        /// <param name="questionSubstring"></param>
        /// <returns></returns>
        public List<Question> SearchQuestion(string questionSubstring)
        {
            return new List<Question>(db.Questions.Where(x => x.QuestionBrief.Contains(questionSubstring) && x.Group == null));
        }

        /// <summary>
        /// Searches for questions in given the category whose QuestionBrief contains the given substring.
        /// </summary>
        /// <param name="questionSubstring"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Question> SearchQuestionInCategory(string questionSubstring, int id)
        {
            return new List<Question>(db.Questions.Where(x => x.QuestionBrief.Contains(questionSubstring) && x.Category.Id == id));
        }

        /// <summary>
        /// Gets question from the database by given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Question GetQuestionById(int id)
        {
            if (!db.Questions.Any(x => x.Id == id))
            {
                throw new ArgumentException("Question does not exist.");
            }
            else
            {
                Question question = db.Questions.Single(x => x.Id == id);
                if (question.Group != null && !(question.Group.Members.Any(x => x.Id == (int)HttpContext.Current.Session["Id"])))
                {
                    throw new ArgumentException("You cannot access private group question because you are not member of " + question.Group.Name);
                }
                else
                {
                    return question;
                }
            }
        }

        /// <summary>
        /// Adds answer to the given question and add notification to the user who asked the question.
        /// </summary>
        /// <param name="question"></param>
        /// <param name="author"></param>
        /// <param name="answerBody"></param>
        public void AnswerQuestion(Question question, User author, string answerBody)
        {
            question.Answers.Add(new Answer
            {
                Question = question,
                Author = author,
                AnswerBody = answerBody,
                Date = (int)DateTime.Now.Ticks,
                IsReported = false
            });
            question.Author.Notifications.Add(new Notification
            {
                Message = "Your question was answered by" + author.Username,
                ActionLink = "/Questions/Details/" + question.Id
            });
            db.SaveChanges();
        }

        /// <summary>
        /// Deletes question by given id and removes all notifications related to it.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteQuestion(int id)
        {
            if (!db.Questions.Any(x => x.Id == id))
            {
                throw new ArgumentException("Question does not exist.");
            }
            else
            {
                var questionToRemove = db.Questions.Single(x => x.Id == id);
                for (int i = 0; i < questionToRemove.Comments.Count; i++)
                {
                    db.Comments.Remove(questionToRemove.Comments[i]);
                }
                for (int i = 0; i < questionToRemove.Answers.Count; i++)
                {
                    for (int j = 0; j < questionToRemove.Answers[i].Comments.Count; j++)
                    {
                        db.Comments.Remove(questionToRemove.Answers[i].Comments[j]);
                    }
                    db.Answers.Remove(questionToRemove.Answers[i]);
                }
                string urlInNotification = "Questions/Details/" + id.ToString();
                foreach (var notification in db.Notifications.Where(x => x.ActionLink.Contains(urlInNotification)))
                {
                    db.Notifications.Remove(notification);
                }
                db.Questions.Remove(questionToRemove);
                db.SaveChanges();
            }
        }

        public List<Question> GetQuestionsForGroup(int id)
        {
            return db.Questions.Where(x => x.Group.Id == id).ToList();
        }

        public void ReportQuestion(int id)
        {
            if (!db.Questions.Any(x => x.Id == id))
            {
                throw new ArgumentException("Question does not exist.");
            }
            else
            {
                db.Questions.Single(x => x.Id == id).IsReported = true;
                db.SaveChanges();
            }
        }

        public List<Question> GetAllReportedQuestions()
        {
            return db.Questions.Where(x => x.IsReported == true).ToList();
        }

        /// <summary>
        /// Clears reported state of question if it exists.
        /// </summary>
        /// <param name="id"></param>
        public void ClearQuestionState(int id)
        {
            if (!db.Questions.Any(x => x.Id == id))
            {
                throw new ArgumentException("Question does not exist.");
            }
            else
            {
                Question question = db.Questions.Single(x => x.Id == id);
                if (question.IsReported == false)
                {
                    throw new ArgumentException("Question is not reported.");
                }
                else
                {
                    question.IsReported = true;
                }
            }
        }

    }
}