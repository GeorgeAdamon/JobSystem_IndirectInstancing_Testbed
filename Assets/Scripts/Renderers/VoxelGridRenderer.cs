using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(JobifiedVectorField))]
[DefaultExecutionOrder(100)]
public class VoxelGridRenderer : MonoBehaviour
{

    //============ PUBLIC VARIABLES ==========//
    [Tooltip("Reference to a JobifiedVectorField script")]
    public JobifiedVectorField voxelGridScript;

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

    private Bounds bounds;
    private float cellSize;
    private float prevCellSize;

    // GPU Buffers
    private ComputeBuffer positionBuffer;
    private ComputeBuffer directionBuffer;
    private ComputeBuffer argsBuffer;

    // Instanced Shader Arguments array
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    //============ MONOBEHAVIOUR METHODS ==========//
    void Awake()
    {
        voxelGridScript = GetComponent<JobifiedVectorField>();
    }

    void Start()
    {
        // Initialize
        instanceCount = voxelGridScript.voxelCount;
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();

    }

    void Update()
    {
        // Read the current instance count from the acceleration script
        instanceCount = voxelGridScript.XCount * voxelGridScript.YCount * voxelGridScript.ZCount;
        cellSize = voxelGridScript.CellSize;
    }

    void LateUpdate()
    {
        // If a change in the instance count or the submesh is detected, regenrate the Buffers
        if (cachedInstanceCount != instanceCount || cachedSubMeshIndex != subMeshIndex || prevCellSize != cellSize)
        {
            UpdateBuffers();
            // Cache the instancecount and the submesh index
            cachedInstanceCount = instanceCount;
            cachedSubMeshIndex = subMeshIndex;
            prevCellSize = cellSize;
        }

        // Update the Buffers with the new data for this frame
        directionBuffer.SetData(voxelGridScript._directions);

        // Pass the Buffers to the shader
        instanceMaterial.SetBuffer("directionBuffer", directionBuffer);
        instanceMaterial.SetMatrix("voxelGridMatrix", transform.localToWorldMatrix);

        // Render instance meshes
        Graphics.DrawMeshInstancedIndirect
            (instanceMesh,
            subMeshIndex,
            instanceMaterial,
            bounds,
            argsBuffer,
            0,
            new MaterialPropertyBlock(),
            UnityEngine.Rendering.ShadowCastingMode.Off);
    }

    void OnDestroy()
    {
        if (positionBuffer != null)
            positionBuffer.Dispose();

        if (directionBuffer != null)
            directionBuffer.Dispose();

        if (argsBuffer != null)
            argsBuffer.Dispose();
    }

    //============ CUSTOM PRIVATE METHODS ==========//

    /// <summary>
    /// Regenerates the ComputeBuffers. It should be called when a change in instanceCount is detected, and in the beginning of the script.
    /// </summary>
    private void UpdateBuffers()
    {
        float3 size = new float3(voxelGridScript.XCount, voxelGridScript.YCount, voxelGridScript.ZCount) * voxelGridScript.CellSize;
        bounds = new Bounds(size * 0.5f, size);

        // Ensure submesh index is in range
        if (instanceMesh != null)
        {
            subMeshIndex = Mathf.Clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1);
        }

        // Release the memory allocated for the Directions Buffer
        if (directionBuffer != null)
        {
            directionBuffer.Release();
        }
        // A float3 has size of 3 x 4bytes = 12 bytes
        directionBuffer = new ComputeBuffer(instanceCount, 12);


        // Release the memory allocated for the Positions Buffer
        if (positionBuffer != null)
        {
            positionBuffer.Release();
        }

        // A float3 has size of 3 x 4bytes = 12 bytes
        positionBuffer = new ComputeBuffer(instanceCount, 12);
        // Set the Buffer data and then pass the buffer to the shader
        positionBuffer.SetData(voxelGridScript._positions);
        instanceMaterial.SetBuffer("positionBuffer", positionBuffer);


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


    }


}

