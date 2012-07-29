using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Media.Media3D;
using WindowsInput;


namespace kinecontrol
{
    class JointProcessor
    {
        public float umbral_movimiento
        {
            get;
            set;
        }

        public float umbral_altura
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
            }
            get { return _calibratedPoints; }
        }

        //Mouse Simulator
        private MouseController _mouseController;
        public MouseController mouseController
        {
            get { return _mouseController; }
            set { _mouseController = value; }
        }



        public JointProcessor()
        {
            _mouseController = new MouseController(MouseModes.DELTA);
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
            if (-diff[Joints.SHOULDER_L].Z > umbral_movimiento && -diff[Joints.SHOULDER_R].Z > umbral_movimiento)
            {
                //See if it's pressed
                /*if (InputSimulator.IsKeyDown(VirtualKeyCode.VK_W) == false)
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_W);*/
            }
            else if (InputSimulator.IsKeyDown(VirtualKeyCode.VK_W))
                InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_W);

            //Reverse Movement
            if (diff[Joints.SHOULDER_L].Z > umbral_movimiento && diff[Joints.SHOULDER_R].Z > umbral_movimiento)
            {
                /*if (InputSimulator.IsKeyDown(VirtualKeyCode.VK_S) == false)
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_S);*/
            }
            else if (InputSimulator.IsKeyDown(VirtualKeyCode.VK_S))
                InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_S);


            //Jump - problems when going forward
            if (diff[Joints.SHOULDER_L].Y > (umbral_altura/2) || diff[Joints.SHOULDER_R].Y > (umbral_altura/2))
            {
                //if (InputSimulator.IsKeyDown(VirtualKeyCode.SPACE) == false)
                InputSimulator.SimulateKeyPress(VirtualKeyCode.SPACE);
            }
            /*else if (InputSimulator.IsKeyDown(VirtualKeyCode.SPACE))
                InputSimulator.SimulateKeyUp(VirtualKeyCode.SPACE);*/

            //Crouch - Problems when going back ... it thinks one is crouching
            //TODO: Configure for type of crouching
            if (-diff[Joints.SHOULDER_L].Y > (umbral_altura) || -diff[Joints.SHOULDER_R].Y > (umbral_altura))
            {
                /*if (InputSimulator.IsKeyDown(VirtualKeyCode.CONTROL) == false)
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.CONTROL);*/
            }
            else if (InputSimulator.IsKeyDown(VirtualKeyCode.CONTROL))
                InputSimulator.SimulateKeyUp(VirtualKeyCode.CONTROL);

            //Left Movement
            if (-diff[Joints.SHOULDER_L].X > umbral_movimiento && -diff[Joints.SHOULDER_R].X > umbral_movimiento)
            {
                /*if (InputSimulator.IsKeyDown(VirtualKeyCode.VK_A) == false)
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_A);*/
            }
            else if (InputSimulator.IsKeyDown(VirtualKeyCode.VK_A))
                InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_A);

            //Right Movement
            if (diff[Joints.SHOULDER_L].X > umbral_movimiento && diff[Joints.SHOULDER_R].X > umbral_movimiento)
            {
                /*if (InputSimulator.IsKeyDown(VirtualKeyCode.VK_D) == false)
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_D);*/
            }
            else if (InputSimulator.IsKeyDown(VirtualKeyCode.VK_D))
                InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_D);

            //Process Mouse Movements
            //We give whole body
            _mouseController.processHands(transformToPoint3D(point));
        }

        static public Point3D normalizeXY(Point3D p, double f)
        {
            p.X = p.X / f;
            p.Y = p.Y / f;
            return p;
        }

        static public Point3D[] normalizeXYUsingSkeletonPoint(Point3D[] o, SkeletonPoint[] s)
        {
            Point3D[] sp = new Point3D[Joints.length];


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

            //Spine
            sp[Joints.SPINE].X = o[Joints.SPINE].X / s[Joints.SPINE].Z;
            sp[Joints.SPINE].Y = o[Joints.SPINE].Y / s[Joints.SPINE].Z;
            sp[Joints.SPINE].Z = o[Joints.SPINE].Z;

            //HIP
            sp[Joints.HIP_L].X = o[Joints.HIP_L].X / s[Joints.HIP_L].Z;
            sp[Joints.HIP_L].Y = o[Joints.HIP_L].Y / s[Joints.HIP_L].Z;
            sp[Joints.HIP_L].Z = o[Joints.HIP_L].Z;

            sp[Joints.HIP_R].X = o[Joints.HIP_R].X / s[Joints.HIP_R].Z;
            sp[Joints.HIP_R].Y = o[Joints.HIP_R].Y / s[Joints.HIP_R].Z;
            sp[Joints.HIP_R].Z = o[Joints.HIP_R].Z;

            //HANDS
            sp[Joints.HAND_L].X = o[Joints.HAND_L].X / s[Joints.HAND_L].Z;
            sp[Joints.HAND_L].Y = o[Joints.HAND_L].Y / s[Joints.HAND_L].Z;
            sp[Joints.HAND_L].Z = o[Joints.HAND_L].Z;

            sp[Joints.HAND_R].X = o[Joints.HAND_R].X / s[Joints.HAND_R].Z;
            sp[Joints.HAND_R].Y = o[Joints.HAND_R].Y / s[Joints.HAND_R].Z;
            sp[Joints.HAND_R].Z = o[Joints.HAND_R].Z;

            //ELBOW
            sp[Joints.ELBOW_L].X = o[Joints.ELBOW_L].X / s[Joints.ELBOW_L].Z;
            sp[Joints.ELBOW_L].Y = o[Joints.ELBOW_L].Y / s[Joints.ELBOW_L].Z;
            sp[Joints.ELBOW_L].Z = o[Joints.ELBOW_L].Z;

            sp[Joints.ELBOW_R].X = o[Joints.ELBOW_R].X / s[Joints.ELBOW_R].Z;
            sp[Joints.ELBOW_R].Y = o[Joints.ELBOW_R].Y / s[Joints.ELBOW_R].Z;
            sp[Joints.ELBOW_R].Z = o[Joints.ELBOW_R].Z;

            //HEAD
            sp[Joints.HEAD].X = o[Joints.HEAD].X / s[Joints.HEAD].Z;
            sp[Joints.HEAD].Y = o[Joints.HEAD].Y / s[Joints.HEAD].Z;
            sp[Joints.HEAD].Z = o[Joints.HEAD].Z;

            return sp;

        }

        static public Point3D normalizeXYPoint3D(Point3D o, double s)
        {
            Point3D sp = new Point3D(o.X / s, o.Y / s, o.Z);
            return sp;
        }

        public void setArmLength()
        {

            //Set arm length
            _mouseController.armLength = Math.Abs(_calibratedPoints[Joints.SHOULDER_L].Y - _calibratedPoints[Joints.ANKLE_L].Y);
            _mouseController.armLength /= 2;

            //Arm length needs the person to put his hand in front of him, palm open
            //mouseController.armLength = Math.Abs(_calibratedPoints[Joints.SHOULDER_R].Z - _calibratedPoints[Joints.HAND_R].Z);
          
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

        static public Point3D transformToPoint3D(SkeletonPoint s)
        {
            double x = s.X;
            double y = s.Y;
            double z = s.Z;

            return new Point3D(x, y, z);
        }

        static public Point3D[] transformToPoint3D(SkeletonPoint[] s)
        {

            Point3D[] p = new Point3D[s.Length];

            for (int i = 0; i < s.Length; i++)
                p[i] = transformToPoint3D(s[i]);

            return p;
        }

        /// <summary>
        /// Method for knowing if a point is close to another point
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <param name="closeThreshold">Threshold for comparison</param>
        /// <returns>True if both points are close, false if not</returns>
        static public bool isPointCloseTo(Point3D p1, Point3D p2, float closeThreshold)
        {
            //True if distance is < some threshold
            double x = p1.X - p2.X;
            x *= x;

            double y = p1.Y - p2.Y;
            y *= y;

            /*double z = p1.Z - p2.Z;
            z *= z;*/

            //Distance
            float distance = Approximate.Sqrt((float)(x + y));

            if(distance > closeThreshold)
                return false;

            return true;

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
        public const int SPINE = 6;

        //New Added
        public const int HIP_L = 7;
        public const int HIP_R = 8;
        public const int HAND_L = 9;
        public const int HAND_R = 10;

        //TODO add elbow
        public const int ELBOW_L = 11;
        public const int ELBOW_R = 12;

        //HEAD
        public const int HEAD = 13;

        //Length
        public const int length = 14;


    }

    static class Instruction
    {
        public const int ON_RANGE = 0;
        public const int MOVE_UP = 1;
        public const int MOVE_DOWN = 2;
        public const int UNDEF = -1;

    }
}
