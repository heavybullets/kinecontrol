using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Timers;
using Microsoft.Kinect;

namespace kinecontrol
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private KinectUtils util
        {
            set;
            get;
        }

        public MainWindow()
        {
            InitializeComponent();
            util = new KinectUtils();
            util.window = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);

        }

        //Constructor de la kinect
        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor oldK = (KinectSensor)e.OldValue;
            KinectSensor newK = (KinectSensor)e.NewValue;

            //Stop the old kinet
            if(oldK != null)
                util.StopKinect(oldK);

            util.initNewKinect(newK);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            util.StopKinect(kinectSensorChooser1.Kinect);
        }



        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Timer time = new Timer(5000);
            time.AutoReset = false;
            time.Elapsed += new ElapsedEventHandler(calibrateTimeElapsed);
            time.Start();
            
        }

        void calibrateTimeElapsed(object sender, ElapsedEventArgs e)
        {
            //Prepare for calibration
            util.startCalibratingSequence();
        }

        }

    }

