using RDCMedia.Common;
using System;
using System.Linq;

namespace Snoovies.Domain
{
    /// <summary>
    /// Holds the HTML content available on the "Reach Out" page of a given film
    /// </summary>
    [Serializable]
    public class ReachOutContent : DomainBase
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

                    //Update Data Object
                    DataObjects.Values.Where(o => o.ObjectName == "filmid").Single().Value = value.Id;
                    value.IsSynced = IsSynced;
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
                    IsSynced = false;
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
        /// Holds the HTML content
        /// </summary>
        public string Content
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "description").Single().ToString(); }
            set
            {
                DataObjects.Values.Where(o => o.ObjectName == "description").Single().Value = value;
                IsSynced = false;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// To create a ReachOutCOntent object, no arguments are required. Properties can freely be set, besides its unique Id. The Id can only be set by a dataReader object provided by a Data Access Layer
        /// </summary>
        /// 
        public ReachOutContent()
            : base()
        {
            //Set domain (table) name
            DomainName = "reach_out";

            //Set dependency, a reach out content object can only exist when the film it refers to exists
            IsDependent = true;

            //Set primary key (column) name
            PrimaryKeyName = "id";

            //Create Database representation of properties
            DataObjects.Add("FilmId",new DatabaseHelper.DataObject(DatabaseHelper.DataType.INT, "filmid", typeof(int)));
            DataObjects.Add("Description",new DatabaseHelper.DataObject(DatabaseHelper.DataType.VARCHAR, "description", typeof(string), 500));
           
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
        /// Method to inform other data access layer about parent-child relationships with other domain objects, not applicable for Reach Out Content
        /// </summary>
        public override Type[] getPotentialLinkedDomains()
        {
            return new Type[] { };
        }

     
        #endregion
    }
}
