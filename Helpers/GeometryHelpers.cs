namespace GrapplingHookAlternatives.Helpers;

public static class GeometryHelpers
{
	public static Point Clamp(this Point point, int minX, int maxX, int minY, int maxY) => new(Math.Clamp(point.X, minX, maxX), Math.Clamp(point.Y, minY, maxY));
}
