# Spinning Cube

This is a very basic 3D simulation that I made as part of a programming challenge.  
  
The hardest part was determining draw depth for the lines between points. 
Since Monogame (and Monogame Extended) are both designed with 2D in mind,
there is no native support drawing 3D vectors or shapes.   
  
My solution here was to default to a simple heuristic that works for the majority of cases : 
3D vectors are truncated into 2D vectors, and their draw layer is determined
by the maximum z-coordinate of the two edge points.  
  
Unfortunately the drawback is that 
for any given line, its entire length will be rendered at the exact same draw depth.

## Controls
The "application" has very elementary controls for debugging purposes : 
- Escape : Quit the application.  
- Space : Pause/Unpause the rotation of the cube.
- Left Arrow : Decrease the rotation speed (cannot be negative).
- Right Arrow : Increase the rotation speed.
