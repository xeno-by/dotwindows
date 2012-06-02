namespace FarPod.Helpers
{
    using FarNet;

    /// <summary>
    /// MatchPattern wrapper
    /// </summary>
    class MaskMatcher
    {
        private readonly string _pattern;

        public MaskMatcher(string pattern)
        {            
            _pattern = pattern;
        }

        public bool Compare(string value)
        {           
            return Far.Net.MatchPattern(value, _pattern);
        }
    }
}
