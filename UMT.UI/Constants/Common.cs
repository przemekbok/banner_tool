using System.ComponentModel;

namespace UMT.UI.Constants
{
    public static class Common
    {
        public const string ProvidePathFileTitle = "Select File";
        public const string ProvidePathFileDescription = "Please provide the 'tdx' file.";
        public const string ProvidePathFileFilter = "tdx files (*.tdx)|*.tdx|All files (*.*)|*.*";

    }

    public enum AppMode
    {
        [Description("Only default banner")]
        DefaultBanner,

        [Description("Default banner and redirect")]
        DefaultBannerRedirect,

        [Description("Custom banner")]
        CustomBanner,

        [Description("Remove all applied banners")]
        RemoveAllAppliedBanners
    }
}
