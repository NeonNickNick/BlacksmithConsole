namespace BlacksmithCore.Backend.Utils
{
    public static class MathFExtensions
    {
        public static (float, float) Cancel(float a, float b)
        {
            return (MathF.Max(0, a - b), MathF.Max(0, b - a));
        }
    }
}
