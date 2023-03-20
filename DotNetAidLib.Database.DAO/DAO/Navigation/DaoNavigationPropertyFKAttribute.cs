using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Database.DAO.Core;
                 
namespace DotNetAidLib.Database.DAO.Navigation
{
    /// <summary>
    /// DAO navigation property FK for '1 - 1/N' relations.
    /// ReferencePropertyName: Navigation Property name in '1' part
    /// ForeignKeyProperties: Foreigns IDs properties names in 1/N part
    /// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DaoNavigationPropertyFKAttribute : DaoNavigationPropertyAttribute
	{
        /// <summary>
        /// Constructor
        /// </summary>
		public DaoNavigationPropertyFKAttribute()
			: base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public DaoNavigationPropertyFKAttribute(String referencePropertyName, String foreignKeyProperty)
            : base() {
            this.ReferencePropertyName = referencePropertyName;
            this.ForeignKeyProperties = new string[] { foreignKeyProperty };
        }

        /// <summary>
        /// ReferencePropertyName: Navigation Property name in '1' part
        /// </summary>
        public String ReferencePropertyName { get; set; }

        /// <summary>
        /// Foreigns properties names in 1/N part
        /// </summary>
        public String[] ForeignKeyProperties { get; set; }
	}
}
