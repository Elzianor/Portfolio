using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace PBR.Utils;

internal static class Perlin3D
{
    private const int _permutationTableSize = 1024;

    public static Random Random { get; private set; }

    //permutation table
    public static byte[] PermutationTable { get; }

    //pseudorandom hash modifiers
    public static int MX { get; private set; }
    public static int MY { get; private set; }
    public static int MZ { get; private set; }

    // gradients' set
    public static readonly List<Vector3> GradientSet;

    // cube's eight corners
    private static readonly Vector3[] _cubeCorners;

    public static float StatisticsSingleNoiseMin { get; private set; }
    public static float StatisticsSingleNoiseMax { get; private set; }
    public static float StatisticsOctavesNoiseMin { get; private set; }
    public static float StatisticsOctaveNoiseMax { get; private set; }

    static Perlin3D()
    {
        PermutationTable = new byte[_permutationTableSize];
        GradientSet = new List<Vector3>();
        _cubeCorners = new Vector3[8];

        for (var i = 0; i < 8; ++i)
        {
            _cubeCorners[i] = new Vector3();
        }

        // fill the gradients' set
        for (var x = -1; x <= 1; ++x) // from -1 to 1
        {
            for (var y = -1; y <= 1; ++y)
            {
                for (var z = -1; z <= 1; ++z)
                {
                    if ((x != 0) || (y != 0) || (z != 0))
                    {
                        GradientSet.Add(new Vector3(x, y, z));
                    }
                }
            }
        }

        StatisticsSingleNoiseMin = float.MaxValue;
        StatisticsSingleNoiseMax = float.MinValue;
        StatisticsOctavesNoiseMin = float.MaxValue;
        StatisticsOctaveNoiseMax = float.MinValue;
    }

    public static void SetSeed(int? seed = null)
    {
        Random = (seed == null) ? new Random() : new Random((int)seed);

        Random.NextBytes(PermutationTable);

        MX = Random.Next();
        MY = Random.Next();
        MZ = Random.Next();
    }

    private static Vector3 GetGradient(int x, int y, int z)
    {
        // pick random cell in permutation table (cells 0 to '_permutationTableSize')
        var index = ((x * MX) ^ ((y * MY) + (z * MZ) + (MX * MY * MZ))) & (_permutationTableSize - 1);
        // pick random cell in GradientSet vector
        index = PermutationTable[index] & (GradientSet.Count - 1);

        // return the content of the picked cell
        return GradientSet[index];
    }

    private static float FastPow(float value, uint pow)
    {
        var powOfValue = 1.0f;

        for (uint i = 0; i < pow; ++i)
        {
            powOfValue *= value;
        }

        return powOfValue;
    }

    private static float BlendingCurve(float d)
    {
        return d * d * d * ((d * ((d * 6.0f) - 15.0f)) + 10.0f);
    }

    private static float Interpolation(float a, float b, float t)
    {
        return ((1.0f - t) * a) + (t * b);
    }

