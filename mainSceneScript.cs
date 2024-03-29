using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEngine.Analytics;
using UnityEngine.Analytics.Experimental;
// NOTE ---------------------------
// This class is the main controller script for the scene so has been set in the script execution order as first (so -100).

public class mainSceneScript : MonoBehaviour {
	private GameObject PersistentGameObject;
	private PersistentGameObjectScript PersistentGameObjectScript;
	private persistedPlayerDataController persistedPlayerDataController;
	private ropeGameConstants ropeGameConstants;
	private runtimeVariables persistentGOruntimeVars;
	private GameObject CurrentPlayer;
	private GameObject CharacterRoot;
	private GameObject CharacterPelvis;
	private bool haveSetPlayerOffset = false;
	private GameObject MainCamera;
	private CameraController CameraControllerScript;
	public int livesLeft;
	private CharacterInformationScript CharacterInformationScript;
	private float startTime;
	public float elapsedTime;
	private GameObject DebugManager;
	private DebugScript DebugScript;
	private GameObject deadCharacterClone;
	public bool killPlayerSubroutineAlreadyRunning;
	private GameObject LowerGuidelineCube;
	private GameObject PlayerCameraMarker;
	private GameObject GameEventManagerObject;
	private GameEventManager GameEventManager;
	private GameObject LevelManager;
	private LevelManagerScript LevelManagerScript;
	public Vector3 lastCheckpointPosition;
	public bool atLeastOneCheckpointSet = false;
	private GameObject GUIManagerObject;
	private GUIManager GUIManager;
	public float distanceTraveled;
	public int coinsCollected;
	private Rigidbody runnerRB;
	private Rigidbody pelvisRB;
	private Runner RunnerScript;
	private SwingController SwingControllerScript;
	private GameObject CurrentPlayerOriginal;
	private GameObject BackgroundManager;
	private BackgroundManagerScript BackgroundManagerScript;
	private GameObject ShatterCharacterCube;
	private GameObject[] shatterCubeArray;
	private int shatterCubeArrayIndex;
	private GameObject OffscreenMarkerBottom;
	private GameObject OffscreenMarkerTop;
	private MeshRenderer OffscreenMarkerBottomCube1MeshRenderer;
	private MeshRenderer OffscreenMarkerBottomCube2MeshRenderer;
	private MeshRenderer OffscreenMarkerTopCube1MeshRenderer;
	private MeshRenderer OffscreenMarkerTopCube2MeshRenderer;
	public bool AllowMainGameInput = true;
	public bool showTutorial = true;
	public bool charactersMysteryItemCollected = false;
	public int currentRopeOffset;
	public int totalRopesThrown;
	private float ropeThrowTimer;
	private float ropeThrowTimerDifference;
	public int ropesThrownInLastFewSeconds;
	public GameObject TestObjects;
	public bool recordPlayerData = true; // This could be set to false, for example at the end when you have finished the game but are watching the death animation before the scoring screen shows.
	public float PlayerCameraMarkerOffset; // DO NOT SET A DEFAULT FOR THIS! It is set it in the inspector, or programmatically at run time. It can be increased to make it so that the 
										   //player is more left of the camera and can SEE MORE COMING, and decreased to make it so that it is more right of the camera 
										  // to give them more time to get around obstacles etc. 7 is an ok baseline for learning characters. 11 perhaps for fast/difficult characters.
	private bool firstFrameRendered = false;
	private GameObject MasterAudioGO;
	private bool hasBeenOfferedRevive = false;
	public bool revivePanelShowing = false;
	private bool dealtWithCountdownEnd = false;
	private bool stillWaitingForLevelsToLoad = true;
	private bool stillWaitingForFirstRopeThrow = true;
	private bool needToHideIconsOnStart = false;
	private bool gameFinished = false;
	public GameObject camTutorialFinger;
	private bool camTutorialFingerAlreadyDisabled = false;
	public GameObject ParticleSystems;
	public GameObject RunnerBumpCollisionParticleSystem;
	public Material RunnerBumpCollisionParticleSystemMaterial;

	void Awake(){ // this has to be in awake or it breaks references on the first call of Update() in other scripts ??
		//Take chosen character from persisted data, find it in scene, move it to 0,0,0 and give it CurrentPlayerOriginal tag. remove other characters from scene.
		Application.targetFrameRate = 30;
		PersistentGameObject = GameObject.FindGameObjectWithTag("PersistentGameObject");
		DebugManager = GameObject.Find ("DebugManager");
		DebugScript = DebugManager.GetComponent<DebugScript> ();
		Debug.Log ("starting scene awake");

		if (PersistentGameObject == null) {
			PersistentGameObject = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("PersistentObjects/PersistentGameObject"));
			DontDestroyOnLoad (PersistentGameObject);
		}
		if (PersistentGameObject != null) {
			PersistentGameObjectScript = PersistentGameObject.GetComponent<PersistentGameObjectScript> ();
			ropeGameConstants = PersistentGameObject.GetComponent<ropeGameConstants>();
			persistentGOruntimeVars = PersistentGameObject.GetComponent<runtimeVariables> ();
		}

		Debug.Log ("got persistent object sorted");

