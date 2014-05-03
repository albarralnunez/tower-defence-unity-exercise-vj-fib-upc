using UnityEngine;
using System.Collections;
using Pathfinding.RVO;

namespace Pathfinding {
	/** AI controller specifically made for the spider robot.
	 * The spider robot (or mine-bot) which is got from the Unity Example Project
	 * can have this script attached to be able to pathfind around with animations working properly.\n
	 * This script should be attached to a parent GameObject however since the original bot has Z+ as up.
	 * This component requires Z+ to be forward and Y+ to be up.\n
	 * 
	 * It overrides the AIPath class, see that class's documentation for more information on most variables.\n
	 * Animation is handled by this component. The Animation component refered to in #anim should have animations named "awake" and "forward".
	 * The forward animation will have it's speed modified by the velocity and scaled by #animationSpeed to adjust it to look good.
	 * The awake animation will only be sampled at the end frame and will not play.\n
	 * When the end of path is reached, if the #endOfPathEffect is not null, it will be instantiated at the current position. However a check will be
	 * done so that it won't spawn effects too close to the previous spawn-point.
	 * \shadowimage{mine-bot.png}
	 * 
	 * \note This script assumes Y is up and that character movement is mostly on the XZ plane.
	 */
	[RequireComponent(typeof(Seeker))]
	public class MineBotAI : AIPath {

		enum States {fight,walk};

		/** Animation component.
		 * Should hold animations "awake" and "forward"
		 */
		public Animation anim;
		public float distanceAlert = 20.0F;
		/** Minimum velocity for moving */
		public float sleepVelocity = 0.4F;
		
		/** Speed relative to velocity with which to play animations */
		public float animationSpeed = 0.2F;
		
		/** Effect which will be instantiated when end of path is reached.
		 * \see OnTargetReached */
		public GameObject endOfPathEffect;
		public int statep;

		public void SetToFight() {
			statep = (int)States.fight;
		}

		public new void Start () {

			statep = (int)States.walk;

			//Prioritize the walking animation
			anim["forward"].layer = 10;
			
			//Play all animations
			anim.Play ("awake");
			anim.Play ("forward");
			
			//Setup awake animations properties
			anim["awake"].wrapMode = WrapMode.Clamp;
			anim["awake"].speed = 0;
			anim["awake"].normalizedTime = 1F;
			target = GameObject.Find("Target").transform;
			//Call Start in base script (AIPath)
			base.Start ();
		}
		
		/** Point for the last spawn of #endOfPathEffect */
		protected Vector3 lastTarget;
		
		/**
		 * Called when the end of path has been reached.
		 * An effect (#endOfPathEffect) is spawned when this function is called
		 * However, since paths are recalculated quite often, we only spawn the effect
		 * when the current position is some distance away from the previous spawn-point
		*/
		public override void OnTargetReached () {
			
			if (endOfPathEffect != null && Vector3.Distance (tr.position, lastTarget) > 1) {
				GameObject.Instantiate (endOfPathEffect,tr.position,tr.rotation);
				lastTarget = tr.position;
			}
			Toolbox toolbox = Toolbox.Instance;
			toolbox.EnemyBusy.Remove (gameObject.GetInstanceID ());
			Destroy (gameObject);
		}	

		public override Vector3 GetFeetPosition ()
		{
			return tr.position;	
		}

		private Transform GetNearestTaggedObject(){
			// and finally the actual process for finding the nearest object:
			
			float nearestDistanceSqr = Mathf.Infinity;
			GameObject[] taggedGameObjects = GameObject.FindGameObjectsWithTag("Ally"); 
			Transform nearestObj = null;
			
			// loop through each tagged object, remembering nearest one found
			foreach (GameObject obj in taggedGameObjects) {
				Vector3 objectPos = obj.transform.position;
				float distanceSqr = (objectPos - transform.position).sqrMagnitude;
				//get Ally state fighting
				//Pathfinding.SoldierAI comp = obj.GetComponent<Pathfinding.SoldierAI>();
 				if (distanceSqr < nearestDistanceSqr && distanceSqr <= distanceAlert){
					nearestObj = obj.transform;
					nearestDistanceSqr = distanceSqr;
				}
			}
			if (nearestObj == null) {
				return GameObject.Find("Target").transform;
			}
			return nearestObj;
		}
		int asdf = 0;
		protected new void Update () {
			if (statep == (int)States.walk) {
				target = GameObject.Find("Target").transform;
				//Get velocity in world-space
				Vector3 velocity;
				if (canMove) {
				
					//Calculate desired velocity
					Vector3 dir = CalculateVelocity (GetFeetPosition ());

					//Rotate towards targetDirection (filled in by CalculateVelocity)
					RotateTowards (targetDirection);

					dir.y = 0;
					if (dir.sqrMagnitude > sleepVelocity * sleepVelocity) {
							//If the velocity is large enough, move
					} else {
							//Otherwise, just stand still (this ensures gravity is applied)
							dir = Vector3.zero;
					}

					if (navController != null) {
					} else if (controller != null)
							controller.SimpleMove (dir);
					else
							Debug.LogWarning ("No NavmeshController or CharacterController attached to GameObject");

					velocity = controller.velocity;
					} else {
							velocity = Vector3.zero;
					}
					//Animation

					//Calculate the velocity relative to this transform's orientation
					Vector3 relVelocity = tr.InverseTransformDirection (velocity);
					relVelocity.y = 0;

					if (velocity.sqrMagnitude <= sleepVelocity * sleepVelocity) {
							//Fade out walking animation
							anim.Blend ("forward", 0, 0.2F);
					} else {
							//Fade in walking animation
							anim.Blend ("forward", 1, 0.2F);

							//Modify animation speed to match velocity
							AnimationState state = anim ["forward"];

							float speed = relVelocity.z;
							state.speed = speed * animationSpeed;
					}

				} else if (statep == (int)States.fight) {
					++asdf;
					if (asdf == 600) {
						Toolbox toolbox = Toolbox.Instance;
						toolbox.EnemyBusy.Remove (gameObject.GetInstanceID ());
						Destroy(gameObject);
						
					}
				}
		}
	}
}