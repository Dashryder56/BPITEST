# **Joe Reilly - BPI Unity Project Technical Documentation**

## **Overview**

This Unity project implements a modular screen management system that allows for smooth transitions between different UI screens . It includes a dynamic content gallery with streaming image assets, preloaded on play for a smoother experience, an attract screen with a dynamic slideshow pulling in content from streaming assets, and a flexible, data driven screen management system.

## **Architecture**

The project follows a modular, **data-oriented design** with a focus on separation of concerns. The key components include:

### **1\. Screen Management System (State Machine Pattern)**

The screen system is designed using the **State Machine Pattern**, where different screens act as states that can be transitioned between.

* **`ScreenManager`**: Handles screen transitions, user inactivity timeouts, and screen change requests.  
* **`IScreenState`**: Interface for defining screens that can be entered and exited.  
* **`IFadeableScreen`**: Interface for screens that support fade-in and fade-out transitions (currently only the attract screen).

### **2\. Screens**

* **`AttractScreen`**: Displays a looping slideshow and fades between dynamically loaded images of any amount (images set in the **`AttractScreenData`** file)  
* **`ContentScreen`**: Displays a set of buttons for accessing dynamic image galleries and manages a modal popup for viewing images (dynamic data for the modal and image galleries set in the **`ContentScreenData`** file.)

### **3\. Image Management**

* **`ImageLoader`**: Handles preloading and loading of the streaming asset images, dynamically.  
* **`UIImageFactory`**: Creates UI images dynamically and attaches them to the canvas.

### **4\. UI Utilities**

* **`TitleTextManager`**: Handles the creation and updating of a title text field with an outline effect.

### **5\. Data-Driven Design**

The screens are **data-driven**, with all configurations and content handled by **ScriptableObject data files**. This approach allows flexibility in updating content without modifying code:

* **`ScreenManagerData`** : Defines screen transition duration and inactivity timeout.  
* **`AttractScreenData`** : Stores a **dynamic list of slideshow images**, the title text, slideshow intervals, and fade durations.  
* **`ContentScreenData`** : Defines **a dynamic set of modal images** for different buttons, the button titles, and image descriptions.  Also configurable is an auto-close timer for modal inactivity.

## **Key Functionalities**

### **Screen Transitions & Inactivity Handling**

* Screens are managed by `ScreenManager`, which also tracks user inactivity to reset the experience after a certain timeframe.  
* If the user is inactive for a specified duration, the system automatically returns to the `AttractScreen`.  
* Screens can implement the `IFadeableScreen` interface to add smooth fade transitions.

### **Attract Screen (Slideshow)**

* Displays a set of dynamically loaded-in images in a loop, fading between them.  
* Listens for user taps to transition to the `ContentScreen`.  
* Uses `ImageLoader` to load images dynamically from a **configurable list** stored in `AttractScreenData`.

### **Content Screen (Gallery)**

* Displays a **pre-set** 3 buttons that open different galleries.  
* When a button is clicked, a modal appears showing dynamically preloaded images from the selected gallery.  
* Uses `ImageLoader` to preload streaming asset images in the background based on data from `ContentScreenData`.

### **Image Preloading & Loading**

* The `ImageLoader` component preloads images at application startup for quick access.  
* If an image is not preloaded, it attempts to load it dynamically.  
* `UIImageFactory` is used to generate UI images dynamically.

## **How to Use & Extend**

### **Adding a New Screen**

1. Create a new class implementing `IScreenState`.  
2. Implement the `EnterScreen` and `ExitScreen` methods.  
3. Add the screen to the scene hierarchy.  
4. Register the screen in `ScreenManager`.  
5. Call `ScreenManager.RequestScreenChange("YourScreenName")` to transition to it.

### **Customizing the Attract Screen**

* Modify the `AttractScreenData` scriptable object to define slideshow images, slideshow image duration for each image, and slideshow transition times.  
* In `ScreenManagerData,` Adjust the fade-in and fade-out duration of the screen transitions, as well as screen timeout duration to return to the `AttractScreen`.

### **Adding More Galleries to Content Screen**

* Modify `ContentScreenData` to modify the **buttons and gallery items dynamically,** including names of buttons, images to load and use, and image titles.  
* Due to time constraints, I kept this screen to just the requested 3 buttons, although as the data is all being loaded in dynamically, would be an easy adjustment to allow dynamic button adding to the page, populating any new buttons with the dynamically loaded content.  
* Adjust the timeout variable in `ContentScreenData` to handle how long the duration is before the Modals close on their own.  
* Ensure images are placed in the `StreamingAssets` folder for dynamic loading for both `ContentScreen` and `AttractScreen`.

### **Enabling Image Preloading**

* Call `EnablePreloading()` on `ImageLoader` to enable preloading (currently used on the `ContentScreen` to load the images needed while the `AttractScreen` is running.

## **Conclusion**

This system provides a modular, flexible, and extendable framework for managing UI screens in a Unity application. Developers can easily add new screens, customize the UI, and optimize performance through **data-driven design and the State Machine Pattern**.

