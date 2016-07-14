using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDCMedia.Common
{
    public static class DatabaseHelper
    {
        /// <summary>
        ///All the possible data types that a database can hold. 
        /// </summary>
        /// <remarks>
        /// This can be used to convert database objects to c# objects.
        /// </remarks>
       
        public enum DataType { INT, TINYINT, SMALLINT, MEDIUMINT, BIGINT, FLOAT, DOUBLE, DECIMAL, DATE, DATETIME, TIMESTAMP, TIME, YEAR, CHAR, VARCHAR, TEXT, TINYTEXT, MEDIUMTEXT, LONGTEXT, ENUM };

        /// <summary>
        /// Objects of this class hold data from a database, using a SQL type instead of a c# type. It allows data to be imported from a dataReader object and can convert to c# types.
        /// </summary>
        [Serializable]
        public class DataObject
        {
            #region Properties
            /// <summary>
            /// The  preferred type this object should be converted to
            /// </summary>
            public Type PreferredType { get; private set; }

            /// <summary>
            /// The name of the object in the database
            /// </summary>
            public string ObjectName { get; private set; }

            /// <summary>
            /// The datatype of the object in the database
            /// </summary>
            public DataType ObjectType { get; private set; }

            /// <summary>
            /// The value of the object. This needs to be cast to a corresponding c# datatype based on its database datatype
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// The maximum length of the value. This can be used in c# properties to constrain values
            /// </summary>
            public int MaxLength { get; set; }

            #endregion
            #region Constructors

            /// <summary>
            /// To create a DataObject, provide a SQL DataType, a name and a preferred type to cast to
            /// </summary>
            public DataObject (DataType objectType, string objectName, Type type, int maxLength = 0)
            {
                ObjectType = objectType;
                ObjectName = objectName;
                PreferredType = type;
                MaxLength = maxLength;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Imports data from a database into the object's value property. This method can only run when the name of this data object is set and a valid reader containing matching data is provided.
            /// </summary>
            public void ImportData(IDataReader dataReader)
            {
                if (String.IsNullOrEmpty(ObjectName))
                {
                    throw new InvalidOperationException("In order to import data, a data object name has to be set");
                }

                int ordinal = dataReader.GetOrdinal(ObjectName);

                if (dataReader.IsDBNull(ordinal))
                {
                    this.Value = null;
                }
                else
                {
                    if (ObjectType.Equals(DataType.INT) || ObjectType.Equals(DataType.SMALLINT) || ObjectType.Equals(DataType.TINYINT) || ObjectType.Equals(DataType.MEDIUMINT))
                    {
                        Value = dataReader.GetInt32(ordinal);
                    }
                    else if (ObjectType.Equals(DataType.BIGINT))
                    {
                        Value = dataReader.GetInt64(ordinal);
                    }

                    else if (ObjectType.Equals(DataType.CHAR) || ObjectType.Equals(DataType.VARCHAR) || ObjectType.Equals(DataType.TEXT) || ObjectType.Equals(DataType.LONGTEXT) || ObjectType.Equals(DataType.MEDIUMTEXT) || ObjectType.Equals(DataType.TINYTEXT))
                    {
                        Value = dataReader.GetString(ordinal);
                    }
                    else if (ObjectType.Equals(DataType.DOUBLE) || ObjectType.Equals(DataType.FLOAT))
                    {
                        Value = dataReader.GetDouble(ordinal);
                    }
                    else if (ObjectType.Equals(DataType.DECIMAL))
                    {
                        Value = dataReader.GetDecimal(ordinal);
                    }
                    else if (ObjectType.Equals(DataType.DATETIME) || ObjectType.Equals(DataType.DATE) || ObjectType.Equals(DataType.TIMESTAMP) || ObjectType.Equals(DataType.YEAR))
                    {
                        Value = dataReader.GetDateTime(ordinal);
                    }
                    else if (ObjectType.Equals(DataType.TIME))
                    {
                        Value = (TimeSpan)dataReader.GetValue(ordinal);
                    }
                    else if (ObjectType.Equals(DataType.ENUM))
                    {
                        throw new InvalidCastException("Cannot cast enum to DataObject");
                    }
                    
                }
            }

            /// <summary>
            /// Converts this dataobject to a c# bool
            /// </summary>
            public bool ToBool()
            {
                if (ObjectType.Equals(DataType.TINYINT))
                {
                    if (Value.Equals(0))
                    {
                        return false;
                    }
                    else if (Value.Equals(1))
                    {
                        return true;
                    }
                    else
                    {
                        throw new InvalidCastException("Cannot convert this DataObject to bool");
                    }
                }

                else
                {
                    throw new InvalidCastException("Cannot convert this DataObject to Bool");
                }

            }

            /// <summary>
            /// Converts this dataobject to a c# integer
            /// </summary>
            public int ToInt()
            {
                if (Value == null)
                {
                    return 0;
                }

                else if (ObjectType.Equals(DataType.INT) || ObjectType.Equals(DataType.SMALLINT) || ObjectType.Equals(DataType.TINYINT) || ObjectType.Equals(DataType.MEDIUMINT) || ObjectType.Equals(DataType.BIGINT) || ObjectType.Equals(DataType.DECIMAL) || ObjectType.Equals(DataType.DOUBLE) || ObjectType.Equals(DataType.FLOAT))
                {
                    return (int)Value;
                }
         
                else
                {
                    throw new InvalidCastException("Cannot convert this DataObject to Integer");
                }

            }

            /// <summary>
            /// Converts this dataobject to a c# 64 bit integer
            /// </summary>
            public Int64 ToInt64()
            {
                if (Value == null)
                {
                    return 0;
                }

                else if (ObjectType.Equals(DataType.INT) || ObjectType.Equals(DataType.SMALLINT) || ObjectType.Equals(DataType.TINYINT) || ObjectType.Equals(DataType.MEDIUMINT) || ObjectType.Equals(DataType.BIGINT) || ObjectType.Equals(DataType.DECIMAL) || ObjectType.Equals(DataType.DOUBLE) || ObjectType.Equals(DataType.FLOAT))
                {
                    return (Int64)Value;
                }

                else
                {
                    throw new InvalidCastException("Cannot convert this DataObject to Integer");
                }

            }

            /// <summary>
            /// Converts this dataobject to a c# float
            /// </summary>
            public float ToFloat()
            {
                if (Value == null)
                {
                    return 0;
                }
                else if (ObjectType.Equals(DataType.INT) || ObjectType.Equals(DataType.SMALLINT) || ObjectType.Equals(DataType.TINYINT) || ObjectType.Equals(DataType.MEDIUMINT) || ObjectType.Equals(DataType.BIGINT) || ObjectType.Equals(DataType.DECIMAL) || ObjectType.Equals(DataType.DOUBLE) || ObjectType.Equals(DataType.FLOAT))
                {
                    return (float)Value;
                }

                else
                {
                    throw new InvalidCastException("Cannot convert this DataObject to Integer");
                }

            }

            /// <summary>
            /// Converts this dataobject to a c# double
            /// </summary>
            public double ToDouble()
            {
                if (Value == null)
                {
                    return 0;
                }

                else if (ObjectType.Equals(DataType.INT) || ObjectType.Equals(DataType.SMALLINT) || ObjectType.Equals(DataType.TINYINT) || ObjectType.Equals(DataType.MEDIUMINT) || ObjectType.Equals(DataType.BIGINT) || ObjectType.Equals(DataType.DECIMAL) || ObjectType.Equals(DataType.DOUBLE) || ObjectType.Equals(DataType.FLOAT))
                {
                    return (double)Value;
                }

                else
                {
                    throw new InvalidCastException("Cannot convert this DataObject to Integer");
                }

            }

            /// <summary>
            /// Converts this dataobject to a c# string
            /// </summary>
            public override string ToString()
            {
                if (Value == null)
                {
                    return null;
                }
                
                else if (ObjectType.Equals(DataType.CHAR) || ObjectType.Equals(DataType.VARCHAR) || ObjectType.Equals(DataType.TEXT) || ObjectType.Equals(DataType.LONGTEXT) || ObjectType.Equals(DataType.MEDIUMTEXT) || ObjectType.Equals(DataType.TINYTEXT)) 
                {
                    return (string)Value;
                }

                else
                {
                    throw new InvalidCastException("Cannot convert this DataObject to String");
                }

            }
            /// <summary>
            /// Converts this dataobject to a c# decimal
            /// </summary>
            public decimal ToDecimal()
            {
                if (Value == null)
                {
                    return 0;
                }
                
                else if (ObjectType.Equals(DataType.INT) || ObjectType.Equals(DataType.SMALLINT) || ObjectType.Equals(DataType.TINYINT) || ObjectType.Equals(DataType.MEDIUMINT) || ObjectType.Equals(DataType.BIGINT) || ObjectType.Equals(DataType.DECIMAL)|| ObjectType.Equals(DataType.DOUBLE) || ObjectType.Equals(DataType.FLOAT))
                {
                    return (decimal)Value;
                }

                else
                {
                    throw new InvalidCastException("Cannot convert this DataObject to Decimal");
                }

            }
            /// <summary>
            /// Converts this dataobject to a c# datetime
            /// </summary>
            public DateTime ToDateTime()
            {
                if (Value == null)
                {
                    return System.DateTime.MinValue;
                }
                else if (ObjectType.Equals(DataType.DATETIME) || ObjectType.Equals(DataType.DATE) || ObjectType.Equals(DataType.TIME) || ObjectType.Equals(DataType.TIMESTAMP) || ObjectType.Equals(DataType.YEAR))
                {
                    return (DateTime)Value;
                }

                else
                {
                    throw new InvalidCastException("Cannot convert this DataObject to DateTime");
                }

            }

            /// <summary>
            /// Converts this dataobject to a c# timespan
            /// </summary>
            public TimeSpan ToTimeSpan()
            {
                if (Value == null)
                {
                    return TimeSpan.MinValue;
                }
                else if (ObjectType.Equals(DataType.TIME))
                {
                    return (TimeSpan)Value;
                }

                else
                {
                    throw new InvalidCastException("Cannot convert this DataObject to TimeSpan");
                }

            }

            /// <summary>
            /// Converts this dataobject to its preferred type
            /// </summary>
            public object ToPreferredType()
            {
                if (PreferredType == typeof(int))
                {
                    return ToInt();
                }
                else if (PreferredType == typeof(string))
                {
                    return ToString();
                }
                else if (PreferredType == typeof(bool))
                {
                    return ToBool();
                }
                else if (PreferredType == typeof(TimeSpan))
                {
                    return ToTimeSpan();
                }
                else if (PreferredType == typeof(DateTime))
                {
                    return ToDateTime();
                }
                else if (PreferredType == typeof(float))
                {
                    return ToFloat();
                }
                else if (PreferredType == typeof(double))
                {
                    return ToDouble();
                }
                else if (PreferredType == typeof(decimal))
                {
                    return ToDecimal();
                }
                else
                {
                    return Value;
                }
            }
            #endregion
        }
    }
}
