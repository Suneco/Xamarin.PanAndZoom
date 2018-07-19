# Xamarin.Forms Interactive ImageView
This repository contains a small Xamarin.Forms example project which contains a custom ImageView which
can be panned and zoomed with. The custom view is implemented using the platforms native classes.

The project consists of the following two important classes:

- **PanZoomView:** A simple helper class which extends from `Image`
- **PanZoomImageRenderer:**  Platform specific exported renderer which contains all logic for the image

 For Android the native `Canvas` is being used. For iOS the native `UIImageView` is used.
 
 For the sake of user experience, the renderer will limit scaling to a defined limit. The renderer
 will also prevent the image from moving out of the screen boundaries.
 
![alt text](images/ios.gif "Screen recording")
