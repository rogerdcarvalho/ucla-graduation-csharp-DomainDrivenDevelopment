using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace RDCMedia.Common
{
    /// <summary>
    /// The baseline class for any domain class that has a representation in a SQL database. Is setup in such a way that data access classes can easily deduct relationships and construct SQL queries based on whatever content is stored in the domain object's table
    /// </summary>
    [Serializable]
    public abstract class DomainBase : INotifyPropertyChanged
    {
        #region Fields
 
        private Type _type;
        private string _domainName;
        private string _foreignKeyName;
        private string _primaryKeyName;
        private int _id;

        private bool _isSynced;
        private bool _isDependent;
        private bool _isMarkedForDeletion;
        private bool _isNew;
        private Dictionary<String,DatabaseHelper.DataObject> _dataObjects = new Dictionary<string, DatabaseHelper.DataObject>();
        private List<DomainBase> _linkedDomains = new List<DomainBase>();

        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;


        #endregion
        
        #region Properties
        /// <summary>
        /// The column name of the primary key
        /// </summary>
        public string PrimaryKeyName
        {
            get { return _primaryKeyName; }
            protected set { _primaryKeyName = value; }
        }

        /// <summary>
        /// The column name of the primary key when it is used as a foreign key by linked domains
        /// </summary>
        public string ForeignKeyName
        {
            get { return _foreignKeyName; }
            protected set { _foreignKeyName = value; }
        }
        
        /// <summary>
        /// The unique identifier of the object (primary key value). Can only be set via the ImportData method, where it is provided by the database
        /// </summary>
        public int Id
        {
            get { return _id; }
            private set
            {
                _id = value;
            }
        }

        /// <summary>
        /// The name of the domain (table name)
        /// </summary>
        /// 
        public string DomainName
        {
            get { return _domainName; }
            protected set { _domainName = value; }
        }

        /// <summary>
        /// The type of the domain (used to cast back to the right type in polymorphic lists)
        /// </summary>
        /// 
        public Type Type
        {
            get { return _type; }
            private set { _type = value;}
        }

        /// <summary>
        /// Property list holding the database representation information of this object
        /// </summary>
        public Dictionary<String, DatabaseHelper.DataObject> DataObjects
        {
            get { return _dataObjects; }
            protected set { _dataObjects = value; }
        }
        /// <summary>
        /// Property list holding any entities that link to this object as a foreign key
        /// </summary>
        public List<DomainBase> LinkedDomains
        {
            get { return _linkedDomains; }
            set 
            { 
                _linkedDomains = value;
                IsSynced = false;
            }
        }

        /// <summary>
        /// Property indicating whether entity's existence is dependent on another entity 
        /// (has a foreign key in the database)
        /// </summary>
        public bool IsDependent
        {
            get { return _isDependent; }
            protected set { _isDependent = value; }
        }

        /// <summary>
        /// Property indicating whether entity has changed since it
        /// was retrieved from database.
        /// </summary>
        public bool IsSynced
        {
            get { return _isSynced; }
            set {
                _isSynced = value;
                OnPropertyChanged("IsSynced");
                if (IsDependent)
                {
                    propagateToParent();
                }
            }
        }


        /// <summary>
        /// Property that can be set to cause entity to be deleted
        /// from database when persisted. Will automatically (un)mark any dependent linked entities for deletion as well
        /// </summary>
        public bool IsMarkedForDeletion
        {
            get { return _isMarkedForDeletion; }
            set 
            { 
                _isMarkedForDeletion = value;
                foreach (DomainBase linkedDomain in LinkedDomains)
                {
                    linkedDomain.IsMarkedForDeletion = value;
                }
                OnPropertyChanged("IsMarkedForDeletion");
                IsSynced = false;

            }
        }

        /// <summary>
        /// Property indicating whether entity is new and does not yet exist in the database
        /// </summary>
        public bool IsNew
        {
            get { return _isNew; }
            protected set {
                _isNew = value;
                OnPropertyChanged("IsNew");
            }
        }
        #endregion

        #region Constructors
        public DomainBase()
        {
            //Store Type and load default switches
            Type = this.GetType();
            IsNew = true;
            IsSynced = false;
            IsMarkedForDeletion = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method to notifiy UI of any property changes
        /// </summary>
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Imports data from an IDataReader object. This object is assumed to contain all the fields that are represented in the domain's dataObjects list. This method should only be called after the data reader has executed a SQL Query and contains a resultset
        /// </summary>
        public void importData(IDataReader dataReader)
        {
            foreach (DatabaseHelper.DataObject column in DataObjects.Values)
            {
                column.ImportData(dataReader);
            }

            //Set primary key (outside of scope of dataObjects list)
            DatabaseHelper.DataObject primaryKey = new DatabaseHelper.DataObject(DatabaseHelper.DataType.INT, PrimaryKeyName, typeof(int));
            primaryKey.ImportData(dataReader);
            Id = primaryKey.ToInt();
            
            //The domain is now considered synced. Update properties
            IsNew = false;
            IsSynced = true;
        }

        /// <summary>
        /// Should provide the caller with an array of all potential types of domains that may link to this domain as a foreign key. This can help Data Access classes determine where to look for relationships when importing data
        /// </summary>
        abstract public Type[] getPotentialLinkedDomains();

        /// <summary>
        /// Allows a universal polymorphic method of setting an object's parent
        /// </summary>
        abstract public void setParent(DomainBase domain);

        /// <summary>
        /// Allows a universal method of propagating state to parent
        /// </summary>
        abstract protected void propagateToParent();

  
        #endregion
        
    }
    
}
