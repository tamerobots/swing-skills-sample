using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.spacepuppy;


public class RatioSetter : MonoBehaviour {

	public string reminderTR = "set the hard coded global wanted aspect ratio in the RatioSetter.cs code";
	// ********************************** be sure to search for 1.7777777 in the code if you change this because ppdc uses this for screenshots too.
	private float _wantedAspectRatio = 1.7777777f; // this is 16:9, which is the best. yes the '1.7777777 not 1.77' thing matters. 
	public bool landscapeModeOnly = true;
	static public bool _landscapeModeOnly = true;
	static float wantedAspectRatio;
	static Camera cam;
	static Camera backgroundCam;

	void Awake () {
		_landscapeModeOnly = landscapeModeOnly;
		cam = GetComponent<Camera>();
		if (!cam) {
			cam = Camera.main;
			Debug.Log ("Setting the main camera " + cam.name);
		}
		else {
			Debug.Log ("Setting the main camera " + cam.name);
		}

		if (!cam) {
			Debug.LogError ("No camera available");
			return;
		}
		wantedAspectRatio = _wantedAspectRatio;
		SetCamera();
	}

	public static void SetCamera () {
		float currentAspectRatio = 0.0f;
		if(Screen.orientation == ScreenOrientation.LandscapeRight ||
			Screen.orientation == ScreenOrientation.LandscapeLeft) {
			//Debug.Log ("Landscape detected...");
			currentAspectRatio = (float)Screen.width / Screen.height;
		}
		else {
			//Debug.Log ("Portrait detected...?");
			if(Screen.height  > Screen.width && _landscapeModeOnly) {
				currentAspectRatio = (float)Screen.height / Screen.width;
			}
			else {
				currentAspectRatio = (float)Screen.width / Screen.height;
			}
		}
		// If the current aspect ratio is already approximately equal to the desired aspect ratio,
		// use a full-screen Rect (in case it was set to something else previously)

		//Debug.Log ("currentAspectRatio = " + currentAspectRatio + ", wantedAspectRatio = " + wantedAspectRatio);

		if ((int)(currentAspectRatio * 100) / 100.0f == (int)(wantedAspectRatio * 100) / 100.0f) {
			cam.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
			if (backgroundCam) {
				Destroy(backgroundCam.gameObject);
			}
			return;
		}

		// Pillarbox
		if (currentAspectRatio > wantedAspectRatio) {
			// TR EDIT - Don't pillarbox.
			//float inset = 1.0f - wantedAspectRatio/currentAspectRatio;
			//cam.rect = new Rect(inset/2, 0.0f, 1.0f-inset, 1.0f);
		}
		// Letterbox
		else {
			float inset = 1.0f - currentAspectRatio/wantedAspectRatio;
			cam.rect = new Rect(0.0f, inset/2, 1.0f, 1.0f-inset);
		}
		if (!backgroundCam) {
			// Make a new camera behind the normal camera which displays black; otherwise the unused space is undefined
			backgroundCam = new GameObject("BackgroundCam", typeof(Camera)).GetComponent<Camera>();
			//backgroundCam.renderingPath = RenderingPath.Forward;
			backgroundCam.depth = int.MinValue;
			backgroundCam.clearFlags = CameraClearFlags.SolidColor; //this is causing the tiled GPU warning but I dont think its bad enough to need to fix. At least in theory?
			//Color bgColor = new Color (139/255, 255/255, 77/255, 255/255);
//			Color bgColor = new Color (0, 0.301f, 0.098f, 1); 
//			ColorHSV currentHSV = new ColorHSV (Color.white);
//			currentHSV.h = 99;
//			currentHSV.s = 255;
//			currentHSV.v = 39;
//			Color bgColor = ColorHSV.ToColor (currentHSV);
			// experiments with making the bars the same as the background colour (or any non-black color) made it look wrong, as 
			// though the bars were supposed to be part of the level - but realised I had to do this as it was the only option to enforce the ratio.
			// ended up making it so that you can pick a colour for the camera background in the character information script, then its a different
			// colour for each character. TR
			GameObject CurrentPlayer = GameObject.FindWithTag ("CurrentPlayer");
			if (CurrentPlayer != null) {
				CharacterInformationScript cis = CurrentPlayer.GetComponent<CharacterInformationScript> ();
				if (cis != null) {
					backgroundCam.backgroundColor = cis.letterboxColor;
				}
			} else {
				Debug.LogError ("could not find CurrentPlayer to set ratio background with!");
				//default
				backgroundCam.backgroundColor = Color.white; //don't make it black or it will look like a cutscene. White is safe.
			}
			//backgroundCam.backgroundColor = bgColor;
			backgroundCam.cullingMask = 0;
		}
	}

	public static int screenHeight {
		get {
			return (int)(Screen.height * cam.rect.height);
		}
	}

	public static int screenWidth {
		get {
			return (int)(Screen.width * cam.rect.width);
		}
	}

	public static int xOffset {
		get {
			return (int)(Screen.width * cam.rect.x);
		}
	}

	public static int yOffset {
		get {
			return (int)(Screen.height * cam.rect.y);
		}
	}

	public static Rect screenRect {
		get {
			return new Rect(cam.rect.x * Screen.width, cam.rect.y * Screen.height, cam.rect.width * Screen.width, cam.rect.height * Screen.height);
		}
	}

	public static Vector3 mousePosition {
		get {
			Vector3 mousePos = Input.mousePosition;
			mousePos.y -= (int)(cam.rect.y * Screen.height);
			mousePos.x -= (int)(cam.rect.x * Screen.width);
			return mousePos;
		}
	}

	public static Vector2 guiMousePosition {
		get {
			Vector2 mousePos = Event.current.mousePosition;
			mousePos.y = Mathf.Clamp(mousePos.y, cam.rect.y * Screen.height, cam.rect.y * Screen.height + cam.rect.height * Screen.height);
			mousePos.x = Mathf.Clamp(mousePos.x, cam.rect.x * Screen.width, cam.rect.x * Screen.width + cam.rect.width * Screen.width);
			return mousePos;
		}
	}
}
