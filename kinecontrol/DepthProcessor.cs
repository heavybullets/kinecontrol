using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media.Media3D;


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
            //bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8,null);   //Grayscale 1 byte per pixel
            rect = new Int32Rect(0, 0, width, height);

        }


        public void setPlayerDepthData(DepthImageFrame frame)
        {
            int player, depth;
            int stride = nchannels * frame.Width;
            //Get Raw Data!
            frame.CopyPixelDataTo(rawData);

            //Using player index, filter the player depth image
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

            }

            //Update 
            //bitmap.WritePixels(rect, pixelData, stride, 0);
        }

        public double getAreaAroundPointWithThreshold(Point3D point, double threshold)
        {
            //Get the point in pixels
            Point p = KinectUtils.mapSkeletonPoint3DToDepth(point);

            //Need to threshold the image
        }

    }
}
