using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsInputB;
using System.Windows.Media.Media3D;

namespace kinecontrol
{
    /// <summary>
    /// Class that controls the mouse movements, sets the speed of movement and the direction of movement
    /// </summary>
    class MouseController
    {
        private MouseSimulator mouse;
        private int mode
        {
            get;
            set;
        }

        /// <summary>
        /// The sensibility parameter
        /// </summary>
        private int sensibility
        {
            get;
            set;
        }

        public float armLength
        {
            get;
            set;
        }

        public double threshold
        {
            get;
            set;
        }

        public bool rightHanded
        {
            get;
            set;
        }

        /// <summary>
        /// Main Constructor, set the Mode to Delta
        /// </summary>
        public MouseController()
        {
            mouse = new MouseSimulator();
            mode = MouseDefaults.mode;
            sensibility = MouseDefaults.sens;
            threshold = MouseDefaults.threshold;
            rightHanded = MouseDefaults.rightHanded;
        }

        /// <summary>
        /// Main Constructor with Mode Setting
        /// </summary>
        /// <param name="right_hand_center">The center of the right hand</param>
        public MouseController(int _mode)
        {
            mouse = new MouseSimulator();
            mode = _mode;
            sensibility = MouseDefaults.sens;
            threshold = MouseDefaults.threshold;
            rightHanded = MouseDefaults.rightHanded;
        }

        /// <summary>
        /// Main Constructor with Mode and Sensibility setting
        /// </summary>
        /// <param name="_mode"></param>
        /// <param name="sens"></param>
        public MouseController(int _mode, int sens)
        {
            mouse = new MouseSimulator();
            mode = _mode;
            sensibility = sens;
            threshold = MouseDefaults.threshold;
            rightHanded = MouseDefaults.rightHanded;
        }

        /// <summary>
        /// Method for processing and getting the hand movements
        /// </summary>
        /// <param name="body">The array of Points that define the movement</param>
        public void processHands(Point3D[] body)
        {
            //Perform Hand Actions
            supportHandActions(body);
            primaryHandActions(body);

        }

        /// <summary>
        /// Method for interpretating Support Hand Movements as Modified Mouse Actions, Stores the actions in the SupportHand Static Class
        /// </summary>
        /// <param name="body">The array of points that define the body</param>
        private void supportHandActions(Point3D[] body)
        {
            Point3D shand, ship, pwrist, pelbow;

            if (rightHanded)
            {
                pwrist = body[Joints.WRIST_R];
                shand = body[Joints.HAND_L];
                ship = body[Joints.HIP_L];
                pelbow = body[Joints.ELBOW_R];
            }
            else
            {
                pwrist = body[Joints.WRIST_L];
                shand = body[Joints.HAND_R];
                ship = body[Joints.HIP_R];
                pelbow = body[Joints.ELBOW_L];
            }

            //Fine Aim
            bool isCloseToWrist = JointProcessor.isPointCloseTo(shand, pwrist, MouseDefaults.closeThreshold);
            bool isCloseToElbow = JointProcessor.isPointCloseTo(shand, pelbow, MouseDefaults.closeThreshold);
            bool isCloseToSpine = JointProcessor.isPointCloseTo(shand, body[Joints.SPINE], MouseDefaults.closeThreshold);

            if (isCloseToSpine || isCloseToElbow || isCloseToWrist)
            {
                if (SupportHand.shouldReloadGun)
                {
                    WindowsInput.InputSimulator.SimulateKeyPress(WindowsInput.VirtualKeyCode.VK_R);
                    SupportHand.shouldReloadGun = false;
                }
                SupportHand.inFineAimMode = true;
            }
            else
                SupportHand.inFineAimMode = false;

            //Reload
            if (JointProcessor.isPointCloseTo(shand, ship, MouseDefaults.closeThreshold/2))
                SupportHand.shouldReloadGun = true;


        }

        /// <summary>
        /// Method for interpretating Main Hand Movements as mouse actions
        /// Aim is made with the Primary arm wrist... and shooting is made by moving the hand
        /// </summary>
        /// <param name="body">The array of points that define the body</param>
        private void primaryHandActions(Point3D[] body)
        {
            Point3D phand, pwrist, center;

            center = getCenterPoint(body);

            if (rightHanded)
            {
                phand = body[Joints.HAND_R];
                pwrist = body[Joints.WRIST_R];
            }
            else
            {
                phand = body[Joints.HAND_L];
                pwrist = body[Joints.WRIST_L];
            }

            //Mouse movements
            if (mode == MouseModes.DELTA)
            {

                Point3D delta = (Point3D)(center - pwrist);

                //Normalize
                delta = JointProcessor.normalizeXY(delta, pwrist.Z);

                //Have the delta movement, need to call the mouse
                Vector3D movement = processPointWithThresholdAndSensibility(delta);

                int x = (int)Math.Round(movement.X);
                int y = (int)Math.Round(movement.Y);

                mouse.MoveMouseBy(x, y);

            }

            //Mouse shoot

            //Old method
            /*double z = center.Z - phand.Z;

            if (z > armLength/3)
                mouse.LeftButtonDown();
            else
                mouse.LeftButtonUp();
            */
        }

