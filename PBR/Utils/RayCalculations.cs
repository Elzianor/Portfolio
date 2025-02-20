using Beryllium.Camera;
using Beryllium.MonoInput.MouseInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PBR.Utils;
internal static class RayCalculations
{
    public static Ray CalculateRay(Viewport viewport, Camera camera)
    {
        var mouseLocation = new Vector2(MouseManager.MouseStatus.Position.X, MouseManager.MouseStatus.Position.Y);
        var view = camera.ViewMatrix;
        var projection = camera.ProjectionMatrix;

        var nearPoint = viewport.Unproject(
            new Vector3(mouseLocation, 0.0f),
            projection,
            view,
            camera.OffsetWorldMatrix);

        var farPoint = viewport.Unproject(
            new Vector3(mouseLocation, 1.0f),
            projection,
            view,
            camera.OffsetWorldMatrix);

        var direction = farPoint - nearPoint;
        direction.Normalize();

        return new Ray(nearPoint, direction);
    }

    public static Vector3? GetRayPlaneIntersectionPoint(Ray ray, Plane plane)
    {
        var distance = ray.Intersects(plane);
        return distance.HasValue ? ray.Position + ray.Direction * distance.Value : null;
    }
}
