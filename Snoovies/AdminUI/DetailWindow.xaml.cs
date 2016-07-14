using AdminUI.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using RDCMedia.Common;
using Snoovies.Domain;
using Snoovies.DataAccess;

namespace AdminUI
{
    /// <summary>
    /// Interaction logic for DetailWindow.xaml
    /// </summary>
    public partial class DetailWindow : Window, INotifyPropertyChanged
    {
        #region Properties

        public Film Film;
        public IEnumerable<Category> Categories;
        private bool _canRevert;
        private Film archivedFilm;
        private List<BehindTheScenesUI> addedBts;
        private List<CreditUI> addedCredits;

        public bool CanRevert
        {
            get { return _canRevert; }
            set
            {
                _canRevert = value;
                OnPropertyChanged("CanRevert");
            }
        }
        #endregion

        #region Constructors
     
        /// <summary>
        /// New Constructor: When the window has opened without an existing film 
        /// </summary>
        public DetailWindow(IEnumerable<Category> categories)
        {
            //Setup Window and global properties
            InitializeComponent();
            this.Title = "New Film";
            this.Film = new Film();
            this.Categories = categories;
            drawCategoriesUI(categories);
            CanRevert = false;

            //Setup local lists
            addedBts = new List<BehindTheScenesUI>();
            addedCredits = new List<CreditUI>();

            //Create a baseline behind the scenes object to link to the existing UI elements
            BehindTheScenesSection btsSection = new BehindTheScenesSection();
            btsSection.Position = 0;
            btsSection.Parent = Film;

            IntroTextBox.DataContext = btsSection;
            var btsBinding = new Binding("IntroText");
            IntroTextBox.SetBinding(TextBox.TextProperty, btsBinding);

            //Create a baseline credit object to get the user started
            Credit credit = new Credit();
            credit.Position = 0;
            credit.Parent = Film;

            CreditUI creditUI = new CreditUI(credit, CreditsGrid);
            creditUI.Draw();

            //Create a baseline reach out object to get the user started
            ReachOutContent reachOut = new ReachOutContent();
            reachOut.Parent = Film;

            ReachOutTextBox.DataContext = reachOut;
            var roBinding = new Binding("Content");
            ReachOutTextBox.SetBinding(TextBox.TextProperty, roBinding);

            //Run additional code
            sharedConstructor();
        }

        /// <summary>
        /// Film Constructor: When the window is opened with an existing film
        /// </summary>
        public DetailWindow(IEnumerable<Category> categories, Film film)
        {
            //Setup Window and global properties
            InitializeComponent();
            this.Film = film;
            this.Title = film.Title;
            this.Categories = categories;
            CanRevert = true;
            drawCategoriesUI(categories);

            //Setup local lists
            addedBts = new List<BehindTheScenesUI>();
            addedCredits = new List<CreditUI>();

            //Make a backup of the Film object
            archivedFilm = StaticHelpers.DeepClone<Film>(Film);

            //Extract all contents of the Film object to relevent UI elements
            drawUI(film);

            //Run additional code
            sharedConstructor();
        }
        
        /// <summary>
        /// Shared logic that should run after any of the constructors has setup the state
        /// </summary>
        public void sharedConstructor()
        {
            //Set the datacontexts
            this.DataContext = Film;
            RevertButton.DataContext = this;

            //Bind the buttons to the state
            var revertBinding = new Binding("CanRevert");
            RevertButton.SetBinding(Button.IsEnabledProperty, revertBinding);

            var saveBinding = new Binding("IsSynced");
            saveBinding.Converter = new InverseBooleanConverter();
            SaveButton.SetBinding(Button.IsEnabledProperty, saveBinding);

            //Bind the entry form maxlengths to the allowed length
            TitleTextBox.MaxLength = Film.DataObjects["Title"].MaxLength;
            ShortDescTextBox.MaxLength = Film.DataObjects["Description"].MaxLength;
            LongDescTextBox.MaxLength = Film.DataObjects["LongDescription"].MaxLength;
            TimeTextBox.MaxLength = Film.DataObjects["Time"].MaxLength;
            VideoLinkTextBox.MaxLength = Film.DataObjects["VideoLink"].MaxLength;
            
            //Setup eventhandler for when the user closes the window
            this.Closing += new CancelEventHandler(handleClosing);   
        }
        #endregion

