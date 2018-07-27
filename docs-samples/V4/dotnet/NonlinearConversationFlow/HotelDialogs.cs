namespace NonlinearConversationFlow
{
    public static class HotelDialogs
    {
        public const string Cancel = "Cancel";
        public const string Result = "Result";

        public static bool Is<T>(this object obj, T value) => obj is T item && item.Equals(value);
    }
}
