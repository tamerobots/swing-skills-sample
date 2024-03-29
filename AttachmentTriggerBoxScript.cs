using UnityEngine;
using System.Collections;

public class AttachmentTriggerBoxScript : MonoBehaviour {
	private GameObject GameEventManagerObject;
	private GameEventManager GameEventManager;
	private GameObject mainSceneObject;
	private mainSceneScript mainSceneScript;
	private bool alreadyTriggered = false;
	public enum AttachmentTriggerBoxType{
		sequenceBox,
		RopeTargetObject
	}
	public AttachmentTriggerBoxType ATBType;
	private GameObject ATBHolderObject;
	private bool showingColour1 = false;
	public Color colorToFlash;
	private Color originalColor;
	private GameObject ATBParticleSystemGameObject;

	/* HOW ATBs work --------------------------------------------------------------------------
	 * Attachment Trigger Boxes (ATBs) are object that, when you throw a rope at them, they 'trigger' an action on attachment of the rope. 
	 * The rope slinging targeting auto-aim code is designed to favour ATBs above anything else to encourage their use.
	 * You can use them to essentially be buttons - so the player sees an ATB, they throw a rope to it, and it raises a drawbridge or something.
	 * There are prefabs in Assets/NonQubicleAssets/ATBsObstaclesEtc for you to use.
	 * There are several components to how an ATB works.
	 * 1. The AttachmentTriggerBox
	 * 2. The AttachmentTriggerBoxHolder.
	 * 3. The AttachmentTriggerBoxTarget	 
	 * 
	 * 1. The AttachmentTriggerBox
	 * This can be either: a ropeTargetobject (i.e. the astronaut's satellite, an object you sling to basically in order to launch from a position). in this case you do not need a ATBHolder, you can just attach
	 * the AttachmentTriggerBoxScript to an object, set the ATB type on the script to ropeTargetObject and you're good to go.
	 *                     a sequenceBox, which means that you attach a rope to 1 OR MORE boxes in order to trigger a reaction in the ATBHolderObject, which will then make its child ATBTargets react in an
	 * 	individually specified way (i.e. move this obejct 50 above the scene after 1.1s, move this object 3 to the right after 0.8s, etc.
	 * 
	 * 2. The AttachmentTriggerBoxHolder.
	 * If it's a sequenceBox type ATB, then you put the AttachmentTriggerBox(es) as children of an ATBHolder object. This object should be set to position 0,0,0, scale 1,1,1. It holds all the ATBS and ATBTargets.
	 * It is very important that if you're doing sequenceBoxes (even if its just 1!) that they are children of an ATBHolder.
	 * 
	 * 3. The AttachmentTriggerBoxTarget	 
	 * This is the objects that change as a result of you hitting all the ATB(s), for example barriers that raise or coins that move into shot. You MUST tag each ATBTarget as 'AttachmentTriggerBoxTarget'. You must also 
	 * attach the AttachmentTriggerBoxTargetScript to them, and thus specify how they will move when the ATBs are all hit.
	 * The ATBHolder will make all the ATBTargets react if all the ATBs are hit.
	 * For example, in order to have a 3 button setup where you must press them in order to raise a barrier in the way of the player and show some coins, you could do the following:
	 * make an empty 'ATBHolder' object, attach the AttachmentTriggerBoxHolderScript to the ATBHolder object
	 * Set the number of 'Target Triggered Number' to 3.
	 * make a 3D cube barrier, call it 'BarrierCube' and tag it as AttachmentTriggerBoxTarget
	 * Set the Target Position to 0,-50,0 (which will drop the 3d cube barrier 50 downwards)	 
	 * Set the delay to 0.8 seconds or so (how long will it wait before moving. This should usually be 0.8 to remain in the 'feels in response to your action' area(?) xTODO: check this is right.
	 * Drag in the 'CoinHolder' prefab from Assets\NonQubicleAssets\ATBsObstaclesEtc
	 * Place the 'CoinHolder' object in the scene where you want the coin to appear (it works by having the coin below the scene, then on activation moving the coin up by however much, so place the coinholder where
	 * you want the coin to appear).
	 * 
	 * 
	 * Checks: 
	 * * ATBs should really be a child of 'powerupsmysteryitems' in the level, but they dont have to be.
	 * * EVERYTHING to do with the ATB should be a child of the ATBHolder object (if its a sequenceBox), including any coins or any course parts that move.
	 * * DONT move the ATBHolder into the level, it should just be at 0,0,0.
	 * 
	 * Troubleshooting:
	 * * Make sure all objects are tagged appropriately
	 * * Make sure that you have set the target triggered number in the ATBHolder object
	 * * Coins need to be under a parent holder object ('coinHolder') because otherwise they can't be tagged as ATBTargetObject because they already have the tag of 'hittableByRope'.
	 * * Place CoinHolders where you want the Coin to appear.
	 * * You can apply the 'CoreLevelDontOverride' layer to the ATBs and to anything.
	 * */

