using DigitalRiseModel;
using DigitalRiseModel.Animation;
using Microsoft.Xna.Framework;
using Nursia.SceneGraph;
using System;

namespace Nursia.Samples.Character
{
	/// <summary>Manages character locomotion, weapon state machine, animation transitions and jump physics.</summary>
	internal class Character
	{
		/// <summary>Locomotion states. Drives which clip set the lower body plays.</summary>
		private enum AnimationState
		{
			/// <summary>Standing still.</summary>
			Idle,
			/// <summary>Moving on the ground.</summary>
			Running,
			/// <summary>Jump is in progress, still travelling upwards.</summary>
			Jumping,
			/// <summary>Apex is behind us, falling towards the ground.</summary>
			Landing
		}

		/// <summary>Weapon states. Drives which clip set the upper body plays.</summary>
		private enum WeaponState
		{
			/// <summary>Sword is carried on the back.</summary>
			Sheathed,
			/// <summary>Draw animation is playing.</summary>
			Drawing,
			/// <summary>Sword is held in the hand.</summary>
			Drawn,
			/// <summary>Sheath animation is playing.</summary>
			Sheathing,
			/// <summary>Slash attack animation is playing.</summary>
			Slashing
		}

		/// <summary>Ground height of the character.</summary>
		private const float DefaultY = 0.0f;
		/// <summary>Jump gravity acceleration per second.</summary>
		private const float Gravity = 54.0f;
		/// <summary>Initial jump velocity per second.</summary>
		private const float JumpForce = 30.0f;
		/// <summary>Height at which the landing clip takes over from the takeoff clip.</summary>
		private const float JumpEndHeight = 6.0f;

		/// <summary>Duration used for every animation transition between clips.</summary>
		private static readonly TimeSpan AnimationCrossfadeDelay = TimeSpan.FromSeconds(0.2f);

		private readonly AnimationController _player;

		private readonly NursiaModelNode _modelNode;
		private readonly ModelBoneAttachment _weaponAttachment;

		private readonly DrModelBone _boneSpine;
		private readonly DrModelBone _boneHand;

		/// <summary>Lower body runs, upper body draws the sword.</summary>
		private readonly AnimationBlendNode _runDrawAnimation;
		/// <summary>Lower body runs with greatsword, upper body sheathes the sword.</summary>
		private readonly AnimationBlendNode _runSheathAnimation;
		/// <summary>Lower body runs with greatsword, upper body slashes.</summary>
		private readonly AnimationBlendNode _runSlashAnimation;

		private readonly TimeSpan _jumpStartDuration;

		private AnimationState _animationState = AnimationState.Idle;
		private WeaponState _weaponState = WeaponState.Sheathed;

		/// <summary>Locomotion requested by input, applied once no crossfade is running.</summary>
		private bool _wantsToRun;

		/// <summary>Controller time at which the most recently requested crossfade has finished.</summary>
		private TimeSpan _transitionDeadline;

		/// <summary>Whether the sword is attached to the right hand instead of the spine.</summary>
		private bool _swordInHand;

		private bool _jumpStarted;
		private TimeSpan _jumpElapsed;
		private Vector3 _jumpMovement;

		/// <summary>The scene node driven by this character.</summary>
		public NursiaModelNode ModelNode => _modelNode;

		/// <summary>Indicates whether the character is currently holding a weapon (armed state).</summary>
		public bool WeaponDrawn => _weaponState == WeaponState.Drawn;

