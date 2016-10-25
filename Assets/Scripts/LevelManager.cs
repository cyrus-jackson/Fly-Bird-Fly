using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public enum ChooseLevel
{
	Level,
	SubLevel,
}
public class LevelManager : MonoBehaviour 
{
	public GUISkin skin,skin480;
	public Texture2D[] backgrounds;
	ChooseLevel currentChoose;
	//level vars
	public Texture2D[] levelIcons;
	public Texture2D[] lockedIcons;
	public Texture2D[] starIcons;
	float levelButtonWidth;
	float levelButtonHeight;
	float currentLeft;
	float startLeft;
	bool isShowingBox = false;
	//int currentPage;
	int maxLevel = 10;
	public static List<int> starNumbers = new List<int>();
	public static int[] subLevelsUnlocked = new int[]{1,0,0,0,0,0,0,0,0,0};
	public static int[] maxSubLevels = new int[]{10,12,14,15,16,18,15,0,0,0};
	public static int clickedLevel;
	List<Rect> listRect = new List<Rect>();
	float limitLeft;
	float limitRight;
	bool isMovingLevel;
	bool isLeft;
	int direct = 0;
	public Texture previousButtonBg;
	public Texture nextButtonBg;
	// Use this for initialization
	void Start () 
	{
		if(GamePlay.version == Version.free)
		{
			maxSubLevels = new int[]{6,6,6,6,0,0,0,0,0,0};
		}
		else if(GamePlay.version == Version.demo)
		{
			maxSubLevels = new int[]{3,3,3,0,0,0,0,0,0,0};
		}
		if(GamePlay.isTesting)
		{
			subLevelsUnlocked = new int[]{1,1,1,1,1,0,0,0,0,0};
		}
		string version = PlayerPrefs.GetString("Current_Version");
		string currentVersion = GamePlay.version.ToString() + GamePlay.versionNumber;
		if(version != currentVersion)
		{
			PlayerPrefs.SetString("Current_Version",currentVersion);
			PlayerPrefs.SetString("Sublevel_Unlocked",string.Empty);
			PlayerPrefs.SetString("Star_Number",string.Empty);
		}
		else
		{
			//Load unlocked sub level
			if(PlayerPrefs.GetString("Sublevel_Unlocked") != string.Empty)
			{
				Debug.Log(PlayerPrefs.GetString("Sublevel_Unlocked"));
				string[] stringSublevel = PlayerPrefs.GetString("Sublevel_Unlocked").Split(new char[]{','},System.StringSplitOptions.RemoveEmptyEntries);
				for(int i=0; i<stringSublevel.Length; i++)
				{
					int temp = int.Parse(stringSublevel[i]);
					if(temp > subLevelsUnlocked[i])
						subLevelsUnlocked[i] = temp;
				}
			}
			//Load star numbers
			string starNumber = PlayerPrefs.GetString("Star_Number");
			if(starNumber != string.Empty)
			{
				Debug.Log(starNumber);
				string[] stringStars = starNumber.Split(new char[]{','},System.StringSplitOptions.RemoveEmptyEntries);
				starNumbers.Clear();
				for(int i=0; i< stringStars.Length; i++)
				{
					int temp = int.Parse(stringStars[i]);
					starNumbers.Add(temp);
				}
			}
		}
		maxLevel = maxSubLevels.Length;
		currentChoose = ChooseLevel.Level;
		levelButtonWidth = Screen.width/7.0f;
		levelButtonHeight = Screen.height/1.5f;
		//startLeft = (Screen.width-levelButtonWidth)/2.0f;
		currentLeft = Screen.width/14.0f;
		//currentPage = 0;
		limitLeft = - Mathf.CeilToInt(maxLevel/5)*Screen.width +Screen.width / 14.0f;
		limitRight =  Screen.width/14.0f;
		isLeft = false;
	}
	