		if ( PersistentGameObject != null) { //this could be during debug - you could just be playing and moving around a character while you build a level, in which case you won't have loaded persistent data from going through startup etc. 
			//Get a handle on the persisted data
			persistedPlayerDataController = PersistentGameObject.GetComponent<persistedPlayerDataController> (); 
			// find the empty gameobject containing all the characters in scene
			string requestedCharacterHardName = "";
			requestedCharacterHardName = persistedPlayerDataController.persistedPlayerData.currentlySelectedCharacterHardName;
//			Debug.Log("outputting stored file paths");
//			foreach (playableCharacterFilePath pcfp in ropeGameConstants.playableCharacterFilePaths) {
//				Debug.Log (pcfp.characterGUID+" "+pcfp.characterFilePath);
//			}

			persistentGOruntimeVars.ResetCurrentGameDefaults(); // this just clears the stored data for the current attempt with this character


			if (persistedPlayerDataController.persistedPlayerData.firstRun) {
				Debug.Log ("first run set to false");
				persistedPlayerDataController.persistedPlayerData.firstRun = false;
				persistentGOruntimeVars.currentGameIsFirstRunTutorial = true;
				needToHideIconsOnStart = true;
				AnalyticsExtenderTR.TutorialStart ("TutorialStart");//testedTR
				//persistedPlayerDataController.fileSave (); //fileSave will happen when you end the game anyway, so no need doing it here, too intensive on startup.
			}
			// ***************************************************************************
			string defaultCharHardName = "orangutan"; //Set default character GUID for game here
			if (requestedCharacterHardName == null || requestedCharacterHardName == string.Empty) {				
				requestedCharacterHardName = defaultCharHardName;
			}

			// find any other 'loose' CurrentPlayerOriginals that I may have left out while building and testing levels
			Debug.Log("looking for loose CurrentPlayerOriginals that should not be in scene"); 
			GameObject[] looseCurrentPlayerOriginals = GameObject.FindGameObjectsWithTag("CurrentPlayerOriginal");
			bool existingCurrentPlayerOriginalActiveInScene = false;
			for(var i = 0 ; i < looseCurrentPlayerOriginals.Length ; i ++)
			{
				// tried DELETING them here, but couldn't make it work for some reason - perhaps to do with this being in awake();?
				Debug.LogError ("TR error - there is an existing CurrentPlayerOriginal left in the scene! Remove it to be able to load a character based on persistent data choice! Also try loading from mainMenu to this scene first to get persistent data right. More information on course creation document - Putting a test char in scene.");
				if (looseCurrentPlayerOriginals [i].activeSelf) {
					existingCurrentPlayerOriginalActiveInScene = true;
				}
			}

			//GameObject playableCharacters = GameObject.Find ("playableCharacters");
			if (DebugScript.ensureCharacterBasedOnPersistentData || (DebugScript.ensureCharacterBasedOnPersistentData == false && !existingCurrentPlayerOriginalActiveInScene)) {
				foreach (string playableCharacterHardName in ropeGameConstants.playableCharacterHardNames) {
					if (playableCharacterHardName == requestedCharacterHardName) {		
						

						/* NOTE the following line is kept to demonstrate how NOT to  instantiate prefabs - as this will actually set the prefab to inactive, ------------------------------------------------------------------
					// http://answers.unity3d.com/questions/319427/prefab-loading-via-resources-forced-to-inactive.html
					// so when you next run the program you will be unable to instantiate a prefab! The uncommented line below this shows how to do it correctly. 
					//CurrentPlayerOriginal =  Resources.Load ("RiggedCharacterPrefabs/" + playableCharacterFilePath.characterFilePath, typeof(GameObject)) as GameObject;
					*/
						CurrentPlayerOriginal = (GameObject)GameObject.Instantiate ((GameObject)Resources.Load (("RiggedCharacterPrefabs/" + playableCharacterHardName)));
						break; //exit foreach
					}
				}	
			} else {
				for (var i = 0; i < looseCurrentPlayerOriginals.Length; i++) {
					if (looseCurrentPlayerOriginals [i].activeSelf) {
						CurrentPlayerOriginal = looseCurrentPlayerOriginals [i];
					}
				}
			}
			if (CurrentPlayerOriginal == null) {
				Debug.LogError ("Either there is most likely not an entry in the ropeGameConstants.playableCharacterFilePaths for this character GUID, or you have not set your new characters tag to CurrentPlayerOriginal!");
			}


//			Debug.Log ("outputting names of characters in playablecharacters that are about to be deleted");
//			foreach (Transform playableChar in playableCharacters.transform) {
//				Debug.Log (playableChar.name);
//				//Debug.Log (PersistentGameObject.);
//			}
//			Debug.Log ("setting inactive playable characters inc octopusman");
//			//Destroy (playableCharacters); // xTODO Remove the playable characters, you dont need them any more(?) Depends if when you restart the scene they come back after being destroyed.
//			playableCharacters.SetActive(false);
			//Before you clone the original player, you want to set it to the correct outfit, then when it is cloned in future (on respawns 
			// etc), the new clones will still be in the right outfit.
			////		requestedCharacterHardName
			genericKeyValueStringPairTR foundItem = persistedPlayerDataController.persistedPlayerData.chosenOutfits.Find(m => m.Key == requestedCharacterHardName);
			//persistedPlayerDataController.persistedPlayerData.chosenOutfits.;
			if (foundItem != null && (DebugScript.ensureCharacterBasedOnPersistentData || (DebugScript.ensureCharacterBasedOnPersistentData == false && !existingCurrentPlayerOriginalActiveInScene))) {
				//ensureCharacterBasedOnPersistentData should never be changed, see ensureCharacterBasedOnPersistentData definition
				CurrentPlayerOriginal.applyOutfit (foundItem.Value);
			}
		
			//persistedPlayerDataController.persistedPlayerData.currentlySelectedCharacterGUID

		} else {
			//This should never be reached because you always load persistent data now. *****************************************
			// You're using the Debug Manager's spawnCharacterBasedOnPersistentData setting instead.
			Debug.Log ("Running Scene straight for building, not using persistent data. You should have a player tagged CurrentPlayerOriginal in the scene somewhere in order to be able to play while testing.");
			CurrentPlayerOriginal = GameObject.FindWithTag ("CurrentPlayerOriginal");
			if (CurrentPlayerOriginal == null) {
				Debug.LogError ("TR You did not put a player in the scene! see above!");
			}
			GameObject[] CurrentPlayerOriginals = GameObject.FindGameObjectsWithTag ("CurrentPlayerOriginal");
			if (CurrentPlayerOriginals.Length > 1){
				Debug.LogError ("TR Too many playable characters in scene! should be just one!");
			}
			//********************************************************************************************************************
		}
		if (DebugScript.startAtXZero) {
			Debug.Log("setting to X Zero via debug");
			CurrentPlayerOriginal.transform.position = new Vector3 (0, 8, 0);
		}

		lastCheckpointPosition = CurrentPlayerOriginal.transform.position;

