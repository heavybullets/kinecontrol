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

using System.Drawing;
using System.IO;

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

            byte[] dilated = thresholded;
            for(int i = 0; i < 3; i++)
                dilated = binaryDilateImage(dilated, 1, rect.Height, rect.Width);

            //With the thresholded Image we need the area around the coordinate Points
            int left = 1, right = 1, top = 1,bottom = 1;
            bool stopL = false, stopR = false, stopT = false, stopB = false;
            bool stop = false;
            while(!stop)
            {
                //Horizontal
                if(dp.Y * stride + dp.X + right < dilated.Length)
                    if (dilated[dp.Y * stride + dp.X + right] == 255)
                    {   
                        dilated[dp.Y * stride + dp.X + right] = 0;
                        right++;
                    }
                    else
                        stopR = true;

                if(dp.Y * stride + dp.X + -left > 0)
                    if (dilated[dp.Y * stride + dp.X - left] == 255)
                    {
                        dilated[dp.Y * stride + dp.X - left] = 0;
                        left++;
                    }
                    else stopL = true;

                //Vertical
                if ((dp.Y - top) *stride + dp.X > 0)
                    if (dilated[(dp.Y - top) * stride + dp.X] == 255)
                    {
                        dilated[(dp.Y - top) * stride + dp.X] = 0;
                        top++;
                    }
                    else
                        stopT = true;

                if((dp.Y + bottom) * stride + dp.X < dilated.Length)
                    if (dilated[(dp.Y + bottom) * stride + dp.X] == 255)
                    {
                        dilated[(dp.Y + bottom) * stride + dp.X] = 0;
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
            bitmap.WritePixels(rect, dilated, stride, 0);

            //Return Area
            return height * width;

            
        }

        public Byte[] binaryDilateImage(Byte[] src, int nchannels, int height, int width)
        {
            Byte[] img = new byte[height*width];
            int length = height*width;
            int lstride;
            for(int j = 0; j < height; j++)
                for (int i = 0; i < width; i += nchannels)
                {
                    //Dilate
                    lstride = width * nchannels;
                    int index = i + j * lstride;

                    if (src[index] != 0)
                        img[index] = 255;

                    if (index + 1 < length)
                        if (src[index + 1] != 0)
                            img[index] = 255;

                    if (index - 1 > 0)
                        if (src[index - 1] != 0)
                            img[index] = 255;

                    if (index + lstride < length)
                        if (src[index + lstride] != 0)
                            img[index] = 255;

                    if (index - lstride > 0)
                        if (src[index - lstride] != 0)
                            img[index] = 255;
                    
                }   

            return img;

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
