using RDCMedia.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Snoovies.Domain
{
    /// <summary>
    /// The Category object holds all data related to a given category of films
    /// </summary>
    [Serializable]
    public class Category : DomainBase
    {
        #region Fields

        #endregion

        #region Properties
        /// <summary>
        ///  The films within a given category
        /// </summary>
        /// <returns>
        /// Returns a collection of FilmCategory objects, that contains all films that are in this category
        /// </returns>
        /// 
        public IEnumerable<FilmCategory> FilmCategories
        {
            get
            {
                IEnumerable<DomainBase> linkedFilmCategories = LinkedDomains.Where(o => o.Type == typeof(FilmCategory));
                return linkedFilmCategories.Cast<FilmCategory>();
            }
        }
        /// <summary>
        /// Holds the description of the category, which is shown in listings
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
        /// Holds the name of the category
        /// </summary>
        /// 
        public string Name
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "name").Single().ToString(); }
            set
            {
                DataObjects.Values.Where(o => o.ObjectName == "name").Single().Value = value;
                IsSynced = false;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// To create a category object, no arguments are required.  Properties can freely be set, besides its unique Id. The Id can only be set by a dataReader object provided by a Data Access Layer
        /// </summary>
        /// 
        public Category()
            : base()
        {
            //Set domain (table) name
            DomainName = "categories";

            //Set dependency
            IsDependent = false;

            //Set primary and foreign key (column) names
            PrimaryKeyName = "id";
            ForeignKeyName = "categoryid";

            //Create Database representation of properties
            DataObjects.Add("Name", new DatabaseHelper.DataObject(DatabaseHelper.DataType.VARCHAR, "name", typeof(string), 25));
            DataObjects.Add("Description", new DatabaseHelper.DataObject(DatabaseHelper.DataType.VARCHAR, "description", typeof(string), 500));
       
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method to inform other data access layer about parent-child relationships with other domain objects, Category objects are parents to FilmCategory objects
        /// </summary>
        public override Type[] getPotentialLinkedDomains()
        {
            return new Type[]{
                   typeof(FilmCategory)
               };
        }

        /// <summary>
        /// Simply list the category name as ToString
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Method to set a parent object, not applicable to Category, since it has no parents
        /// </summary>
        public override void setParent(DomainBase domain)
        {
            throw new NotImplementedException("Category objects cannot have a parent");
        }

        /// <summary>
        /// Method to propagate any changes to the parent object, not applicable to category objects since they have no parent
        /// </summary>
        protected override void propagateToParent()
        {
            throw new NotImplementedException("Category objects cannot have a parent");
        }
        #endregion
    }
}