		cloneOriginalPlayer (false, true);


//		//fixes the AnimatorPreview ghost clone issue that you get when you leave Animator window open
//		//https://stackoverflow.com/questions/27980776/gameobject-findgameobjectwithtag-returning-clone
//		Debug.Log ("checking whether to remove clone");
//		GameObject[] remaining = GameObject.FindGameObjectsWithTag("CharacterRoot");
//		foreach (GameObject clone in remaining) {
//			if (clone.transform.parent.parent.name.Contains ("AnimatorPreview")) {
//				Debug.Log ("Destroying AnimatorPreview clone");
//				//Destroy (clone.transform.parent.parent.gameObject);
//				clone.tag = "removedTagTR";
//				int x = 1;
//			}
//		}
//
//		GameObject[] remainingcheck = GameObject.FindGameObjectsWithTag("CharacterRoot");
		// original player cloned, but references not gotten yet
		GetGameReferences ();
		GameEventManager.GetGameReferences += GetGameReferences;
		GameEventManager.GameOver += GameOver;
		GameEventManager.GameFinish += GameFinish;
		GameEventManager.GameExitOrRestart += GameExitOrRestart;
	}


	private void GameExitOrRestart(){
		//this is important so the courses dont stack up between characters.
		//GameObject LevelsBig = GameObject.Find ("Levels");
		//GameObject.Destroy (LevelsBig);
		persistentGOruntimeVars.currentGameCoinsCollected = coinsCollected;
		persistentGOruntimeVars.currentGameCharactersMysteryItemCollected = charactersMysteryItemCollected;
		persistentGOruntimeVars.currentGameDistanceTravelled = distanceTraveled;
		if (CharacterRoot != null) {
			persistentGOruntimeVars.currentGameActualCharacterPositionOnEnd = CharacterRoot.transform.position.x;
		}
		persistentGOruntimeVars.currentGameElapsedTime = elapsedTime;
		persistentGOruntimeVars.currentGameRopesThrown = totalRopesThrown;
		persistedPlayerDataController.fileSave (); 
		//freezes the camera so it won't go forward and kill the player while you wait for scoring screen to load (or main scene to reload), and also freeze player so they can't drop off screen.
		CameraControllerScript.cameraStopped = true;
		//Application.Quit ();
	}

	private void GameOver(){
		recordPlayerData = false;
		AllowMainGameInput = false;
	}

	private void GameFinish(){
		gameFinished = true;
		persistedPlayerDataController.InvokeCaptureScreenshot();
	}

	// Use this for initialization
	void Start () {
		
		GameEventManager.triggerGetGameReferences ();
		GetGameReferences ();
		if (persistedPlayerDataController != null) {
			persistedPlayerDataController.findReferenceToSceneObject ();
		}

		MasterAudio.StopPlaylist ();

		livesLeft = CharacterInformationScript.numberOfLives;
		distanceTraveled = 0f;
		currentRopeOffset = 0;
		totalRopesThrown = 0;
		GUIManager.SetScoreText(distanceTraveled);

		playerOption currentPlayerOption = persistedPlayerDataController.persistedPlayerData.playerOptions.Find(r => r.name == "basicGraphicsEnabledOption");
		if (currentPlayerOption != null) {
			if (currentPlayerOption.enabled) {
				// Hide the particle systems if the basic graphics option is set!
				ParticleSystems.layer = LayerMask.NameToLayer("HideOnLowRes");
				foreach (Transform child in ParticleSystems.transform) {
					child.gameObject.layer = LayerMask.NameToLayer("HideOnLowRes");
				}
				CameraControllerScript.hideLayersOnCameraForLowRes ();
			}
		}

		ShatterCharacterCube = GameObject.Find("ShatterCharacterCube");
		//Sets the shatter cube to the right colour, which is specified in the character information script for each character.
		if (CharacterInformationScript != null) {
			ShatterCharacterCube.GetComponent<MeshRenderer> ().material.color = CharacterInformationScript.shatterCubeColourOnDeath; 
			//RunnerBumpCollisionParticleSystemMaterial.color = CharacterInformationScript.coreLevelColor;
			//RunnerBumpCollisionParticleSystem.GetComponent<ParticleSystemRenderer> ().material = RunnerBumpCollisionParticleSystemMaterial;
		}
		AllowMainGameInput = true; 

		//If the user is starting from a non-zero point it means they're starting from a checkpoint. So don't show the tutorial when they loop back to the start.
		// The tutorial script attached to the tutorial cube objects will detect this boolean and destroy themselves as a result.
		// During debugging you may of course start from a different point - so make it so that it doesn't show if 'pretendStartedFromXZero' is checked
		if (CurrentPlayer.transform.position.x > 0 && !DebugScript.pretendStartedAtXZero) {
			showTutorial = false;
		}
		//startTime = Time.time; //no, this is 
		ropeThrowTimer = Time.time;

		if (!DebugScript.showTestObjects) {
			TestObjects.SetActive (false);
		}


	}

	private void GetGameReferences(){
		if (this != null) {
			MainCamera = GameObject.FindGameObjectWithTag ("MainCamera");
			PlayerCameraMarker = GameObject.FindGameObjectWithTag ("PlayerCameraMarker");
			CameraControllerScript = MainCamera.GetComponent<CameraController> ();
			CurrentPlayer = GameObject.FindWithTag ("CurrentPlayer");
			if (CurrentPlayer != null) { // On Game Over, the Current Player may well be null, so to avoid an exception, have this if statement
				RunnerScript = CurrentPlayer.GetComponent<Runner> ();
				SwingControllerScript = CurrentPlayer.GetComponent<SwingController> ();
				CharacterRoot = CurrentPlayer.findDeepChildWithTag("CharacterRoot");
				CharacterPelvis = CurrentPlayer.findDeepChildWithTag("CharacterPelvis");
				CharacterInformationScript = CurrentPlayer.GetComponent<CharacterInformationScript> ();
			}
			LowerGuidelineCube = GameObject.Find ("LowerGuidelineCube");
			GameEventManagerObject = GameObject.Find ("GameEventManager");
			GameEventManager = GameEventManagerObject.GetComponent<GameEventManager> ();
			GUIManagerObject = GameObject.Find ("GUIManager");
			GUIManager = GUIManagerObject.GetComponent<GUIManager> ();
			BackgroundManager = GameObject.Find ("BackgroundManager");
			BackgroundManagerScript = BackgroundManager.GetComponent<BackgroundManagerScript> ();
			OffscreenMarkerBottom = GameObject.Find ("OffscreenMarkerBottom");
			OffscreenMarkerBottomCube1MeshRenderer = GameObject.Find ("OffscreenMarkerBottomCube1").GetComponent<MeshRenderer> ();
			OffscreenMarkerBottomCube2MeshRenderer = GameObject.Find ("OffscreenMarkerBottomCube2").GetComponent<MeshRenderer> ();
			OffscreenMarkerTop = GameObject.Find ("OffscreenMarkerTop");
			OffscreenMarkerTopCube1MeshRenderer = GameObject.Find ("OffscreenMarkerTopCube1").GetComponent<MeshRenderer> ();
			OffscreenMarkerTopCube2MeshRenderer = GameObject.Find ("OffscreenMarkerTopCube2").GetComponent<MeshRenderer> ();
			LevelManager = GameObject.Find ("LevelManager");
			LevelManagerScript = LevelManager.GetComponent<LevelManagerScript> ();
			MasterAudioGO = GameObject.Find ("MasterAudio");
		}
	}

	void OnEnable() {
		GetGameReferences ();
	}
	 
	// Update is called once per frame
	void Update () {		
		if (!firstFrameRendered) {	
			if (persistedPlayerDataController != null) {
				float loadTimeDifference = Time.realtimeSinceStartup - persistedPlayerDataController.startLoadMainSceneProcessTimeStamp; //yes realtimesince startup is the right thing to use. 
																																		//Read more of this code and you will see that it is used correctly.
																																		//It's being used to measure time from when loading scene calls main scene, to when main scene's first 'update()' is called.
				Debug.Log ("took this long to load main scene:" + loadTimeDifference);
				if (loadTimeDifference > 0 ) {
					persistedPlayerDataController.persistedPlayerData.mainSceneLoadTime = loadTimeDifference;
					//Debug.Log ("remove filesave here");
					//persistedPlayerDataController.fileSave (); // should have this commented as its done at end of play this is just to test loading screen. You can just click 'end run' to achieve the same effect anyway.
				}
			}
			firstFrameRendered = true;
		}

		if (stillWaitingForFirstRopeThrow) {
			//this essentially makes it so that the timer starts ticking when they throw their first rope. This is a lot better than having a countdown 
			// timer on start (i.e. a big 3! 2! 1! on screen, which is too much pressure). It also helps in conjunction with stillWaitingForLevelsToLoad.
			if (totalRopesThrown > 0) {
				startTime = Time.time;
				stillWaitingForFirstRopeThrow = false;
			}
		}

		if (recordPlayerData) { // This could be set to false, for example at the end when you have finished the game but are watching the death animation before the scoring screen shows.
			if (CharacterRoot.transform.position.x > 0 && CharacterRoot.transform.position.x > distanceTraveled) { //if you fall off the left of the course at the start this would show -5 and mess things up!
				distanceTraveled = CharacterRoot.transform.position.x;
			}

			if (camTutorialFingerAlreadyDisabled == false && distanceTraveled > 0 && camTutorialFinger != null) {
				if (persistedPlayerDataController.persistedPlayerData.currentlySelectedCharacterHardName == "orangutan" && distanceTraveled > 170f) { // gets them past first checkpoint
					camTutorialFinger.GetComponent<cameraTutorialFinger> ().hideFinger ();
					camTutorialFingerAlreadyDisabled = true;
				} else if ( persistedPlayerDataController.persistedPlayerData.currentlySelectedCharacterHardName != "orangutan" && distanceTraveled > 80f) {
					camTutorialFinger.GetComponent<cameraTutorialFinger> ().hideFinger ();
					camTutorialFingerAlreadyDisabled = true;
				}
			}
				
			if (stillWaitingForLevelsToLoad || stillWaitingForFirstRopeThrow) {
				elapsedTime = 0f;							
			} else  {
				elapsedTime = Time.time - startTime;
			}
			if (CharacterInformationScript.charScoringType == characterScoringType.timeTrial || CharacterInformationScript.charScoringType == characterScoringType.countdown) { 				
				GUIManager.SetScoreText (elapsedTime);
			} else if (CharacterInformationScript.charScoringType == characterScoringType.numberOfRopesThrown) {
				GUIManager.SetScoreText (totalRopesThrown);
			} else if (CharacterInformationScript.charScoringType == characterScoringType.maxDistance) {
				GUIManager.SetScoreText(distanceTraveled);
			}
			//GUIManager.SetSecondsElapsedText (elapsedTime);
		}

		//quick check every 10 seconds to make sure they're not doing the 'TAP LOADS ALL THE TIME' method, 
		//and if they are, to show them the no finger icon for a few seconds and stop them throwing.
		ropeThrowTimerDifference = Time.time - ropeThrowTimer;
		if (ropeThrowTimerDifference > 5) {
			ropeThrowTimer = Time.time; //reset it first
			//run a check on the number of ropes 
			if (ropesThrownInLastFewSeconds > 20 && distanceTraveled > 100f) { //allow superfast casting at start because a lot of people will be tapping a lot on screen to get char moving right away, because they don't realise the timer only starts when you tap first time, and doesn't just start automatically.
				Debug.Log ("too many throws!");
				GUIManager.startDisableInputForTimePeriodCoRoutine(5f, true);
			}
			ropesThrownInLastFewSeconds = 0;
		}

		// Removed - from days of 'start at 1000m, start at 2000m' stuff.
////		if (!haveSetPlayerOffset && PersistentGameObjectScript != null && PersistentGameObjectScript.CharacterStartOffset > 0) {
//			
//			CurrentPlayer.transform.position = new Vector3 (PersistentGameObjectScript.CharacterStartOffset, CurrentPlayer.transform.position.y, CurrentPlayer.transform.position.z);
//			Debug.Log ("moving player to " + PersistentGameObjectScript.CharacterStartOffset);
//			haveSetPlayerOffset = true;
//		}
	
	}

	public void KillPlayer(int deathSplatterDirection){
		if (!killPlayerSubroutineAlreadyRunning) {			
			StartCoroutine (KillPlayerSubroutine (deathSplatterDirection));
			killPlayerSubroutineAlreadyRunning = true;
		}
	}

	IEnumerator KillPlayerSubroutine(int deathSplatterDirection){
		// ----------------------------NOTES -----------------------------------------------------------------------------------------------------------------------------------		
		// This function should be called by several different scripts. It is not the end of the game necessarily, depending on if the player has lives left etc.
		/// EXPLANATION OF deathSplatterDirection --------------------------------------------------------------------------------------------------------------------------
		//					// 0 is 'death on the left'
		//				//2 is 'death by enemy'
		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------
		// Kill should now work for everwhere the player can die - from hitting an enemy, to falling down off screen, to getting trapped to the left of screen, or hitting an 
		// enemy high in the air. It works by creating a clone of the player, then an explosion just behind this clone, and the explosion start point is moved in a direction it behind the 
		// clone, so that the clone is between the explosion and a point 10m in front of the camera. This means that wherever it goes it throws the body parts towards camera.
		//NOTE: if you want to know what the actual sphere that the explosion to scatter the body parts looks like, use the ExplosionVisualisationSphere:
		// It's set to transform to the place where the explosion is, and mirror its radius. Find it in the Hierarchy and enable its mesh renderer to see it.
		// This is an extremely complex subroutine.
		// --------------------------------------------------------------------------------------------------------------------------------------------------------------------

		//freeze camera		
		CameraControllerScript.cameraStopped = true;	
		//get rid of any rope that's been drawn
		SwingControllerScript.DestroyRope ();

		//return players materials to original in case its been changed
		RunnerScript.EndCharacterColoursFlashing();
		RunnerScript.endLauncherMusic ();
		RunnerScript.LauncherTravelParticleSystem.GetComponent<ParticleSystem> ().Stop(); // Stops little cubes or whatever coming out the player if this was active
		GUIManager.blinkingGUI = false;
		AllowMainGameInput = true;

		//freeze original Player
		runnerRB = CharacterRoot.GetComponent<Rigidbody> ();
		runnerRB.isKinematic = true;
		runnerRB.detectCollisions = false;
		pelvisRB = CharacterRoot.GetComponent<Rigidbody> ();
		pelvisRB.isKinematic = true;
		pelvisRB.detectCollisions = false;
		Debug.Log ("record player data "+recordPlayerData);

		if (deathSplatterDirection == 2 && recordPlayerData){ //This should only be for enemies or on-screen deaths. Otherwise it adds to the delay. Otherwise, if it's after the finish line and youre 
																// not recording player data any more then don't wait either.
			MasterAudio.PlaySound ("bibblipQuickDeathNoise_TR"); //this means a mario-style quick death noise
			yield return new WaitForSeconds (0.5f); // like mario, wait a second  
		}
			
		//clone player
		deadCharacterClone = Instantiate(CurrentPlayer);
		CurrentPlayer.SetActive (false);

		//remove scripts etc from player
		deadCharacterClone.GetComponent<SwingController>().enabled = false;
		deadCharacterClone.GetComponent<Runner>().enabled = false;
		deadCharacterClone.GetComponent<CharacterInformationScript>().enabled = false;
		deadCharacterClone.GetComponent<LineRenderer> ().enabled = false;

		//set all the bits to not be part of the Runner layer, otherwise bits of character may hit things and trigger reactions.
		deadCharacterClone.tag = "Untagged";
		deadCharacterClone.layer = LayerMask.NameToLayer("DeadCharacterClone");
		Transform[] deadCharacterCloneTransforms = deadCharacterClone.GetComponentsInChildren<Transform>();
		int DeadCharacterCloneLayer = LayerMask.NameToLayer ("DeadCharacterClone");
		foreach (Transform dcct in deadCharacterCloneTransforms) {
			dcct.gameObject.layer = DeadCharacterCloneLayer;
		}

		//remove all the body collider scripts so that you can be sure random limbs won't trigger objects
		BodyCollider[] BodyColliderScripts = deadCharacterClone.GetComponentsInChildren<BodyCollider> ();
		foreach (BodyCollider bc in BodyColliderScripts) {
			Destroy (bc);
		}

		//throw all parts towards camera center point
		//find the characterroot and pelvis first
		GameObject deadCharacterCloneCharacterRoot = null;
		GameObject deadCharacterPelvis = null;
		foreach(Transform child in deadCharacterClone.transform.GetChild(0)){
			if (child.gameObject.tag == "CharacterRoot") {
				deadCharacterCloneCharacterRoot = child.gameObject;
				deadCharacterCloneCharacterRoot.tag = "Untagged"; //remove the tags to avoid confusion
			} else if (child.gameObject.tag == "CharacterPelvis") {
				deadCharacterPelvis = child.gameObject;
				deadCharacterPelvis.tag = "Untagged";
			}
		}

		Rigidbody cloneRigidBody = null;
			if (deadCharacterCloneCharacterRoot != null && deadCharacterPelvis != null){			
			//if the player died because it was at gameOverY or off screen in general, then move the clone up so that the explosion is actually visible
			if (deadCharacterCloneCharacterRoot.transform.position.y < LowerGuidelineCube.transform.position.y) {
				//have to move both otherwise you'll move the character root (the ribs) up, and the pelvis won't come with it, so you end up with the ribs being in the right position for a millisecond before being snapped back down by the joint between ribs and pelvis
				//If the child was found.
				deadCharacterCloneCharacterRoot.transform.position = new Vector3 (deadCharacterCloneCharacterRoot.transform.position.x, LowerGuidelineCube.transform.position.y -2, deadCharacterCloneCharacterRoot.transform.position.z);
				deadCharacterPelvis.transform.position = new Vector3 (deadCharacterCloneCharacterRoot.transform.position.x, LowerGuidelineCube.transform.position.y-2, deadCharacterCloneCharacterRoot.transform.position.z);
			} 

			// This part is to move the dead player to just outside screen limits so that on explosion it's actually visible by the player. It works ok now.
			float offscreenXDifference = PlayerCameraMarker.transform.position.x - MainCamera.transform.position.x; // yes it is correct to use the marker
			if (offscreenXDifference < 0 && deathSplatterDirection != 2){ // see top of function for explanation of deathSplatterDirection variable 
				if (offscreenXDifference <= CameraControllerScript.offscreenXDistance) { // this if is important, otherwise it will move it regardless of whether it's left of the screen or not. it should only move the x position when it's left of screen.
					offscreenXDifference = Mathf.Abs (offscreenXDifference); //convert to positive number so it's easier to work with
					float howMuchCombined = Mathf.Abs (CameraControllerScript.offscreenXDistance) - offscreenXDifference;
					howMuchCombined = Mathf.Abs (howMuchCombined);
					deadCharacterCloneCharacterRoot.transform.position = new Vector3 (deadCharacterCloneCharacterRoot.transform.position.x + howMuchCombined, deadCharacterCloneCharacterRoot.transform.position.y, deadCharacterCloneCharacterRoot.transform.position.z);
					deadCharacterPelvis.transform.position = new Vector3 (deadCharacterCloneCharacterRoot.transform.position.x + howMuchCombined, deadCharacterCloneCharacterRoot.transform.position.y, deadCharacterCloneCharacterRoot.transform.position.z);
				}
			}
			//Disable the markers as they're no longer needed, character is dead.
			OffscreenMarkerBottomCube1MeshRenderer.enabled = false;
			OffscreenMarkerBottomCube2MeshRenderer.enabled = false;
			OffscreenMarkerTopCube1MeshRenderer.enabled = false;
			OffscreenMarkerTopCube2MeshRenderer.enabled = false;
			yield return new WaitForSeconds (0.1f); 
			MasterAudio.PlaySoundAndForget ("death");
			cloneRigidBody = deadCharacterCloneCharacterRoot.GetComponent<Rigidbody> ();
			cloneRigidBody.isKinematic = false;
			if (cloneRigidBody != null ) { 
				//Vector3 explosionPosition = deadCharacterCloneCharacterRoot.transform.position;
				//				explosionPosition.x -= 2;
				//				explosionPosition.y -= 2;
				//				explosionPosition.z -= 2;
				//				Debug.Log ("Starting explosion at "+ explosionPosition);
				//cloneRigidBody.AddExplosionForce (10f, explosionPosition, 5f, 3.0F);
				Vector3 deathSplatterDirectionVector;
				Vector3 currentCameraPosition = MainCamera.transform.position;
				currentCameraPosition.z += 15; //This gets a position that the camera can actually see when the body parts are launched at it
				deathSplatterDirectionVector = (currentCameraPosition - deadCharacterCloneCharacterRoot.transform.position).normalized; //aims splatter towards camera/center of screen

				//deathSplatterDirectionVector = Vector3.forward;
				//float power = 250f;
//				switch (deathSplatterDirection)
//					{
//					case 0:
//						// 0 is 'death on the left'
//						break;
//					case 1://1 is 'death from below'
//						break;
//					case 2://2 is 'death by enemy'
//					//power = 8f;						
//						break;
//					default:
//						break;
//					}

				//cloneRigidBody.AddForce(deathSplatterDirectionVector * power, ForceMode.VelocityChange); // BEST ALTERNATE OPTION ---------------
				//cloneRigidBody.AddExplosionForce(100f, cloneRigidBody.transform.position, 200f, 0f);

				//add a load of little cubes here for extra dramatic effect
				Vector3 currentCubePosition = cloneRigidBody.transform.position;
				// The following lines make it so that the cubes are built behind the body, by offsetting where the cube stack begins building, otherwise it would just end up on top if the characters head.
				// It'd be a little neater if you had variables here so you could alter the number of cubes simply.
				currentCubePosition.x = currentCubePosition.x - 2f;
				currentCubePosition.y = currentCubePosition.y - 2f;
				Vector3 originalCubePosition = currentCubePosition;
				shatterCubeArray = new GameObject[9];
				shatterCubeArrayIndex = 0;
				currentCubePosition.z += 0.2f;
				// Creates an array of cubes just behind the player - uses the template of the ShatterCharacterCube, which 
				//is coloured correctly based on the colour on the character information script elsewhere in the code
				//First get the cube to copy and unfreeze it ready to copy
				ShatterCharacterCube.GetComponent<Rigidbody>().isKinematic = false;
				ShatterCharacterCube.GetComponent<MeshRenderer> ().enabled = true;
				for (int x = 0; x < 3; x++) {
					currentCubePosition.x += 1f;
					for (int y = 0; y < 3; y++) {
//						currentCubePosition.z += 0.5f;
						currentCubePosition.y += 1f;
						shatterCubeArray [shatterCubeArrayIndex] = (GameObject)Instantiate (ShatterCharacterCube, currentCubePosition, Random.rotation);
						shatterCubeArrayIndex++;
						//currentShatterCube.GetComponent<Rigidbody> ().AddForce(deathSplatterDirectionVector * (power/5), ForceMode.VelocityChange);
					}
					currentCubePosition.y = originalCubePosition.y;
				}
				//freeze the original again so its ready for use in the future, and hide it again.
				ShatterCharacterCube.GetComponent<Rigidbody>().isKinematic = true;
				ShatterCharacterCube.GetComponent<MeshRenderer> ().enabled = false;

				Vector3 explosionPos = cloneRigidBody.transform.position;
				if (deathSplatterDirection == 2) { 
					// if it's just hit an enemy, put the explosion directly behind the player
					explosionPos.z = explosionPos.z + 2;
				} else {
					// This should move the explosion in the opposite directon from the camera, so that it points the explosion towards camera.
					explosionPos = explosionPos + (-deathSplatterDirectionVector * 2f); 
				}
				//explosionPos.z = explosionPos.z + 1f;
				//explosionPos.x = explosionPos.x + 0.5f;
				float explosionRadius = 18f;
				if (DebugScript.showDebugMarkers){
					GameObject ExplosionVisualisationSphere = GameObject.Find ("ExplosionVisualisationSphere");
					ExplosionVisualisationSphere.GetComponent<MeshRenderer> ().enabled = true;
					ExplosionVisualisationSphere.transform.position = explosionPos;
					ExplosionVisualisationSphere.transform.localScale = new Vector3 (explosionRadius * 2, explosionRadius * 2, explosionRadius * 2); //XTODO not sure why these are divided by 2? shouldn't it be * 2 as diameter = 2* radius?
				}

//				ExplosionVisualisationSphere.transform.localScale.x = explosionRadius;
//				ExplosionVisualisationSphere.transform.localScale.y = explosionRadius;
//				ExplosionVisualisationSphere.transform.localScale.z = explosionRadius;
				//yield return new WaitForSeconds (0.5f); //xDEBUG
				float upwardsExplosiveForce = 1f;
				if (cloneRigidBody.transform.position.y > 8) {
					//If it's already on the top half of the screen, don't bother making the explosive force below.
					upwardsExplosiveForce = 0f;
				}
				// this makes a sphere which can tell all the colliders that are hit by it, simulating an explosion.
				Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
				foreach (Collider hit in colliders) {					
					Rigidbody rb = hit.GetComponent<Rigidbody>();
					if (rb != null && rb.gameObject.layer == DeadCharacterCloneLayer) { // the DeadCharacterCloneLayer makes sure that the explosion only affects the dead character clone pieces and not any other random 
																						// obstacles or decorations with rigidbodies that might be nearby.
						rb.AddExplosionForce (3000f, explosionPos, explosionRadius, upwardsExplosiveForce);
					}
				}
			}

			if (deathSplatterDirection != 2 || !recordPlayerData) { 
				//If it's not hit an enemy, you need to remove all the colliders for the limbs and stuff, then they can actually pass through objects.
				// Otherwise, when you make the explosion, if the character is moved by the script to inside or underneath or above a solid object, it will get stopped by that on the way to 
				// the camera and the user won't see anything.
				// do it on end of game as well so that you can drop things onto the player and it doesnt just sit on top of their dead limbs etc
				//first do it for all the colliders in the characters limbs
				Collider[] characterColliders = deadCharacterClone.GetComponentsInChildren<Collider> ();
				foreach (Collider cc in characterColliders) {
					Destroy (cc);
				}		

				// Now do it for all the little cubes you spawned earlier to make the death more dramatic
				if (shatterCubeArray != null && shatterCubeArrayIndex > 0) {
					for (int p = 0; p < shatterCubeArrayIndex; p++) {
						if (shatterCubeArray [p] != null) {
							Destroy (shatterCubeArray [p].GetComponent<Collider> ());
						}
					}
				}
			}

			yield return new WaitForSeconds (0.1f); // This has to be there so that the force can actually be spread through the entire character and not just the ribs, or characterroot
			// remove all joints from clone
			CharacterJoint[] characterJoints = deadCharacterClone.GetComponentsInChildren<CharacterJoint> ();
			foreach (CharacterJoint cj in characterJoints) {
				Destroy (cj);
			}		
								
			//if they have no lives left
			if (livesLeft <= 0) {
				if ((CharacterInformationScript.charScoringType == characterScoringType.maxDistance || CharacterInformationScript.charScoringType == characterScoringType.numberOfRopesThrown)
					&& !hasBeenOfferedRevive && !gameFinished && distanceTraveled > 600f) {
					//offer revive to 'max distance' and 'number of ropes characters'
					hasBeenOfferedRevive = true;
					CameraControllerScript.startShakeCameraSubroutine(0.4f);
					yield return new WaitForSeconds (1f); 
					GUIManager.showRevivePanel ();
					revivePanelShowing = true;
				} else {
					//wait a second for the death animation to play out
					CameraControllerScript.startShakeCameraSubroutine(0.4f);
					yield return new WaitForSeconds (0.5f); // must be a short time so that the player can restart fast if it's a single-life 
					cloneOriginalPlayer (true, false); //although this causes a problem where if you die near a checkpoint or near start, you can see another clone being spawned just before you go to scoring screen.
											// i'm still keeping it active because there's probably code that feeds off of there being a current player active 
											// in the scene. I've modified it so it just shrinks the player down to an unseeable size. It doesn't matter, we're
											//moving to scoring screen v shortly. yes this is awful.
					recordPlayerData = false;
					if (CharacterInformationScript.charScoringType != characterScoringType.maxDistance && !gameFinished) {
						// X908DZR: this is a bit of a bodge - it means that if a countdown/time trial/rope throws character runs out of time or lives, 
						// and they haven't finished the course, then 
						// they don't recieve a new good score on scoring screen, and it's treated as though they just ended the 
						//run manually. The only way a countdown/time trial/ rope throws character gets a new top score is if they finish the course.
						Debug.Log ("this is a countdown/minropes/ time trial char that has run out of lives or time, so setting currentGameEndedRunManually to true - see code comment X908DZR ");
						persistentGOruntimeVars.currentGameEndedRunManually = true;
					} 
					GameEventManager.TriggerLoadScoringScreenAfterDelay (1f, false);
				}
			} else if (recordPlayerData){ //you may have called the kill player stuff from an object AFTER the finish line in order to use the kill player's explode visuals.
										 //, and if so you would not be recording player data any more (i), so you don't need to deal with respawning and all that stuff. 

				/*********************
					NOTE: do not replace the following with a simple startCorRoutine of respawnPlayerAfterRevive() function below, as 
					this means that it would start the coroutine and keep running, running cleanUpKillPlayerRoutine (); before it had
					finished. Yes this creates duplication of code so is a bit messy, but it works. Also AfterRevive is slightly different
					in that it has no wait time and also spawns the player at the start of the current level, not the checkpoint.
				********************************************************************************************************
				*/

				//wait a second for the death animation to play out
				CameraControllerScript.startShakeCameraSubroutine(0.4f);
				yield return new WaitForSeconds (2f); 
				// destroy all parts:
				// destroy dead character clone
				Destroy(deadCharacterClone);
				// destroy cubes
				if (shatterCubeArray != null && shatterCubeArrayIndex > 0) {
					for (int p = 0; p < shatterCubeArrayIndex; p++) {
						Destroy (shatterCubeArray [p]);
					}
				}
				// reduce lives left
				livesLeft -= 1;
				if (recordPlayerData) {
					GUIManager.SetLivesText (livesLeft);
				}
				//move the player back to last checkpoint place (or back to zero)
				//float torsoDifference = Mathf.Abs(CharacterRoot.transform.position.y - CharacterPelvis.transform.position.y);
				//CurrentPlayer.transform.rotation = Quaternion.Slerp(CurrentPlayer.transform.rotation, RunnerScript.characterStartingRotation, 1f);
				cloneOriginalPlayer (false, false);
				//CurrentPlayer.SetActive (true);
				GameEventManager.triggerGetGameReferences (); // This triggers gamereferences for ALL the scripts that have this method implemented, so that they still work
				if (CharacterInformationScript.charScoringType == characterScoringType.maxDistance &&
					!hasBeenOfferedRevive &&
					distanceTraveled > LevelManagerScript.getTotalLevelOffset()){
					// If they've looped around the entire course, and they haven't been offered a revive yet, set their respawn point to be 
					// the leftmost point of the current level, as if they were respawning from revive. This is a better alternative to them 
					// being thrown all the way back to when they last used a checkpoint miles back, and having to work through
					// the whole course again (potentially several times!) in order to increase their score.
					lastCheckpointPosition = LevelManagerScript.getCurrentLevelTransformPosition ();
					lastCheckpointPosition = new Vector3 (lastCheckpointPosition.x, 5, 0);
				}
				CharacterRoot.transform.position = lastCheckpointPosition;
				//CharacterPelvis.transform.rotation = Quaternion.identity;
				CharacterPelvis.transform.position = new Vector3(lastCheckpointPosition.x, CharacterPelvis.transform.position.y, lastCheckpointPosition.z);
				//RunnerScript.resetCharacterOrientation();
				// move the camera - this +4 number is hacky because it's based on the positions of hte playercameramarker etc, and even that has a +1 on it in places so the playcameramarker is not particularly cleanly implemented -but it works for now.
				BackgroundManagerScript.repositionBackgroundBehindPlayer();
				if (DebugScript.levelCycling && atLeastOneCheckpointSet) {
					LevelManagerScript.repositionLevelBehindPlayer ();
				} else if (DebugScript.levelCycling && CharacterInformationScript.numberOfLives > 0) {
					Debug.Log("TR you enabled level cycling, and the player has more than 1 life - but they didn't set a checkpoint. Either you just died before setting a checkpoint which is fine, or this means that you haven't put a checkpoint in the course, which you need for rewinding the course back to 0, so put a checkpoint anywhere except the 1st or last level. ");
				}
				MainCamera.transform.position = new Vector3(CharacterRoot.transform.position.x + (PlayerCameraMarkerOffset-1), MainCamera.transform.position.y, MainCamera.transform.position.z);
				PlayerCameraMarker.transform.position = new Vector3(CharacterRoot.transform.position.x + PlayerCameraMarkerOffset, CharacterRoot.transform.position.y, CharacterRoot.transform.position.z);
				Debug.Log("start blinking player routine");
				CameraControllerScript.startBlinkPlayerRenderingSubroutine (2, 0.15f, 0.15f);
				yield return new WaitForSeconds (2f);
				RunnerScript.enabled = true;
				//GameEventManager.triggerGetGameReferences (); 
				//recursively re-enable all rigidbodies
				//GameEventManager.triggerGetGameReferences ();
				RunnerScript.restartCharacterLimbs();
				//GameEventManager.triggerGetGameReferences ();
				//				runnerRB = CharacterRoot.GetComponent<Rigidbody> ();
				//				runnerRB.isKinematic = false;
				//				runnerRB.detectCollisions = true;
				//				pelvisRB = CharacterPelvis.GetComponent<Rigidbody> ();
				//				pelvisRB.isKinematic = false;
				//				pelvisRB.detectCollisions = true;
				//set the player to flash for a few seconds
				//re enable camera movement.
				// uncheck the 'is kinematic' flag on the player
			
			}

		}
		if (!revivePanelShowing) {
			cleanUpKillPlayerRoutine ();
		}
		yield return null;
	}

	public void startRespawnPlayerAfterReviveCoroutine(){
		//just a wrapper
		StartCoroutine(respawnPlayerAfterRevive());
	}

	public IEnumerator respawnPlayerAfterRevive(){ 
		yield return new WaitForSeconds (0.5f); 
		// destroy all parts:
		// destroy dead character clone
		Destroy(deadCharacterClone);
		// destroy cubes
		if (shatterCubeArray != null && shatterCubeArrayIndex > 0) {
			for (int p = 0; p < shatterCubeArrayIndex; p++) {
				Destroy (shatterCubeArray [p]);
			}
		}
		// reduce lives left
		//livesLeft -= 1; //don't do this on respawn because they'll already be on 0.
		if (recordPlayerData) {
			GUIManager.SetLivesText (livesLeft);
		}
		//move the player back to last checkpoint place (or back to zero)
		//float torsoDifference = Mathf.Abs(CharacterRoot.transform.position.y - CharacterPelvis.transform.position.y);
		//CurrentPlayer.transform.rotation = Quaternion.Slerp(CurrentPlayer.transform.rotation, RunnerScript.characterStartingRotation, 1f);
		cloneOriginalPlayer (false, false);
		//CurrentPlayer.SetActive (true);
		GameEventManager.triggerGetGameReferences (); // This triggers gamereferences for ALL the scripts that have this method implemented, so that they still work
		//CharacterRoot.transform.position = lastCheckpointPosition;
		Vector3 startOfLastLevelPosition = LevelManagerScript.getCurrentLevelTransformPosition ();
		Vector3 revivePosition = new Vector3 (startOfLastLevelPosition.x, 5, 0);
		CharacterRoot.transform.position = revivePosition;
		//CharacterPelvis.transform.rotation = Quaternion.identity;
		CharacterPelvis.transform.position = new Vector3(revivePosition.x, CharacterPelvis.transform.position.y, revivePosition.z); 
		//RunnerScript.resetCharacterOrientation();
		// move the camera - this +4 number is hacky because it's based on the positions of hte playercameramarker etc, and even that has a +1 on it in places so the playcameramarker is not particularly cleanly implemented -but it works for now.
		BackgroundManagerScript.repositionBackgroundBehindPlayer();
//		if (DebugScript.levelCycling && atLeastOneCheckpointSet) {
//			LevelManagerScript.repositionLevelBehindPlayer ();
//		} else if (DebugScript.levelCycling && CharacterInformationScript.numberOfLives > 0) {
//			Debug.Log("TR you enabled level cycling, and the player has more than 1 life - but they didn't set a checkpoint. Either you just died before setting a checkpoint which is fine, or this means that you haven't put a checkpoint in the course, which you need for rewinding the course back to 0, so put a checkpoint anywhere except the 1st or last or second to last level. ");
//		}
		MainCamera.transform.position = new Vector3(CharacterRoot.transform.position.x + (PlayerCameraMarkerOffset-1), MainCamera.transform.position.y, MainCamera.transform.position.z);
		PlayerCameraMarker.transform.position = new Vector3(CharacterRoot.transform.position.x + PlayerCameraMarkerOffset, CharacterRoot.transform.position.y, CharacterRoot.transform.position.z);
		Debug.Log("start blinking player routine");
		CameraControllerScript.startBlinkPlayerRenderingSubroutine (2, 0.15f, 0.15f);
		yield return new WaitForSeconds (2f);
		RunnerScript.enabled = true;
		//GameEventManager.triggerGetGameReferences (); 
		// recursively re-enable all rigidbodies
		//GameEventManager.triggerGetGameReferences ();
		RunnerScript.restartCharacterLimbs();
		cleanUpKillPlayerRoutine ();
		yield return null;
	}

	public void cleanUpKillPlayerRoutine (){
		killPlayerSubroutineAlreadyRunning = false;
		if (recordPlayerData) { //only need to do these things if the game is still running.
			CameraControllerScript.cameraStopped = false;	
			GameEventManager.triggerGetGameReferences (); 
			AllowMainGameInput = true;
		}
	}

	public void handleCountdownPlaymodeRunningOutOfTime(){
		if (!dealtWithCountdownEnd) {
			dealtWithCountdownEnd = true;
			livesLeft = 0;
			elapsedTime = 0;
			KillPlayer (0);
		}
	}

	private void cloneOriginalPlayer(bool shrinkToHide, bool waitToSetActiveAfterStart){
		// If you have the original character in the game, then just instantiate it when you start, you can insantiate it again at every checkpoint when you destroy the 
		// original character, and you don't have to worry about restoring all the angles and distances of the ragdoll.
		Destroy(CurrentPlayer); 
		CurrentPlayerOriginal.SetActive(true); 
		CurrentPlayerOriginal.SetActiveRecursivelyCustom(true);
		CurrentPlayer = Instantiate(CurrentPlayerOriginal);
		CurrentPlayer.tag = "CurrentPlayer";
		CurrentPlayer.transform.position = lastCheckpointPosition;
		//All the tags on the original have the string 'Original'
//		CurrentPlayer.transform.FindDeepChildAndReplaceTag ("CurrentPlayerOriginal", "CurrentPlayer");
//		CurrentPlayer.transform.FindDeepChildAndReplaceTag ("CurrentPlayerOriginal", "CurrentPlayer");
		//CurrentPlayerOriginal.SetActive(false);
		CurrentPlayerOriginal.SetActiveRecursivelyCustom(false);
		CurrentPlayer.SetActiveRecursivelyCustom (true);
		if (waitToSetActiveAfterStart) {
			//CurrentPlayer.SetActiveRecursivelyCustom (false);
			StartCoroutine (restartCurrentPlayerAfterWaitingAtStart());
		} 
		if (shrinkToHide) {
			CurrentPlayer.transform.localScale = new Vector3 (0.1f, 0.1f, 0.1f); //this is a travesty but never mind
		}
	}

	private IEnumerator restartCurrentPlayerAfterWaitingAtStart(){
		// This coroutine just hides the lag as you load all the levels in levelManager. It:
		// $ disables game input
		// $ shows a loading panel
		// $ hides the player from the camera for a second, 
		// $ stops player from falling
		// $ waits for levels to load
		// $ re enables input and camera once levels loaded, after short delay, with a sound effect.
		//similar to when you lose a life, you hide the player from the camera.
		yield return null; //wait for first frame and start() to happen so you can wait for getGameReferences() etc to fire
		AllowMainGameInput = false; //this doesn't seem to have an effect
		SwingControllerScript.enabled = false;
		CameraControllerScript.blinkCamerasOff ();
		Vector3 originalGameGravitySetting = new Vector3 (0, -7, 0);
		Physics.gravity = Vector3.zero;
		//grab playlist ready to start in a second.
		MasterAudio.Playlist testPl = MasterAudio.GrabPlaylist (CharacterInformationScript.audioPlaylistName, false);

		while (Time.timeSinceLevelLoad < 15f && LevelManagerScript.getLevelsFullyLoadedStatus() == false) { //the time is just a failsafe, but some devices may take that long.
			//Debug.Log("returning. time.time: "+ Time.time+" endWaitToRestartCurrentPlayerTime: "+endWaitToRestartCurrentPlayerTime);
			//Debug.Log("Time.timeSinceLevelLoad  "+ Time.timeSinceLevelLoad );
//			/Debug.Log ("waiting still.");
			yield return new WaitForSeconds(0.1f);
		}
		GUIManager.showLoadingPanel (false);
		yield return new WaitForSeconds (0.5f); //this is just here to give the player a second to prepare and recognise that something's happening, but no longer than this or they'll think game is broken
		//CurrentPlayer.SetActiveRecursivelyCustom (true);
		CameraControllerScript.blinkCamerasOn();
		startTime = Time.time; //reset the start time so that the counter starts actually ticking when they can see their player.
		Debug.Log ("setting gravity vai main scene script.");
		if (persistedPlayerDataController.persistedPlayerData.currentlySelectedCharacterHardName == "astronaut") {
			Physics.gravity = new Vector3(0, -0.15F, 0);
		} else if (persistedPlayerDataController.persistedPlayerData.currentlySelectedCharacterHardName == "diver") {
			Physics.gravity = new Vector3(0, -2F, 0);
		} else if (DebugScript.enableGravity == false) {			
			Physics.gravity = new Vector3 (0, 0, 0); //Set gravity to nothing (usually 0,-7,0)
		} else {
			Physics.gravity = originalGameGravitySetting;
		}
		stillWaitingForLevelsToLoad = false;
		MasterAudio.PlaySoundAndForget ("death", 0.5f, 1.5f); //little pop sound as they appear (this is good for 'game start time!' mental conditioning?)

		if (testPl != null) {
			//MasterAudio.ChangePlaylistByName(CharacterInformationScript.audioPlaylistName, true);
			MasterAudio.StartPlaylist(CharacterInformationScript.audioPlaylistName);
		} else {
			Debug.Log ("Could not find specified playlist for this character in CharacterInformationScript.audioPlaylistName! using default..");
			testPl = MasterAudio.GrabPlaylist ("defaultPlaylist", false);
			if (testPl != null) {
				//MasterAudio.ChangePlaylistByName ("defaultPlaylist", true);
				MasterAudio.StartPlaylist("defaultPlaylist");
			} else {
				Debug.LogError ("could not retrieve default playlist!");
			}
		}


		//yield return new WaitForSeconds (2f);
		AllowMainGameInput = true;
		SwingControllerScript.enabled = true;
		yield return null;
	}

	public bool getNeedToHideIconsOnStart(){
		return needToHideIconsOnStart;
	}

	public bool getStillWaitingForLevelsToLoad(){
		return stillWaitingForLevelsToLoad;
	}

	void OnApplicationQuit ()
	{
		if (CharacterRoot != null) {
			AnalyticsExtenderTR.Custom ("quitFromMainScene_possible_rage_quit", new Dictionary<string, object> { 
				{"currentlySelectedCharacterHardName", persistedPlayerDataController.persistedPlayerData.currentlySelectedCharacterHardName},
				{ "ActualCharacterPositionOnEnd", CharacterRoot.transform.position.x },
				{ "maxDistanceTraveled", distanceTraveled },
				{ "currentGameElapsedTime", Time.timeSinceLevelLoad }
			});
		} else {
			AnalyticsExtenderTR.Custom ("quitFromMainScene_possible_rage_quit", new Dictionary<string, object> { 
				{"currentlySelectedCharacterHardName", persistedPlayerDataController.persistedPlayerData.currentlySelectedCharacterHardName},
				{ "maxDistanceTraveled", distanceTraveled },
				{ "currentGameElapsedTime", Time.timeSinceLevelLoad }
			});
		}
	}


		
}