		/// <summary>Initializes character model, animations, and weapon attachment.</summary>
		public Character(NursiaModelNode characterModelNode, NursiaModelNode swordModelNode)
		{
			if (characterModelNode == null)
			{
				throw new ArgumentNullException(nameof(characterModelNode));
			}

			if (swordModelNode == null)
			{
				throw new ArgumentNullException(nameof(swordModelNode));
			}

			_modelNode = characterModelNode;

			_weaponAttachment = ModelBoneAttachment.CreateFromModelNode(swordModelNode);
			_modelNode.BonesAttachments.Add(_weaponAttachment);

			_player = new AnimationController(_modelNode.ModelInstance);
			_player.StartClip("Idle", AnimationFlags.Looped);
			_modelNode.Translation = new Vector3(0, DefaultY, 0);

			var characterModel = _modelNode.Model;

			// Sword attachment points: spine while sheathed, right hand once drawn.
			_boneSpine = characterModel.FindBoneByName("mixamorig:Spine");
			_boneHand = characterModel.FindBoneByName("mixamorig:RightHand");

			// Requires the attachment bones, so it has to run after they are resolved.
			SetSheathedTransform();

			// Complementary bone filters so weapon actions can drive the upper body
			// while locomotion keeps driving the lower body.
			var topFilter = characterModel.CreateBoneFilter("mixamorig:Spine");
			var bottomFilter = characterModel.CreateInverseBoneFilter(topFilter);

			_runDrawAnimation = new AnimationBlendNode();
			_runDrawAnimation.AddLayer(characterModel.Animations["Run"], AnimationFlags.Looped).BoneFilter = bottomFilter;
			_runDrawAnimation.AddLayer(characterModel.Animations["DrawGreatSword"]).BoneFilter = topFilter;

			_runSheathAnimation = new AnimationBlendNode();
			_runSheathAnimation.AddLayer(characterModel.Animations["RunGreatSword"], AnimationFlags.Looped).BoneFilter = bottomFilter;
			_runSheathAnimation.AddLayer(characterModel.Animations["DrawGreatSword"], AnimationFlags.PlayBackwards).BoneFilter = topFilter;

			_runSlashAnimation = new AnimationBlendNode();
			_runSlashAnimation.AddLayer(characterModel.Animations["RunGreatSword"], AnimationFlags.Looped).BoneFilter = bottomFilter;
			_runSlashAnimation.AddLayer(characterModel.Animations["SlashGreatSword"]).BoneFilter = topFilter;

			// Length of the takeoff clip, used to avoid cutting it short. The controller's
			// HasFinished cannot be used here: AnimationController.Time clamps to the clip
			// duration but skips the assignment when the clamped value is within epsilon of
			// the current one, which can leave HasFinished stuck false indefinitely.
			_jumpStartDuration = characterModel.Animations["JumpStart"].Duration;
		}

		private void SetSheathedTransform()
		{
			_swordInHand = false;
			_weaponAttachment.Bone = _boneSpine;

			var transform = new SrtTransform
			{
				Translation = new Vector3(-12f, 0, -20f),
				Scale = new Vector3(16),
				Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(180.0f))
			};
			_weaponAttachment.Transform = transform.ToMatrix();
		}

