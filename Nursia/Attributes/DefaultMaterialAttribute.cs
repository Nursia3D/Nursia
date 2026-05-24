using System;

namespace Nursia.Attributes
{
	/// <summary>
	/// Marks a property as the primary material property of a scene node.
	/// </summary>
	/// <remarks>
	/// This attribute is used by the Nursia Editor to identify which property on a scene node
	/// holds the default material. The editor provides UI controls to allow users to select and
	/// change the material type for any node decorated with this attribute. When a new scene
	/// node is created, the editor automatically instantiates a default material (typically
	/// BlinnPhongMaterial) for properties marked with this attribute.
	///
	/// Typically applied to properties of type IMaterial that represent the primary rendering
	/// material for mesh-based scene nodes such as MeshNodeBase and InstancedMeshNode.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Property)]
	public class DefaultMaterialAttribute: Attribute
	{
	}
}
