Made by:
Jack Rosenberg (8287775) and Silas Peters (4419197) (both Honours students)

Camera:
	- Each camera is setup with a Front, Up and calculated Right vector, along with other
	  basic features and a Rectangle DisplayRegion which represents what part of the display
	  is renderedd by the camera. This way, the screen can be devided between multiple cameras.
	  When rotating or moving the camera, the vectors and position are manipulated.
	- The camera contains a Lens, which is explained in more detail in the source code. However,
	  it follows the camera because it is based on properties of the Camera, and efficiently
	  calculates what direction a ray must be sent to in order to 'reach' a pixel in the lens.
	  That's the whole function of the lens: an easy way to calculate where to shoot a ray to.
	- The previous things mentioned are part of the abstract class BasicCamera. There are two
	  implementations of it: MainCamera and DebugCamera. They only differ in their version of the
	  'RenderImage' method.
	    - The MainCamera shoots rays using the lens, in a MULTITHREADED way. It divides the region
	      it draws in coloums which are spread over independent threads. Per thread it shoots a ray
	      per pixel (using the lens to calcuale the direction) and determines the color using
	      Scene.CalculatePixel(), which performs all the intersections etc.
	    - The DebugCam renders using Rasterization: it draws on the screen per object.
	- The FOV of the camera can be set by changing the value of Lens.Distance, quite realistic.
	  When creating the camera a certain amount of degrees can be given as parameter which will
	  be converted to a distance.
	- The camera (and all its vectors, and thus also its lens) can be rotated, moved, and zoomed in
	  using keybinds: WASD, arrows, Z and X, respectively. This way all orientations and positions
	  are possible and you can set it to a target.
	  Even better: Raytracer.cs, used to set up the scene, allows for multiple camera stances.
	  In one stance you might add 1 camera, but in the other 2 cameras and a debug cam. All cams
	  can be set up in the way you like, the program picks it up as easy as trash, because we work
	  environmental friendly. With the spacebar you can cycle through the stances, and the current
	  stance index is displayed in the top left of the scean.
Primitives:
    - Planes and spheres can be placed and rendered as you please. Multiple (types of) instantions
      can be added to the scene in Raytracer.cs. Try it out!
      All objects are promised to contain a material and methods like GetNormalAt(point) and
      TryIntersect.
    - As mentioned, each single object can be tested for an intersection with a ray using TryIntersect().
      TryIntersect returns a boolean which tells if there was an intersection. Also, there is
      an out parameter IntersectionInfo. Thus, at all times intersection information is available
      as well, which may optionally be assesed when the method itself returns true. Otherwise it
      still holds information yet unreliable or incomplete, but that is never used.
      In short, intersections are tested efficiently by following the TryX hand-rule: it lets
      you know if it was succesful and more information may be assesed base on that outcome.
    - When a ray is shot to check for any intersection at all, for all objects, Scene.TryIntersect
      is run which in turn runs Object.TryIntersect (mentioned above). As the name suggests,
      Scene.TryIntersecet follows the same principle and is the main point for intersections in
      the scene.
Lights:
    - A scene contains LightSources as well as primitives. A lightsource has a position and a color.
      You can add as many as you like. When Scene.CalculatePixel is run, all the intersection and
      color logics are tested. Please refer to the sourcecode comments. Ligtsources are included
      and tests are run for all storered lightsources.
Materials:
    - Each object has a Material struct. This struct simply represents all properties like the
      diffusion and specular coëfficient, whether its a mirror or refractive object etc.
Application:
    - As mentioned above, there are multiple keybinds to manipulate the camera stances.
Debug Output:
    - As handed in, you can press spacebar to switch to a camera stance which includes the debug
      cam. De debug cam shows the spheres, camera, lens, lightsources and primary rays.

Extra assignments:
    - Multithreading: as can be seen in MainCamera.RenderImage() in RaytracerComponents.cs,
      the rendering is divided between an arbitrary number of threads, as the screen is
      divided in columns.
    - Refraction: an object can be fully refractive when its type is set to "Refractive".
    
Honours assignment:
    - We would like to mention that we discovered that Random.NextDouble() can run out of new values
      after enough values have been generated. Thus, our pathtracer which is dependend on random values
      stops working after a second or so. This took us a long time to discover and thus impacted our final
      result. We hope that that can be taken into consideration.

Sources and materials used: (when code is used, it will be mentioned in the sourceode)
    - OpenTK 3.x (as per template)
    - Lecture notes
        (code used, CTRL+F for 'lecture notes')
    - https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-plane-and-ray-disk-intersection
        (code used, CTRL+F a part of the URL)
    Path-tracing:
    - https://hugopeters.me/posts/9/
        (no code used)
    - https://www.scratchapixel.com/lessons/3d-basic-rendering/global-illumination-path-tracing
        (no cude used)