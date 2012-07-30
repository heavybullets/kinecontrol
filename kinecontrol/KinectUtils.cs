using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;

namespace kinecontrol
{
    class KinectUtils
    {
        WriteableBitmap colorBitmap;

        private bool calibrate = false;
        
        private JointProcessor proc
        {
            set;
            get;
        }

        public MainWindow window
        {
            get;
            set;
        }

        private DepthProcessor depthproc = null;
        

        public static KinectSensor kinect
        {
            set;
            get;
        }

        public KinectUtils(KinectSensor k)
        {
            kinect = k;
            this.proc = new JointProcessor();
            proc.umbral_movimiento = 0.03f;
            proc.umbral_altura = 0.15f;
        }

        public KinectUtils()
        {
            this.proc = new JointProcessor();
            proc.umbral_movimiento = 0.05f;
            proc.umbral_altura = 0.1f;
        }

        public WriteableBitmap getDepthProcessorBitMap()
        {
            return depthproc.bitmap;
        }

        //For Calibrating the Kinect
        public void startCalibratingSequence()
        {
            //Turn off the standard skeleton frame ready
            //kinect.AllFramesReady -= new EventHandler<AllFramesReadyEventArgs>(AllFramesReady);
            kinect.ElevationAngle = (kinect.MaxElevationAngle + kinect.MinElevationAngle) / 2;

            //For some reason we can't erase the handler... and that cause another thread to come up
            //And that gives problems when changing the bitmap
            //So we use a calibrate flag
            calibrate = true;
            //First change the tilt angle of the Kinect to the lowest
            

            //Start looking for the best position
            //kinect.AllFramesReady +=new EventHandler<AllFramesReadyEventArgs>(allFramesCalibrateReady);
        }

        //When calibration was succesful, we resume the normal mode and store the calibrated skeleton
        public void doneCalibratingSequence(Skeleton s)
        {
            this.proc.calibratedPoints = getPointsOfInterest(s);
            this.proc.setArmLength();

            calibrate = false;
            //kinect.AllFramesReady -= new EventHandler<AllFramesReadyEventArgs>(allFramesCalibrateReady);

            //resume normal mode
            //kinect.AllFramesReady +=new EventHandler<AllFramesReadyEventArgs>(AllFramesReady);
        }

        //Skeleton Frame Ready
        void AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {

            
            if (calibrate)
            {
                allFramesCalibrateReady(sender, e);
                return;
            }

            SkeletonFrame SFrame = e.OpenSkeletonFrame();
            DepthImageFrame DFrame = e.OpenDepthImageFrame();

            if (SFrame == null)
                return; //Couldn't Open Frame

            DepthImagePoint[] p = new DepthImagePoint[20];

            using (DFrame)
            {
                if (DFrame == null)
                    return;
                //See if the depth Proc is null
                if (depthproc == null)
                {
                    depthproc = new DepthProcessor(DFrame.PixelDataLength, DFrame.Width, DFrame.Height, 1);
                    proc.mouseController.dproc = depthproc;
                    window.depthPlayer.Source = depthproc.bitmap;
                }

                depthproc.setPlayerDepthData(DFrame);
            }

            using (SFrame)
            {
                Skeleton[] skeletons = new Skeleton[SFrame.SkeletonArrayLength];
                SFrame.CopySkeletonDataTo(skeletons);

                SkeletonPoint[] processingPoints;

                foreach (Skeleton s in skeletons)
                {
                    if (s.TrackingState == SkeletonTrackingState.Tracked)
                    {

                        processingPoints = getPointsOfInterest(s);
                        
                        //Procesar los puntos de interes
                        proc.processJoints(processingPoints);

                        /*for (int i = 0; i < processingPoints.Length; i++)
                            p[i] = kinect.MapSkeletonPointToDepth(processingPoints[i], 0);*/
                    }
                }

            }
            
            

        }

