using SmartTalk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartTalk.Services
{
    public class AnswersService
    {
        public AnswersService()
        {
            this.db = new AppContext();
        }

        private AppContext db;

        public Answer GetAnswerById(int id)
        {
            if (!db.Answers.Any(x => x.Id == id))
            {
                throw new ArgumentException("Answer does not exist.");
            }
            else
            {
                return db.Answers.Single(x => x.Id == id);
            }
        }

        public void ReportAnswer(int id)
        {
            if (!db.Answers.Any(x => x.Id == id))
            {
                throw new ArgumentException("Answer does not exist.");
            }
            else
            {
                db.Answers.Single(x => x.Id == id).IsReported = true;
                db.SaveChanges();
            }
        }

        public void DeleteAnswer(int id)
        {
            if (!db.Answers.Any(x => x.Id == id))
            {
                throw new ArgumentException("Answer does not exist.");
            }
            else
            {
                db.Answers.Remove(db.Answers.Single(x => x.Id == id));
                db.SaveChanges();
            }
        }

        public List<Answer> GetAllReportedAnswers()
        {
            return db.Answers.Where(x => x.IsReported == true).ToList();
        }

        public void ClearAnswerState(int id)
        {
            if (!db.Answers.Any(x => x.Id == id))
            {
                throw new ArgumentException("Answer does not exist.");
            }
            else
            {
                Answer answer = db.Answers.Single(x => x.Id == id);
                if (answer.IsReported == false)
                {
                    throw new ArgumentException("Question is not reported.");
                }
                else
                {
                    answer.IsReported = true;
                }
            }
        }

    }
}