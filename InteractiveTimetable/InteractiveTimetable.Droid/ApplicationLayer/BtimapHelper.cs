using System.Threading.Tasks;
using Android.Graphics;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    public static class BtimapHelper
    {
        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
        {
            /* 
             * First we get the the dimensions of the file on disk 
             */
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileName, options);

            /* 
             * Next we calculate the ratio that we need to resize the image by
             * in order to fit the requested dimensions.
             */
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }

            /* 
             * Now we will load the image and have BitmapFactory resize it for us. 
             */
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

            return resizedBitmap;
        }

        public static async Task<Bitmap> LoadScaledDownBitmapForDisplayAsync(
            this string fileName,
            int reqWidth,
            int reqHeight)
        {
            /* Get options of image */
            BitmapFactory.Options options = await GetBitmapOptionsOfImageAsync(fileName);

            /* Calculate inSampleSize */
            options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);

            /* Decode bitmap with inSampleSize set */
            options.InJustDecodeBounds = false;

            return await BitmapFactory.DecodeFileAsync(fileName, options);
        }

        public static async Task<Bitmap> LoadScaledDownBitmapForDisplayAsync(
            this int resourceId,
            Android.Content.Res.Resources resources,
            int reqWidth,
            int reqHeight)
        {
            /* Get options of image */
            BitmapFactory.Options options = await GetBitmapOptionsOfImageAsync(resourceId, resources);
            

            /* Calculate inSampleSize */
            options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);

            /* Decode bitmap with inSampleSize set */
            options.InJustDecodeBounds = false;


            return await BitmapFactory.DecodeResourceAsync(resources, resourceId, options);
        }

        private static async Task<BitmapFactory.Options> GetBitmapOptionsOfImageAsync(
            string fileName)
        {
            var options = new BitmapFactory.Options
            {
                InJustDecodeBounds = true
            };

            /* The result will be null because InJustDecodeBounds == true */
            var result = await BitmapFactory.DecodeFileAsync(fileName, options);

            return options;
        }

        private static async Task<BitmapFactory.Options> GetBitmapOptionsOfImageAsync(
            int resourceId,
            Android.Content.Res.Resources resources)
        {
            var options = new BitmapFactory.Options
            {
                InJustDecodeBounds = true
            };

            /* The result will be null because InJustDecodeBounds == true */
            var result = await BitmapFactory.DecodeResourceAsync(resources, resourceId, options);

            return options;
        }

        private static int CalculateInSampleSize(
            BitmapFactory.Options options, 
            int reqWidth, 
            int reqHeight)
        {
            /* Raw height and width of image */
            float height = options.OutHeight;
            float width = options.OutWidth;
            double inSampleSize = 1D;

            if (height > reqHeight || width > reqWidth)
            {
                int halfHeight = (int) (height / 2);
                int halfWidth = (int) (width / 2);

                /* 
                 * Calculate a inSampleSize that is a power of 2 - 
                 * the decoder will use a value that is a power of two anyway. 
                 */
                while ((halfHeight / inSampleSize) > reqHeight && (halfWidth / inSampleSize) > reqWidth)
                {
                    inSampleSize *= 2;
                }
            }

            return (int) inSampleSize;
        }
    }
}