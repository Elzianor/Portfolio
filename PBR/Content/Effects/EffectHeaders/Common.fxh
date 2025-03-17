#ifndef COMMON_FXH
#define COMMON_FXH

// --- CONSTANTS ---

static const float PI = 3.14159265359;

// --- SAFE OPERATIONS ---

float Dot(float3 x, float3 y)
{
    return max(dot(x, y), 0.0);
}

float NonZeroDenominator(float val)
{
    return max(val, 0.000001);
}

// --- EQUALITY CHECKS ---

bool IsEqual(float a, float b)
{
    return (abs(a - b) < 1e-6);
}

bool IsZero(float2 v)
{
    return all(v == 0);
}

bool IsZero(float3 v)
{
    return all(v == 0);
}

// --- PERLIN 3D ---

// constants
const static int permutationTableSize = 1024;
const static int gradientSetSize = 26;
const static float ZeroTolerance = 1e-6f;

// pseudorandom hash modifiers
int mX;
int mY;
int mZ;

// permutation table
int permutationTable[permutationTableSize];
// gradients' set
float3 gradientSet[gradientSetSize];

float3 GetGradient(int x, int y, int z, bool isLessGradients)
{
    //pick random cell in permutation table (cells 0 to 'permutationTableSize')
    int index = (x * mX ^ y * mY + z * mZ + mX * mY * mZ) & (permutationTableSize - 1);

    if (isLessGradients == false)
    {
        //pick random cell in gradientSet vector
        index = permutationTable[index] & (gradientSetSize - 1);

        //return the content of the picked cell
        return gradientSet[index];
    }

	//ALTERNATIVE IMPLEMENTATION FOR 12 GRADIENT VECTORS
    index = permutationTable[index] & 11;

    switch (index)
    {
        case 0:
            return float3(0.0, 1.0, 1.0);
        case 1:
            return float3(0.0, 1.0, -1.0);
        case 2:
            return float3(0.0, -1.0, 1.0);
        case 3:
            return float3(0.0, -1.0, -1.0);
        case 4:
            return float3(1.0, 0.0, 1.0);
        case 5:
            return float3(1.0, 0.0, -1.0);
        case 6:
            return float3(-1.0, 0.0, 1.0);
        case 7:
            return float3(-1.0, 0.0, -1.0);
        case 8:
            return float3(1.0, 1.0, 0.0);
        case 9:
            return float3(1.0, -1.0, 0.0);
        case 10:
            return float3(-1.0, 1.0, 0.0);
        default:
            return float3(-1.0, -1.0, 0.0);
    }
}

float BlendingCurve(float d)
{
    return d * d * d * (d * (d * 6.0 - 15.0) + 10.0);
}

float Interpolation(float a, float b, float t)
{
    return (1.0 - t) * a + t * b;
}

float Get3DNoiseValue(float x, float y, float z)
{
    // find unit grid cell containing point
    int floorX = floor(x);
    int floorY = floor(y);
    int floorZ = floor(z);

    // get relative XYZ coordinates of point in cell
    float relX = x - floorX;
    float relY = y - floorY;
    float relZ = z - floorZ;

    //gradients of cube vertices
    float3 g000 = GetGradient(floorX, floorY, floorZ, false);
    float3 g001 = GetGradient(floorX, floorY, floorZ + 1, false);
    float3 g010 = GetGradient(floorX, floorY + 1, floorZ, false);
    float3 g011 = GetGradient(floorX, floorY + 1, floorZ + 1, false);
    float3 g100 = GetGradient(floorX + 1, floorY, floorZ, false);
    float3 g101 = GetGradient(floorX + 1, floorY, floorZ + 1, false);
    float3 g110 = GetGradient(floorX + 1, floorY + 1, floorZ, false);
    float3 g111 = GetGradient(floorX + 1, floorY + 1, floorZ + 1, false);

    // noise contribution from each of the eight corners
    float n000 = dot(g000, float3(relX, relY, relZ));
    float n100 = dot(g100, float3(relX - 1.0, relY, relZ));
    float n010 = dot(g010, float3(relX, relY - 1.0, relZ));
    float n110 = dot(g110, float3(relX - 1.0, relY - 1.0, relZ));
    float n001 = dot(g001, float3(relX, relY, relZ - 1.0));
    float n101 = dot(g101, float3(relX - 1.0, relY, relZ - 1.0));
    float n011 = dot(g011, float3(relX, relY - 1.0, relZ - 1.0));
    float n111 = dot(g111, float3(relX - 1.0, relY - 1.0, relZ - 1.0));

    // compute the fade curve value for each x, y, z
    float u = BlendingCurve(relX);
    float v = BlendingCurve(relY);
    float w = BlendingCurve(relZ);

    // interpolate along x the contribution from each of the corners
    float nx00 = lerp(n000, n100, u);
    float nx01 = lerp(n001, n101, u);
    float nx10 = lerp(n010, n110, u);
    float nx11 = lerp(n011, n111, u);

    // interpolate the four results along y
    float nxy0 = lerp(nx00, nx10, v);
    float nxy1 = lerp(nx01, nx11, v);

    // interpolate the two last results along z
    float nxyz = lerp(nxy0, nxy1, w);

    return clamp(nxyz, -1.0f, 1.0f);
}

float NormalizeValue(float value, float min, float max)
{
    return (value - min) / (max - min);
}

float GetMultioctave3DNoiseValue(float x, float y, float z, uint startOctaveNumber, uint octaveCount, float persistence, bool normalizeOutput = false)
{
    float total = 0.0f;
    float frequency = pow(2.0, startOctaveNumber);
    float amplitude = pow(abs(persistence), startOctaveNumber);

    float maxAmplitude = amplitude * startOctaveNumber;

    for (uint i = startOctaveNumber; i < (startOctaveNumber + octaveCount); ++i)
    {
        total += Get3DNoiseValue(x / frequency, y / frequency, z / frequency) * amplitude;

        maxAmplitude += amplitude;

        frequency *= 2.0f;
        amplitude *= persistence;
    }

    float outputValue = normalizeOutput ?
        clamp(NormalizeValue(total / maxAmplitude, -1.0, 1.0), 0.0, 1.0) :
        clamp(total / maxAmplitude, -1.0, 1.0);

    return outputValue;
}

// --- SDF SHAPE FUNCTIONS ---

// p - point in space
// s - sphere radius
float SdfSphere(float3 p, float s)
{
    return length(p) - s;
}

#endif // COMMON_FXH
