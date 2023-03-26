using System;

namespace DotNetAidLib.Database.DAO.Navigation
{
	/// <summary>
	///     DAO navigation property FK for '1 - 1/N' relations.
	///     ReferencePropertyName: Navigation Property name in '1' part
	///     ForeignKeyProperties: Foreigns IDs properties names in 1/N part
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
    public class DaoNavigationPropertyFKAttribute : DaoNavigationPropertyAttribute
    {
	    /// <summary>
	    ///     Constructor
	    /// </summary>
	    public DaoNavigationPropertyFKAttribute()
        {
        }

	    /// <summary>
	    ///     Constructor
	    /// </summary>
	    public DaoNavigationPropertyFKAttribute(string referencePropertyName, string foreignKeyProperty)
        {
            ReferencePropertyName = referencePropertyName;
            ForeignKeyProperties = new[] {foreignKeyProperty};
        }

	    /// <summary>
	    ///     ReferencePropertyName: Navigation Property name in '1' part
	    /// </summary>
	    public string ReferencePropertyName { get; set; }

	    /// <summary>
	    ///     Foreigns properties names in 1/N part
	    /// </summary>
	    public string[] ForeignKeyProperties { get; set; }
    }
}