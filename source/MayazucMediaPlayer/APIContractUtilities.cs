using MayazucMediaPlayer.Services;
using Windows.Foundation.Metadata;

namespace MayazucMediaPlayer
{
    public static class APIContractUtilities
    {
        private static bool? springCreators = null;
        [PropertyDescriptor(Description = "Device supports Windows 10 Spring Creator's Update")]
        public static bool UniversalContract4
        {
            get
            {
                if (!springCreators.HasValue)
                {
                    springCreators = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4, 0);
                }

                return springCreators.Value;
            }
        }

        private static bool? fallCreators = null;
        [PropertyDescriptor(Description = "Device supports Windows 10 Fall Creator's Update")]
        public static bool UniversalContract5
        {
            get
            {
                if (!fallCreators.HasValue)
                {
                    fallCreators = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5, 0);
                }

                return fallCreators.Value;
            }
        }

        private static bool? apiContract8 = null;
        [PropertyDescriptor(Description = "Windows 1903 supported")]
        public static bool UniversalContract8
        {
            get
            {
                if (!apiContract8.HasValue)
                {
                    apiContract8 = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8);

                }

                return apiContract8.Value;
            }
        }
    }
}
