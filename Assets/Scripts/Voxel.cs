using Unity.Mathematics;

public struct Voxel
{
    public int X, Y, Z, i;
    public float3 position;
    public float3 velocity;

    public override string ToString()
    {
        return string.Format("Voxel {0},{1},{2}_Velocity={3}", X, Y, Z, velocity);
    }
}