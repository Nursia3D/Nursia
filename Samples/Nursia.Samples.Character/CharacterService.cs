using AssetManagementBase;
using DigitalRiseModel.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.SceneGraph;
using System;

namespace DigitalRiseModel
{
	/// <summary>Manages character animation, physics, and weapon state.</summary>
	internal class CharacterService
	{
		private const float Gravity = 0.015f;
		private const float DefaultY = 0.0f;
		private const float JumpForce = 0.5f;

		/// <summary>Character locomotion and action states.</summary>
		private enum MainState
		{
			Idle,
			Run,
			Jump,
			Draw,
			Sheath,
			Slash
		}

		/// <summary>Jump animation phases: Start, Loop (airtime), Land, Finished.</summary>
		private enum JumpState
		{
			Start,
			Loop,
			Land,
			Finished
		}

		private readonly AnimationController _player;

		private readonly NursiaModelNode _modelNode;
		private readonly ModelBoneAttachment _weaponAttachment;

		public NursiaModelNode ModelNode => _modelNode;

		private MainState _mainState = MainState.Idle;
		private JumpState _jumpState = JumpState.Start;
		private float _jumpVelocity = 0.0f;
		private readonly AnimationBlendNode _runDrawAnimation;
		private readonly AnimationBlendNode _runSheathAnimation;
		private readonly AnimationBlendNode _runSlashAnimation;

		/// <summary>Indicates whether the character is currently holding a weapon (armed state).</summary>
		public bool WeaponDrawn { get; private set; }

		/// <summary>Whether the character is busy with a non-interruptible animation.</summary>
		private bool IsBusy
		{
			get
			{
				if (_mainState != MainState.Jump && _mainState != MainState.Sheath && ((_player.RootNode.Flags & AnimationFlags.Looped) != 0 || _player.HasFinished))
					return false;

				if (_mainState == MainState.Sheath && !WeaponDrawn)
					return false;

				if (_mainState == MainState.Jump && _jumpState == JumpState.Finished)
					return false;

				return true;
			}
		}

		/// <summary>Initializes character model, animations, and weapon attachment.</summary>
		public CharacterService(NursiaModelNode characterModelNode, NursiaModelNode swordModelNode)
		{
			if (characterModelNode == null)
			{
				throw new ArgumentNullException(nameof(characterModelNode));
			}

			if (swordModelNode == null)
			{
				throw new ArgumentNullException(nameof(swordModelNode));
			}

			_modelNode = characterModelNode ?? throw new ArgumentNullException(nameof(characterModelNode));

			_weaponAttachment = ModelBoneAttachment.CreateFromModelNode(swordModelNode);
			_modelNode.BonesAttachments.Add(_weaponAttachment);
			SetSheathedTransform();

			_player = new AnimationController(_modelNode.Skeleton);
			_player.StartClip("Idle", true);
			_modelNode.Translation = new Vector3(0, DefaultY, 0);

			var characterModel = _modelNode.Model;
			var topFilter = characterModel.CreateBoneFilter("mixamorig:Spine");
			var bottomFilter = characterModel.CreateInverseBoneFilter(topFilter);

			_runDrawAnimation = new AnimationBlendNode();
			_runDrawAnimation.AddLayer(characterModel.Animations["Run"], isLooped: true).BoneFilter = bottomFilter;
			_runDrawAnimation.AddLayer(characterModel.Animations["DrawGreatSword"]).BoneFilter = topFilter;

			_runSheathAnimation = new AnimationBlendNode();
			_runSheathAnimation.AddLayer(characterModel.Animations["RunGreatSword"], isLooped: true).BoneFilter = bottomFilter;
			_runSheathAnimation.AddLayer(characterModel.Animations["DrawGreatSword"], AnimationFlags.PlayBackwards).BoneFilter = topFilter;

			_runSlashAnimation = new AnimationBlendNode();
			_runSlashAnimation.AddLayer(characterModel.Animations["RunGreatSword"], isLooped: true).BoneFilter = bottomFilter;
			_runSlashAnimation.AddLayer(characterModel.Animations["SlashGreatSword"]).BoneFilter = topFilter;
		}

		private void SetSheathedTransform()
		{
			_weaponAttachment.Bone = _modelNode.Model.FindBoneByName("mixamorig:Spine");

			var transform = new SrtTransform
			{
				Translation = new Vector3(-0.6f, 0, -1.4f),
				Scale = new Vector3(16),
				Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(180.0f))
			};
			_weaponAttachment.Transform = transform.ToMatrix();
		}

