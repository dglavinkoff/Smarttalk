using SmartTalk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartTalk.Services
{
    public class CommentsService
    {
        public CommentsService()
        {
            this.db = new AppContext();
        }

        private AppContext db;

        public Comment GetCommentById(int id)
        {
            if (!db.Comments.Any(x => x.Id == id))
            {
                throw new ArgumentException("Comment does not exist.");
            }
            else
            {
                return db.Comments.Single(x => x.Id == id);
            }
        }

        /// <summary>
        /// Adds comment to the given question.
        /// </summary>
        /// <param name="question"></param>
        /// <param name="author"></param>
        /// <param name="commentBody"></param>
        public void AddCommentToQuestion(Question question, User author, string commentBody)
        {
            question.Comments.Add(new Comment
            {
                Author = author,
                CommentBody = commentBody,
                IsReported = false
            });
            db.SaveChanges();
        }

        /// <summary>
        /// Adds comment to given answer.
        /// </summary>
        /// <param name="answer"></param>
        /// <param name="author"></param>
        /// <param name="commentBody"></param>
        public void AddCommentToAnswer(Answer answer, User author, string commentBody)
        {
            answer.Comments.Add(new Comment
            {
                Author = author,
                CommentBody = commentBody,
                IsReported = false
            });
            db.SaveChanges();
        }

        public void ReportComment(int id)
        {
            if (!db.Comments.Any(x => x.Id == id))
            {
                throw new ArgumentException("Comment does not exist.");
            }
            else
            {
                db.Comments.Single(x => x.Id == id).IsReported = true;
                db.SaveChanges();
            }
        }

        public void DeleteComment(int id)
        {
            if (!db.Comments.Any(x => x.Id == id))
            {
                throw new ArgumentException("Comment does not exist.");
            }
            else
            {
                db.Comments.Remove(db.Comments.Single(x => x.Id == id));
                db.SaveChanges();
            }
        }

        public List<Comment> GetAllReportedComments()
        {
            return db.Comments.Where(x => x.IsReported == true).ToList();
        }

        public void ClearCommentState(int id)
        {
            if (!db.Comments.Any(x => x.Id == id))
            {
                throw new ArgumentException("Comment does not exist.");
            }
            else
            {
                Comment comment = db.Comments.Single(x => x.Id == id);
                if (comment.IsReported == false)
                {
                    throw new ArgumentException("Question is not reported.");
                }
                else
                {
                    comment.IsReported = true;
                }
            }
        }

    }
}