	// Update is called once per frame
	float nextLeft = -9999;
	void OnGUI()
	{
		if(Screen.width <= 480)
			GUI.skin  = skin480;
		else
			GUI.skin = skin;
		if(currentChoose == ChooseLevel.Level)
		{
			DrawLevels();
		}			
		else
		{
			DrawSubLevelButtons();
		}				
		//DRAW UPGRADE BUTTON
		if(GamePlay.version != Version.full)
		{
			if(isShowingBox)
				DrawAnnounce();
			else
			{
				Rect upgradeRect = new Rect();
				upgradeRect.height = 80*Screen.height/480;
				upgradeRect.width = upgradeRect.height*3;
				upgradeRect.x = Screen.width/2-upgradeRect.width/2;
				upgradeRect.y = Screen.height-upgradeRect.height;
				if(GUI.Button(upgradeRect,"","upgrade_button"))
				{
					//LINK HERE
					Application.OpenURL("http://play.google.com/store/apps/details?id=com.unicorn.wb");
				}		
			}
		}
		
	}
	void Update () 
	{		
		 if(currentChoose == ChooseLevel.Level)
		 {
			 InputHandleInMainLevels();
		 }
		 else
		 {
			 InputHandleInSubLevels();
		 }		
	}
	
	void InputHandleInMainLevels()
	{
		
		if(Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if(Input.touchCount > 0)
			{
				float deltaX = Input.touches[0].deltaPosition.x;
				if(Mathf.Abs(deltaX) > 5f)
					direct =  deltaX > 0? 1: -1;
			}
		}
		else
		{
			if(Input.GetKeyDown(KeyCode.RightArrow))		
				direct = 1;
			if(Input.GetKeyDown(KeyCode.LeftArrow))
				direct = -1;
		}
		if(direct != 0)
		{
			nextLeft = MoveLevelButton(direct);
			if(direct == 1)
				isLeft = false;
			else isLeft = true;
			direct = 0;
		}
		if(isMovingLevel)
		{
			currentLeft = Mathf.Lerp(currentLeft,nextLeft,2 * Time.deltaTime);			
			if(Mathf.Abs(currentLeft - nextLeft) <= 0.5f)
				isMovingLevel = false;
			if(currentLeft <= limitLeft)
			{
				currentLeft = limitLeft;
			}
			else if(currentLeft >= limitRight)
			{
				currentLeft = limitRight;
			}
			
		}
	}
	
	float MoveLevelButton(int direct)
	{
		isMovingLevel = true;
		return currentLeft + 7.0f*levelButtonWidth*direct;
	}
	
