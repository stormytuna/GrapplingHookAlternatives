using System;
using Microsoft.Xna.Framework;

namespace GrapplingHookAlternatives.Common;

public static class Helpers
{
    public static Point Clamp(this Point point, int minX, int maxX, int minY, int maxY) => new(Math.Clamp(point.X, minX, maxX), Math.Clamp(point.Y, minY, maxY));
}
