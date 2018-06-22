namespace ContosoCafeBot
{
    using System.Collections.Generic;

    /// <summary>
    /// Class for storing conversation state. 
    /// </summary>
    public class CafeBotConvState : Dictionary<string, object>
    {
        public Dictionary<string, object> convContextKVPair;

        private struct bookTableEntities
        {
            string cafeLocation;
            string dateTime;
            int partySize;
        }

        private struct whoAreYouEntities
        {
            string userName;
        }
    }

    
    
}