		private void SetDrawnTransform()
		{
			_weaponAttachment.Bone = _modelNode.Model.FindBoneByName("mixamorig:RightHand");

			var transform = new SrtTransform
			{
				Translation = new Vector3(3.5f, 0f, 0f),
				Scale = new Vector3(16),
				Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(270.0f))
			};
			_weaponAttachment.Transform = transform.ToMatrix();
		}

		/// <summary>Transitions to idle animation based on weapon state.</summary>
		private void InternalIdle(bool weaponDrawn)
		{
			if (IsBusy || (_mainState == MainState.Idle && WeaponDrawn == weaponDrawn))
				return;

			if (weaponDrawn)
				_player.CrossfadeToClip("IdleGreatSword", TimeSpan.FromSeconds(0.1), true);
			else
				_player.CrossfadeToClip("Idle", TimeSpan.FromSeconds(0.1), true);

			_mainState = MainState.Idle;
			WeaponDrawn = weaponDrawn;
		}

		/// <summary>Applies movement velocity and transitions to run animation.</summary>
		private void InternalRun(Vector3 velocity, bool weaponDrawn)
		{
			_modelNode.Translation += velocity;

			if (IsBusy || (_mainState == MainState.Run && WeaponDrawn == weaponDrawn))
				return;

			if (weaponDrawn)
				_player.CrossfadeToClip("RunGreatSword", TimeSpan.FromSeconds(0.1), true);
			else
				_player.CrossfadeToClip("Run", TimeSpan.FromSeconds(0.1), true);

			_mainState = MainState.Run;
			WeaponDrawn = weaponDrawn;
		}

		/// <summary>Transitions to idle state.</summary>
		public void Idle() => InternalIdle(WeaponDrawn);

		/// <summary>Transitions to running state with movement velocity.</summary>
		public void Run(Vector3 velocity) => InternalRun(velocity, WeaponDrawn);

		/// <summary>Draws the weapon with animation.</summary>
		public void DrawWeapon()
		{
			if (IsBusy || _mainState == MainState.Draw || WeaponDrawn)
				return;

			if (_mainState == MainState.Idle)
			{
				_player.CrossfadeToClip("DrawGreatSword", TimeSpan.FromSeconds(0.1), false);
			}
			else
			{
				_player.CrossfadeToClip(_runDrawAnimation, TimeSpan.FromSeconds(0.1));
			}

			_mainState = MainState.Draw;
			SetDrawnTransform();
			WeaponDrawn = true;
		}

		/// <summary>Sheathes the weapon by reversing the draw animation.</summary>
		public void SheathWeapon()
		{
			if (IsBusy || _mainState == MainState.Sheath || !WeaponDrawn)
				return;

			if (_mainState == MainState.Idle)
			{
				_player.CrossfadeToClip("DrawGreatSword", TimeSpan.FromSeconds(0.1), AnimationFlags.PlayBackwards);
			}
			else
			{
				_player.CrossfadeToClip(_runSheathAnimation, TimeSpan.FromSeconds(0.1));
			}

			_mainState = MainState.Sheath;
		}

		/// <summary>Performs a slash attack (requires weapon drawn).</summary>
		public void Slash()
		{
			if (IsBusy || _mainState == MainState.Slash || !WeaponDrawn)
			{
				return;
			}

			if (_mainState == MainState.Idle)
			{
				_player.CrossfadeToClip("SlashGreatSword", TimeSpan.FromSeconds(0.1), false);
			}
			else
			{
				_player.CrossfadeToClip(_runSlashAnimation, TimeSpan.FromSeconds(0.1));
			}
			_mainState = MainState.Slash;
		}

		/// <summary>Initiates a jump with animation.</summary>
		public void Jump(Vector3 velocity)
		{
			if (IsBusy || _mainState == MainState.Jump)
				return;

			_jumpVelocity = JumpForce;
			_mainState = MainState.Jump;
			_jumpState = JumpState.Start;
			_player.CrossfadeToClip("JumpStart", TimeSpan.FromSeconds(0.2), false);
		}

		/// <summary>Updates character animation and jump physics. Call once per frame.</summary>
		public void Update(TimeSpan elapsed)
		{
			if (_mainState == MainState.Jump)
			{
				switch (_jumpState)
				{
					case JumpState.Start:
						if (_player.HasFinished)
							_jumpState = JumpState.Loop;
						break;

					case JumpState.Loop:
						if (_modelNode.Translation.Y <= 6f)
						{
							_jumpState = JumpState.Land;
							_player.CrossfadeToClip("JumpEnd", TimeSpan.FromSeconds(0.2), false);
						}
						break;
				}

				if (_jumpState != JumpState.Finished)
				{
					_jumpVelocity -= Gravity;
					_modelNode.Translation += Vector3.Up * _jumpVelocity;
				}

				if (_modelNode.Translation.Y <= DefaultY)
				{
					_modelNode.Translation = new Vector3(_modelNode.Translation.X, DefaultY, _modelNode.Translation.Z);
					_jumpState = JumpState.Finished;
				}
			}

			if (_mainState == MainState.Sheath && _player.HasFinished)
			{
				SetSheathedTransform();
				WeaponDrawn = false;
			}

			_player.Update(elapsed);
		}
	}
}
