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

        public double threshold
        {
            get;
            set;
        }

        public Point3D rh_center
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
        }

        /// <summary>
        /// Process the Hand Movements into Mouse Actions
        /// </summary>
        /// <param name="rhand">Right Hand position</param>
        /// <param name="lhand">Left Hand position</param>
        public void processHands(Point3D rhand, Point3D lhand)
        {
            //TODO: sacar angulo de desfase y magnitud para obtener vector de movimiento

            if (mode == MouseModes.DELTA)
            {
                //Obtener Deltas de movimiento
                Point3D delta = (Point3D)(rh_center - rhand);

                //Normalize
                delta = JointProcessor.normalizeXY(delta, rhand.Z);
                
                //Have the delta movement, need to call the mouse
                //We use a 15-direction based movement, ranging from -2 to +2 on the X and Y axis
                Vector3D movement = getDeltaVector(delta);

                int x = (int) Math.Round(movement.X */* movement.Z */ sensibility);
                int y = (int)Math.Round(movement.Y * /*movement.Z */ sensibility);

                mouse.MoveMouseBy(x,y);
            }

        }

        /// <summary>
        /// This methods process the delta point given transforming it into a Movement Vector, to be used by the mouse
        /// </summary>
        /// <param name="d">The Delta Point holding the relative movement of the hand</param>
        /// <returns>The Delta Vector holding X and Y movements, scaled to a -2 , 2 Range each, and Z representing the Magnitude</returns>
        public Vector3D getDeltaVector(Point3D d)
        {
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
            
            //TEST
            return new Vector3D(-x, y, 0);
            //Get the approximate magnitude
            double temp = x * x + y * y;
            double magnitude = Approximate.Sqrt((float)temp) * 1.5;
            bool a;
            //Scale the Variables to get the directions
            if (abx > aby)
            {
                x = Math.Sign(x) * 2;
                y = Math.Round(2 * y / abx);
                if (y > 2) 
                a = true;

            }

            else
            {
                y = Math.Sign(y) * 2;
                x = Math.Round(2 * x / aby);

                if (x > 2) 
                    a = true;

            }

            //Return the vector
            return new Vector3D(-x, y, magnitude);
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
    }

}


