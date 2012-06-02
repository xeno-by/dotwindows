namespace FarPod.Helpers
{
    using System;

    /// <summary>
    /// String format utils
    /// </summary>
    static class DisplayFormat
    {
        public static string GetTimeString(double seconds)
        {
            int minutes = (int)(seconds / 60);
            int hours = minutes / 60;

            minutes = minutes - hours * 60;
            seconds = seconds - minutes * 60 - hours * 3600;

            return hours.ToString("00") + minutes.ToString(":00") + seconds.ToString(":00");
        }

        public static string GetFileSizeString(double fileSizeBytes, int decimalPoints = 2)
        {
            double mbSize = fileSizeBytes / 1048576;

            if (mbSize > 1024)
            {
                double gbSize = fileSizeBytes / 1073741824;

                return Math.Round(gbSize, decimalPoints, MidpointRounding.ToEven).ToString("00.00") + " GB";
            }
            else
            {
                return Math.Round(mbSize, decimalPoints, MidpointRounding.AwayFromZero).ToString("00.00") + " MB";
            }
        }
    }
}
