using RDCMedia.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Snoovies.Domain
{
    /// <summary>
    /// The film object holds all data related to a given film
    /// </summary>
    [Serializable]
    public class Film : DomainBase
    {
        #region Fields

        #endregion

        #region Properties
        /// <summary>
        ///  The content for the behind-the-scenes page
        /// </summary>
        /// <returns>
        /// Returns a collection of behind-the-scenes sections that make up this film's behind the scenes page.
        /// </returns>
        /// 
        public List<BehindTheScenesSection> BehindTheScenes
        {
            get
            {
                IEnumerable<DomainBase> linkedBehindTheScenesSections = LinkedDomains.Where(o => o.Type == typeof(BehindTheScenesSection));
                return linkedBehindTheScenesSections.Cast<BehindTheScenesSection>().OrderBy(o => o.Position).ToList();
            }
        }

        /// <summary>
        ///  The credits of people that worked on this film
        /// </summary>
        /// <returns>
        /// Returns a collection of credits 
        /// </returns>
        /// 
        public List<Credit> Credits
        {
            get
            {
                IEnumerable<DomainBase> linkedCredits = LinkedDomains.Where(o => o.Type == typeof(Credit));
                return linkedCredits.Cast<Credit>().OrderBy(o => o.Position).ToList();
            }
        }

        /// <summary>
        ///  The reach out content page of the film
        /// </summary>
        /// <returns>
        /// Returns the reach out content in a IENumerable format
        /// </returns>
        /// 
        public IEnumerable<ReachOutContent> ReachOutContent
        {
            get
            {
                IEnumerable<DomainBase> linkedReachOut = LinkedDomains.Where(o => o.Type == typeof(ReachOutContent));
                return linkedReachOut.Cast<ReachOutContent>();
            }
        }

        /// <summary>
        ///  The categories of this film
        /// </summary>
        /// <returns>
        /// Returns a collection of FilmCategory objects 
        /// </returns>
        public IEnumerable<FilmCategory> Categories
        {
            get
            {
                IEnumerable<DomainBase> linkedCategories = LinkedDomains.Where(d => d.Type == typeof(FilmCategory));
                return linkedCategories.Cast<FilmCategory>();
            }
        }
        
        /// <summary>
        /// Holds whether this film should be excluded from the app or visible
        /// </summary>
        /// 
        public bool ExcludeApp
        {
            get { return Convert.ToBoolean(DataObjects.Values.Where(o => o.ObjectName == "exclude_app").Single().ToBool()); }
            set
            {
                if (value == true)
                {
                    DataObjects.Values.Where(o => o.ObjectName == "exclude_app").Single().Value = 1;
                }
                else
                {
                    DataObjects.Values.Where(o => o.ObjectName == "exclude_app").Single().Value = 0;
                }
                    IsSynced = false;
            }
        }

        /// <summary>
        /// Holds the date and time when the film was first added to the Snoovies service
        /// </summary>
        /// 
        public DateTime TimeStamp
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "timestamp").Single().ToDateTime(); }
            set
            {
                DataObjects.Values.Where(o => o.ObjectName == "timestamp").Single().Value = value;
                IsSynced = false;
            }
        }

        /// <summary>
        /// Holds the length of the film
        /// </summary>
        /// 
        public TimeSpan Time
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "time").Single().ToTimeSpan();}
            set
            {
                DataObjects.Values.Where(o => o.ObjectName == "time").Single().Value = value;
                IsSynced = false;
            }
        }

        /// <summary>
        /// Holds the link where the poster image of the film can be downloaded
        /// </summary>
        /// 
        public string ImageLink
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "vimeo_img").Single().ToString(); }
            set
            {
                DataObjects.Values.Where(o => o.ObjectName == "vimeo_img").Single().Value = value;
                IsSynced = false;
            }
        }

        /// <summary>
        /// Holds the link of the http live stream of the film
        /// </summary>
        /// 
        public string VideoLink
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "vimeo_hls").Single().ToString(); }
            set
            {
                DataObjects.Values.Where(o => o.ObjectName == "vimeo_hls").Single().Value = value;
                IsSynced = false;
            }
        }

        /// <summary>
        /// Holds the description of the film, which is shown on the main page
        /// </summary>
        /// 
        public string LongDescription
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "long_desc").Single().ToString(); }
            set 
            { 
                DataObjects.Values.Where(o => o.ObjectName == "long_desc").Single().Value = value;
                IsSynced = false;
            }
        }

        /// <summary>
        /// Holds the short description of the film, which is shown in listings
        /// </summary>
        /// 
        public string Description
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "description").Single().ToString(); }
            set 
            { 
                DataObjects.Values.Where(o => o.ObjectName == "description").Single().Value = value;
                IsSynced = false;
            }
        }

        /// <summary>
        /// Holds the title of the film
        /// </summary>
        /// 
        public string Title
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "title").Single().ToString(); }
            set 
            { 
                DataObjects.Values.Where(o => o.ObjectName == "title").Single().Value = value;
                IsSynced = false;
            }
        }

        #endregion
        
        #region Constructors

        /// <summary>
        /// To create a film object, no arguments are required. Film properties can freely be set, besides its unique Id. The Id can only be set by a dataReader object provided by a Data Access Layer
        /// </summary>
        /// 
        public Film() : base()
        {
            //Set domain (table) name
            DomainName = "films";

            //Set dependency
            IsDependent = false;

            //Set primary and foreign key (column) names
            PrimaryKeyName = "id";
            ForeignKeyName = "filmid";

            //Create Database representation of properties
            DataObjects.Add("Title", new DatabaseHelper.DataObject(DatabaseHelper.DataType.VARCHAR, "title", typeof(string),40 ));
            DataObjects.Add("Description", new DatabaseHelper.DataObject(DatabaseHelper.DataType.VARCHAR, "description", typeof(string), 339));
            DataObjects.Add("LongDescription", new DatabaseHelper.DataObject(DatabaseHelper.DataType.VARCHAR, "long_desc", typeof(string),500));
            DataObjects.Add("VideoLink",new DatabaseHelper.DataObject(DatabaseHelper.DataType.VARCHAR, "vimeo_hls", typeof(string),200));
            DataObjects.Add("ImageLink", new DatabaseHelper.DataObject(DatabaseHelper.DataType.VARCHAR, "vimeo_img", typeof(string),100));
            DataObjects.Add("Time",new DatabaseHelper.DataObject(DatabaseHelper.DataType.TIME, "time", typeof(TimeSpan)));
            DataObjects.Add("TimeStamp",new DatabaseHelper.DataObject(DatabaseHelper.DataType.DATETIME, "timestamp", typeof(DateTime)));
            DataObjects.Add("ExcludeApp",new DatabaseHelper.DataObject(DatabaseHelper.DataType.TINYINT, "exclude_app", typeof(bool)));

            //Set ExcludeApp to true, it should only be set to false when we're sure the film is ready to be viewed in the app
            ExcludeApp = true;

            //Set default timespan to 0
            Time = TimeSpan.Zero;
        }

        #endregion
       
        #region Methods
        /// <summary>
        /// Method to inform other data access layer about parent-child relationships with other domain objects, a film is parent to all other domain objects besides Category
        /// </summary>
        public override Type[] getPotentialLinkedDomains()
        {
               return new Type[]{
                   typeof(BehindTheScenesSection),
                   typeof(Credit), 
                   typeof(ReachOutContent),
                   typeof(FilmCategory)
               };
        }
        /// <summary>
        /// Method to set a parent object, not applicable to Film, since it has no parents
        /// </summary>
        public override void setParent(DomainBase domain)
        {
            throw new NotImplementedException("Film objects cannot have a parent");
        }

        /// <summary>
        /// ToString gives a quick summary of what is contained in a Film object
        /// </summary>
        public override string ToString()
        {
            string hasReachOut= "has no reach out content";
            if (ReachOutContent.Count() > 0)
                hasReachOut = "has reach out content";

            string categories = "";

            foreach (FilmCategory filmCategory in Categories)
            {
                categories += "- " + filmCategory.ParentCategory.Name + "\n";
            }

            return Title + " (" + Credits.Count().ToString() + " Credits). This film " + hasReachOut + " and " + BehindTheScenes.Count().ToString() + " behind-the-scenes sections. This film is part of the following categories:\n" + categories;
        }
        /// <summary>
        /// Method to propagate any changes to the parent object, not applicable to film objects since they have no parent
        /// </summary>
        protected override void propagateToParent()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
