using Unity.Mathematics;
using Unity.Burst;

/// <author> George Adamopoulos </author>
/// <summary>
/// Provides Utility functions for Voxel Grids / Vector Fields
/// </summary>
public static class VectorFieldUtils 
{
    public const float TWOPI = 6.28318530718f;

    public static void XYZ_to_i(int x, int y, int z, int width, int height, out int i)
    {
        i = x + y * width + z * width * height;
    }

    public static void i_to_XYZ(int i, int width, int height, out int x, out int y, out int z)
    {
        z = i / (width * height);
        y = (i / width) % height;
        x = i % width;
    }

    [BurstCompile]
    public static float3 DirectionalNoise(float3 pos, float3 scale, float3 speed, float time)
    {
        // Calculate the VERTICAL angle at which to move for the next step, using a perlin noise function
        float theta = noise.snoise(pos * scale - time * speed) * -2 + 1;

        // Calculate the HORIZONTAL angle at which to move for the next step, using a perlin noise function
        float phi = noise.snoise(pos * scale - time * speed) * TWOPI;

        // Using the angles, find the direction, using simple trigonometry for the conversion from Spherical Coordinates to Cartesian
        float _x = math.cos(phi);
        float _y = theta;
        float _z = math.sin(phi);

        return math.normalize(new float3(_x, _y, _z));
    }
}
