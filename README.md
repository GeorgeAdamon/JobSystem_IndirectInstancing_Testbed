# Unity Job System & Indirect Instancing Testbed
Testbed project exploring the performance of Unity's Job System / Burst compiler combined with GPU mesh instancing (using the [DrawMeshInstancedIndirect](https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstancedIndirect.html) function, and passing Job System's NativeArrays directly to the shaders as ComputeBuffers).

The functionality provided by this project is slowly becoming obsolete, as the Visual Effect Graph can handle **at least** an order of magnitude more particles at comparable frame rates.  
It's still a good example for basic DOTS data generation & useful for situations where the use of GPU / VFX Graph is not preferred.  

_Tested on Unity 2022.1.2f1_

## Index
* [Jobified Scripts](https://github.com/GeorgeAdamon/InstancedIndirect_And_JobSystem/tree/master/Assets/Scripts/Jobified)
* [Instanced Indirect Renderers](https://github.com/GeorgeAdamon/InstancedIndirect_And_JobSystem/tree/master/Assets/Scripts/Renderers)
* [Shaders](https://github.com/GeorgeAdamon/InstancedIndirect_And_JobSystem/tree/master/Assets/Shaders)

## Modes
Currently 2 modes are explored:
* **Particle Swarm**
<br/>_524.288 Particles_
<br/>[**See it in action**](https://vimeo.com/327553555)
![524.288 Particles](Recordings/Particles.gif?raw=true "524.288 Particles")
* **Animated Vector Field**
<br/>_262.144 Vectors_
![262.144 Vectors](Recordings/gif_animation_010.gif?raw=true "262.144 Vectors")

## Performance:
#### CPU: Intel i7 5930k, GPU: Nvidia GTX 970
#### Swarm:<br/>
**524.288 Particles** particles at 60-70 fps <br/>
**1.048.576 Particles** particles at 30 fps

## Packages Used
* [Unity.Jobs](https://docs.unity3d.com/Packages/com.unity.jobs@0.0/manual/index.html)
* [Unity.Collections](https://docs.unity3d.com/Packages/com.unity.collections@0.0/api/Unity.Collections.html)
* [Unity.Mathematics](https://docs.unity3d.com/Packages/com.unity.mathematics@1.0/manual/index.html)
* [Unity.Burst](https://docs.unity3d.com/Packages/com.unity.burst@1.0/manual/index.html)
* [Unity.PostProcessing](https://docs.unity3d.com/Packages/com.unity.postprocessing@2.1/manual/index.html)

## To Do
* Add support for HDRP and URP
* [Particle Swarm] Implement spatial binning using the Concurrent NativeMultiHashMap collection.
* [Particle Swarm] Implement flocking / Boid behaviour.
* [Particle Swarm] Implement Unity's ECS (Entity Component System)
