using RDCMedia.Common;
using System;
using System.Linq;


namespace Snoovies.Domain
{
    /// <summary>
    /// Holds the relationships between Films and Categories
    /// </summary>
    [Serializable]
    public class FilmCategory : DomainBase
    {
        #region Fields
        [field: NonSerializedAttribute()]
        private Film _parentFilm;

        [field: NonSerializedAttribute()]
        private Category _parentCategory;

        #endregion

        #region Properties

        /// <summary>
        /// Holds the parent object (Film) that this object relates to
        /// </summary>
        /// 
        public Film ParentFilm
        {
            get { return _parentFilm; }
            set
              {
                //Check if this FilmCategory already has a parent film
                if (_parentFilm == null && _parentFilm != value)
                {
                    //Set new Parent
                    _parentFilm = value;

                    //delete any potential previous reference in the parent
                    value.LinkedDomains.Remove(this);

                    value.LinkedDomains.Add(this);
                    value.IsSynced = IsSynced;

                    //Update Data Object
                    DataObjects.Values.Where(o => o.ObjectName == "filmid").Single().Value = value.Id;

                }
                else
                {
                    //Remove old parent reference
                    IsSynced = false;
                    _parentFilm.LinkedDomains.Remove(this);

                    //Set new Parent
                    _parentFilm = value;
                    value.LinkedDomains.Add(this);
                    value.IsSynced = IsSynced;

                    //Update Data Object
                    DataObjects.Values.Where(o => o.ObjectName == "filmid").Single().Value = value.Id;
                    IsSynced = false;
                }
            }
        }

        
        /// <summary>
        /// Holds the parent object (Category) that this object relates to
        /// </summary>
        public Category ParentCategory
        {
             get 
             { 
                 return _parentCategory; 
             }
            set
            {
                //Check if this FilmCategory already has a parent
                if (_parentCategory == null && ParentCategory != value)
                {
                    //Set new Parent
                    _parentCategory = value;
                    value.LinkedDomains.Add(this);
                    value.IsSynced = IsSynced;

                    //Update Data Object
                    DataObjects.Values.Where(o => o.ObjectName == "categoryid").Single().Value = value.Id;
                    
                }
                else
                {
                    //Remove old parent reference
                    IsSynced = false;
                    _parentCategory.LinkedDomains.Remove(this);

                    //Set new Parent
                    _parentCategory = value;
                    value.LinkedDomains.Add(this);
                    value.IsSynced = IsSynced;

                    //Update Data Object
                    DataObjects.Values.Where(o => o.ObjectName == "categoryid").Single().Value = value.Id;
                    IsSynced = false;
                }
            }
        }
        /// <summary>
        /// Holds the Id of the parent category
        /// </summary>
        public int CategoryId
        {
            get
            {
                return DataObjects.Values.Where(o => o.ObjectName == "categoryid").Single().ToInt();
            }

        }

        /// <summary>
        /// Holds the Id of the parent film
        /// </summary>
        public int FilmId
        {
            get
            {
                return DataObjects.Values.Where(o => o.ObjectName == "filmid").Single().ToInt();
            }

        }

        #endregion

        #region Constructors

        /// <summary>
        /// To create a FilmCategory object, no arguments are required. Properties can freely be set, besides its unique Id. The Id can only be set by a dataReader object provided by a Data Access Layer
        /// </summary>
        /// 
        public FilmCategory()
            : base()
        {
            //Set domain (table) name
            DomainName = "film_categories";

            //Set dependency, a Film Category object can only exist when the film and category it refers to exist
            IsDependent = true;

            //Set primary key (column) name
            PrimaryKeyName = "id";

            //Create Database representation of properties
            DataObjects.Add("FilmId",new DatabaseHelper.DataObject(DatabaseHelper.DataType.INT, "filmid", typeof(int)));
            DataObjects.Add("CategoryId",new DatabaseHelper.DataObject(DatabaseHelper.DataType.INT, "categoryid", typeof(int)));

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
                this.ParentFilm = (Film)domain;
            }
            if (domain.Type == typeof(Category))
            {
                this.ParentCategory = (Category)domain;
            }
        }

        /// <summary>
        /// Method to propagate any changes to the parent object
        /// </summary>
        protected override void propagateToParent()
        {
            if (this.ParentFilm != null)
            {
                this.ParentFilm.IsSynced = IsSynced;
            }
            if (this.ParentCategory != null)
            {
                this.ParentCategory.IsSynced = IsSynced;
            }

        }

        /// <summary>
        /// Method to inform other data access layer about parent-child relationships with other domain objects, not applicable for FilmCategory objects
        /// </summary>
        public override Type[] getPotentialLinkedDomains()
        {
            return new Type[] { };
        }
        #endregion
    }
}
