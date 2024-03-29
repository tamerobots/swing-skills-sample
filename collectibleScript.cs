using UnityEngine;
using System.Collections;
using DarkTonic.MasterAudio; 

public class collectibleScript : MonoBehaviour {
	private GameObject GameEventManagerObject;
	private GameEventManager GameEventManager;
	private GameObject mainSceneObject;
	private mainSceneScript mainSceneScript;
	private GameObject GUIManagerObject;
	private GUIManager GUIManager;
	private bool alreadyTriggered = false;
	public enum CollectibleVariant{
		Coin,
		SequenceCoin,
		ExtraLife,
		mysteryItem
	}
	public CollectibleVariant collectibleVariant;
	private GameObject CollectibleParticleSystemGameObject;
	private bool alreadyMovingTowards;
	public float timeToTake = 0.5f;


	void Start(){
		//GetGameReferences (); //removed this because on levels with lots of coins it adds a load of loading time to startup.
		GameEventManager.GetGameReferences += GetGameReferences;
	}

	void GetGameReferences(){
		if (this != null) {
			GameEventManagerObject = GameObject.Find ("GameEventManager");
			GameEventManager = GameEventManagerObject.GetComponent<GameEventManager> ();
			mainSceneObject = GameObject.Find ("mainSceneObject");
			mainSceneScript = mainSceneObject.GetComponent<mainSceneScript> ();
			GUIManagerObject = GameObject.Find ("GUIManager");
			GUIManager = GUIManagerObject.GetComponent<GUIManager> ();
		}
	}

	void OnEnable() {

		switch (collectibleVariant)
		{
		case CollectibleVariant.Coin:
			Collider objectCollider = this.GetComponent<Collider> ();
			if (objectCollider != null) {
				objectCollider.enabled = true;
			}
			Collider[] objectColliders = GetComponentsInChildren<Collider> ();
			foreach (Collider collider in objectColliders) {
				//disable all colliders so it just falls off screen
				collider.enabled = true;
			}
			MeshRenderer objectMeshRenderer = this.GetComponent<MeshRenderer> ();
			objectMeshRenderer.enabled = true;
			CollectibleParticleSystemGameObject = GameObject.Find ("CoinParticleSystem");
			break;
		case CollectibleVariant.SequenceCoin:
			CollectibleParticleSystemGameObject = GameObject.Find ("SequenceCoinParticleSystem");
			break;
		case CollectibleVariant.ExtraLife:
			CollectibleParticleSystemGameObject = GameObject.Find ("ExtraLifeParticleSystem");
			break;
		case CollectibleVariant.mysteryItem:
			CollectibleParticleSystemGameObject = GameObject.Find ("MysteryItemParticleSystem");

			//check whether the player has already gotten this mystery item. If they have, don't bother showing it in level again.
			GameObject PersistentGameObject = GameObject.FindGameObjectWithTag("PersistentGameObject");
			persistedPlayerDataController ppdc = PersistentGameObject.GetComponent<persistedPlayerDataController> ();
			GameObject CurrentPlayer = GameObject.FindWithTag ("CurrentPlayer");
			if (CurrentPlayer != null){
				CharacterInformationScript cis = CurrentPlayer.GetComponent<CharacterInformationScript> ();
				if (cis != null){
					if (cis.mysteryItemTrophyName == gameObject.name && ppdc.isTrophyUnlocked(cis.mysteryItemTrophyName)){
						Debug.Log ("disabling mystery item "+cis.mysteryItemTrophyName+" as the player has already collected it:"+gameObject.name);
						this.gameObject.SetActive (false);
					}
				}			
			}
			break;
		default:
			// do nothing
			break;
		}			

		GetGameReferences ();
	}

	public void beginMoveTowards(Transform targetTransform){
		// This starts the collectible object moving towards the player.
		if (!alreadyMovingTowards) {			
			StartCoroutine (moveTowards (targetTransform, timeToTake));
		}
	}
		
	public IEnumerator moveTowards(Transform targetTransform, float timeToTake){		
		yield return new WaitForSeconds(0.4f); //wait for rope to get to it
		float timeElapsed = 0;
		Vector3 originalPosition = this.transform.position;
		alreadyMovingTowards = true;
		float acceleration = 1.1f;
		while (timeElapsed < 1){ // if it hits 1 then it's made the full distance
			timeElapsed += Time.deltaTime / timeToTake;
			this.transform.position = Vector3.Lerp (originalPosition, targetTransform.position, timeElapsed*acceleration);
			acceleration = acceleration + 0.2f;
			yield return null;
		}
		alreadyMovingTowards = false;
	}		
	
	void OnTriggerEnter(Collider other) {		
		if (other.gameObject.layer == LayerMask.NameToLayer ("Runner") && !alreadyTriggered) { 			
			GetGameReferences ();
			alreadyTriggered = true;
			switch (collectibleVariant)
			{
			case CollectibleVariant.Coin:				
				// increment number of coins collected
				mainSceneScript.coinsCollected++;
				GUIManager.SetCoinsText ((int)mainSceneScript.coinsCollected);
				//move the coin particle system to the position of this coin
				CollectibleParticleSystemGameObject.transform.position = this.transform.position;
				//emit some particles
				CollectibleParticleSystemGameObject.GetComponent<ParticleSystem> ().Play ();
				MasterAudio.PlaySoundAndForget ("coin_TR2");
				//disable and hide the coin
				//disable any collider on the root transform first
				Collider objectCollider = this.GetComponent<Collider> ();
				if (objectCollider != null) {
					objectCollider.enabled = false;
				}
				Collider[] objectColliders = GetComponentsInChildren<Collider> ();
				foreach (Collider collider in objectColliders) {
					//disable all colliders so it just falls off screen
					collider.enabled = false;
				}
				MeshRenderer objectMeshRenderer = this.GetComponent<MeshRenderer> ();
				objectMeshRenderer.enabled = false;

				break;			
			case CollectibleVariant.SequenceCoin:
				CollectibleParticleSystemGameObject.transform.position = this.transform.position;
				//emit some particles
				CollectibleParticleSystemGameObject.GetComponent<ParticleSystem> ().Play ();
				this.gameObject.SetActive(false);
				break;
			case CollectibleVariant.ExtraLife:
				mainSceneScript.livesLeft++;
				CollectibleParticleSystemGameObject.transform.position = this.transform.position;
				MasterAudio.PlaySoundAndForget ("lifeGet");
				//emit some particles
				CollectibleParticleSystemGameObject.GetComponent<ParticleSystem> ().Play ();
				GUIManager.SetLivesText(mainSceneScript.livesLeft);
				Destroy(this.gameObject);
				break;
			case CollectibleVariant.mysteryItem:
				mainSceneScript.charactersMysteryItemCollected = true;
				CollectibleParticleSystemGameObject.transform.position = this.transform.position;
				MasterAudio.PlaySoundAndForget ("mysteryItemGet");
				//emit some particles
				CollectibleParticleSystemGameObject.GetComponent<ParticleSystem> ().Play ();
				Destroy(this.gameObject);
				break;
			default:
				Debug.Log ("Collectible Variant not set in the collectible script on a collectible item you just collected. (or no applicable variant in switch statement.) TR");
				break;
			
			}
			alreadyTriggered = false;

		}
	}
}
