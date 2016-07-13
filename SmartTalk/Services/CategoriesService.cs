using SmartTalk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartTalk.Services
{
    public class CategoriesService
    {
        public CategoriesService()
        {
            this.db = new AppContext();
        }

        private AppContext db;

        /// <summary>
        /// Return a list with all active categories in the database.
        /// </summary>
        /// <returns></returns>
        public List<Category> GetAllActiveCategories()
        {
            return new List<Category>(db.Categories.Where(x => x.IsActive == true));
        }

        /// <summary>
        /// Creates new category.
        /// </summary>
        /// <param name="name"></param>
        public void CreateCategory(string name)
        {
            if (db.Categories.Any(x => x.Name == name))
            {
                throw new ArgumentException("Category already exist.");
            }
            else
            {
                db.Categories.Add(new Category
                {
                    Name = name,
                    IsActive = true
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes category by given id.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteCategory(int id)
        {
            if (!db.Categories.Any(x => x.Id == id))
            {
                throw new ArgumentException("Category does not exist.");
            }
            else
            {
                db.Categories.Remove(db.Categories.Single(x => x.Id == id));
                foreach (var question in db.Questions.Where(x => x.Id == id))
                {
                    db.Questions.Remove(question);
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Gets all questions for given category.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Question> GetAllQuestions(int id)
        {
            if (!db.Categories.Any(x => x.Id == id))
            {
                throw new ArgumentException("Category does not exist.");
            }
            else
            {
                return new List<Question>(db.Questions.Where(x => x.Category.Id == id && x.Group == null));
            }
        }

        /// <summary>
        /// Gets category from the database by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Category GetCategoryById(int id)
        {
            return db.Categories.Single(x => x.Id == id);
        }


        public void RequestCategory(string name)
        {
            if (db.Categories.Any(x => x.Name == name && x.IsActive == true))
            {
                throw new ArgumentException("Category already exist.");
            }
            if (db.Categories.Any(x => x.Name == name && x.IsActive == false))
            {
                throw new ArgumentException("Category is alredy requested by another user.");
            }
            else
            {
                db.Categories.Add(new Category
                {
                    Name = name,
                    IsActive = false
                });
                db.SaveChanges();
            }
        }

        public void ConfirmCategory(int id)
        {
            if (!db.Categories.Any(x => x.Id == id)) { throw new ArgumentException("Category does not exist."); }
            if (db.Categories.Single(x => x.Id == id).IsActive == true) { throw new ArgumentException("Category is alredy activated."); }
            else
            {
                db.Categories.Single(x => x.Id == id).IsActive = true;
                db.SaveChanges();
            }
        }

        public List<Category> GetAllRequestedCategories()
        {
            return db.Categories.Where(x => x.IsActive == false).ToList();
        }

        public void DenyCategory(int id)
        {
            if (!db.Categories.Any(x => x.Id == id))
            {
                throw new ArgumentException("Category hasn't been requested.");
            }
            else
            {
                Category category = db.Categories.Single(x => x.Id == id);
                if (category.IsActive == true)
                {
                    throw new ArgumentException("Category is alredy active.");
                }
                else
                {
                    db.Categories.Remove(category);
                }
            }
        }
    }
}