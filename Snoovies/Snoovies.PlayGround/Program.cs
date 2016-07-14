using Snoovies.DataAccess;
using Snoovies.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snoovies.PlayGround
{
    class Program
    {
        static void Main(string[] args)
        {
            //Get all categories (will also fetch all related FilmCategory objects)
            Repository<Category> categoryRepo = new Repository<Category>();
            IEnumerable<Category> categories = categoryRepo.Fetch();

            //Get all films (will also fetch all related Credit, BehindTheScenesSection, FilmCategory and ReachOutContent objects) 
            Repository<Film> filmRepo = new Repository<Film>();
            IEnumerable<Film> films = filmRepo.Fetch();

            //Merge duplicate FilmCategory objects, as they were foreign key dependent on both categories as well as films, they were fetched twice
            List<FilmCategory> filmCategories = new List<FilmCategory>();
            foreach (Film film in films)
            {
                filmCategories.AddRange(film.Categories.ToList());
            }

            foreach (FilmCategory filmCategory in filmCategories)
            {
                //Find the Parent category and set it as a property
                Category parentCategory = categories.Where(o => o.Id == filmCategory.ParentCategory.Id).Single();
                filmCategory.ParentCategory = parentCategory;

                //Find the duplicate filmCategory object that is currently held by the Parent category
                FilmCategory duplicateObject = parentCategory.FilmCategories.Where(o => o.Id == filmCategory.Id).Single();

                //Replace duplicate filmCategory object with self
                parentCategory.LinkedDomains.Remove(duplicateObject);
                parentCategory.LinkedDomains.Add(filmCategory);
            }

            //Print every film loaded
            Console.WriteLine("This program has loaded the following films from the Database:\n");
            foreach (Film film in films)
            {
                Console.WriteLine(film);
            }

            Console.ReadLine();

        }
    }
}
