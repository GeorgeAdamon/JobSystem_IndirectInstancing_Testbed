using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

/// <author> George Adamopoulos </author>
/// <summary>
/// Accelerates particle swarms using the Unity Job System, Unity Mathematics and Burst Compiler
/// </summary>

[DefaultExecutionOrder(49)]
public class JobifiedParticles: MonoBehaviour
{
    [Tooltip("Define the number of particles using powers of 2 (1024, 2048...)")]
    [Range(10, 20)] public int PowerOfTwo;

    [Tooltip("Total number of particles. Not editable directly.")]
    public int _objectCount;

    private int cachedObjectCount;

    [Tooltip("Overall directional pull")]
    public Vector3 m_Acceleration = new Vector3(0.0002f, 0.0001f, 0.0002f);

    private float3[] Positions;

    //========== JOB SYSTEM RELATED VARIABLES ( NativeArrays & JobHandles) ============//

    // Native Arrays
    public NativeArray<float3> _Positions;
    public NativeArray<float3> _Velocities;
    public NativeArray<float4> _Colors;

    // Job structs
    PositionUpdateJob m_Job;
    AccelerationJob m_AccelJob;

    // Job Handles
    JobHandle m_PositionJobHandle;
    JobHandle m_AccelJobHandle;


    //============ MONOBEHAVIOUR METHODS ==========//
    void Start()
    {
        _objectCount = (int)Mathf.Pow(2, PowerOfTwo);
        UpdateArrays();
    }

    void Update()
    {
        _objectCount = (int)Mathf.Pow(2, PowerOfTwo);

        if (cachedObjectCount != _objectCount)
        {
            UpdateArrays();
        }
        // Prepare jobs with data
        m_AccelJob = new AccelerationJob()
        {
            deltaTime = Time.deltaTime,
            velocity = _Velocities,
            acceleration = m_Acceleration,
            positions = _Positions,
            colors = _Colors,
            Time = Time.time
        };

        m_Job = new PositionUpdateJob()
        {
            deltaTime = Time.deltaTime,
            velocity = _Velocities,
            position = _Positions
        };

        //Schedule the first job
        m_AccelJobHandle = m_AccelJob.Schedule(_objectCount, 64);
        //Schedule the second job, chained to the first
        m_PositionJobHandle = m_Job.Schedule(_objectCount, 64, m_AccelJobHandle);
    }

    void LateUpdate()
    {
        // Finalize the jobs
        m_PositionJobHandle.Complete();

        //Copy the data to the arrays
        _Positions.CopyTo(Positions);

    }

    void OnDestroy()
    {
        // Dispose the NativeArrays
        _Velocities.Dispose();
        _Positions.Dispose();
        _Colors.Dispose();
    }

    //============ CUSTOM PRIVATE METHODS ==========//
    private void UpdateArrays()
    {     
        Positions = new float3[_objectCount];

        // Generate points at random locations
        for (int i = 0; i < _objectCount; i++)
        {
            Positions[i] = new Vector3(UnityEngine.Random.Range(-50, 50), UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-50, 50));
        }

        // Manually release the memory allocated by the NativeArrays
        if (_Velocities.IsCreated)
        {
            _Velocities.Dispose();
        }

        if (_Positions.IsCreated)
        {
            _Positions.Dispose();
        }

        if (_Colors.IsCreated)
        {
            _Colors.Dispose();
        }

        // Reallocate memory for the arrays.
        _Velocities = new NativeArray<float3>(_objectCount, Allocator.Persistent);
        _Positions = new NativeArray<float3>(Positions, Allocator.Persistent);
        _Colors = new NativeArray<float4>(_objectCount, Allocator.Persistent);

        cachedObjectCount = _objectCount;
    }


    //============ JOB DEFINITIONS ==========//
    [BurstCompile]
    struct PositionUpdateJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float3> velocity;  // the velocities from AccelerationJob
        public NativeArray<float3> position;
        public float deltaTime;

        public void Execute(int i)
        {
            position[i] += velocity[i] * deltaTime;

            // WRAP AROUND THE BOUNDS
            /*
            float x = position[i].x;
            float y = position[i].y;
            float z = position[i].z;

            if (x > 1000 || x < -1000) x *= -1;
            if (y > 1000 || y < -1000) y *= -1;
            if (z > 1000 || z < -1000) z *= -1;

            position[i] = new float3(x, y, z);
            */
        }
    }

    [BurstCompile]
    struct AccelerationJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> positions;
        public NativeArray<float3> velocity;
        public NativeArray<float4> colors;
        public float3 acceleration;
        public float deltaTime;
        public float Time;

        public void Execute(int i)
        {
            // Calculate the VERTICAL angle at which to move for the next step, using a perlin noise function
            //float theta = noise.snoise(positions[i]*0.01f - Time*0.1f) * -2 + 1;  // Noise moves while particles move through it
            float theta = noise.snoise(positions[i] * 0.01f) * -2 + 1;          // Noise doesn't move. Particles move through a constant noise field.


            // Calculate the HORIZONTAL angle at which to move for the next step, using a perlin noise function
            //float phi = noise.snoise(positions[i] * 0.01f - Time * 0.5f) * TWOPI; // Noise moves while particles move through it
           float phi = noise.snoise(positions[i] * 0.01f) * VectorFieldUtils.TWOPI; // Noise doesn't move. Particles move through a constant noise field.


            // Using the angle, find the direction, using simple trigonometry
            float x = math.cos(phi) * 4;
            float y = theta;
            float z = math.sin(phi);

            float3 _noise = math.normalize(new float3(x, y, z)) ;

            // Add the noise direction to the primary direction
            velocity[i] += 10 * (_noise +  acceleration*(i/(float)positions.Length)) * deltaTime;

          //  colors[i] = new float4(1, 1, 1, 1) * theta;
        }
    }


}