		private void SetDrawnTransform()
		{
			_swordInHand = true;
			_weaponAttachment.Bone = _boneHand;

			var transform = new SrtTransform
			{
				Translation = new Vector3(50f, 0f, 0f),
				Scale = new Vector3(16),
				Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(270.0f))
			};
			_weaponAttachment.Transform = transform.ToMatrix();
		}

		private void Transition(string clip, AnimationFlags flags = AnimationFlags.None)
		{
			_player.CrossfadeToClip(clip, AnimationCrossfadeDelay, flags);
			TrackTransition();
		}

		private void Transition(AnimationTreeNode node)
		{
			_player.CrossfadeToClip(node, AnimationCrossfadeDelay);
			TrackTransition();
		}

		private void TrackTransition()
		{
			// AnimationController restarts Time on every crossfade that does not keep the
			// current time, so the deadline can be expressed in controller time either way.
			_transitionDeadline = _player.Time + AnimationCrossfadeDelay;
		}

		/// <summary>Indicates whether a crossfade requested earlier is still blending.</summary>
		private bool TransitionInFlight => _player.Time < _transitionDeadline;

		/// <summary>
		/// Applies the requested locomotion as soon as the running crossfade has settled.
		/// Requests that arrive mid-blend are deferred instead of restarting it, so a
		/// flickering movement key cannot pin the blend open and starve one-shot weapon
		/// animations of the HasFinished signal they rely on to advance.
		/// </summary>
		private void ApplyLocomotion()
		{
			if (_jumpStarted || TransitionInFlight)
			{
				return;
			}

			if (_wantsToRun)
			{
				AnimateRunning();
			}
			else
			{
				AnimateIdle();
			}
		}

		/// <summary>Transitions to the idle clip set matching the current weapon state.</summary>
		private void AnimateIdle()
		{
			if (_animationState == AnimationState.Idle)
			{
				return;
			}

			switch (_weaponState)
			{
				case WeaponState.Sheathed:
					Transition("Idle", AnimationFlags.Looped);
					break;

				case WeaponState.Drawing:
					// Resume the draw where it was interrupted instead of restarting it.
					Transition("DrawGreatSword", AnimationFlags.KeepTime);
					break;

				case WeaponState.Drawn:
					Transition("IdleGreatSword", AnimationFlags.Looped);
					break;

				case WeaponState.Sheathing:
					Transition("DrawGreatSword", AnimationFlags.PlayBackwards | AnimationFlags.KeepTime);
					break;

				case WeaponState.Slashing:
					Transition("SlashGreatSword", AnimationFlags.KeepTime);
					break;
			}

			_animationState = AnimationState.Idle;
		}

		/// <summary>Transitions to the running clip set matching the current weapon state.</summary>
		private void AnimateRunning()
		{
			if (_animationState == AnimationState.Running)
			{
				return;
			}

			switch (_weaponState)
			{
				case WeaponState.Sheathed:
					Transition("Run", AnimationFlags.Looped);
					break;

				case WeaponState.Drawing:
					// The controller time restarts on every crossfade, so the upper body
					// layer has to be seeded with it to keep the draw running in sync.
					_runDrawAnimation.Layers[1].TimeOffset = _player.Time;
					Transition(_runDrawAnimation);
					break;

				case WeaponState.Drawn:
					Transition("RunGreatSword", AnimationFlags.Looped);
					break;

				case WeaponState.Sheathing:
					_runSheathAnimation.Layers[1].TimeOffset = _player.Time;
					Transition(_runSheathAnimation);
					break;

				case WeaponState.Slashing:
					_runSlashAnimation.Layers[1].TimeOffset = _player.Time;
					Transition(_runSlashAnimation);
					break;
			}

			_animationState = AnimationState.Running;
		}

		/// <summary>Transitions the weapon state, keeping the lower body on the current locomotion clip.</summary>
		private void AnimateWeapon(WeaponState newWeaponState)
		{
			if (_weaponState == newWeaponState)
			{
				return;
			}

			if (_animationState == AnimationState.Running)
			{
				// Running: the lower body keeps looping, so every weapon action is a blend node.
				switch (newWeaponState)
				{
					case WeaponState.Sheathed:
						Transition("Run", AnimationFlags.Looped);
						break;

					case WeaponState.Drawing:
						// A freshly requested action always starts from the beginning.
						_runDrawAnimation.Layers[1].TimeOffset = TimeSpan.Zero;
						Transition(_runDrawAnimation);
						break;

					case WeaponState.Drawn:
						Transition("RunGreatSword", AnimationFlags.Looped);
						break;

					case WeaponState.Sheathing:
						_runSheathAnimation.Layers[1].TimeOffset = TimeSpan.Zero;
						Transition(_runSheathAnimation);
						break;

					case WeaponState.Slashing:
						_runSlashAnimation.Layers[1].TimeOffset = TimeSpan.Zero;
						Transition(_runSlashAnimation);
						break;
				}
			}
			else
			{
				// Standing: the weapon clip owns the whole body, no blending needed.
				switch (newWeaponState)
				{
					case WeaponState.Sheathed:
						Transition("Idle", AnimationFlags.Looped);
						break;

					case WeaponState.Drawing:
						Transition("DrawGreatSword");
						break;

					case WeaponState.Drawn:
						Transition("IdleGreatSword", AnimationFlags.Looped);
						break;

					case WeaponState.Sheathing:
						Transition("DrawGreatSword", AnimationFlags.PlayBackwards);
						break;

					case WeaponState.Slashing:
						Transition("SlashGreatSword");
						break;
				}
			}

			_weaponState = newWeaponState;
		}

		/// <summary>Transitions to idle state. Ignored while airborne.</summary>
		public void Idle()
		{
			if (_jumpStarted)
			{
				return;
			}

			_wantsToRun = false;
			ApplyLocomotion();
		}

		/// <summary>Applies movement velocity and transitions to running state. Ignored while airborne.</summary>
		public void Run(Vector3 velocity)
		{
			if (_jumpStarted)
			{
				return;
			}

			_modelNode.Translation += velocity;
			_wantsToRun = true;
			ApplyLocomotion();
		}

		/// <summary>Draws the weapon. Only allowed from <see cref="WeaponState.Sheathed"/>.</summary>
		public void DrawWeapon()
		{
			if (_jumpStarted || _weaponState != WeaponState.Sheathed)
			{
				return;
			}

			AnimateWeapon(WeaponState.Drawing);
		}

		/// <summary>Sheathes the weapon by playing the draw animation backwards. Only allowed from <see cref="WeaponState.Drawn"/>.</summary>
		public void SheathWeapon()
		{
			if (_jumpStarted || _weaponState != WeaponState.Drawn)
			{
				return;
			}

			AnimateWeapon(WeaponState.Sheathing);
		}

		/// <summary>Performs a slash attack (requires weapon drawn).</summary>
		public void Slash()
		{
			if (_jumpStarted || _weaponState != WeaponState.Drawn)
			{
				return;
			}

			AnimateWeapon(WeaponState.Slashing);
		}

		/// <summary>Initiates a jump, preserving the current horizontal momentum. Ignored while airborne or while a weapon action is playing.</summary>
		public void Jump(Vector3 velocity)
		{
			if (_jumpStarted || (_weaponState != WeaponState.Sheathed && _weaponState != WeaponState.Drawn))
			{
				return;
			}

			_jumpStarted = true;
			_jumpElapsed = TimeSpan.Zero;
			_jumpMovement = new Vector3(velocity.X, 0.0f, velocity.Z);
			_animationState = AnimationState.Jumping;
			Transition("JumpStart");
		}

		/// <summary>Integrates the jump arc and drives the takeoff to landing transition.</summary>
		private void UpdateJump(TimeSpan elapsed)
		{
			_jumpElapsed += elapsed;

			var t = (float)_jumpElapsed.TotalSeconds;

			// Kinematic equation: h = v0 * t - 0.5 * g * t^2, v = v0 - g * t
			var jumpHeight = JumpForce * t - (0.5f * Gravity * t * t);
			var jumpVelocity = JumpForce - Gravity * t;

			_modelNode.Translation = new Vector3(
				_modelNode.Translation.X + _jumpMovement.X,
				jumpHeight,
				_modelNode.Translation.Z + _jumpMovement.Z);

			// Switch to the landing clip once the apex is behind us, but never before
			// the takeoff clip had a chance to play through.
			if (_animationState == AnimationState.Jumping && jumpVelocity < 0 && jumpHeight < JumpEndHeight && _jumpElapsed >= _jumpStartDuration)
			{
				Transition("JumpEnd");
				_animationState = AnimationState.Landing;
			}

			if (jumpHeight <= DefaultY)
			{
				_modelNode.Translation = new Vector3(_modelNode.Translation.X, DefaultY, _modelNode.Translation.Z);
				_jumpStarted = false;
			}
		}

		/// <summary>Advances one-shot weapon animations once they finish playing.</summary>
		private void UpdateAnimations()
		{
			switch (_weaponState)
			{
				case WeaponState.Drawing:
					if (_player.HasFinished)
					{
						AnimateWeapon(WeaponState.Drawn);
					}
					else if (!_swordInHand && _player.Time >= _player.RootNode.Duration / 3)
					{
						// The hand crosses the hilt roughly a third into the draw clip.
						SetDrawnTransform();
					}
					break;

				case WeaponState.Sheathing:
					if (_player.HasFinished)
					{
						SetSheathedTransform();
						AnimateWeapon(WeaponState.Sheathed);
					}
					break;

				case WeaponState.Slashing:
					if (_player.HasFinished)
					{
						AnimateWeapon(WeaponState.Drawn);
					}
					break;
			}
		}

		/// <summary>Updates jump physics, weapon state transitions and the animation controller. Call once per frame.</summary>
		public void Update(TimeSpan elapsed)
		{
			if (_jumpStarted)
			{
				UpdateJump(elapsed);
			}

			UpdateAnimations();

			_player.Update(elapsed);
		}
	}
}