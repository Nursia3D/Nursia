using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Materials;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Billboards;
using Nursia.Utilities;
using Nursia.Utility;

namespace Nursia.Editor.Utility
{
	internal static class _3DUtils
	{
		private static DrMeshPart _selectionMesh;
		private static EditorNode _selectionNode;

		private static DrMeshPart SelectionMesh
		{
			get
			{
				if (_selectionMesh == null)
				{
					_selectionMesh = MeshPrimitives.CreateBoxLinesMeshPart(Nrs.GraphicsDevice, new BoundingBox(Vector3.Zero, Vector3.One));
				}

				return _selectionMesh;
			}
		}

		private static EditorNode SelectionNode
		{
			get
			{
				if (_selectionNode == null)
				{
					_selectionNode = new EditorNode(
						SelectionMesh,
						new UnlitMaterial
						{
							DiffuseColor = Color.Orange,
							CastsShadows = false
						});
				}

				return _selectionNode;
			}
		}


		public static EditorNode GetSelectionNode(this SceneNode node)
		{
			if (node == null)
			{
				return null;
			}

			var box = node.FullBoundingBox ?? Mathematics.DefaultBox;

			var result = SelectionNode;

			var scale = Matrix.CreateScale(box.ToScale()) * Matrix.CreateTranslation(box.Min);
			result.Transform = scale * node.GlobalTransform;

			return result;
		}

		public static BillboardNode GetGizmo(this SceneNode n, Texture2D texture, Color? color)
		{
			BillboardNode gizmo;
			if (n.Tag == null)
			{
				gizmo = new BillboardNode
				{
					Texture = texture
				};

				n.Tag = gizmo;
			}
			else
			{
				gizmo = (BillboardNode)n.Tag;
			}

			gizmo.Translation = n.GlobalTransform.Translation;

			if (color != null)
			{
				gizmo.Color = color.Value;
			}

			return gizmo;
		}
	}
}