# Unity Job System & Indirect Instancing Testbed
Testbed project exploring the performance of Unity's Job System/Burst compiler combined with GPU mesh instancing.

Currently 2 modes are explored:
* Particle Swarm
![512.000 Particles](Recordings/gif_animation_007.gif?raw=true "512.000 Particles")
* Animated Vector Field
![262.144 Vectors](Recordings/gif_animation_010.gif?raw=true "262.144 Vectors")

### Performance:
Swarm: **512.000** particles at 60-70 fps (CPU: Intel i7 5930k, GPU: Nvidia GTX 970)
