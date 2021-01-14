using System.Runtime.InteropServices;

namespace WhereTheFile.Windows
{
    public static class WindowsInterop
    {
        //From https://github.com/dotnet/runtime/issues/27976#issuecomment-445515585

        [DllImport("ntdll.dll")]
        public static extern sbyte RtlSetProcessPlaceholderCompatibilityMode(sbyte pcm);

        const sbyte PHCM_APPLICATION_DEFAULT = 0;
        const sbyte PHCM_DISGUISE_PLACEHOLDER = 1;
        const sbyte PHCM_EXPOSE_PLACEHOLDERS = 2;
        const sbyte PHCM_MAX = 2;
        const sbyte PHCM_ERROR_INVALID_PARAMETER = -1;
        const sbyte PHCM_ERROR_NO_TEB = -2;

    }
}