using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Materials;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Lights;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	/// <summary>
	/// Interface for batches that collect lights for rendering.
	/// </summary>
	public interface ILightsBatch
	{
		/// <summary>
		/// Adds a light to the batch.
		/// </summary>
		/// <param name="light">The light to add.</param>
		void AddLight(BaseLight light);
	}

	/// <summary>
	/// Interface for batches that collect render jobs for rendering.
	/// </summary>
	public interface IRenderJobsBatch
	{
		/// <summary>
		/// Adds a mesh to the render batch.
		/// </summary>
		/// <param name="mesh">The mesh to render.</param>
		/// <param name="material">The material to apply to the mesh.</param>
		/// <param name="transform">The transformation matrix for the mesh.</param>
		/// <param name="flags">Optional rendering flags.</param>
		/// <param name="bonesTransforms">Optional bone transformation matrices for skinned meshes.</param>
		/// <param name="clipPlane">Optional clip plane for clipping geometry.</param>
		/// <param name="reflectionPlane">Optional reflection plane for reflective surfaces.</param>
		void AddMesh(DrMeshPart mesh, IMaterial material, Matrix transform,
			RenderJobFlags flags = RenderJobFlags.None,
			Matrix[] bonesTransforms = null, Plane? clipPlane = null, Plane? reflectionPlane = null);

		/// <summary>
		/// Adds an instanced mesh to the render batch.
		/// </summary>
		/// <param name="mesh">The mesh to render.</param>
		/// <param name="material">The material to apply to the mesh.</param>
		/// <param name="transform">The base transformation matrix.</param>
		/// <param name="instancesTransforms">Vertex buffer containing instance transformation data.</param>
		/// <param name="boundingBox">The bounding box of the instanced geometry.</param>
		/// <param name="flags">Optional rendering flags.</param>
		/// <param name="clipPlane">Optional clip plane for clipping geometry.</param>
		/// <param name="reflectionPlane">Optional reflection plane for reflective surfaces.</param>
		void AddInstancedMesh(DrMeshPart mesh, IMaterial material, Matrix transform, VertexBuffer instancesTransforms, BoundingBox boundingBox,
			RenderJobFlags flags = RenderJobFlags.None,
			Plane? clipPlane = null, Plane? reflectionPlane = null);
	}

	/// <summary>
	/// Interface for sources that provide lights and renderable geometry for a scene.
	/// </summary>
	public interface IRenderSource
	{
		/// <summary>
		/// Queries lights that are visible to the specified camera and adds them to the batch.
		/// </summary>
		/// <param name="camera">The camera viewing the scene.</param>
		/// <param name="batch">The batch to add lights to.</param>
		void QueryLights(Camera camera, ILightsBatch batch);

		/// <summary>
		/// Queries renderable geometry visible to the specified camera and adds it to the batch.
		/// </summary>
		/// <param name="camera">The camera viewing the scene.</param>
		/// <param name="batch">The batch to add render jobs to.</param>
		void QueryRenderJobs(Camera camera, IRenderJobsBatch batch);
	}
}
