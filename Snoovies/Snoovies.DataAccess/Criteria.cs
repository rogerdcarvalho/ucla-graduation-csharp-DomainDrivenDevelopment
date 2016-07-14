using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snoovies.DataAccess
{
    /// <summary>
    /// Simple class to build SQL queries with a set criteria
    /// </summary>
    public class Criteria
    {
        /// <summary>
        /// Name of the field in the table that should be matched
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// The value that should be matched
        /// </summary>
        public int Expression { get; set; }

        /// <summary>
        /// Whether the value should be an "=" or "LIKE" in the matching
        /// </summary>
        public bool PreciseMatch { get; set; }
    }
}
