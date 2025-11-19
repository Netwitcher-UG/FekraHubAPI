namespace FekraHubAPI.Helpers
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// يحوّل أي DateTime إلى Utc ويتعامل مع Unspecified و Local
        /// </summary>
        public static DateTime ToUtcSafe(this DateTime dt)
        {
            return dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Local => dt.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
                _ => dt
            };
        }

        /// <summary>
        /// نسخة للـ DateTime? (nullable)
        /// </summary>
        public static DateTime? ToUtcSafe(this DateTime? dt)
        {
            if (!dt.HasValue) return null;
            return dt.Value.ToUtcSafe();
        }
    }
}