        #region Methods
        /// <summary>
        /// Whenever the user clicks on "Add Credit" in Credits, a new Credit is created and added to the global Film object
        /// </summary>
        private void AddCreditButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new Behind The Scenes Object
            Credit credit = new Credit();

            //Set the position
            int lastPosition = 0;

            if (Film.Credits != null)
            {
                //Get the last position that is not marked for deletion
                for (int i = Film.Credits.Last().Position; i > 0; i--)
                {
                    Credit existingCredit = Film.Credits[i];
                    if (!existingCredit.IsMarkedForDeletion)
                    {
                        lastPosition = existingCredit.Position;
                        break;
                    }
                }
            }

            //set the position of the new section
            credit.Position = lastPosition + 1;

            //Add it to the film
            credit.Parent = Film;

            //Draw it and add it to the list
            CreditUI creditUI = new CreditUI(credit, CreditsGrid);
            addedCredits.Add(creditUI);
            creditUI.Draw();
        }


        /// <summary>
        /// Whenever the user clicks on "Add Section" in Behind-The-Scenes, a new BehindTheScenesSection is created and added to the global Film object
        /// </summary>
        private void AddSectionButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new Behind The Scenes Object
            BehindTheScenesSection btsSection = new BehindTheScenesSection();

            //Set the position
            int lastPosition = 0;

            //Get the last position that is not marked for deletion
            for (int i = Film.BehindTheScenes.Last().Position; i > 0; i--)
            {
                BehindTheScenesSection section = Film.BehindTheScenes[i];
                if (!section.IsMarkedForDeletion)
                {
                    lastPosition = section.Position;
                    break;
                }
            }

            //set the position of the new section
            btsSection.Position = lastPosition + 1;

            //Add it to the film
            btsSection.Parent = Film;
            Film.IsSynced = false;

