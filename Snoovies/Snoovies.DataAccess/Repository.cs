
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using RDCMedia.Common;
using System.Transactions;

namespace Snoovies.DataAccess
{
    /// <summary>
    /// Universal repository to fetch and persist any table (and its linked tables) from any MySQL database. Tables that are to be fetched or persisted need to be provided as DomainBase objects
    /// </summary>
    public class Repository<T> where T : DomainBase, new()
    {
        #region Public Methods

        /// <summary>
        /// Fetches all records of a given domain in the database. It will also fetch any other domains that link to the main domain using a foreign key dependency (these linked domains need to be provided by the main domain via the getPotentialLinkedDomains() method.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>All domain objects (containing their dependent objects) in an IEnumerable </returns>
        public IEnumerable<T> Fetch(Criteria criteria = null)
        {
            var data = new List<T>();
            var connString = ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString;
            T coreObject = new T();
           
            using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString))
            {
                conn.Open();
             
                using (var cmd = conn.CreateCommand())
                {
                    //Create SQL Queries, based on however many potential linked domains are using the main domain as a foreign key
                    Dictionary<Type, string> sqlQueries = sqlSelectStatementBuilder(coreObject, criteria);
                    var sql = new StringBuilder();
                    foreach (var query in sqlQueries)
                    {
                        sql.Append(query.Value);
                    }

                    //Set the queries to the connection command
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = sql.ToString();

                    //Execute the queries
                    var dr = cmd.ExecuteReader();

                    //Parse all core domain objects
                    while (dr.Read())
                    {
                        //Create new domain object
                        T domain = new T();

                        //Import data into the new domain object
                        domain.importData(dr);
                        data.Add(domain);
                    }

                    //Completed the core query. Now remove it from the queries dictionary
                    sqlQueries.Remove(coreObject.Type);

                    //Now run all remaining queries related to domains linked to the main domain
                    foreach (var query in sqlQueries)
                    {
                        dr.NextResult();
                        while (dr.Read())
                        {
                            //Create new domain object using reflection
                            object temp = Activator.CreateInstance(query.Key);
                            DomainBase domain = (DomainBase)temp;

                            //Import data into the new domain object
                            domain.importData(dr);

                            //Get the foreign key column within the new domain
                            DatabaseHelper.DataObject foreignKey = domain.DataObjects.Values.Where(o => o.ObjectName == coreObject.ForeignKeyName).SingleOrDefault();

                            //Ensure that the foreign key value is not null and that parent domains were found
                            if (foreignKey.Value != null && data.Count() > 0)
                            {
                                //Find the parent in the data list and link the objects
                                if (data.Where(o => o.Id == foreignKey.ToInt()).SingleOrDefault() != null)
                                {
                                    DomainBase parent = data.Where(o => o.Id == foreignKey.ToInt()).Single();
                                    domain.setParent(parent);
                                }
                            }
                        }
                    }
                }                
            }

