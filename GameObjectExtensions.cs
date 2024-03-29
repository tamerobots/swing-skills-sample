using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
	private static bool isShaking = false;

	public static string nameWithCloneSuffixesRemoved (this GameObject input)
	{
		string result = input.name.Replace ("(Clone)", "");
		if (result != null)
			return result;
		
		return null;
	}

	public static GameObject findDeepChildWithTag(this GameObject input, string tagToFind){
		//This exists because I was having problems with the wrong tagged object being returned 
		// for 'CharacterRoot' - Animator was creating some ghost objects that were being returned
		// with the normal FindGameObjectWithTag, so specifying that we're looking for child objects 
		// means I can ensure that I return the CharacterRoot that's actually the child of what I expect,
		// which in most cases will just be a deep child of CurrentPlayer. TR
		// it just returns the first result.

		GameObject result;
		if (input.tag == tagToFind && input.name.Contains ("AnimatorPreview") == false) {
			result = input;
			return result;
		}

		foreach (Transform child in input.transform) {
			result = child.gameObject.findDeepChildWithTag (tagToFind);
			if (result != null) {
				return result;
			}
		}
		return null;
	}

//	public static bool isObjectDeclaredAsSecretInRopeGameConstants (this GameObject input)
//	{
//		// Secret does not appear AT ALL unless unlocked *******************************************
//
//		//input.nameWithCloneSuffixesRemoved
//
//		OutfitInformationScript iis = input.GetComponent<OutfitInformationScript> ();
//		if (iis != null) {
//			return iis.isSecret;
//		}
//
//		trophyInformationScript tis = input.GetComponent<trophyInformationScript> ();
//		if (tis != null) {
//			return tis.isSecret;
//		}
//
//		return false;
//	}
//
//	public static bool isObjectDeclaredAsHiddenInInformationScript (this GameObject input)
//	{
//		// Hidden appears BLACK **************************************************		
////		CharacterInformationScript cis = input.GetComponent<CharacterInformationScript> ();
////
////		if (cis != null) {
////			return cis.isHidden;
////		}
////
//		OutfitInformationScript iis = input.GetComponent<OutfitInformationScript> ();
//		if (iis != null) {
//			return iis.isHidden;
//		}
//
//		trophyInformationScript tis = input.GetComponent<trophyInformationScript> ();
//		if (tis != null) {
//			return tis.isHidden;
//		}
//
//		return false;
//	}

	public static void applyOutfit (this GameObject input, string outfitHardName)
	{		
		GameObject spawnedOutfitGO;
		Dictionary<string, Transform> outfitLimbsDict;
		/* Apply an outfit to a character (rigged or not).
		 * Goes through all the materials used in this gameobject and changes the materials to use the texture of what the ones
		 * in the outfit uses.
		 *Do not LOCK which characters can wear which outfits, as you will want to change some NPCs to look like certain characters etc. For example,
		 *if you play as an enemy, you'd want to change all enemies to look like orangutans.
		 *You could then have a script for enemies where they load a variable in the character information script that sets them to
		 *use a specific outfit. this may be a lot of computation to run at the start of a level though so may be a bad idea.
		Astronauts still need to be able to load the materials etc for their specific parts in their specific outfits, so 
		make it so that the new material/mesh etc is only loaded for the character if it exists.
		Is it better to change the material's texture, then all instances of that material in the scene will just change 
		to the new material, and you don't have to go through each limb?
    	change the  .mainTexture and the .albedoColour (in case you changed that)
    	actually don't go through each texture because the same texture may be used by several different things - there may be 
		NPCs in the scene or other decorations that use that texture and would suffer if you changed it!
		$ Can't create new material and then set all limbs to use that material, because you might as well just set all limbs to use outfits material, doesn't save time
		$ Can't just change used texture of material, because if other NPCs etc in the scene are using that material they'll change as well and you don't want that
				(xTODO if you wanted to, you could create a separate procedure to change the materials directly and thus affect all the uses of that materials in scene, thus 
				changing all the enemies etc to use same outfit, which would actually be a good idea, but has to be in separate function -
				start of function is below in applyOutfitToAllUsesOfTheseMaterials 

		Changing material of each limb in turn is best bet because it restricts it to only this character object. 
		Also change the mesh of each limb to use the mesh of the outfit - although this should prob be restricted to head for performance reasons?
		*/
		GameObject outfitResource = (GameObject)Resources.Load (("RiggedCharacterPrefabs/Outfits/" + outfitHardName));
		if (outfitResource == null) {
			Debug.LogError ("requested outfit not loaded successfully?! TR. outfit:"+outfitHardName);
		} else {
			Debug.Log ("loaded character outfit from resources successfully, applying to character..");
			spawnedOutfitGO = (GameObject)GameObject.Instantiate (outfitResource);		
			Transform[] outfitLimbTransforms = spawnedOutfitGO.GetComponentsInChildren<Transform> ();
			outfitLimbsDict = new Dictionary<string, Transform> ();
			foreach (Transform outfitLimbTransform in outfitLimbTransforms) {
				outfitLimbsDict.Add (outfitLimbTransform.name, outfitLimbTransform);
			}

			Transform[] limbTransforms = input.GetComponentsInChildren<Transform> ();
			Transform currentLimb = null;
			MeshRenderer currentMR = null;
			MeshRenderer characterLimbMR = null;
			MeshFilter currentMF = null;
			MeshFilter characterLimbMF = null;

			foreach (Transform limbTransform in limbTransforms) {
				if (outfitLimbsDict.ContainsKey (limbTransform.name)) {
					currentLimb = null;
					currentLimb = outfitLimbsDict [limbTransform.name];
					if (currentLimb != null) {
						currentMR = null;
						currentMR = currentLimb.GetComponent<MeshRenderer> ();
						if (currentMR != null) {						
							characterLimbMR = null;
							characterLimbMR = limbTransform.GetComponent<MeshRenderer> ();
							if (characterLimbMR != null) {
								characterLimbMR.material = currentMR.material;
							}
						}
						if (limbTransform.name == "head") { //performance - Should remove this line if there are other things on the body other than the head that change mesh between outfits? remember
							// , this is currently just here to save a little time during load? if it even has that effect? Edit: you are only changing the head's mesh between outfits, its decided for time saving reasons, so you
							// can keep this as is for now, unless in the future you decide you want to have other things on the outfit that change mesh.
							currentMF = null;
							currentMF = currentLimb.GetComponent<MeshFilter> ();
							if (currentMF != null) {
								characterLimbMF = null;
								characterLimbMF = limbTransform.GetComponent<MeshFilter> ();
								if (characterLimbMF != null) {
									characterLimbMF.mesh = currentMF.mesh;
								}
							}
						}
					}
				}
			}

			spawnedOutfitGO.SetActive (false); //hide it because not needed any more.
		} 
	}

//	public static void applyOutfitToAllUsesOfTheseMaterials (this GameObject input, string outfitHardName)
//	{		
//		GameObject spawnedOutfitGO;
//		spawnedOutfitGO = (GameObject)GameObject.Instantiate ((GameObject)Resources.Load (("RiggedCharacterPrefabs/Outfits/" + outfitHardName)));
//		MeshRenderer[] inputGOMeshRenderers = input.GetComponentsInChildren<MeshRenderer> ();
//		List<string> materialNames = new List<string> ();
//		Material currentMaterial = null;
//		//Transform matchingOutfit
//		foreach (MeshRenderer mr in inputGOMeshRenderers) {
//			currentMaterial = mr.material;
//			if (materialNames.Contains (mr.material.name) == false) {
//				materialNames.Add (mr.material.name);
//				//mr.material.mainTexture = 
//			}
//		}
//
//		spawnedOutfitGO.SetActive (false); //hide it because not needed any more.
//
//	}

}
