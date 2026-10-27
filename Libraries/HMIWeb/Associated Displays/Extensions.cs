namespace Honeywell.HMIWeb
{
    public static class Extensions
    {
        public static string? ToStringOrNull(this object obj)
        {
            var value = obj.ToString();
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return value;
        }
    }
}
