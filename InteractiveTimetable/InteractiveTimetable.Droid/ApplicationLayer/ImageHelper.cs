
namespace InteractiveTimetable.Droid.ApplicationLayer
{
    public static class ImageHelper
    {
        public static readonly string HexFrameColor = "#00BFFF";

        public static int ConvertDpToPixels(int dp, float density)
        {
            return (int)(dp * density + 0.5f);
        }

        public static int ConvertDpToPixels(int dp)
        {
            float density = InteractiveTimetable.Current.ScreenDensity;
            return (int)(dp * density + 0.5f);
        }

        public static int ConvertPixelsToDp(int pixels, float density)
        {
            return (int)((pixels) / density);
        }

        public static int ConvertPixelsToDp(int pixels)
        {
            float density = InteractiveTimetable.Current.ScreenDensity;
            return (int)((pixels) / density);
        }
    }
}