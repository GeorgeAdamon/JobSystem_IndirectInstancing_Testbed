using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

/// <author> George Adamopoulos </author>
/// <summary> 
/// A dynamic VectorField calculated using Unity's DOTS(Data Oriented Technology Stack).
/// More specifically, the Job System, the Burst Compiler and the Unity.Mathematics namespace
/// </summary>
/// 
[DefaultExecutionOrder(99)]
public class JobifiedVectorField : MonoBehaviour
{
    //========== GRID RELATED VARIABLES ==========//
    [Header("Size & Resolution Parameters")]
    [Range(1,100)] public int XCount= 100;
    [Range(1, 100)] public int YCount= 100;
    [Range(1, 100)] public int ZCount= 100;
    [Range(0.01F,10)] public float CellSize;
    private float prevCellSize;
    public int voxelCount;
    private int cachedVoxelCount;

    //========== NOISE FIELD RELATED VARIABLES ============//
    [Header("Directional Noise Field Parameters")]
    [Range(0.0001f, 0.01f)] public float NoiseScaleX;
    [Range(0.0001f, 0.01f)] public float NoiseScaleY;
    [Range(0.0001f, 0.01f)] public float NoiseScaleZ;

    [Range(0.01f, 2)] public float NoiseSpeedX;
    [Range(0.01f, 2)] public float NoiseSpeedY;
    [Range(0.01f, 2)] public float NoiseSpeedZ;
    
    //========== JOB SYSTEM RELATED VARIABLES ( NativeArrays & JobHandles) ============//
    NativeArray<Voxel> Voxels;
    public NativeArray<float3> _positions;
    public NativeArray<float3> _directions;
    public JobHandle updateVoxelsJobHandle;
    public JobHandle createVoxelsJobHandle;

    //============ MONOBEHAVIOUR METHODS ==========//
    void Awake()
    {
        voxelCount = XCount * YCount * ZCount;
        cachedVoxelCount = voxelCount;
        
        Voxels = new NativeArray<Voxel>(voxelCount, Allocator.Persistent);
        _positions = new NativeArray<float3>(voxelCount, Allocator.Persistent);
        _directions = new NativeArray<float3>(voxelCount, Allocator.Persistent);

        CalculateVoxels();
    }

    void Update()
    {
        voxelCount = XCount * YCount * ZCount;

        // Recreate the grid if necessary
        if (voxelCount != cachedVoxelCount || prevCellSize != CellSize)
        {
            CalculateVoxels();
            cachedVoxelCount = voxelCount;
            prevCellSize = CellSize;
        }

        if (createVoxelsJobHandle.IsCompleted)
        {
            // Create the job
            var fillVoxelsJob = new FillVoxels
            {
                voxels = Voxels,
                directions = _directions,
                time = Time.time,
                noiseScale = new float3(NoiseScaleX, NoiseScaleY, NoiseScaleZ),
                noiseSpeed = new float3(NoiseSpeedX, NoiseSpeedY, NoiseSpeedZ),
                _matrix = transform.localToWorldMatrix
            };

            //Schedule the job
            updateVoxelsJobHandle = fillVoxelsJob.Schedule(XCount * YCount * ZCount, 32);
        }

    }
   
    void LateUpdate()
    {
        // Complete the job at the very end of each frame
        updateVoxelsJobHandle.Complete();

    }
 
    void OnDestroy()
    {
        // Manually release all the memory allocated by the NativeArrays when this script is destroyed
        Voxels.Dispose();
        _positions.Dispose();
        _directions.Dispose();
    }

    //============ CUSTOM PRIVATE METHODS ==========//
    private void CalculateVoxels()
    {
       Voxels.Dispose();
       _positions.Dispose();
        _directions.Dispose();

        Voxels = new NativeArray<Voxel>(voxelCount, Allocator.Persistent);
        _positions = new NativeArray<float3>(voxelCount, Allocator.Persistent);
        _directions = new NativeArray<float3>(voxelCount, Allocator.Persistent);

        var createVoxelsJob = new CreateVoxels
        {
            voxels = Voxels,
            positions = _positions,
            directions = _directions,
            width = XCount,
            height = YCount,
            size = CellSize
        };

        createVoxelsJobHandle = createVoxelsJob.Schedule(voxelCount, 32);

        createVoxelsJobHandle.Complete();


    }

    //============ JOB DEFINITIONS ==========//
    [BurstCompile]
    struct CreateVoxels : IJobParallelFor
    {
        [WriteOnly] public NativeArray<Voxel> voxels;
        [WriteOnly] public NativeArray<float3> positions;
        [WriteOnly] public NativeArray<float3> directions;

        [ReadOnly] public float3 noiseScale, noiseSpeed;
        [ReadOnly] public int width, height;
        [ReadOnly] public float size;

        public void Execute(int index)
        {
            VectorFieldUtils.i_to_XYZ(index, width, height, out int x, out int y, out int z);
            float3 pos = new float3(x,y,z)*size;
            float3 _noise = VectorFieldUtils.DirectionalNoise(pos, noiseScale, noiseSpeed, 0);

            voxels[index] = new Voxel
            {
                X = x,
                Y = y,
                Z = z,
                i = index,
                position= pos,
                velocity = _noise
        };

            positions[index] = pos;
            directions[index] = _noise;
        }
    }

    [BurstCompile]
    struct FillVoxels : IJobParallelFor
    {
        public NativeArray<Voxel> voxels;
        public NativeArray<float3> directions;
        public float time;
        public float3 noiseScale, noiseSpeed;
        public float4x4 _matrix;

        public void Execute(int index)
        {
            Voxel vox = voxels[index];
            vox.velocity = VectorFieldUtils.DirectionalNoise( math.mul(_matrix, new float4(vox.position,1)).xyz, noiseScale, noiseSpeed, time);
            voxels[index] = vox;

            directions[index] = vox.velocity;
        }
    }

}