            //Draw it and add the UI to the list 
            BehindTheScenesUI btsUI = new BehindTheScenesUI(btsSection, BehindTheScenesGrid);
            addedBts.Add(btsUI);
            btsUI.Draw();

        }

        /// <summary>
        /// Whenever the user clicks on "Cancel", the film is reverted where applicable and the window is closed
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanRevert && !Film.IsSynced)
            {
                revertFilm();
            }
            else
            {
                OnCancelled(EventArgs.Empty);
            }
            this.Close();
        }

        /// <summary>
        /// Deletes an existing film from the database and/or memory
        /// </summary>
        private void DeleteFilmButton_Click(object sender, RoutedEventArgs e)
        {

            String title = AdminUI.Properties.Resources.ResourceManager.GetString("DeleteFilmConfirmationTitle");
            String message = AdminUI.Properties.Resources.ResourceManager.GetString("DeleteFilmConfirmationText");
            MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                //Show loading window
                DatabaseLoading indicator = new DatabaseLoading();
                indicator.Owner = this;
                indicator.Show();

                Film.IsMarkedForDeletion = true;
                Repository<Film> repo = new Repository<Film>();
                Film = repo.Persist(Film);
                indicator.Close();

                this.Close();
                OnSaved(EventArgs.Empty);

            }

        }

        /// <summary>
        /// Adds all available categories as checkboxes for the user to select
        /// </summary>
        private void drawCategoriesUI(IEnumerable<Category> categories)
        {
            foreach (Category category in categories)
            {
                //Create the checkbox
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(1.0, GridUnitType.Auto);
                FilmGrid.RowDefinitions.Add(row);
                CheckBox categoryCheckBox = new CheckBox();
                categoryCheckBox.Content = category.Name;
                categoryCheckBox.SetValue(Grid.ColumnProperty, 2);
                categoryCheckBox.SetValue(Grid.RowProperty, FilmGrid.RowDefinitions.Count);
                categoryCheckBox.Tag = category.Id;
                FilmGrid.Children.Add(categoryCheckBox);

                //Check to see if it should be checked
                if (Film.Categories.Where(o => o.ParentCategory.Id == category.Id).Count() > 0)
                {
                    categoryCheckBox.IsChecked = true;
                }

                //Set eventhandlers
                categoryCheckBox.Checked += new RoutedEventHandler(setCategory);
                categoryCheckBox.Unchecked += new RoutedEventHandler(setCategory);
            }

            //Add extra row to prevent the last category overlaps the previous one
            FilmGrid.RowDefinitions.Add(new RowDefinition());
        }

        /// <summary>
        /// Creates required UI elements for all metadata of a given film (excluding the categories)
        /// </summary>
        private void drawUI(Film film)
        {
            //Draw all behind the scenes
            foreach (var section in film.BehindTheScenes)
            {
                if (section.Position == 0)
                {
                    //Set intro text
                    IntroTextBox.DataContext = section;
                    var binding = new Binding("IntroText");
                    IntroTextBox.SetBinding(TextBox.TextProperty, binding);
                }
                else
                {
                    //Create UI for the section
                    BehindTheScenesUI sectionUI = new BehindTheScenesUI(section, BehindTheScenesGrid);
                    addedBts.Add(sectionUI);
                    sectionUI.Draw();
                }
            }

            //Draw all credits
            foreach (var credit in film.Credits)
            {
                CreditUI creditUI = new CreditUI(credit, CreditsGrid);
                addedCredits.Add(creditUI);
                creditUI.Draw();
            }

            //Bind reach out content
            ReachOutTextBox.DataContext = film.ReachOutContent;
            var roBinding = new Binding("Content");
            ReachOutTextBox.SetBinding(TextBox.TextProperty, roBinding);
        }

        /// <summary>
        /// Ensures that all entered fields will be accepted by the DB
        /// </summary>
        private bool entriesAreValid()
        {
            //Check if all metadata is valid for inclusion in Snoovies app
            try
            {
                if (Film.Title.Length < 1)
                {
                    String message = AdminUI.Properties.Resources.ResourceManager.GetString("IncorrectTitleError");
                    String title = AdminUI.Properties.Resources.ResourceManager.GetString("ErrorMessageBoxTitle");
                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

                    return false;
                }

                if (Film.LongDescription.Length < 5 || Film.Description.Length < 5)
                {
                    String message = AdminUI.Properties.Resources.ResourceManager.GetString("IncorrectDescriptionError");
                    String title = AdminUI.Properties.Resources.ResourceManager.GetString("ErrorMessageBoxTitle");
                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (StaticHelpers.RemoteFileExists(Film.VideoLink) == false)
                {
                    String message = AdminUI.Properties.Resources.ResourceManager.GetString("IncorrectVideoLinkError");
                    String title = AdminUI.Properties.Resources.ResourceManager.GetString("ErrorMessageBoxTitle");
                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                TimeSpan filmTime;
                if (!TimeSpan.TryParse(TimeTextBox.Text, out filmTime))
                {
                    String message = AdminUI.Properties.Resources.ResourceManager.GetString("IncorrectFilmTimeError");
                    String title = AdminUI.Properties.Resources.ResourceManager.GetString("ErrorMessageBoxTitle");
                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (Film.Categories.Where(c => c.IsMarkedForDeletion == false).Count() < 1)
                {
                    String message = AdminUI.Properties.Resources.ResourceManager.GetString("NoCategoriesError");
                    String title = AdminUI.Properties.Resources.ResourceManager.GetString("ErrorMessageBoxTitle");
                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                foreach (var btsSection in Film.BehindTheScenes)
                {
                    if (btsSection.IsMarkedForDeletion == true)
                    {
                        break;
                    }

                    if (btsSection.IntroText != null && btsSection.IntroText.Length < 10)
                    {
                        if (btsSection.Headline == null || btsSection.Headline.Length < 3 || btsSection.Body == null || btsSection.Body.Length < 10)
                        {
                            String message = AdminUI.Properties.Resources.ResourceManager.GetString("BTSError");
                            String title = AdminUI.Properties.Resources.ResourceManager.GetString("ErrorMessageBoxTitle");
                            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                    }
                    if (btsSection.IntroText == null && (btsSection.Headline == null || btsSection.Body == null))
                    {
                        String message = AdminUI.Properties.Resources.ResourceManager.GetString("BTSError");
                        String title = AdminUI.Properties.Resources.ResourceManager.GetString("ErrorMessageBoxTitle");
                        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    if (btsSection.IntroText == null && (btsSection.Headline != null && btsSection.Headline.Length < 3 || btsSection.Body != null && btsSection.Body.Length < 10))
                    {
                        String message = AdminUI.Properties.Resources.ResourceManager.GetString("BTSError");
                        String title = AdminUI.Properties.Resources.ResourceManager.GetString("ErrorMessageBoxTitle");
                        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    if (btsSection.Headline == null && btsSection.IntroText == null || btsSection.Body == null && btsSection.IntroText == null)
                    {
                        String message = AdminUI.Properties.Resources.ResourceManager.GetString("BTSError");
                        String title = AdminUI.Properties.Resources.ResourceManager.GetString("ErrorMessageBoxTitle");
                        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

                if (Film.ReachOutContent.Single().Content.Length < 5)
                {
                    String message = AdminUI.Properties.Resources.ResourceManager.GetString("ReachOutError");
                    String title = AdminUI.Properties.Resources.ResourceManager.GetString("ErrorMessageBoxTitle");
                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                foreach (var credit in Film.Credits)
                {
                    if (credit.Role.Length < 2 || credit.Persons.Length < 3)
                    {
                        String message = AdminUI.Properties.Resources.ResourceManager.GetString("CreditsError");
                        String title = AdminUI.Properties.Resources.ResourceManager.GetString("ErrorMessageBoxTitle");
                        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

            }
            catch (Exception e)
            {
                String message = AdminUI.Properties.Resources.ResourceManager.GetString("GeneralError");
                String title = AdminUI.Properties.Resources.ResourceManager.GetString("ErrorMessageBoxTitle");
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Whenever the user clicks on the window's close button or presses alt f4, ensure that the film is either reverted or cancelled where applicable
        /// </summary>
        void handleClosing(object sender, CancelEventArgs e)
        {
            if (Film != null)
            {

                if (!Film.IsSynced && CanRevert)
                {
                    revertFilm();
                }
                else if (!Film.IsSynced)
                {
                    OnCancelled(EventArgs.Empty);
                }
            }

            //Return focus to main window
            if (null != Owner)
            {
                Owner.Activate();
            }

        }

        /// <summary>
        /// Renumbers all records not marked for deletetion so that there is no missing position
        /// </summary>
        private void renumberCreditsAndBTS()
        {
            int position = 0;
            bool anElementWasDeleted = false;
            foreach (var credit in Film.Credits)
            {
                //If any of the previous credits was marked for deletion, renumber position
                if (credit.IsMarkedForDeletion == false)
                {
                    if (anElementWasDeleted == true)
                    {
                        credit.Position = position;
                    }

                    position++;
                }
                else
                {
                    anElementWasDeleted = true;
                }

            }

            position = 0;
            anElementWasDeleted = false;

            foreach (var BehindTheScenes in Film.BehindTheScenes)
            {
                if (BehindTheScenes.IsMarkedForDeletion == false)
                {
                    if (anElementWasDeleted == true)
                    {
                        BehindTheScenes.Position = position;
                    }
                    position++;
                }
                else
                {
                    anElementWasDeleted = true;
                }
            }
        }

        /// <summary>
        /// Whenever the user clicks on "Revert", the film is reverted to the archived backup 
        /// </summary>
        private void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            revertFilm();
        }

        /// <summary>
        /// Reverts the film to the archived copy
        /// </summary>
        private void revertFilm()
        {
            //Link the backup copy to the parent references, as they are not serializable
            foreach (FilmCategory category in archivedFilm.Categories.ToList())
            {
                category.ParentCategory = this.Film.Categories.Where(c => c.CategoryId == category.CategoryId).Single().ParentCategory;
                category.ParentFilm = archivedFilm;
            }

            //Restore film from archive
            Film = archivedFilm;

            //Create a new copy as archive
            archivedFilm = StaticHelpers.DeepClone<Film>(Film);

            //Update data context
            this.DataContext = Film;

            //Remove all credits and behind the scenes
            foreach (var btsUI in addedBts)
            {
                if (btsUI != null)
                {
                    btsUI.Clear(this, null);
                }
            }

            foreach (var creditUI in addedCredits)
            {
                if (creditUI != null)
                {
                    creditUI.Clear(this, null);
                }
            }

            //Recreate credits and behind the scenes to the restored archive objects
            drawUI(Film);

            //reset all category checkboxes
            foreach (var uiItem in FilmGrid.Children)
            {
                Type uiType = uiItem.GetType();
                if (uiType == typeof(CheckBox))
                {
                    CheckBox categoryCheckBox = (CheckBox)uiItem;
                    int categoryID = Int32.Parse(categoryCheckBox.Tag.ToString());

                    if (Film.Categories.Where(o => o.ParentCategory.Id == categoryID).Count() == 0)
                    {
                        categoryCheckBox.IsChecked = false;
                    }
                    else
                    {
                        categoryCheckBox.IsChecked = true;
                    }
                }
            }

            //Inform main window
            OnReverted(EventArgs.Empty);
        }

        /// <summary>
        /// Whenever the user clicks on "Save", the film is persisted to the database. TODO: Add validation and error handling
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

            //Ensure all credits and behind the scenes have proper positions in case records were deleted
            renumberCreditsAndBTS();
            
            //Check if the data is valid to be saved
            if (entriesAreValid())
            {
                //Show loading window
                DatabaseLoading indicator = new DatabaseLoading();
                indicator.Owner = this;
                indicator.Show();

                //Images have been taken out of scope for this release. For now, just revert to default referencing to image data in app
                if (Film.ImageLink == null)
                {
                    //Enter a temporary image link, the real image link will be based on the generated id
                    Film.ImageLink = "http://www.snoovies.com";

                    Repository<Film> repo = new Repository<Film>();
                    Film = repo.Persist(Film);

                    //Get the new film Id
                    int id = Film.Id;

                    //Set the real film image link
                    Film.ImageLink = "http://www.snoovies.com/images/films/" + id + "/poster.jpg";
                    Film = repo.Persist(Film);
                }
                else
                {
                    //simply save the film
                    Repository<Film> repo = new Repository<Film>();
                    Film = repo.Persist(Film);
                }

                //Inform main window
                OnSaved(EventArgs.Empty);
                indicator.Close();
                this.Close();
            }
           
        }
        
        /// <summary>
        /// Adds or removes (un)selected categories from the global Film object
        /// </summary>
        private void setCategory(object sender, RoutedEventArgs e)
        {
            //Find out which category was changed
            CheckBox categoryCheckBox = (CheckBox)sender;
            int categoryID = Int32.Parse(categoryCheckBox.Tag.ToString());


            if (categoryCheckBox.IsChecked == true)
            {
                //If it does not yet exist, add the category to the Film object
                if (Film.Categories.Where(o => o.ParentCategory.Id == categoryID).Count() == 0)
                {
                    //Add FilmCategory to film
                    FilmCategory attachedCategory = new FilmCategory();
                    attachedCategory.ParentFilm = Film;

                    //Add Film to Category
                    Category parentCategory = Categories.Where(o => o.Id == categoryID).Single();
                    attachedCategory.ParentCategory = parentCategory;
                }
                //If it already exists, ensure that it wasn't previously marked for deletion
                else
                {
                    Film.Categories.Where(o => o.ParentCategory.Id == categoryID).Single().IsMarkedForDeletion = false;
                }
            }
            else
            {
                //If the category was already existing in the Film object, mark it for deletion
                if (Film.Categories.Where(o => o.ParentCategory.Id == categoryID).Count() > 0)
                {
                    Film.Categories.Where(o => o.ParentCategory.Id == categoryID).Single().IsMarkedForDeletion = true;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///Event to inform main window when the user cancels editing
        /// </summary>
        public event EventHandler Cancelled;

        protected virtual void OnCancelled(EventArgs e)
        {
            if (Cancelled != null)
                Cancelled(this, e);
        }

        /// <summary>
        ///Event to inform the current window when properties change
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        ///Event to inform main window when the user reverts the film
        /// </summary>
        public event EventHandler Reverted;

        protected virtual void OnReverted(EventArgs e)
        {
            if (Reverted != null)
                Reverted(this, e);
        }


        /// <summary>
        ///Event to inform main window when the user saves the film
        /// </summary>
        public event EventHandler Saved;

        protected virtual void OnSaved(EventArgs e)
        {
            if (Saved != null)
                Saved(this, e);
        }
        #endregion
       
    }

    /// <summary>
    /// Helper method: Inverts boolean values for XAML binding
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    /// <summary>
    /// Handles BehindTheScenes window components
    /// </summary>
    public class BehindTheScenesUI
    {
        private BehindTheScenesSection section;
        private TextBox heading;
        private TextBox body;
        private TextBox videoLink;

        private Label headingLabel;
        private Label bodyLabel;
        private Label videoLabel;
        private Button deleteButton;

        private Grid grid;
        private int baseRow;

        public BehindTheScenesUI(BehindTheScenesSection section, Grid grid)
        {
            //Set fields
            this.grid = grid;
            this.section = section;
            baseRow = grid.RowDefinitions.Count;
        }

        /// <summary>
        /// Deletes this UI
        /// </summary>
        public void Clear(object sender, RoutedEventArgs e)
        {
            //Remove UI elements
            grid.Children.Remove(headingLabel);
            grid.Children.Remove(heading);
            grid.Children.Remove(bodyLabel);
            grid.Children.Remove(body);
            grid.Children.Remove(deleteButton);
            grid.Children.Remove(videoLabel);
            grid.Children.Remove(videoLink);

            //Remove BehindTheScenesObject
            section.IsMarkedForDeletion = true;
        }

        /// <summary>
        /// Draws this UI
        /// </summary>
        public void Draw()
        {

            //Create headline label
            grid.RowDefinitions.Add(new RowDefinition());
            headingLabel = new Label();
            headingLabel.SetValue(Grid.RowProperty, baseRow + 1);
            headingLabel.Content = Resources.ResourceManager.GetString("BehindTheScenesHeadlineLabel");
            headingLabel.FontWeight = FontWeights.Bold;
            grid.Children.Add(headingLabel);

            //Create button to delete this UI
            deleteButton = new Button();
            deleteButton.SetValue(Grid.RowProperty, baseRow + 1);
            deleteButton.Content = Resources.ResourceManager.GetString("DeleteButton");
            deleteButton.Click += new RoutedEventHandler(this.Clear);
            deleteButton.HorizontalAlignment = HorizontalAlignment.Right;
            grid.Children.Add(deleteButton);


            //Create Headline Text Box
            grid.RowDefinitions.Add(new RowDefinition());
            heading = new TextBox();
            heading.SetValue(Grid.RowProperty, baseRow + 2);

            //Bind to the headline property
            heading.DataContext = section;
            var binding = new Binding("Headline");
            heading.SetBinding(TextBox.TextProperty, binding);
            grid.Children.Add(heading);

            //Create body label
            grid.RowDefinitions.Add(new RowDefinition());
            bodyLabel = new Label();
            bodyLabel.SetValue(Grid.RowProperty, baseRow + 3);
            bodyLabel.Content = Resources.ResourceManager.GetString("BehindTheScenesSectionLabel");
            bodyLabel.FontWeight = FontWeights.Bold;
            grid.Children.Add(bodyLabel);

            //Create body Text Box
            grid.RowDefinitions.Add(new RowDefinition());
            body = new TextBox();
            body.SetValue(Grid.RowProperty, baseRow + 4);
            body.Height = 100;

            //Bind to the body property
            body.DataContext = section;
            binding = new Binding("Body");
            body.SetBinding(TextBox.TextProperty, binding);
            grid.Children.Add(body);

            //Create video label
            grid.RowDefinitions.Add(new RowDefinition());
            videoLabel = new Label();
            videoLabel.SetValue(Grid.RowProperty, baseRow + 5);
            videoLabel.Content = Resources.ResourceManager.GetString("VideoLinkLabel");
            videoLabel.FontWeight = FontWeights.Bold;
            grid.Children.Add(videoLabel);

            //Create video Text Box
            grid.RowDefinitions.Add(new RowDefinition());
            videoLink = new TextBox();
            videoLink.SetValue(Grid.RowProperty, baseRow + 6);

            //Bind to the video property
            videoLink.DataContext = section;
            binding = new Binding("VideoLink");
            videoLink.SetBinding(TextBox.TextProperty, binding);
            grid.Children.Add(videoLink);

            //Create video Text Box
            grid.RowDefinitions.Add(new RowDefinition());
        }

    
    }

    /// <summary>
    /// Handles Credit window components
    /// </summary>
    public class CreditUI
    {
        private Credit credit;
        private TextBox role;
        private TextBox persons;

        private Label roleLabel;
        private Label personsLabel;
        private Button deleteButton;
        private CheckBox primary;

        private Grid grid;
        private int baseRow;

        public Credit Credit
        {
            get { return credit; }
            set { credit = value; }
        }


        public CreditUI(Credit credit, Grid grid)
        {
            //Set fields
            this.grid = grid;
            this.credit = credit;
            baseRow = grid.RowDefinitions.Count;
        }
        /// <summary>
        /// Delets this UI
        /// </summary>
        public void Clear(object sender, RoutedEventArgs e)
        {
            //Remove UI elements
            grid.Children.Remove(roleLabel);
            grid.Children.Remove(personsLabel);
            grid.Children.Remove(deleteButton);
            grid.Children.Remove(primary);
            grid.Children.Remove(role);
            grid.Children.Remove(persons);

            //Remove BehindTheScenesObject
            credit.IsMarkedForDeletion = true;
        }
        /// <summary>
        /// Draws this UI
        /// </summary>
        public void Draw()
        {

            //Create headline label
            grid.RowDefinitions.Add(new RowDefinition());
            roleLabel = new Label();
            roleLabel.SetValue(Grid.RowProperty, baseRow + 1);
            roleLabel.SetValue(Grid.ColumnProperty, 0);
            roleLabel.Content = Resources.ResourceManager.GetString("CreditRoleLabel");
            roleLabel.FontWeight = FontWeights.Bold;
            grid.Children.Add(roleLabel);

            //Create Role Text Box
            grid.RowDefinitions.Add(new RowDefinition());
            role = new TextBox();
            role.SetValue(Grid.RowProperty, baseRow + 2);
            role.SetValue(Grid.ColumnProperty, 0);

            //Bind to the Role property
            role.DataContext = credit;
            var binding = new Binding("Role");
            role.SetBinding(TextBox.TextProperty, binding);
            grid.Children.Add(role);

            //Create Persons label
            grid.RowDefinitions.Add(new RowDefinition());
            personsLabel = new Label();
            personsLabel.SetValue(Grid.RowProperty, baseRow + 1);
            personsLabel.SetValue(Grid.ColumnProperty, 1);
            personsLabel.Content = Resources.ResourceManager.GetString("CreditPersonsLabel");
            personsLabel.FontWeight = FontWeights.Bold;
            grid.Children.Add(personsLabel);

            //Create body Text Box
            grid.RowDefinitions.Add(new RowDefinition());
            persons = new TextBox();
            persons.SetValue(Grid.RowProperty, baseRow + 2);
            persons.SetValue(Grid.ColumnProperty, 1);

            //Bind to the body property
            persons.DataContext = credit;
            binding = new Binding("Persons");
            persons.SetBinding(TextBox.TextProperty, binding);
            grid.Children.Add(persons);

            //Create the checkbox for primary credit
            grid.RowDefinitions.Add(new RowDefinition());
            primary = new CheckBox();
            primary.Content = Resources.ResourceManager.GetString("CreditPrimaryCheckbox");
            primary.SetValue(Grid.RowProperty, baseRow + 3);
            primary.SetValue(Grid.ColumnProperty, 0);

            //Bind to the primary credit property
            primary.DataContext = credit;
            binding = new Binding("Primary");
            primary.SetBinding(CheckBox.IsCheckedProperty, binding);
            grid.Children.Add(primary);


            //Create button
            deleteButton = new Button();
            deleteButton.SetValue(Grid.RowProperty, baseRow + 3);
            deleteButton.SetValue(Grid.ColumnProperty, 1);
            deleteButton.Content = Resources.ResourceManager.GetString("DeleteButton");
            deleteButton.Click += new RoutedEventHandler(this.Clear);
            deleteButton.HorizontalAlignment = HorizontalAlignment.Right;
            grid.Children.Add(deleteButton);

            grid.RowDefinitions.Add(new RowDefinition());
        }
    }
  }