	void DrawLevels()
	{
		GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),backgrounds[0],ScaleMode.StretchToFill);		
		float startNearestLeft = currentLeft;
		for(int i=0; i< maxLevel; i++)
		{			
			float left = 0;			
			if(i!= 0 && i%5 == 0)
			{
				left = startNearestLeft + 2*levelButtonWidth;
				startNearestLeft = left;
			}
			else if(i == 0)
			{
				left = startNearestLeft;
				startNearestLeft = left;
			}
			else
			{
				left = startNearestLeft + 1.25f * levelButtonWidth;
				startNearestLeft = left;
			}
		   	Rect rect = new Rect();
		    rect.width = levelButtonWidth;
		    rect.height = levelButtonHeight;
		    rect.x = left;
		    rect.y = 30.0f;
		    //GUI.Box(rect,"");
		    GUI.DrawTexture(rect,levelIcons[i]);
		    if(GUI.Button(rect,"","trans_button"))
		    {
				//if this level was unlocked, can click on it
				if(subLevelsUnlocked[i] >0)
				{
					Debug.Log("Level : "+i);
					clickedLevel = i;
	 		   		LoadSubLevel(clickedLevel);
				}
				else if(maxSubLevels[i] == 0)
				{
					//Show box announce that player cannot play that level
					isShowingBox = true;
				}
		    }
			//Draw locked, unlocked button
			Rect lockRect = new Rect();
			lockRect.width = rect.width-30;
			lockRect.height = lockRect.width;
			lockRect.x = left + 15;
			lockRect.y = rect.y + rect.height - lockRect.height-25;
			Texture2D texture = subLevelsUnlocked[i]>0?lockedIcons[0]:lockedIcons[1];
			//if this level was unlocked, show unlocked button				
			GUI.DrawTexture(lockRect,texture,ScaleMode.ScaleToFit);	
			
			if(subLevelsUnlocked[i] >0)
			{
				//Draw unlockedLevel/Level
				Rect unlockedRect = new Rect();
				unlockedRect.width = rect.width-10;
				unlockedRect.height = 40;
				unlockedRect.x = left + 5;
				unlockedRect.y = rect.y + rect.height - lockRect.height-30 - unlockedRect.height;
				GUI.Label(unlockedRect,subLevelsUnlocked[i] +"/"+maxSubLevels[i],"label_unlocked");
			}
		}
		Rect backButtonRect = new Rect(10, Screen.height * 0.85f,Screen.height*0.15f,Screen.height*0.15f);
		if(GUI.Button(backButtonRect,"","back_button"))
		{
			//Load other scene in here
			return;
		}
		if(isLeft)
		{
			if(GUI.Button(new Rect(0,Screen.height/2- 30,60,60),previousButtonBg,"trans_button"))
			{
				direct = 1;
			}
		}
		else
		{
			if(GUI.Button(new Rect(Screen.width-60,Screen.height/2- 30,60,60),nextButtonBg,"trans_button"))
			{
				direct = -1;
			}
		}
	}
	#region "DRAW SUB LEVEL"
	public Texture2D[] sublevelIcons;
	public Texture2D[] pageIcons;
	float subLevelButtonWidth;
	float currentSubLevelLeft;
	float startSubLevelLeft;
	int numberItemPerRow = 6;
	int numberRowPerPage = 3;
	int numberSubLevelItems = 280;
	int numberSubLevelPages;
	int currentSubLevelPage;
	bool isMovingSubLevel;
	float nextSubLevelLeft;
	List<int> tmpUnlockedSubLevel = new List<int>();
	float dY,dX;
	void LoadSubLevel(int parentLevel)
	{
		tmpUnlockedSubLevel.Clear();
		for(int i=0; i<subLevelsUnlocked[parentLevel];i++)
		{
			tmpUnlockedSubLevel.Add(i);
		}
		currentChoose = ChooseLevel.SubLevel;
		numberSubLevelItems = maxSubLevels[parentLevel];
		numberSubLevelPages = numberSubLevelItems/(numberItemPerRow * numberRowPerPage) + 1;
		subLevelButtonWidth = Screen.width/(numberItemPerRow * 1.25f + 0.5f);
		dY = (Screen.height - numberRowPerPage * subLevelButtonWidth)/(numberRowPerPage + 1);
		dX = (Screen.width - numberItemPerRow * subLevelButtonWidth)/(numberItemPerRow+1);
		startSubLevelLeft = subLevelButtonWidth * 0.25f;
		currentSubLevelLeft = startSubLevelLeft;
		nextSubLevelLeft = currentSubLevelLeft;
		currentSubLevelPage = 0;
	}
	void InputHandleInSubLevels()
	{		
		int direct = 0;
		//if the platform is android or iphone
		if(Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			//if player touch scene
			if(Input.touchCount > 0)
			{
				//calculate touch's delta position
				float deltaX = Input.touches[0].deltaPosition.x;
				//if player move touch an enough distance, assign direct
				if(Mathf.Abs(deltaX) > 15f)
					direct =  deltaX > 0? -1: 1;
			}
		}
		//else use mouse or keypad
		else
		{
			if(Input.GetKeyDown(KeyCode.RightArrow))		
				direct = 1;
			if(Input.GetKeyDown(KeyCode.LeftArrow))
				direct = -1;
		}
		if(direct !=0)
		{
			nextSubLevelLeft = MoveSubLevelButtons(direct);
			direct = 0;
		}		
		if(isMovingSubLevel)
		{
			currentSubLevelLeft = Mathf.Lerp(currentSubLevelLeft,nextSubLevelLeft,3 * Time.deltaTime);
			if(Mathf.Abs(nextSubLevelLeft - currentSubLevelLeft) <= 0.5f)
			{
				isMovingSubLevel = false;
			}
		}
	}
	float MoveSubLevelButtons(int direct)
	{
		isMovingSubLevel = true;
		currentSubLevelPage += direct;
		if(currentSubLevelPage < 0)
			currentSubLevelPage = 0;
		if(currentSubLevelPage > numberSubLevelPages-1)
			currentSubLevelPage = numberSubLevelPages-1;
		return startSubLevelLeft - currentSubLevelPage * Screen.width;		
	}
	bool Contanst<T>(T[] ts, T t)
	{
		foreach(T _t in ts)
		{
			if(t.Equals(_t))
				return true;
		}
		return false;
	}
	void DrawSubLevelButtons()
	{
		//first of all, draw background of scene
		GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),backgrounds[1],ScaleMode.StretchToFill);		
		float width = 20;
		float height = 20;
		//calculate left of page
		float leftOfPageIcon = Screen.width/2 - numberSubLevelPages/2 * width * 1.5f; 
		//draw each page
		for(int i=0; i< numberSubLevelPages; i++)
		{
			//left of page is current sub level left
			float leftOfPage = currentSubLevelLeft + i * Screen.width;
			if(leftOfPage > -Screen.width && leftOfPage < 2*Screen.width)
			{
				for(int row =0; row < numberRowPerPage; row++)
				{
					for(int col = 0; col < numberItemPerRow; col++)
					{
						int index = i * (numberItemPerRow*numberRowPerPage) + row * numberItemPerRow + col;
						if(index < numberSubLevelItems)
						{
							Texture2D texture = sublevelIcons[0];
							//Texture2D lockTexture = sublevelIcons[0];
							bool isLocked = true;
							if(tmpUnlockedSubLevel.Contains(index))
							{
								isLocked = false;
								texture = sublevelIcons[clickedLevel+1];						
							}
							//Draw sub level icon
							Rect rect = new Rect();
							rect.width = subLevelButtonWidth;
							rect.height = rect.width;	
							//calculate position of button
							rect.y = (dY + rect.height) * row + dY;						
							rect.x = leftOfPage + (dX + rect.width)*col;
							if(rect.x > -rect.width && rect.x < Screen.width)
							{
								//Draw button background
								GUI.DrawTexture(rect,texture);
								//Draw sub level label
								if(!isLocked)
									GUI.Label(rect,(index+1).ToString(),"label_number");
								//Draw sub level button
								if(GUI.Button(rect,"","trans_button") && !isLocked)
								{
									GamePlay.level = clickedLevel;
									GamePlay.subLevel = index;							
									Application.LoadLevel("Main");
								}	
								//Draw stars if have
								int starIndex = index;
								for(int k=0; k< clickedLevel; k++)
								{
									starIndex += maxSubLevels[k];
								}
								if(starIndex < starNumbers.Count)
								{
									Rect starRect = new Rect(rect.x-rect.width/5f,rect.y+rect.height-rect.width/3.5f,rect.width/5,rect.width/5);
									for(int star =0; star < 3; star++)
									{
										starRect.x += starRect.width*1.5f;
										Texture2D starIcon = star<starNumbers[starIndex]?starIcons[0]:starIcons[1];
										GUI.DrawTexture(starRect,starIcon);
									}
								}
							}
						}
					}
				}
			}
			
			Rect rectPage = new Rect();
			rectPage.y = Screen.height - subLevelButtonWidth/2;
			rectPage.x = leftOfPageIcon + width * i * 1.5f;
			rectPage.width = width;
			rectPage.height = height;
			Texture texturePage = pageIcons[0];
			if(i == currentSubLevelPage)
				texturePage = pageIcons[1];
			GUI.DrawTexture(rectPage, texturePage);
		}
		Rect backButtonRect = new Rect(10, Screen.height * 0.85f,Screen.height*0.15f,Screen.height*0.15f);
		if(GUI.Button(backButtonRect,"","back_button"))
		{
			currentChoose = ChooseLevel.Level;
			return;
		}
	}				
	#endregion
	void DrawAnnounce()
	{
		int oldDepth = GUI.depth;
		GUI.depth = -2;
		Rect rect = new Rect();
		rect.height = Screen.height * 10/24f;
		rect.width = rect.height * 1.5f;		
		rect.x = Screen.width/2 - rect.width/2;
		rect.y = Screen.height/2 - rect.height/2;
		GUI.Box(rect,"Opp!You must upgrade to full version to play this level!","confirm_box");
		rect.width = Screen.height*5/48f;
		rect.height = rect.width;
		rect.x = Screen.width/2 + rect.width;
		rect.y = Screen.height/2 + rect.height*1.25f;
		if(GUI.Button(rect,"YES","confirm_button"))
		{
			isShowingBox = false;
		}
		GUI.depth = oldDepth;
	}
}
