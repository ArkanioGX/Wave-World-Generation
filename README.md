# Wave Function Collapse Research Project
## Introduction
This research project will be about using wave function collapse to generate world/structures in video games using the game engine Unity

## State of the art
The Wave Function Collapse is an algorithm that is inspired by Quantum Mechanics ([Wave Function Collapse](https://en.wikipedia.org/wiki/Wave_function_collapse)) it is already used for procedural generation like Townscaper or Bad North and my objective for this research will be to make procedural level in game feel more organic

## Approach
To begin with this research i will make the wave Function Collapse (that will surely the hardest part) works on a 2D Grid where it will apply a single color pixel. The step after will be to make it work in a 2D Tileset and then add a third dimension to have 3D World

## Analysis
First Step of the project was to make the script able to read an image and collect all the possible patterns it was made very fast but now to put those patterns to use and now is the difficult part how can i make the usable patterns update for each position update without taking too much time since the project is aimed for a video game use case long time will be prohibited.

So i had to make Precision a variable to tell to the program how much the information propagate at the risk of getting error (a tile with no possible pattern usable) causing the final result to be obsolete and needed to be redone
The devellopment of this feature took some trial and error to finally get an image to be generated but the way i did was inconsistent so i tried multiple way to propagate information without taking too much time but all of them failed

## Source
-https://youtu.be/rI_y2GAlQFM?si=rhGZ2m9SaP3oQZsb

-https://github.com/mxgmn/WaveFunctionCollapse

-https://youtu.be/2SuvO4Gi7uY?si=5yd7rgJZjvlCAla8


<sub> This research project gave me nightmares :') </sub>
