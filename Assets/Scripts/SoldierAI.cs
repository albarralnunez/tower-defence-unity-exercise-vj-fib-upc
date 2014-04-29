﻿using UnityEngine;
using System.Collections;
using Pathfinding.RVO;
using System;

namespace Pathfinding {
	[RequireComponent(typeof(Seeker))]
	public class SoldierAI : AIPath {

		/** Animation component.
		 * Should hold animations "awake" and "forward"
		 */
		public Animation anim;
		public float distanceAlert = 200.0F;
		/** Minimum velocity for moving */
		public float sleepVelocity = 0.4F;
		
		/** Speed relative to velocity with which to play animations */
		public float animationSpeed = 0.2F;
		
		/** Effect which will be instantiated when end of path is reached.
		 * \see OnTargetReached */
		public GameObject endOfPathEffect;

		private GameObject busy = GameObject.Find ("S");

		public new void Start () {
			
			//Prioritize the walking animation
			anim["forward"].layer = 10;
			
			//Play all animations
			anim.Play ("awake");
			anim.Play ("forward");
			
			//Setup awake animations properties
			anim["awake"].wrapMode = WrapMode.Clamp;
			anim["awake"].speed = 0;
			anim["awake"].normalizedTime = 1F;
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
		}	

		public override Vector3 GetFeetPosition ()
		{
			return tr.position;
		}
		private GameObject GetNearestTaggedObject(){
			// and finally the actual process for finding the nearest object:
			
			float nearestDistanceSqr = Mathf.Infinity;
			GameObject[] taggedGameObjects = GameObject.FindGameObjectsWithTag("Enemy"); 
			GameObject nearestObj = null;
			
			// loop through each tagged object, remembering nearest one found
			foreach (GameObject obj in taggedGameObjects) {
				Vector3 objectPos = obj.transform.position;
				float distanceSqr = (objectPos - transform.position).sqrMagnitude;
				Toolbox toolbox = Toolbox.Instance;
				bool exists =  toolbox.EnemyBusy.Contains(obj.GetInstanceID());
				if (distanceSqr < nearestDistanceSqr && distanceSqr <= distanceAlert && !exists) {
					nearestObj = obj;
					nearestDistanceSqr = distanceSqr;
					toolbox.EnemyBusy.Add(obj.GetInstanceID());
					fighting = true;
				}
			}
			return nearestObj;
		}
		private bool fighting = false;
		private GameObject targetObj;
		protected new void Update () {
			if (target != null) {
				float distanceSqr = (target.position - transform.position).sqrMagnitude;
				if (distanceSqr < 15) Destroy (gameObject);
			}

			if (!fighting) targetObj = GetNearestTaggedObject ();
			else target = targetObj.transform;

			//Get velocity in world-space
			Vector3 velocity;
			if (canMove) {
			
				//Calculate desired velocity
				Vector3 dir = CalculateVelocity (GetFeetPosition());

				//Rotate towards targetDirection (filled in by CalculateVelocity)
				RotateTowards (targetDirection);
				
				dir.y = 0;
				if (dir.sqrMagnitude > sleepVelocity*sleepVelocity) {
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
			
			if (velocity.sqrMagnitude <= sleepVelocity*sleepVelocity) {
				//Fade out walking animation
				anim.Blend ("forward",0,0.2F);
			} else {
				//Fade in walking animation
				anim.Blend ("forward",1,0.2F);
				
				//Modify animation speed to match velocity
				AnimationState state = anim["forward"];
				
				float speed = relVelocity.z;
				state.speed = speed*animationSpeed;
			}
		}
	}
}