        /// <summary>
        /// Method for getting the center point, definition of center point might variate during execution
        /// </summary>
        /// <param name="body">Array with body coordinates</param>
        /// <returns>Definition of Center Point</returns>
        private Point3D getCenterPoint(Point3D[] body)
        {
            Point3D center = new Point3D();
            center.Y = body[Joints.SPINE].Y;

            if (rightHanded)
            {
                center.X = body[Joints.SHOULDER_R].X;
                center.Z = body[Joints.SHOULDER_R].Z - armLength / 2;
            }

            else
            {
                center.X = body[Joints.SHOULDER_L].X;
                center.Z = body[Joints.SHOULDER_L].Z - armLength / 2;
            }
            return center;
        }

        /*//Not working for now
        /// <summary>
        /// Method for setting the enviroment for the aim mode
        /// </summary>
        /// <param name="body">The array of points that define the body</param>
        private void setAimMode(Point3D[] body)
        {
            //Hand Configure
            Point3D shoulder, elbow;

            if (rightHanded)
            {
                shoulder = body[Joints.SHOULDER_R];
                elbow = body[Joints.ELBOW_R];
            }
            else
            {
                shoulder = body[Joints.SHOULDER_L];
                elbow = body[Joints.ELBOW_L];
            }

            //See mode
            double y = shoulder.Y - elbow.Y;
            if (Math.Abs(y) < MouseDefaults.closeThreshold/2)
            {
                PrimaryHand.mode = PrimaryHand.aimMode_Scope;
                //Call the right click button
                //mouse.RightButtonDown(); //Neccesary?

            }
            else
            {
                //Normal Mode
                PrimaryHand.mode = PrimaryHand.aimMode_Normal;
                //mouse.RightButtonUp();
            }

            //Debugging
            System.Console.Out.WriteLine("Mode is" + PrimaryHand.mode);

        }
        */

        /// <summary>
        /// This methods process the delta point given and adds the ThresHold and Sensibility Effect
        /// </summary>
        /// <param name="d">The Delta Point holding the relative movement of the hand, against the Defined CenterPoint</param>
        /// <returns>The Delta Vector holding X and Y movements</returns>
        public Vector3D processPointWithThresholdAndSensibility(Point3D d)
        {
            //TODO: see if necesary
            /*if (SupportHand.shouldMoveAim == false)
                return new Vector3D(0, 0, 0);
            */

            double x,y;
            double abx = Math.Abs(d.X);
            double aby = Math.Abs(d.Y); ;
            //First see if the Vector is greater than the threshold
            if (abx > threshold)
            {

                x = d.X - Math.Sign(d.X) * threshold;

                //Check Y
                //If it's greater see if the Y coordinate is greater, if it is, substract the treshold, if it isn't, the Y coordinate is 0
                if (aby > threshold)
                    y = d.Y - Math.Sign(d.Y) * threshold;

                else
                {
                    y = 0;
                    aby = 0;
                }
            }

            else if (aby > threshold)
            {
                y = d.Y - Math.Sign(d.Y) * threshold;
                x = 0;
                abx = 0;
            }

            else
            {
                //No movement here
                return new Vector3D(0, 0, 0);
            }

            float sens;
            if (SupportHand.inFineAimMode)
                sens = sensibility * SupportHand.fineAimModeFactor;
            else
                sens = sensibility;

            return new Vector3D(-x * sens, y * sens, 0);
           
        }


    }

    static class MouseModes
    {
        public const int DELTA = 0;
        public const int ABSOLUTE = 1;
    }

    static class MouseDefaults
    {
        public const int sens = 100;
        public const int mode = MouseModes.ABSOLUTE;
        public const double threshold = 0.01f;
        public const bool rightHanded = true;
        public const float closeThreshold = 0.2f;
    }

    class SupportHand
    {
        public static bool shouldReloadGun = false;
        //public static bool shouldMoveAim = true; necesary?
        public static bool inFineAimMode = false;
        public static float fineAimModeFactor = 0.2f;
    }

}