    public static float Get3DNoiseValue(float x, float y, float z)
    {
        // find unit grid cell containing point
        var floorX = (int)Math.Floor(x);
        var floorY = (int)Math.Floor(y);
        var floorZ = (int)Math.Floor(z);

        // get relative XYZ coordinates of point in cell
        var relX = x - floorX;
        var relY = y - floorY;
        var relZ = z - floorZ;

        //gradients of cube vertices
        var g000 = GetGradient(floorX, floorY, floorZ);
        var g001 = GetGradient(floorX, floorY, floorZ + 1);
        var g010 = GetGradient(floorX, floorY + 1, floorZ);
        var g011 = GetGradient(floorX, floorY + 1, floorZ + 1);
        var g100 = GetGradient(floorX + 1, floorY, floorZ);
        var g101 = GetGradient(floorX + 1, floorY, floorZ + 1);
        var g110 = GetGradient(floorX + 1, floorY + 1, floorZ);
        var g111 = GetGradient(floorX + 1, floorY + 1, floorZ + 1);

        // noise contribution from each of the eight corner
        _cubeCorners[0].X = relX;
        _cubeCorners[0].Y = relY;
        _cubeCorners[0].Z = relZ;
        _cubeCorners[1].X = relX - 1;
        _cubeCorners[1].Y = relY;
        _cubeCorners[1].Z = relZ;
        _cubeCorners[2].X = relX;
        _cubeCorners[2].Y = relY - 1;
        _cubeCorners[2].Z = relZ;
        _cubeCorners[3].X = relX - 1;
        _cubeCorners[3].Y = relY - 1;
        _cubeCorners[3].Z = relZ;
        _cubeCorners[4].X = relX;
        _cubeCorners[4].Y = relY;
        _cubeCorners[4].Z = relZ - 1;
        _cubeCorners[5].X = relX - 1;
        _cubeCorners[5].Y = relY;
        _cubeCorners[5].Z = relZ - 1;
        _cubeCorners[6].X = relX;
        _cubeCorners[6].Y = relY - 1;
        _cubeCorners[6].Z = relZ - 1;
        _cubeCorners[7].X = relX - 1;
        _cubeCorners[7].Y = relY - 1;
        _cubeCorners[7].Z = relZ - 1;

        var n000 = Vector3.Dot(g000, _cubeCorners[0]);
        var n100 = Vector3.Dot(g100, _cubeCorners[1]);
        var n010 = Vector3.Dot(g010, _cubeCorners[2]);
        var n110 = Vector3.Dot(g110, _cubeCorners[3]);
        var n001 = Vector3.Dot(g001, _cubeCorners[4]);
        var n101 = Vector3.Dot(g101, _cubeCorners[5]);
        var n011 = Vector3.Dot(g011, _cubeCorners[6]);
        var n111 = Vector3.Dot(g111, _cubeCorners[7]);

        // compute the fade curve value for each x, y, z
        var u = BlendingCurve(relX);
        var v = BlendingCurve(relY);
        var w = BlendingCurve(relZ);

        // interpolate along x the contribution from each of the corners
        var nx00 = Interpolation(n000, n100, u);
        var nx01 = Interpolation(n001, n101, u);
        var nx10 = Interpolation(n010, n110, u);
        var nx11 = Interpolation(n011, n111, u);

        // interpolate the four results along y
        var nxy0 = Interpolation(nx00, nx10, v);
        var nxy1 = Interpolation(nx01, nx11, v);

        // interpolate the two last results along z
        var nxyz = Interpolation(nxy0, nxy1, w);

        nxyz = MathHelper.Clamp(nxyz, -1, 1);

        if (nxyz < StatisticsSingleNoiseMin) StatisticsSingleNoiseMin = nxyz;
        else if (nxyz > StatisticsSingleNoiseMax) StatisticsSingleNoiseMax = nxyz;

        return nxyz;
    }

    public static float GetMultioctave3DNoiseValue(float x, float y, float z, uint startOctaveNumber, uint octaveCount,
        float persistence, bool normalizeOutput = false)
    {
        var total = 0.0f;
        var frequency = FastPow(2, startOctaveNumber);
        var amplitude = FastPow(persistence, startOctaveNumber);

        var maxAmplitude = amplitude * startOctaveNumber;

        for (var i = startOctaveNumber; i < (startOctaveNumber + octaveCount); ++i)
        {
            total += Get3DNoiseValue(x / frequency, y / frequency, z / frequency) * amplitude;

            maxAmplitude += amplitude;

            frequency *= 2.0f;
            amplitude *= persistence;
        }

        var outputValue = normalizeOutput ?
            MathHelper.Clamp(NormalizeValue(total / maxAmplitude, -1, 1), 0, 1) :
            MathHelper.Clamp(total / maxAmplitude, -1, 1);

        if (outputValue < StatisticsOctavesNoiseMin) StatisticsOctavesNoiseMin = outputValue;
        else if (outputValue > StatisticsOctaveNoiseMax) StatisticsOctaveNoiseMax = outputValue;

        return outputValue;
    }

    private static float NormalizeValue(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }
}