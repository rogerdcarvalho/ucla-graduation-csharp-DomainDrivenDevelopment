using RDCMedia.Common;
using System;
using System.Linq;

namespace Snoovies.Domain
{
    /// <summary>
    /// Holds information around a given credit of a film
    /// </summary>
    [Serializable]
    public class Credit : DomainBase
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
        /// Holds the role that the person credited performed
        /// </summary>
        public string Role
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "role").Single().ToString(); }
            set
            {
                DataObjects.Values.Where(o => o.ObjectName == "role").Single().Value = value;
                IsSynced = false;
            }
        }

        /// <summary>
        /// Holds the information of the persons that fulfilled this role
        /// </summary>
        public string Persons
        {
            get { return DataObjects.Values.Where(o => o.ObjectName == "persons").Single().ToString(); }
            set
            {
                DataObjects.Values.Where(o => o.ObjectName == "persons").Single().Value = value;
                IsSynced = false;
            }
        }

         /// <summary>
        /// Holds the position of this credit in the overall credit list
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
        /// Holds whether this credit should be listed on top of the film page or not
        /// </summary>
        /// 
        public bool Primary
        {
            get { return Convert.ToBoolean(DataObjects.Values.Where(o => o.ObjectName == "primary").Single().ToBool()); }
            set
            {
                if (value == true)
                {
                    DataObjects.Values.Where(o => o.ObjectName == "primary").Single().Value = 1;
                }
                else
                {
                    DataObjects.Values.Where(o => o.ObjectName == "primary").Single().Value = 0;
                }
                    IsSynced = false;
            }
        }
        #endregion

        #region Constructors

        /// <summary>
        /// To create a credit object, no arguments are required. Properties can freely be set, besides its unique Id. The Id can only be set by a dataReader object provided by a Data Access Layer
        /// </summary>
        /// 
        public Credit()
            : base()
        {
            //Set domain (table) name
            DomainName = "credits";

            //Set dependency, a behind the scenes section object can only exist when the film it refers to exists
            IsDependent = true;

            //Set primary key (column) name
            PrimaryKeyName = "id";

            //Create Database representation of properties
            DataObjects.Add("FilmId", new DatabaseHelper.DataObject(DatabaseHelper.DataType.INT, "filmid", typeof(int)));
            DataObjects.Add("Position",new DatabaseHelper.DataObject(DatabaseHelper.DataType.INT, "record", typeof(int)));
            DataObjects.Add("Role", new DatabaseHelper.DataObject(DatabaseHelper.DataType.VARCHAR, "role", typeof(string), 200));
            DataObjects.Add("Persons", new DatabaseHelper.DataObject(DatabaseHelper.DataType.VARCHAR, "persons", typeof(string), 500));
            DataObjects.Add("Primary",new DatabaseHelper.DataObject(DatabaseHelper.DataType.TINYINT, "primary", typeof(bool)));

            //Set the boolean property to default value
            Primary = false;
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
        /// Method to inform other data access layer about parent-child relationships with other domain objects, not applicable for Credit objects
        /// </summary>
        public override Type[] getPotentialLinkedDomains()
        {
            return new Type[] { };
        }

      
        #endregion
    }
}
