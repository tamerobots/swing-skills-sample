# Swing Skills (Sample Code)
Small samples of the C# code for my 'Swing Skills' game for iOS/Android.

![alt text](https://raw.githubusercontent.com/tamerobots/swing-skills-sample/master/banner.png "Swing Skills Logo")

I spent several years on this project and wrote 10,000+ lines of code for it, so I am not willing to fully open source it just yet, but I thought it would be good to show some of the code on my github for potential employers. If you're a game developer and you find anything you like, please feel free to use it.

### Technologies used
C#, Unity3D, MonoDevelop, Qubicle. 

### Design
I designed this game with a Model-View-Controller + Singleton architecture, separating the in-game display logic from the underlying data. 

I had a Persistent Game Object acting as a Singleton object that communicated data and guided the user between different game scenes, and made the code as scene-independent as possible to allow for code reuse across the game.

Each entity within the game, whether it be a character, a level, a level section, an outfit for a character, a mystery item, or a collectible, were all given separate classes with individual properties and shared/unique methods.

Performance was always a priority, so automated testing and building processes were added to make sure that the game was performant on lots of different devices and emulators without a degrade in quality or performance. I made a macOS server to build any new versions I made, deployed to using Git.

With games, overall filesize is better the lower it is, as it reduces the chance that a casual player (especially in areas where internet access is slow) may abandon your game when they see a large filesize to download. Even with several plugins and a wide range of functionality I still managed to keep it at 40MB, well below the 100MB android limit.

I also made all the text in the game translation-friendly, so it could all be translated easily if it ever became popular.

The game also implements (for both iOS and Android):
* Achievements
* Leaderboards
* In-app purchases 
* Rewarded ads
* Sharing screenshots and scores

### Samples included
* **mainSceneScript.cs** - this runs in the main play scene when you play through the level. It handles all sorts of different in-game events, including your character dying and then restarting, marshalling the camera, background and level managers to do their job properly, and a myriad of other tasks. It also includes code for making an invisible explosion(!) that propels bits of your character towards the camera, wherever the camera may be.
* **AttachmentTriggerBoxScript.cs** - this is a special piece of code for an in-game object, the AttachmentTriggerBox. This is something that acts as a fixed point that you can attach your rope to and, when triggered, has an effect - for example, some coins show or a door opens. 
* **GameObjectExtensions.cs** - various methods to act on several different game objects, including applyOutfit(), which recursively changes the mesh texture of the individual body parts of the character in order to show different outfits or even characters.
* **RatioSetter.cs** - fancy code to detect the screen size of the device and setup a Ratio with letterboxing so that all players have the same experience regardless of device, and no one has an advantage over anyone else.
* **collectibleScript.cs** - code to dictate action once a player collects an object, like a coin or a mystery item.
* **numberExtensionsTR.cs** - some Time handling code for use in different parts of the project.
* **runtimeVariables.cs** - variables that run in memory to avoid unnecessary disk usage

### Links
* [Download Swing Skills for iOS](https://apps.apple.com/app/swing-skills-rope-swing-game/id1373273640)
* [Download Swing Skills for Android](https://play.google.com/store/apps/details?id=com.heavyfrog.ropegame)
* [SwingSkillsGame.com](https://swingskillsgame.com)
* [Swing Skills Project Page](https://www.tamerobots.com/projects/swing-skills)

### Youtube Trailer

<a href="http://www.youtube.com/watch?feature=player_embedded&v=kOu-VKq5jCI
" target="_blank"><img src="http://img.youtube.com/vi/kOu-VKq5jCI/0.jpg" 
alt="Swing Skills Trailer" border="10" /></a>

Thanks for reading!

![alt text](https://raw.githubusercontent.com/tamerobots/swing-skills-sample/master/toucanTyping.gif "Toucan Typing")
