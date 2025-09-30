using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nursia;
using Nursia.SceneGraph;
using System;

namespace FuelCell
{
	public class FuelCarrier : GameObject
	{
		public float ForwardDirection
		{
			get => MathHelper.ToRadians(Model.Rotation.Y);

			set
			{
				var rot = Model.Rotation;
				rot.Y = MathHelper.ToDegrees(value);
				Model.Rotation = rot;
			}
		}
		
		public int MaxRange { get; set; }
		private Vector3 startPosition = new Vector3(0, GameConstants.HeightOffset, 0);
		private SoundEffect engineRumble;

		public FuelCarrier()
			: base()
		{
			MaxRange = GameConstants.MaxRange;
		}

		public void LoadContent(AssetManager content, string modelName)
		{
			Model = (NursiaModelNode)content.LoadSceneNode(modelName);
			Position = startPosition;
			BoundingSphere = CalculateBoundingSphere();

			engineRumble = content.LoadSoundEffect("Audio/engine-rumble.wav");

			BoundingSphere scaledSphere;
			scaledSphere = BoundingSphere;
			scaledSphere.Radius *= GameConstants.FuelCarrierBoundingSphereFactor;
			BoundingSphere = new BoundingSphere(scaledSphere.Center, scaledSphere.Radius);
		}

		internal void Reset()
		{
			Position = startPosition;
			ForwardDirection = 0f;
		}

		public void Update(IInputState inputState, Barrier[] barriers)
		{
			Vector3 futurePosition = Position;

			ForwardDirection += inputState.GetPlayerTurn(PlayerIndex.One) * GameConstants.TurnSpeed;
			Matrix orientationMatrix = Matrix.CreateRotationY(ForwardDirection);

			Vector3 speed = Vector3.Transform(inputState.GetPlayerMove(PlayerIndex.One), orientationMatrix);
			if (speed != Vector3.Zero)
			{
				engineRumble.Play();
			}
			speed *= GameConstants.Velocity;
			futurePosition = Position + speed;

			if (ValidateMovement(futurePosition, barriers))
			{
				Position = futurePosition;

				BoundingSphere updatedSphere;
				updatedSphere = BoundingSphere;

				updatedSphere.Center.X = Position.X;
				updatedSphere.Center.Z = Position.Z;
				BoundingSphere = new BoundingSphere(updatedSphere.Center, updatedSphere.Radius);
			}
		}

		private bool ValidateMovement(Vector3 futurePosition, Barrier[] barriers)
		{
			BoundingSphere futureBoundingSphere = BoundingSphere;
			futureBoundingSphere.Center.X = futurePosition.X;
			futureBoundingSphere.Center.Z = futurePosition.Z;

			//Do not allow off-terrain driving
			if ((Math.Abs(futurePosition.X) > MaxRange) || (Math.Abs(futurePosition.Z) > MaxRange))
				return false;

			//Do not allow driving through a barrier
			if (CheckForBarrierCollision(futureBoundingSphere, barriers))
			{
				return false;
			}

			return true;
		}

		private bool CheckForBarrierCollision(BoundingSphere vehicleBoundingSphere, Barrier[] barriers)
		{
			for (int curBarrier = 0; curBarrier < barriers.Length; curBarrier++)
			{
				if (vehicleBoundingSphere.Intersects(barriers[curBarrier].BoundingSphere))
				{
					return true;
				}
			}

			return false;
		}
	}
}