	void Start(){
		GetGameReferences ();
		GameEventManager.GetGameReferences += GetGameReferences;
		if (ATBType == AttachmentTriggerBoxType.sequenceBox && ATBHolderObject.GetComponent<AttachmentTriggerBoxHolderScript> () == null) {
			Debug.LogError ("TR ATB "+ATBHolderObject.name+" target object does not have an AttachmentTriggerBoxHolderScript attached TR");
		}
		if (ATBType == AttachmentTriggerBoxType.sequenceBox) {
			InvokeRepeating ("changeColours", 0.1f, 0.4F);
			originalColor = this.GetComponent<MeshRenderer> ().material.color;
		}
	}

	void GetGameReferences(){
		if (this != null) {
			GameEventManagerObject = GameObject.Find ("GameEventManager");
			GameEventManager = GameEventManagerObject.GetComponent<GameEventManager> ();
			mainSceneObject = GameObject.Find ("mainSceneObject");
			mainSceneScript = mainSceneObject.GetComponent<mainSceneScript> ();
			ATBHolderObject = this.transform.parent.gameObject;
			if (ATBType == AttachmentTriggerBoxType.sequenceBox){
				ATBParticleSystemGameObject = GameObject.Find ("ATBParticleSystem");
				}
			if (ATBType == AttachmentTriggerBoxType.sequenceBox &&  ATBHolderObject == null) {
				Debug.LogError ("TR ATB "+this.name+" is not a child of an ATB Target Object.");
			}
		}
	}

	void OnEnable() {
		GetGameReferences ();
		alreadyTriggered = false;
	}

	private void changeColours(){
		if (!showingColour1) {
			this.GetComponent<MeshRenderer> ().material.color = colorToFlash;
			showingColour1 = true;
		} else {
			this.GetComponent<MeshRenderer> ().material.color = originalColor;
			showingColour1 = false;
		}
	}


	public void triggerBox() {		
		if (!alreadyTriggered) {
			alreadyTriggered = true;
			if (ATBType == AttachmentTriggerBoxType.RopeTargetObject) {
				
				moveUpAndDown moveUpAndDown = this.gameObject.GetComponent<moveUpAndDown> ();
				if (moveUpAndDown != null) {
					moveUpAndDown.frozen = true;
				}
				BoxCollider RopeTargetObjectBoxCollider = this.gameObject.GetComponent<BoxCollider> ();
				if (RopeTargetObjectBoxCollider != null) {
					RopeTargetObjectBoxCollider.enabled = false;

				}
				StartCoroutine (reenableATBAfterTime ());
			} else {
				Debug.Log ("target object getting script");
				ATBHolderObject.GetComponent<AttachmentTriggerBoxHolderScript> ().IncrementCurrentTriggeredNumber();
				StartCoroutine (animateTriggered ());
			}
		}
	}

	IEnumerator reenableATBAfterTime(){

		yield return new WaitForSeconds (6f);
		alreadyTriggered = false;
		moveUpAndDown moveUpAndDown = this.gameObject.GetComponent<moveUpAndDown> ();
		if (moveUpAndDown != null) {
			moveUpAndDown.frozen = false;
		}
		BoxCollider RopeTargetObjectBoxCollider = this.gameObject.GetComponent<BoxCollider> ();
		if (RopeTargetObjectBoxCollider != null) {
			RopeTargetObjectBoxCollider.enabled = true;
		}

		yield return null;
	}

	IEnumerator animateTriggered(){
		//Debug.Log ("animationg");
		yield return new WaitForSeconds (0.5f); //this delay allows the rope to get to it, works at any distance.
		CancelInvoke ("changeColours");
		this.GetComponent<MeshRenderer> ().material.color = colorToFlash;
		this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 0.5f, this.transform.position.z); //This here for debug to demonstrate trigger working
		ATBParticleSystemGameObject.transform.position = this.transform.position;
		//emit some particles
		ATBParticleSystemGameObject.GetComponent<ParticleSystem> ().Play ();
		yield return null;
	}
}
