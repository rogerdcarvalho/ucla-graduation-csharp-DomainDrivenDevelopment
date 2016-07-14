using RDCMedia.Common;
using System;
using System.Linq;

namespace Snoovies.Domain
{
    /// <summary>
    /// Holds a section within the behind-the-scenes pages of a given film
    /// </summary>
    [Serializable]
    public class BehindTheScenesSection : DomainBase
    {
        #region Fields

        private Film _parent;

        #endregion

        #region Properties
        
        /// <summary>
        /// Holds the parent object (Film) that this object relates to
        /// </summary>
        /// 
        public Film Parent
        {
            get { return _parent; }
            set 
            {
                //Check if this object already has a parent film
                if (_parent == null && _parent != value)
                {
                    //Set new Parent
                    _parent = value;
                    value.LinkedDomains.Add(this);
                    value.IsSynced = IsSynced;

                    //Update Data Object
                    DataObjects.Values.Where(o => o.ObjectName == "filmid").Single().Value = value.Id;

                }
                else
                {

                    //Remove old parent reference
                    IsSynced = false;
                    _parent.LinkedDomains.Remove(this);

                    //Set new Parent
                    _parent = value;
                    value.LinkedDomains.Add(this);
                    value.IsSynced = IsSynced;

                    //Update Data Object
                    DataObjects.Values.Where(o => o.ObjectName == "filmid").Single().Value = value.Id;

                }
            }
        }

        /// <summary>
        /// Holds the foreign key for the parent Film object
        /// </summary>
        public int FilmId
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "filmid").Single().ToInt(); }
           
        }

        /// <summary>
        /// Holds the position of this section in the overall article
        /// </summary>
        public int Position
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "record").Single().ToInt(); }
            set
            {
                DataObjects.Values.Where(o => o.ObjectName == "record").Single().Value = value;
                IsSynced = false;
            }
        }

        /// <summary>
        /// If this section has a video, this is where the link is held
        /// </summary>
        public string VideoLink
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "vimeohls").Single().ToString(); }
            set
            {
                DataObjects.Values.Where(o => o.ObjectName == "vimeohls").Single().Value = value;
                IsSynced = false;
            }
        }

        /// <summary>
        /// Holds the introduction text for the behind-the-scenes page of a given film
        /// </summary>
        public string IntroText
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "introtext").Single().ToString(); }
            set 
            { 
                DataObjects.Values.Where(o => o.ObjectName == "introtext").Single().Value = value;
                IsSynced = false;
            }
        }

        /// <summary>
        /// Holds a quote/headline of a given section
        /// </summary>
        public string Headline
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "quote").Single().ToString(); }
            set 
            { 
                DataObjects.Values.Where(o => o.ObjectName == "quote").Single().Value = value;
                IsSynced = false;
            }
        }

        /// <summary>
        /// Holds the body text of a given section
        /// </summary>
        public string Body
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "section").Single().ToString(); }
            set 
            { 
                DataObjects.Values.Where(o => o.ObjectName == "section").Single().Value = value;
                IsSynced = false;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// To create a behind the scenes section object, no arguments are required. Properties can freely be set, besides its unique Id. The Id can only be set by a dataReader object provided by a Data Access Layer
        /// </summary>
        /// 
        public BehindTheScenesSection() : base()
        {
            //Set domain (table) name
            DomainName = "behind_the_scenes";

            //Set dependency, a behind the scenes section object can only exist when the film it refers to exists
            IsDependent = true;

            //Set primary key (column) name
            PrimaryKeyName = "id";

            //Create Database representation of properties
            DataObjects.Add("Id",new DatabaseHelper.DataObject(DatabaseHelper.DataType.INT, "filmid", typeof(int)));
            DataObjects.Add("Position", new DatabaseHelper.DataObject(DatabaseHelper.DataType.INT, "record", typeof(int)));
            DataObjects.Add("IntroText", new DatabaseHelper.DataObject(DatabaseHelper.DataType.VARCHAR, "introtext", typeof(string), 500));
            DataObjects.Add("Headline",new DatabaseHelper.DataObject(DatabaseHelper.DataType.VARCHAR, "quote", typeof(string), 100));
            DataObjects.Add("Body", new DatabaseHelper.DataObject(DatabaseHelper.DataType.VARCHAR, "section", typeof(string), 500));
            DataObjects.Add("VideoLink",new DatabaseHelper.DataObject(DatabaseHelper.DataType.VARCHAR, "vimeohls", typeof(string), 100));
          
        }

        #endregion

        #region Methods

        /// <summary>
        /// Polymorphic method to set relational links with other objects
        /// </summary>
        public override void setParent(DomainBase domain)
        {
            if (domain.Type == typeof(Film))
            {
                this.Parent = (Film)domain;
            }
        }

        /// <summary>
        /// Method to propagate any changes to the parent object
        /// </summary>
        protected override void propagateToParent()
        {
            if (this.Parent != null)
            {
                this.Parent.IsSynced = IsSynced;
            }
        }

        /// <summary>
        /// Method to inform other data access layer about parent-child relationships with other domain objects, not applicable for Behind The Scenes Content
        /// </summary>
        public override Type[] getPotentialLinkedDomains()
        {
            return new Type[]{};
        }

      
        #endregion
    }
}
