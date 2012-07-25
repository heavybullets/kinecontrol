using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Media.Media3D;
using WindowsInput;

//Use the inputSimulator


namespace kinecontrol
{
    class JointProcessor
    {
        public float umbral
        {
            get;
            set;
        }
        private SkeletonPoint[] _calibratedPoints;

        public SkeletonPoint[] calibratedPoints
        {
            set
            {
                _calibratedPoints = value;
                mouseController.rh_center = JointProcessor.transformToPoint3D(value[Joints.WRIST_R]);
            }
            get { return _calibratedPoints; }
        }

        //Mouse Simulator
        private MouseController mouseController;

        private float armLength;

        public JointProcessor()
        {
            mouseController = new MouseController(MouseModes.DELTA);
        }
        
        //Method for processing Joints and making events
        public void processJoints(SkeletonPoint[] point)
        {
            Point3D[] diff = new Point3D[point.Length];

            float x, y, z;

            if (_calibratedPoints == null)
                return;

            //Get the substraction
            for (int i = 0; i < point.Length; i++)
            {
                x = point[i].X - _calibratedPoints[i].X;
                y = point[i].Y - _calibratedPoints[i].Y;
                z = point[i].Z - _calibratedPoints[i].Z;

                diff[i] = new Point3D(x, y, z);
            }

            diff = normalizeXYUsingSkeletonPoint(diff, point);
            

            //Front-Reverse Movement
            if (-diff[Joints.SHOULDER_L].Z > umbral && -diff[Joints.SHOULDER_R].Z > umbral)
            {
                //See if it's pressed
                if (InputSimulator.IsKeyDown(VirtualKeyCode.VK_W) == false)
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_W);
            }
            else if(InputSimulator.IsKeyDown(VirtualKeyCode.VK_W))
                InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_W);

