# Unity Job System & Indirect Instancing Testbed
Testbed project exploring the performance of Unity's Job System/Burst compiler combined with GPU mesh instancing.

Currently 2 modes are explored:
* **Particle Swarm**
<br/>_524.288 Particles_
![524.288 Particles](Recordings/gif_animation_007.gif?raw=true "524.288 Particles")
* **Animated Vector Field**
<br/>_262.144 Vectors_
![262.144 Vectors](Recordings/gif_animation_010.gif?raw=true "262.144 Vectors")

### Performance:
Swarm: **524.288 Particles** particles at 60-70 fps (CPU: Intel i7 5930k, GPU: Nvidia GTX 970)
