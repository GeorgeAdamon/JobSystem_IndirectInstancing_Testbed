using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// Responsible for fast rendering of particle instances by passing their attributes as ComputeBuffers to a shader, 
/// using the DrawMeshInstancedIndirect command.
/// </summary>
[RequireComponent(typeof(JobifiedParticles))]
[DefaultExecutionOrder(50)]
public class InstancedParticleRenderer : MonoBehaviour {

    //============ PUBLIC VARIABLES ==========//
    [Tooltip("Reference to an AccelerationParallelFor script")]
    public JobifiedParticles accelerationScript;

    [Tooltip("The mesh to be instanced")]
    public Mesh instanceMesh;

    [Tooltip("The material with which to render the instanced meshes")]
    public Material instanceMaterial;

    [Tooltip("The submesh of the original mesh to render")]
    public int subMeshIndex = 0;


    //============ PRIVATE VARIABLES ==========//

    // The total number of particles
    private int instanceCount = 0;
    private int cachedInstanceCount = -1;
    private int cachedSubMeshIndex = -1;

    // GPU Buffers
    private ComputeBuffer positionBuffer;
    private ComputeBuffer colorBuffer;
    private ComputeBuffer argsBuffer;
    
    // Instanced Shader Arguments array
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };


    //============ MONOBEHAVIOUR METHODS ==========//
    void Awake()
    {
        // Caching of the particle script
        accelerationScript = GetComponent<JobifiedParticles>();
    }

    void Start()
    {
        // Initialize
        instanceCount = accelerationScript._objectCount;
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
    }

    private void Update()
    {
        // Read the current instance count from the acceleration script
        instanceCount = accelerationScript._objectCount;
    }

    void LateUpdate()
    {

        // If a change in the instance count or the submesh is detected, regenrate the Buffers
        if (cachedInstanceCount != instanceCount || cachedSubMeshIndex != subMeshIndex)
        {
            UpdateBuffers();
        }

        // Update the Buffers with the new data for this frame
        positionBuffer.SetData(accelerationScript._Positions);
        colorBuffer.SetData(accelerationScript._Colors);

        // Pass the Buffers to the shader
        instanceMaterial.SetBuffer("_positionBuffer", positionBuffer);
        instanceMaterial.SetBuffer("_colorBuffer", colorBuffer);

        // Render instance meshes
        Graphics.DrawMeshInstancedIndirect
            (
            instanceMesh, 
            subMeshIndex, 
            instanceMaterial, 
            new Bounds(Vector3.zero, new Vector3(1000.0f, 1000.0f, 1000.0f)), 
            argsBuffer,
            0,
            new MaterialPropertyBlock(),
            UnityEngine.Rendering.ShadowCastingMode.Off);

    }

    void OnDestroy()
    {
        // Disposing the ComputeBuffers when the game ends
        if (positionBuffer != null)
        {
            positionBuffer.Dispose();
        }

        if (argsBuffer != null)
        {
            argsBuffer.Dispose();
        }
    }

    //============ CUSTOM PRIVATE METHODS ==========//
    /// <summary>
    /// Regenerates the ComputeBuffers. It should be called when a change in instanceCount is detected, and in the beginning of the script.
    /// </summary>
    void UpdateBuffers()
    {
        // Ensure submesh index is in range
        if (instanceMesh != null)
        {
            subMeshIndex = math.clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1);
        }

        // Release the memory allocated for the Positions Buffer
        if (positionBuffer != null)
        {
            positionBuffer.Release();
        }

        // Release the memory allocated for the Color Buffer
        if (colorBuffer != null)
        {
            colorBuffer.Release();
        }

        // A float3 has size of 3 x 4bytes = 12 bytes
        positionBuffer = new ComputeBuffer(instanceCount, 12);
        // A float4 has size of 4 x 4bytes = 12 bytes
        colorBuffer = new ComputeBuffer(instanceCount, 16);

        // Set the Buffer data and then pass the buffer to the shader
        positionBuffer.SetData(accelerationScript._Positions);
        instanceMaterial.SetBuffer("_positionBuffer", positionBuffer);

        // Set the Buffer data and then pass the buffer to the shader
        colorBuffer.SetData(accelerationScript._Colors);
        instanceMaterial.SetBuffer("_colorBuffer", colorBuffer);

        // Update the Indirect arguments
        if (instanceMesh != null)
        {
            args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)instanceCount;
            args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
            args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        argsBuffer.SetData(args);

        // Cache the instancecount and the submesh index
        cachedInstanceCount = instanceCount;
        cachedSubMeshIndex = subMeshIndex;
    }


}

