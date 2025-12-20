using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Materials;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Lights;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	public interface ILightsBatch
	{
		void AddLight(BaseLight light);
	}

	public interface IRenderJobsBatch
	{
		void AddMesh(DrMeshPart mesh, IMaterial material, Matrix transform,
			RenderJobFlags flags = RenderJobFlags.None,
			Matrix[] bonesTransforms = null, Plane? clipPlane = null, Plane? reflectionPlane = null);

		void AddInstancedMesh(DrMeshPart mesh, IMaterial material, Matrix transform, VertexBuffer instancesTransforms, BoundingBox boundingBox,
			RenderJobFlags flags = RenderJobFlags.None,
			Plane? clipPlane = null, Plane? reflectionPlane = null);

		void AddMeshLOD(List<DrMeshPart> levels, IMaterial material, Matrix transform,
			RenderJobFlags flags = RenderJobFlags.None,
			Plane? clipPlane = null, Plane? reflectionPlane = null);
	}

	public interface IRenderSource
	{
		void QueryLights(Camera camera, ILightsBatch batch);
		void QueryRenderJobs(Camera camera, IRenderJobsBatch batch);
	}
}
