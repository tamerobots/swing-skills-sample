using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class numberExtensionsTR{

	// This gives global methods that can be called from anywhere to display a characters score correctly, for use on scoring screen,
	// in game and on character select screen.
	// You should pass a timespan above the code that calls this so that you aren't generating a new timespan with each loop that you
	// call this, which would be inefficient.

	public static string scoreNumberFormattedString (this float inputNumber, characterScoringType cst, TimeSpan timeSpanToUse, float startTimeForCountdownSeconds = 0, bool alreadySubtractedFromCountdownStartTime = false)
	{
		if (cst == characterScoringType.timeTrial) {
			timeSpanToUse = TimeSpan.FromSeconds (inputNumber);

			return timeSpanToUse.FormattedTimeSpanStringMMSSmsms ();
		} else if (cst == characterScoringType.countdown) {
			if (alreadySubtractedFromCountdownStartTime) {
				timeSpanToUse = TimeSpan.FromSeconds (inputNumber);
			} else {
				// this is for in-game while playing use, not for displaying an already stored previous game score.
				timeSpanToUse = TimeSpan.FromSeconds (startTimeForCountdownSeconds - inputNumber);
			}
			return timeSpanToUse.FormattedTimeSpanStringMMSSmsms ();
		} else if (cst == characterScoringType.maxDistance) {
			return inputNumber.ToString ("f0")+"m";
		} else {
			return inputNumber.ToString ("f0");
		}
	}

	public static int roundUpToNearestTen(this int i){
		return (int)(Math.Ceiling(i / 10.0d)*10);
	}
		
}
