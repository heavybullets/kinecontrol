using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media.Media3D;
using Emgu.CV;
using Emgu.CV.Structure;

namespace kinecontrol
{
    class DepthProcessor
    {
        public Byte[] pixelData
        {
            get;
            set;
        }

        public short[] rawData
        {
            get;
            set;
        }

        private int stride; 
        private int nchannels;

        private Int32Rect rect;

        public WriteableBitmap bitmap
        {
            get;
            set;
        }

        public DepthProcessor(int pixelDataLength, int width, int height, int nchannels)
        {
            //allocate the data
            rawData = new short[pixelDataLength];
            pixelData = new byte[width * height * nchannels];
            this.nchannels = nchannels;
            //Allocate bitmap using number of channels
            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8,null);   //Grayscale 1 byte per pixel
            rect = new Int32Rect(0, 0, width, height);

        }


        public void setPlayerDepthData(DepthImageFrame frame)
        {
            int player, depth;
            stride = nchannels * frame.Width;
            //Get Raw Data!
            frame.CopyPixelDataTo(rawData);

            /*//Using player index, filter the player depth image
            for (int rawIndex = 0, pixelIndex = 0; rawIndex < rawData.Length && pixelIndex < pixelData.Length;
                rawIndex++, pixelIndex += this.nchannels)
            {
                //First Acquiere the player index
                player = rawData[rawIndex] & DepthImageFrame.PlayerIndexBitmask;

                //See if the pixel corresponds that of a player
                if (player > 0)
                {
                    //It's a Player pixel, get the Depth
                    depth = rawData[rawIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                    //Put it on the Array
                    pixelData[pixelIndex] = (byte)depth;
                }
                else
                    //If it isn't put 0
                    pixelData[pixelIndex] = 0;

            }*/

            for (int i16 = 0, i8 = 0; i16 < rawData.Length && i8 < this.pixelData.Length; i16++, i8 += nchannels)
            {
                player = rawData[i16] & DepthImageFrame.PlayerIndexBitmask;
                int realDepth = rawData[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                byte intensity = (byte)(~(realDepth >> 4));
                if (player != 0)
                    pixelData[i8] = intensity;
                else
                    pixelData[i8] = 0;
                
            }

            
            //DEBUG
            bitmap.WritePixels(rect, pixelData, stride, 0);
            //Update 
            //bitmap.WritePixels(rect, pixelData, stride, 0);
        }

        public int getAreaAroundPointWithThreshold(Point3D point, double threshold_margins)
        {
            //Transform the point3D to depthImagePoint
            DepthImagePoint dp = KinectUtils.mapSkeletonPoint3DToDepthPoint(point);

            //Get the index in pixels
            System.Windows.Point coord = KinectUtils.DepthPointToIndexes(dp);

            //For processing we need the thresholded Image, we need to give the thresholds, these are
            double threshold_min = dp.Depth - threshold_margins;
            double threshold_max = dp.Depth + threshold_margins;

            //Perform threshold
            byte[] thresholded = thresholdImage((int)threshold_min, (int)threshold_max);

            //With the thresholded Image we need the area around the coordinate Points
            int left = 0, right = 0, top = 0,bottom = 0;
            bool stopL = false, stopR = false, stopT = false, stopB = false;
            bool stop = false;
            for (int i = dp.X, j = dp.Y; stop; i++, j++)
            {
                //Horizontal
                if (pixelData[dp.Y*stride + dp.X + right] == 1)
                    right++;
                else
                    stopR = true;

                if (pixelData[dp.Y * stride + dp.X - left] == 1)
                    left++;
                else stopL = true;

                //Vertical
                if (pixelData[(dp.Y - top) * stride + dp.X] == 1)
                    top++;
                else
                    stopT = true;

                if (pixelData[(dp.Y + bottom) * stride + dp.X] == 1)
                    bottom++;
                else
                    stopB = true;

                stop = stopB & stopT & stopL & stopR;
            }

            //Translate to vertical and horizontal height and width
            int height = top + bottom;
            int width = left + right;

            //Normalize
            height /= dp.Depth;
            width /= dp.Depth;

            //Return Area
            return height * width;

            
        }

        public Byte[] thresholdImage(int min, int max)
        {
            //Create output array
            Byte[] thresholded = new byte[pixelData.Length];
            
            //Allocate Threshold
            byte[] threshold = new byte[256];
            //Prepare thresholding
            for (int i = min; i < max + 1; i++)
                threshold[i] = 1;

            //Threshold
            for (int i = 0; i < pixelData.Length; i++)
                thresholded[i] = threshold[pixelData[i]];

            //DEBUG
            bitmap.WritePixels(rect, pixelData, stride, 0);

            return thresholded;
        }

    }
}
