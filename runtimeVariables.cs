using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class runtimeVariables : MonoBehaviour {
	/* No sense in storing in-between runtime stuff like this in persisted data, it's just going to take up space on disk.
	 * This class just stores random bits of runtime info as-is instead.
	 * 
	 * */
	public characterOutfitTrophySelectScreenState characterOutfitTrophySelectSelectedScreen = characterOutfitTrophySelectScreenState.CharacterBrowse;
	public List<queuedUnlockScreen> queuedUnlockScreens = new List<queuedUnlockScreen>(); //most important first, i.e. [0] = most important in list
	public queuedUnlockScreen priorityUnlockScreen = null; //Allows you to make absolutely sure that this will be shown before any other
	public bool unlockCalledFromElsewhereNotThroughScoringCurrentGame = false; // if they call 'free gift' from the main menu, not the scoring scene, set this flag
	public string sceneToReturnTo = "mainMenu";
	public bool currentGameEndedRunManually;
	public int currentGameCoinsCollected;
	public float currentGameDistanceTravelled;
	public float currentGameActualCharacterPositionOnEnd;
	public bool currentGameCharactersMysteryItemCollected;
	public float currentGameElapsedTime;
	public int currentGameRopesThrown;
	public bool jumpStraightToGiftBoxChoice;
	public bool jumpStraightToProgressTutorial;
	public bool currentGameCharacterWasOnTrial;
	public bool unlockedCharacterByBuyingIt;
	public bool currentGameIsFirstRunTutorial;


	public void ResetCurrentGameDefaults(){
		currentGameEndedRunManually = false;
		currentGameCoinsCollected = 0;
		currentGameDistanceTravelled = 0f;
		currentGameCharactersMysteryItemCollected = false;
		currentGameElapsedTime = 0f;
		currentGameRopesThrown = 0;
		currentGameActualCharacterPositionOnEnd = 0;
		jumpStraightToGiftBoxChoice = false;
		jumpStraightToProgressTutorial = false;
		currentGameCharacterWasOnTrial = false;
		unlockCalledFromElsewhereNotThroughScoringCurrentGame = false; //this means that every time you start a new game it will make doubly sure that the game doesn't think you're entering scoring screen from the menu.
		unlockedCharacterByBuyingIt = false;
		currentGameIsFirstRunTutorial = false;
	}
}
