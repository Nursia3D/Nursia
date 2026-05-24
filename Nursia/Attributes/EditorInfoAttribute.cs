using System;

namespace Nursia.Attributes
{
	/// <summary>
	/// Provides editor information for properties in the Nursia engine.
	/// </summary>
	public class EditorInfoAttribute: Attribute
	{
		/// <summary>
		/// Gets the category for this editor information.
		/// </summary>
		public string Category { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="EditorInfoAttribute"/> class.
		/// </summary>
		/// <param name="category">The category name for editor organization.</param>
		public EditorInfoAttribute(string category)
		{
			Category = category;
		}
	}
}
