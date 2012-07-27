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
        
        private JointProcessor proc
        {
            set;
            get;
        }

        public KinectSensor kinect
        {
            set;
            get;
        }

        public KinectUtils(KinectSensor k)
        {
            kinect = k;
            this.proc = new JointProcessor();
            proc.umbral = 0.1f;
        }

        public KinectUtils()
        {
            this.proc = new JointProcessor();
            proc.umbral = 0.2f;
        }

        //For Calibrating the Kinect
        public void startCalibratingSequence()
        {
            //Turn off the standard skeleton frame ready
            kinect.AllFramesReady -= new EventHandler<AllFramesReadyEventArgs>(AllFramesReady);

            //First change the tilt angle of the Kinect to the lowest
            kinect.ElevationAngle = (kinect.MaxElevationAngle + kinect.MinElevationAngle)/2;

            //Start looking for the best position
            kinect.AllFramesReady +=new EventHandler<AllFramesReadyEventArgs>(allFramesCalibrateReady);
        }

        //When calibration was succesful, we resume the normal mode and store the calibrated skeleton
        public void doneCalibratingSequence(Skeleton s)
        {
            this.proc.calibratedPoints = getPointsOfInterest(s);
            this.proc.setArmAndWristLength();

            kinect.AllFramesReady -= new EventHandler<AllFramesReadyEventArgs>(allFramesCalibrateReady);

            //resume normal mode
            kinect.AllFramesReady +=new EventHandler<AllFramesReadyEventArgs>(AllFramesReady);
        }

        //Skeleton Frame Ready
        void AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            SkeletonFrame SFrame = e.OpenSkeletonFrame();
            DepthImageFrame DFrame = e.OpenDepthImageFrame();

            if (SFrame == null)
                return; //Couldn't Open Frame

            DepthImagePoint[] p = new DepthImagePoint[20];

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
            /*
            short[] pixelData = new short[kinect.DepthStream.FramePixelDataLength];
            using (DFrame)
            {
                DFrame.CopyPixelDataTo(pixelData);
            }*/
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

                this.colorBitmap = new WriteableBitmap(this.kinect.DepthStream.FrameWidth, this.kinect.DepthStream.FrameHeight,
              96.0, 96.0, System.Windows.Media.PixelFormats.Bgr32, null);
            }

    }

    }


