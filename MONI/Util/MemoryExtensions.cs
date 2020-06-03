namespace MONI.Util
{
    public static class MemoryExtensions
    {
        public static float ToMB(this long bytes)
        {
            if (bytes < 0)
            {
                // Negative values are used for invalid values.
                return bytes;
            }

            return (float) (bytes / (1024.0 * 1024));
        }

        public static float ToMB(this float bytes)
        {
            if (bytes < 0)
            {
                // Negative values are used for invalid values.
                return bytes;
            }

            return bytes / (1024 * 1024);
        }

        public static float ToMB(this int bytes)
        {
            return ((long) bytes).ToMB();
        }
    }
}