            //Reverse Movement
            if (diff[Joints.SHOULDER_L].Z > umbral && diff[Joints.SHOULDER_R].Z > umbral)
            {
                if (InputSimulator.IsKeyDown(VirtualKeyCode.VK_S) == false)
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_S);
            }
            else if (InputSimulator.IsKeyDown(VirtualKeyCode.VK_S))
                    InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_S);


            //Jump - problems when going forward
            if (diff[Joints.SHOULDER_L].Y > (umbral / 3) && diff[Joints.SHOULDER_R].Y > (umbral / 3))
            {
                //if (InputSimulator.IsKeyDown(VirtualKeyCode.SPACE) == false)
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.SPACE);
            }
            /*else if (InputSimulator.IsKeyDown(VirtualKeyCode.SPACE))
                InputSimulator.SimulateKeyUp(VirtualKeyCode.SPACE);*/

            //Crouch - Problems when going back ... it thinks one is crouching
            //TODO: Configure for type of crouching
            if (-diff[Joints.SHOULDER_L].Y > (umbral / 3) && -diff[Joints.SHOULDER_R].Y > (umbral / 3))
            {
                if (InputSimulator.IsKeyDown(VirtualKeyCode.CONTROL) == false)
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.CONTROL);
            }
            else if (InputSimulator.IsKeyDown(VirtualKeyCode.CONTROL))
                InputSimulator.SimulateKeyUp(VirtualKeyCode.CONTROL);
            
            //Left Movement
            if (-diff[Joints.SHOULDER_L].X > umbral && -diff[Joints.SHOULDER_R].X > umbral)
            {
                if (InputSimulator.IsKeyDown(VirtualKeyCode.VK_A) == false)
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_A);
            }
            else if (InputSimulator.IsKeyDown(VirtualKeyCode.VK_A))
                InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_A);

            //Right Movement
            if (diff[Joints.SHOULDER_L].X > umbral && diff[Joints.SHOULDER_R].X > umbral)
            {
                if (InputSimulator.IsKeyDown(VirtualKeyCode.VK_D) == false)
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_D);
            }
            else if (InputSimulator.IsKeyDown(VirtualKeyCode.VK_D))
                InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_D);

            //Process Mouse Movements
            //We give whole movements, no Deltas
            mouseController.processHands(transformToPoint3D(point[Joints.WRIST_R]), transformToPoint3D(point[Joints.WRIST_L]));
        }

        static public Point3D normalizeXY(Point3D p, double f)
        {
            p.X = p.X / f;
            p.Y = p.Y / f;

            return p;
        }

        static public Point3D[] normalizeXYUsingSkeletonPoint(Point3D[] o, SkeletonPoint[] s)
        {
            Point3D[] sp = new Point3D[6];

            //Get shoulders
            sp[Joints.SHOULDER_L].X = o[Joints.SHOULDER_L].X / s[Joints.SHOULDER_L].Z;
            sp[Joints.SHOULDER_L].Y = o[Joints.SHOULDER_L].Y / s[Joints.SHOULDER_L].Z;
            sp[Joints.SHOULDER_L].Z = o[Joints.SHOULDER_L].Z;

            sp[Joints.SHOULDER_R].X = o[Joints.SHOULDER_R].X / s[Joints.SHOULDER_R].Z;
            sp[Joints.SHOULDER_R].Y = o[Joints.SHOULDER_R].Y / s[Joints.SHOULDER_R].Z;
            sp[Joints.SHOULDER_R].Z = o[Joints.SHOULDER_R].Z;

            //Get Ankles
            sp[Joints.ANKLE_L].X = o[Joints.ANKLE_L].X / s[Joints.ANKLE_L].Z;
            sp[Joints.ANKLE_L].Y = o[Joints.ANKLE_L].Y / s[Joints.ANKLE_L].Z;
            sp[Joints.ANKLE_L].Z = o[Joints.ANKLE_L].Z;

            sp[Joints.ANKLE_R].X = o[Joints.ANKLE_R].X / s[Joints.ANKLE_R].Z;
            sp[Joints.ANKLE_R].Y = o[Joints.ANKLE_R].Y / s[Joints.ANKLE_R].Z;
            sp[Joints.ANKLE_R].Z = o[Joints.ANKLE_R].Z;

            //Wrists
            sp[Joints.WRIST_L].X = o[Joints.WRIST_L].X / s[Joints.WRIST_L].Z;
            sp[Joints.WRIST_L].Y = o[Joints.WRIST_L].Y / s[Joints.WRIST_L].Z;
            sp[Joints.WRIST_L].Z = o[Joints.WRIST_L].Z;

            sp[Joints.WRIST_R].X = o[Joints.WRIST_R].X / s[Joints.WRIST_R].Z;
            sp[Joints.WRIST_R].Y = o[Joints.WRIST_R].Y / s[Joints.WRIST_R].Z;
            sp[Joints.WRIST_R].Z = o[Joints.WRIST_R].Z;

            return sp;

        }

        static public Point3D normalizeXYPoint3D(Point3D o, double s)
        {
            Point3D sp = new Point3D(o.X/s, o.Y/s, o.Z);
            return sp;
        }

        public void setArmLength()
        {

            //Set arm length
            armLength = _calibratedPoints[Joints.SHOULDER_L].Y - _calibratedPoints[Joints.ANKLE_L].Y;
            armLength /= 2;
        }

        public int trySkeletonRange(Skeleton s)
        {
            if (s.Joints[JointType.Head].TrackingState == JointTrackingState.Tracked && s.Joints[JointType.FootLeft].TrackingState == JointTrackingState.Tracked
                && s.Joints[JointType.FootRight].TrackingState == JointTrackingState.Tracked)
            {
                return Instruction.ON_RANGE;
            }

            //If not on range, tell where to change
            else if (s.Joints[JointType.Head].TrackingState != JointTrackingState.Tracked)
            {
                return Instruction.MOVE_UP;
            }
            else if (s.Joints[JointType.FootRight].TrackingState != JointTrackingState.Tracked || s.Joints[JointType.FootRight].TrackingState != JointTrackingState.Tracked)
            {
                return Instruction.MOVE_DOWN;
            }

            return Instruction.UNDEF;
        }

        static Point3D transformToPoint3D(SkeletonPoint s)
        {
            double x = s.X;
            double y = s.Y;
            double z = s.Z;

            return new Point3D(x, y, z);
        }


    }

    //Joints Class
    static class Joints
    {

        public const int SHOULDER_L = 0;
        public const int SHOULDER_R = 1;
        public const int ANKLE_L = 2;
        public const int ANKLE_R = 3;
        public const int WRIST_L = 4;
        public const int WRIST_R = 5;


    }

    static class Instruction
    {
        public const int ON_RANGE = 0;
        public const int MOVE_UP = 1;
        public const int MOVE_DOWN = 2;
        public const int UNDEF = -1;
    }
}