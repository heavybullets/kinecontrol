using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.Kinect;

namespace kinecontrol
{
    class KinectUtils
    {

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
            kinect.SkeletonFrameReady -= new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);

            //First change the tilt angle of the Kinect to the lowest
            kinect.ElevationAngle = (kinect.MaxElevationAngle + kinect.MinElevationAngle)/2;

            //Start looking for the best position
            kinect.SkeletonFrameReady +=new EventHandler<SkeletonFrameReadyEventArgs>(skeletonFrameCalibrateReady);
        }

        //When calibration was succesful, we resume the normal mode and store the calibrated skeleton
        public void doneCalibratingSequence(Skeleton s)
        {
            this.proc.calibratedPoints = getPointsOfInterest(s);
            this.proc.setArmLength();

            kinect.SkeletonFrameReady -= new EventHandler<SkeletonFrameReadyEventArgs>(skeletonFrameCalibrateReady);

            //resume normal mode
            kinect.SkeletonFrameReady +=new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
        }

        //Skeleton Frame Ready
        void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame SFrame = e.OpenSkeletonFrame();

            if (SFrame == null)
                return; //Couldn't Open Frame

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

                    }
                }

            }   
        }

    //Skeleton Calibrate
        public void skeletonFrameCalibrateReady(object sender, SkeletonFrameReadyEventArgs e)
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
            SkeletonPoint[] sp = new SkeletonPoint[6];

            //Get shoulders
            sp[Joints.SHOULDER_L] = s.Joints[JointType.ShoulderLeft].Position;
            sp[Joints.SHOULDER_R] = s.Joints[JointType.ShoulderRight].Position;

            //Get Ankles
            sp[Joints.ANKLE_L] = s.Joints[JointType.AnkleLeft].Position;
            sp[Joints.ANKLE_R] = s.Joints[JointType.AnkleRight].Position;

            //Wrists
            sp[Joints.WRIST_L] = s.Joints[JointType.WristLeft].Position;
            sp[Joints.WRIST_R] = s.Joints[JointType.WristRight].Position;

<<<<<<< HEAD
=======
            //Shoulder Right and Left
            sp[Joints.SPINE] = s.Joints[JointType.Spine].Position;

>>>>>>> RollBack?
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

                newKinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
            }


        }

    }


