using Snoovies.DataAccess;
using Snoovies.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AdminUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties
        //Global Properties
        ObservableCollection<Film> films;
        Category allCategories;
        private string defaultSearchBoxText;
        IEnumerable<Category> categories;
        Film selectedFilm;

        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();
           
            //Get default search text
            defaultSearchBoxText = SearchBox.Text;

            //Disable edit button until a film has been selected
            EditButton.IsEnabled = false;

            //Setup indicator window informing the user that the app is communicating with the database
            DatabaseLoading indicator = new DatabaseLoading();
            indicator.Show();

            //Get all categories (will also fetch all related FilmCategory objects)
            Repository<Category> categoryRepo = new Repository<Category>();
            categories = categoryRepo.Fetch();

            //Get all films (will also fetch all related Credit, BehindTheScenesSection, FilmCategory and ReachOutContent objects) 
            Repository<Film> filmRepo = new Repository<Film>();
            films = new ObservableCollection<Film>(filmRepo.Fetch().OrderBy(o => o.Title));

            //Merge duplicate FilmCategory objects, as they were foreign key dependent on both categories as well as films, they were fetched twice
            List<FilmCategory> filmCategories = new List<FilmCategory>();
            foreach (Film film in films)
            {
                filmCategories.AddRange(film.Categories.ToList());
            }

            foreach (FilmCategory filmCategory in filmCategories)
            {
                //Find the Parent category
                Category parentCategory = categories.Where(o => o.Id == filmCategory.CategoryId).Single();

                //Find the duplicate filmCategory object that is currently held by the Parent category
                FilmCategory duplicateObject = parentCategory.FilmCategories.Where(o => o.Id == filmCategory.Id).Single();

                //Replace duplicate filmCategory object with self
                parentCategory.LinkedDomains.Remove(duplicateObject);
                duplicateObject = null;

                filmCategory.ParentCategory = parentCategory;
            }
            allCategories = new Category();
            allCategories.Name = "all";

            List<Category> uiCategories = new List<Category>();
            uiCategories.Add(allCategories);
            uiCategories.AddRange(categories.ToList());
            CategorySelector.ItemsSource = uiCategories;
            CategorySelector.SelectedIndex = 0;

            //Set films as the overall data context
            this.DataContext = films;

            //Close the indicator
            indicator.Close();

        }
        #endregion

        #region Methods

        /// <summary>
        /// When a user clicks the new button, open a new window with a new film
        /// </summary>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //Open a new empty DetailWindow and link its new Film to the local list
            DetailWindow dWindow = new DetailWindow(categories);
            dWindow.Owner = this;
            dWindow.Show();

            //Add whatever film the user created to the local list and select it
            films.Add(dWindow.Film);
            FilmTable.SelectedItem = dWindow.Film;
            selectedFilm = dWindow.Film;

            //Setup notifications
            dWindow.Cancelled += FilmCancelled;
            dWindow.Saved += FilmSaved;
        }


        /// <summary>
        /// When a user selects a category, update the list of available films
        /// </summary>
        private void CategorySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategorySelector.SelectedItem != null && CategorySelector.SelectedItem != allCategories)
            {
                List<Film> filter = new List<Film>();
                filter = films.Where
                        (
                           f => f.Categories.Any(c => c.ParentCategory == CategorySelector.SelectedItem)
                        ).ToList();
                FilmTable.ItemsSource = filter;
                SearchBox.Text = defaultSearchBoxText;
            }
            else if (CategorySelector.SelectedItem != null && CategorySelector.SelectedItem == allCategories)
            {
                FilmTable.ItemsSource = films;
                SearchBox.Text = defaultSearchBoxText;
            }
        }


        /// <summary>
        /// When a user clicks the edit button, open a new window with the applicable film
        /// </summary>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            //Open a new DetailWindow with the selected film
            selectedFilm = (Film)FilmTable.SelectedItem;
            DetailWindow dWindow = new DetailWindow(categories, selectedFilm);
            dWindow.Owner = this;

            //Setup notifications
            dWindow.Reverted += FilmReverted;
            dWindow.Saved += FilmSaved;

            //Open Window
            dWindow.Show();
        }

        /// <summary>
        /// When a user cancels a new film in the detail window, ensure it doesn't linger in our local list
        /// </summary>
        private void FilmCancelled(object sender, EventArgs e)
        {
            //Replace current reference to the Detail Window film
            DetailWindow dWindow = (DetailWindow)sender;
            Film cancelledFilm = dWindow.Film;

            //Delete the film in the local list
            films.Remove(cancelledFilm);
        }

        /// <summary>
        /// When a user reverts a film to the database version, ensure our copy has the correct data
        /// </summary>
        private void FilmReverted(object sender, EventArgs e)
        {
            //Replace current reference to the Detail Window film
            DetailWindow dWindow = (DetailWindow)sender;
            Film revertedFilm = dWindow.Film;

            //Replace the film in the local vars
            int index = films.IndexOf(films.Where(f => f.Id == revertedFilm.Id).Single());
            films[index] = revertedFilm;
            selectedFilm = revertedFilm;

            //Refresh listbox and search to ensure the old reference doesn't linger
            SearchBox.Text = defaultSearchBoxText;
            CategorySelector.SelectedItem = allCategories;

            FilmTable.ItemsSource = films;
            FilmTable.ScrollIntoView(revertedFilm);
            FilmTable.SelectedItem = revertedFilm;
        }

        /// <summary>
        /// When a user saves a film, ensure we have the right copy of it in our local list
        /// </summary>
        private void FilmSaved(object sender, EventArgs e)
        {
            //Replace current reference to the Detail Window film
            DetailWindow dWindow = (DetailWindow)sender;
            Film savedFilm = dWindow.Film;

            if (savedFilm == null)
            //The user has removed the film, so we should remove it too
            {
                films.Remove(selectedFilm);

                //Reorder list
                var newFilms = new ObservableCollection<Film>(films.OrderBy(f => f.Title));
                films = null;
                films = newFilms;

                FilmTable.ItemsSource = films;
                SearchBox.Text = defaultSearchBoxText;
                CategorySelector.SelectedItem = allCategories;

            }
            else
            {

                //Replace the film in the local list
                int index = films.IndexOf(films.Where(f => f.Id == savedFilm.Id).Single());
                films[index] = savedFilm;

                //Reorder list and scroll to selected item
                var newFilms = new ObservableCollection<Film>(films.OrderBy(f => f.Title));
                films = null;
                films = newFilms;

                SearchBox.Text = defaultSearchBoxText;
                CategorySelector.SelectedItem = allCategories;

                FilmTable.ItemsSource = films;
                FilmTable.ScrollIntoView(savedFilm);
                FilmTable.SelectedItem = savedFilm;
            }
        }

        /// <summary>
        /// When a user selects a film, enable the edit button
        /// </summary>
        private void FilmTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Enable the edit button if an item was selected
            if (FilmTable.SelectedItem != null)
            {
                EditButton.IsEnabled = true;
            }
            else
            {
                EditButton.IsEnabled = false;
            }
        }


        /// <summary>
        /// When the search box gets focus, ensure users can easily type queries
        /// </summary>
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //Reset searchbox to empty whenever the default text is shown and the user wants to type
            if (SearchBox.Text == defaultSearchBoxText)
            {
                SearchBox.Text = "";
            }
        }


        /// <summary>
        /// As the user types in the search box, update matching films
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text;

            //If nothing is entered, show all available films
            if (searchText == null && films != null)
            {
                SearchBox.Text = searchText;
                FilmTable.ItemsSource = films;
            }
            //if something is entered, filter available films using LINQ
            else if (films != null && searchText != defaultSearchBoxText)
            {
                CategorySelector.SelectedIndex = -1;
                List<Film> filter = new List<Film>();
                filter = films.Where
                        (
                            o => o.Title.ToLower().Contains(SearchBox.Text.ToLower()) ||
                            o.Description.ToLower().Contains(SearchBox.Text.ToLower()) ||
                            o.LongDescription.ToLower().Contains(SearchBox.Text.ToLower()) ||
                            o.Title.ToLower() == SearchBox.Text.ToLower()
                        ).ToList();
                FilmTable.ItemsSource = filter;
            }
        }
     
        #endregion
    }
}
