using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public enum Version
{
	demo,
	free,
	full
}
public class GamePlay : MonoBehaviour 
{
	public static Version version = Version.full;
	public static bool isTesting = false;
	public static string versionNumber = "1.0.0";
	public static int level;
	public static int subLevel;
	int numberOfStar = 0;
	void Start()
	{
		//FOR TESTING, WENT PLAYER MOVE TO THIS SCENE, 
		//IT WILL UNLOCK NEXT LEVEL,  RANDOM THE NUMBER OF STAR PLAYER GET
		numberOfStar = Random.Range(1,4);		
		UnlockNextLevel();
		SaveLevels();
		SaveStars();
		Application.LoadLevel("ChooseLevel");
	}
	///<Summary>
	///Unlock next level after player win the game
	///</Summary>
	public bool UnlockNextLevel()
	{
		//sublevel and level start from 0
		//maxSubLevels and subLevelsUnlocked start from 1
		int maxSubLevel = LevelManager.maxSubLevels[level];	
		//if current sub level isn't last sub level in this level
		//increase current sub level and unlock it
		if(subLevel < maxSubLevel-1)
		{
			subLevel++;
			if(LevelManager.subLevelsUnlocked[level] < subLevel+1)
				LevelManager.subLevelsUnlocked[level] = subLevel+1;	
			//Save unlocked level in string format
			SaveLevels();
			return true;
		}
		//if current sub level is last sub level in this level
		//but the level isn't last level, unlock next level
		int nextMaxSubLevel = 0;
		if(LevelManager.maxSubLevels.Length > level)
			nextMaxSubLevel = LevelManager.maxSubLevels[level+1];
		if(subLevel == maxSubLevel -1 && nextMaxSubLevel > 0)
		{
			level++;
			subLevel = 0;
			if(LevelManager.subLevelsUnlocked[level] < subLevel+1)
				LevelManager.subLevelsUnlocked[level] = subLevel+1;
			//Save unlocked level in string format
			SaveLevels();
			return false;
		}
		return false;
		
	}
	///<Summary>
	///Save number star player get after win the game
	///</Summary>
	void SaveStars()
	{
		//Save star numbers
		
		//Get the real index of sub level, it equals = maxSubLevel[0] + maxSubLevel[1] + ... + index
		int index = subLevel;
		for(int i=0; i< level; i++)
		{
			index += LevelManager.maxSubLevels[i];
		}
		//If the index isn't the last and last time play the game, 
		//player get a lower star than this time
		//update that value
		if(LevelManager.starNumbers.Count > index && LevelManager.starNumbers[index] < numberOfStar)
		{
			LevelManager.starNumbers[index] = numberOfStar;
		}
		else if(LevelManager.starNumbers.Count <= index)
		{
			//If the index is out of starNumbers, this new level, so add it to list
			LevelManager.starNumbers.Add (numberOfStar);
		}
		//Save number of stars into PlayerPrefs with string format
		string starString = "";
		for(int i=0; i<LevelManager.starNumbers.Count; i++)
		{
			if(i!= 0)
				starString += ",";
			starString += LevelManager.starNumbers[i];
		}		
		PlayerPrefs.SetString("Star_Number",starString);	
	}
	///<summary>
	///Save unlocked level into PlayerPrefs with string format
	///</summary>
	void SaveLevels()
	{
		string saveString = "";
		for(int i=0; i<LevelManager.subLevelsUnlocked.Length; i++)
		{
			if(i!=0)
				saveString+= ",";
			saveString += LevelManager.subLevelsUnlocked[i];
		}
		Debug.Log("SaveSTR : "+saveString);
		PlayerPrefs.SetString("Sublevel_Unlocked",saveString);		
				
	}

}