    //Skeleton Calibrate
        public void allFramesCalibrateReady(object sender,AllFramesReadyEventArgs e)
        {
            
            SkeletonFrame SFrame = e.OpenSkeletonFrame();
            if (SFrame == null)
                return; //Couldn't Open Frame

            using (SFrame)
            {
                Skeleton[] skeletons = new Skeleton[SFrame.SkeletonArrayLength];
                SFrame.CopySkeletonDataTo(skeletons);

                foreach (Skeleton s in skeletons)
                {
                    if (s.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        
                        switch (proc.trySkeletonRange(s))
                        {
                            case Instruction.MOVE_UP: 
                                //Move up
                                kinect.ElevationAngle = (kinect.MaxElevationAngle + kinect.ElevationAngle) / 2;
                                break;
                            case Instruction.MOVE_DOWN: 
                                //Move Down
                                kinect.ElevationAngle = (kinect.ElevationAngle + kinect.MinElevationAngle) / 2;
                                break;
                            case Instruction.ON_RANGE: 
                                //This one is good, stop the calibrating and record the calibrated skeleton
                                doneCalibratingSequence(s);
                                break;
                        }
                        
                        //Need to sleep 1 second... so we don't break apart the kinect
                        System.Threading.Thread.Sleep(1000);

                    }
                }
            }

        }
        
    //Get Skeleton Points
            private SkeletonPoint[] getPointsOfInterest(Skeleton s)
        {
            SkeletonPoint[] sp = new SkeletonPoint[Joints.length];

            //Get shoulders
            sp[Joints.SHOULDER_L] = s.Joints[JointType.ShoulderLeft].Position;
            sp[Joints.SHOULDER_R] = s.Joints[JointType.ShoulderRight].Position;

            //Get Ankles
            sp[Joints.ANKLE_L] = s.Joints[JointType.AnkleLeft].Position;
            sp[Joints.ANKLE_R] = s.Joints[JointType.AnkleRight].Position;

            //Wrists
            sp[Joints.WRIST_L] = s.Joints[JointType.WristLeft].Position;
            sp[Joints.WRIST_R] = s.Joints[JointType.WristRight].Position;

            //Spine
            sp[Joints.SPINE] = s.Joints[JointType.Spine].Position;

            //Hips
            sp[Joints.HIP_L] = s.Joints[JointType.HipLeft].Position;
            sp[Joints.HIP_R] = s.Joints[JointType.HipRight].Position;

            //Hands
            sp[Joints.HAND_L] = s.Joints[JointType.HandLeft].Position;
            sp[Joints.HAND_R] = s.Joints[JointType.HandRight].Position;

            //Elbow
            sp[Joints.ELBOW_L] = s.Joints[JointType.ElbowLeft].Position;
            sp[Joints.ELBOW_R] = s.Joints[JointType.ElbowRight].Position;

            //Head
            sp[Joints.HEAD] = s.Joints[JointType.Head].Position;
            return sp;


        }

        //Stop Kinect
        public void StopKinect(KinectSensor old)
            {

                old.Stop();
                old.AudioSource.Stop();

            }

        //Start new Kinect
        public void initNewKinect(KinectSensor newKinect)
            {
                //Enable the modules
                newKinect.DepthStream.Enable();
                newKinect.SkeletonStream.Enable();
                //newKinect.ColorStream.Enable();

                try
                {
                    newKinect.Start();
                }
                catch (System.IO.IOException) { }


                //Add the kinect variable
                kinect = newKinect;
                
                newKinect.AllFramesReady +=new EventHandler<AllFramesReadyEventArgs>(AllFramesReady);
                //newKinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);

                this.colorBitmap = new WriteableBitmap(kinect.DepthStream.FrameWidth, kinect.DepthStream.FrameHeight,
              96.0, 96.0, System.Windows.Media.PixelFormats.Bgr32, null);
            }

        public static DepthImagePoint mapSkeletonPoint3DToDepthPoint(System.Windows.Media.Media3D.Point3D point)
        {
            //The Point must be converted to the original Skeleton
            SkeletonPoint sp = new SkeletonPoint();
            sp.X = (float)point.X;
            sp.Y = (float)point.Y;
            sp.Z = (float)point.Z;

            //Then it must be converted to a DepthFrame Coordinate
            DepthImagePoint dp = kinect.MapSkeletonPointToDepth(sp, kinect.DepthStream.Format);
            dp.X = Math.Min(dp.X, kinect.DepthStream.FrameWidth);
            dp.Y = Math.Min(dp.Y, kinect.DepthStream.FrameHeight);
            return dp;
            
        }

        public static System.Windows.Point DepthPointToIndexes(DepthImagePoint dp)
        {
            float xd = Math.Min(dp.X * kinect.DepthStream.FrameWidth, kinect.DepthStream.FrameWidth);
            float yd = Math.Min(dp.Y * kinect.DepthStream.FrameHeight, kinect.DepthStream.FrameHeight);

            return new System.Windows.Point(xd, yd);
        }

    }

    }