            return data;
        }

        /// <summary>
        /// Saves entity changes to the database
        /// </summary>
        /// <param name="item"></param>
        /// <returns>updated entity, or null if the entity is deleted</returns>
        public T Persist(T item)
        {
            //Check if anything needs to happen (perhaps the domain doesn't need any work)
            if (item.IsNew && item.IsMarkedForDeletion)
            {
                return null;
            }

            //Setup connection
            var connString = ConfigurationManager
                .ConnectionStrings["TestConnection"].ConnectionString;
            using (TransactionScope ts = new TransactionScope())
            {
                using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString))
                {
                    conn.Open();

                    //Sync domain data with database
                    item = (T)SyncData(item, conn);
                }
                ts.Complete();
            }

            //Return updated domain
            return item;
        }
        #endregion

        #region Helper methods

        /// <summary>
        /// Deletes a given entity from the database
        /// </summary>
        /// <param name="item"></param>
        private static void Delete(DomainBase item, MySql.Data.MySqlClient.MySqlConnection conn)
        {
            using (MySql.Data.MySqlClient.MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "DELETE FROM `" + item.DomainName + "` where `" + item.PrimaryKeyName + "` = @Id";
                cmd.Parameters.AddWithValue("@Id", item.Id);
                cmd.ExecuteNonQuery();

                //Now check whether the record was truly deleted.
                cmd.CommandText = "select * from `" + item.DomainName + "` where `" + item.PrimaryKeyName + "` = @Id";

                var dr = cmd.ExecuteReader();
                if (dr.HasRows == true)
                {
                    throw new TransactionInDoubtException("After deleting a record, it was still found in the database");
                }
                dr.Close();
            }
        }

        /// <summary>
        /// Inserts a given entity into the database
        /// </summary>
        /// <param name="item"></param>
        private static DomainBase Insert(DomainBase item, MySql.Data.MySqlClient.MySqlConnection conn)
        {
            //Prepare the list of columns to be used in the SQL statement
            StringBuilder columnList = new StringBuilder();

            //Find out what the last column in the list is
            DatabaseHelper.DataObject last = item.DataObjects.Values.Last();

            foreach (DatabaseHelper.DataObject column in item.DataObjects.Values)
            {
                // Add all columns to the stringBuilder object
                if (column.Equals(last))
                {
                    columnList.Append("`" + column.ObjectName + "`");
                }
                else
                {
                    columnList.Append("`" + column.ObjectName + "` , ");
                }
            }

            //Convert the columnList to string
            string columns = columnList.ToString();

            //Prepare the list of value references to be used in the SQL statement
            StringBuilder valuesList = new StringBuilder();

            foreach (DatabaseHelper.DataObject column in item.DataObjects.Values)
            {
                // Add all value references to the stringBuilder object
                if (column.Equals(last))
                {
                    valuesList.Append("@" + column.ObjectName);
                }
                else
                {
                    valuesList.Append("@" + column.ObjectName + " , ");
                }
            }

            //Convert the valuesList to string
            string values = valuesList.ToString();

            using (MySql.Data.MySqlClient.MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandType = System.Data.CommandType.Text;
                var sql = new StringBuilder();
                sql.Append("INSERT INTO `" + item.DomainName + "` (" + columns + ") ");
                sql.Append("VALUES ( " + values + "); ");
                cmd.CommandText = sql.ToString();

                //Add value parameters for each column
                foreach (DatabaseHelper.DataObject column in item.DataObjects.Values)
                {
                    cmd.Parameters.AddWithValue("@" + column.ObjectName, column.ToPreferredType());
                }
                cmd.ExecuteNonQuery();


                if (cmd.LastInsertedId > 0)
                {
                    // If the query has a last inserted id, add a parameter to hold it.
                    cmd.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("newId", cmd.LastInsertedId));

                    // Store the id of the new record. Convert from Int64 to Int32 (int).
                    int insertedId = Convert.ToInt32(cmd.Parameters["@newId"].Value);

                    //Now request the database to return all data with the new Id. This ensures that the held object truly contains all data represented in the database (in case e.g. strings were cut due to insufficient varchar lengths set in the database)
                    cmd.CommandText = "select * from `" + item.DomainName + "` where `" + item.PrimaryKeyName + "` = @Id";
                    cmd.Parameters.AddWithValue("@Id", insertedId);

                    var dr = cmd.ExecuteReader();
                    if (dr.HasRows == true)
                    {
                        while (dr.Read())
                        {
                            item.importData(dr);
                        }
                        dr.Close();
                        return item;
                    }

                    else
                    {
                        throw new TransactionInDoubtException("After inserting data into the database, the data was not confirmed");
                    }
                }
                else
                {
                    throw new NullReferenceException("While running insert query, no LastInsertedId was provided");
                }
            }
        }

        /// <summary>
        /// Updates a given entity in the database
        /// </summary>
        /// <param name="item"></param>
        private static DomainBase Update(DomainBase item, MySql.Data.MySqlClient.MySqlConnection conn)
        {
            //Prepare the sql query
            StringBuilder sql = new StringBuilder();
            sql.Append("UPDATE `" + item.DomainName + "` SET ");

            //Find out what the last column in the list is
            DatabaseHelper.DataObject last = item.DataObjects.Values.Last();

            foreach (DatabaseHelper.DataObject column in item.DataObjects.Values)
            {
                // Add all columns to the stringBuilder object
                if (column.Equals(last))
                {
                    sql.Append("`" + column.ObjectName + "` = @" + column.ObjectName);
                }
                else
                {
                    sql.Append("`" + column.ObjectName + "` = @" + column.ObjectName + ", ");
                }
            }
            sql.Append(" WHERE `" + item.PrimaryKeyName + "` = " + item.Id);

            using (MySql.Data.MySqlClient.MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = sql.ToString();

                //Add values for each column
                foreach (DatabaseHelper.DataObject column in item.DataObjects.Values)
                {
                    cmd.Parameters.AddWithValue("@" + column.ObjectName, column.ToPreferredType());
                }
                cmd.ExecuteNonQuery();

                //Now request the database to return all data with the new Id. This ensures that the held object truly contains all data represented in the database (in case e.g. strings were cut due to insufficient varchar lengths set in the database)
                cmd.CommandText = "select * from `" + item.DomainName + "` where `" + item.PrimaryKeyName + "` = @Id";
                cmd.Parameters.AddWithValue("@Id", item.Id);

                var dr = cmd.ExecuteReader();
                if (dr.HasRows == true)
                {
                    while (dr.Read())
                    {
                        item.importData(dr);
                    }
                    dr.Close();
                    return item;
                }

                else
                {
                    throw new TransactionInDoubtException("After updating data into the database, the data was not confirmed");
                }
            }
        }

        /// <summary>
        /// Builds SQL "Select" queries based on domain objects (and optionally a criteria object). First it creates a SQL Select query on the domain object (and its criteria) itself, then it checks if the domain object has any linked domains that refer to it with a foreign key. If so, it builds additional queries for each of these linked domains (these queries won't have any criteria, they will simply return all database objects that could potentially link to the main domain). The caller should filter the outcome of the queries. 
        /// </summary>
        /// <param name="item"></param>
        /// <returns>All queries in a dictionary object (with the domain types they refer to as keys) </returns>
        private static Dictionary <Type, string> sqlSelectStatementBuilder(DomainBase domain, Criteria criteria = null)
        {
            //Create the dictionary to be returned
            Dictionary<Type, string> output = new Dictionary<Type, string>();

            //Build the main query
            if (criteria == null)
            {
                output.Add(domain.Type, "SELECT * FROM " + domain.DomainName + ";");
            }
            else
            {
                output.Add(domain.Type, "SELECT * FROM " + domain.DomainName + " WHERE " + criteria.FieldName + " = " + criteria.Expression + ";");
            }

            //Check if the domain has other domains that link to it as a foreign key
            Type[] potentialLinks = domain.getPotentialLinkedDomains();

            if (potentialLinks.Length > 0)
            {
                //Add queries for each potential link
                foreach (Type potentialLink in potentialLinks)
                {
                    //Create a new domain object using reflection and recurse to create all required queries for this new object
                    object temp = Activator.CreateInstance(potentialLink);
                    DomainBase childDomain = (DomainBase)temp;
                    Dictionary<Type, string> additionalQueries = sqlSelectStatementBuilder(childDomain);

                    //Add all created queries for the child domain to the output list
                    foreach(var additionalQuery in additionalQueries)
                    {
                        output.Add(additionalQuery.Key, additionalQuery.Value);
                    }
                }
            }
            return output;
        }
   
        /// <summary>
        /// Decides whether to insert, update or delete data based on the state of an entity. Recurses for any linked entities held by the entity provided.
        /// </summary>
        /// <param name="item"></param>
        private static DomainBase SyncData(DomainBase item, MySql.Data.MySqlClient.MySqlConnection conn)
        {
            if (item.IsMarkedForDeletion)
            {
                //First delete any linked entities where applicable
                for (int i = 0; i < item.LinkedDomains.Count; i++)
                {
                    DomainBase domain = item.LinkedDomains[i];
                    domain = SyncData(domain, conn);
                    item.LinkedDomains[i] = domain;
                }

                //Then delete entity if applicable
                if (!item.IsNew)
                {
                    Delete(item, conn);
                }
                item = null;
            }
            else if (item.IsNew)
            {
                //Insert the entity
                item = Insert(item, conn);

                //Insert any linked entities where applicable
                for (int i = 0; i < item.LinkedDomains.Count; i++)
                {
                  
                    DomainBase domain = item.LinkedDomains[i];

                    //Check for nulls, as previous operation may have deleted children items and set them to null
                    if (domain != null)
                    {
                        //Set foreign key
                        domain.DataObjects.Values.Where(o => o.ObjectName == item.ForeignKeyName).SingleOrDefault().Value = item.Id;

                        //Sync data
                        domain = SyncData(domain, conn);
                        if (domain != null)
                        {
                            item.LinkedDomains[i] = domain;
                        }
                        else
                        {
                            item.LinkedDomains.RemoveAt(i);
                        }
                    }
                    else
                    {
                        item.LinkedDomains.RemoveAt(i);
                    }
                }
            }
            else if (!item.IsSynced)
            {
                //Update entity 
                Update(item, conn);

                //Update any linked entities where applicable
                for (int i = 0; i < item.LinkedDomains.Count; i++)
                {
                    DomainBase domain = item.LinkedDomains[i];
                    domain = SyncData(domain, conn);
                    if (domain != null)
                    {
                        item.LinkedDomains[i] = domain;
                    }
                    else
                    {
                        item.LinkedDomains.RemoveAt(i);
                    }
                }
            }
            else
            {
                //Sync any linked entities
                for (int i = 0; i < item.LinkedDomains.Count; i++)
                {
                    DomainBase domain = item.LinkedDomains[i];
                    domain = SyncData(domain, conn);
                    item.LinkedDomains[i] = domain;
                }
            }
            return item;
        }
        #endregion
    }
}
