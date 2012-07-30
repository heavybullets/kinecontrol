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

        //Debug
        int frame = 0;
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
            int player;
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
                {
                    pixelData[i8] = intensity;
                    //System.Console.Out.WriteLine("index " + i8);
                }
                else
                    pixelData[i8] = 0;

                
            }

            //Update 
            //bitmap.WritePixels(rect, pixelData, stride, 0);
        }

        public int getAreaAroundPointWithThreshold(Point3D point, double threshold_margins)
        {
            frame %= 15;
            if (frame++ != 0)
                return -1;

            //Transform the point3D to depthImagePoint
            DepthImagePoint dp = KinectUtils.mapSkeletonPoint3DToDepthPoint(point);

            //Get the index in pixels
            //System.Windows.Point coord = KinectUtils.DepthPointToIndexes(dp);

            //Intensity reference
            //Fix Errors Here
            dp.Y -= 1;
            byte intensity = pixelData[(int)(dp.Y * stride + dp.X)];

            int threshold_int = (int)Math.Round(threshold_margins);
            byte threshold_intensity = (byte)(threshold_int >> 4);
            //For processing we need the thresholded Image, we need to give the thresholds, these are
            double threshold_min = intensity - threshold_intensity;
            double threshold_max = intensity + threshold_intensity;

            //Perform threshold
            byte[] thresholded = thresholdImage((int)threshold_min, (int)threshold_max);


            //With the thresholded Image we need the area around the coordinate Points
            int left = 1, right = 1, top = 1,bottom = 1;
            bool stopL = false, stopR = false, stopT = false, stopB = false;
            bool stop = false;
            while(!stop)
            {
                //Horizontal
                if (thresholded[dp.Y * stride + dp.X + right] == 255)
                {   
                    thresholded[dp.Y * stride + dp.X + right] = 0;
                    right++;
                }
                else
                    stopR = true;

                if (thresholded[dp.Y * stride + dp.X - left] == 255)
                {
                    thresholded[dp.Y * stride + dp.X - left] = 0;
                    left++;
                }
                else stopL = true;

                //Vertical
                if (thresholded[(dp.Y - top) * stride + dp.X] == 255)
                {
                    thresholded[(dp.Y - top) * stride + dp.X] = 0;
                    top++;
                }
                else
                    stopT = true;

                if (thresholded[(dp.Y + bottom) * stride + dp.X] == 255)
                {
                    thresholded[(dp.Y + bottom) * stride + dp.X] = 0;
                    bottom++;
                }
                else
                    stopB = true;

                stop = stopB || stopT || stopL || stopR;
            }

            //Translate to vertical and horizontal height and width
            int height = top + bottom;
            int width = left + right;

            //Normalize
            //height /= dp.Depth;
            //width /= dp.Depth;


            //DEBUG
            bitmap.WritePixels(rect, thresholded, stride, 0);

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
            for (int i = Math.Max(min,0); i < Math.Min(max,256); i++)
                threshold[i] = 255;

            //Threshold
            for (int i = 0; i < pixelData.Length; i++)
            {
                //if(pixelData[i] > min && pixelData[i]< max)
                thresholded[i] = threshold[pixelData[i]];
            }


            return thresholded;
        }

    }
}
