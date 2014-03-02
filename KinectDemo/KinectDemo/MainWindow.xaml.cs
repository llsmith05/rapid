using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //active sensor
        private KinectSensor sensor;

        //Bitmap
        private WriteableBitmap colorBmp;

        //byte array for intermediate camera data storate
        private byte[] colorPixels;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        //Start items on window load
         private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                //Turn on color stream
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                //allocate space in byte array for pixel data
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                //create bitmap to be displayed
                this.colorBmp = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);

                //Point image in xaml to the bitmap above
                this.imgCanvas.Source = this.colorBmp;

                //add event handler for incoming frames
                this.sensor.ColorFrameReady += sensor_ColorFrameReady;

                //start the sensor
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusText.Text = "No Kinect Found!";
            }
        }

        //Event handler for colorframe
         void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
         {
             using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
             {
                 if (colorFrame != null)
                 {
                     //copy from frame to temp array
                     colorFrame.CopyPixelDataTo(this.colorPixels);

                     //write pixel data to bitmap
                     this.colorBmp.WritePixels(
                         new Int32Rect(0, 0, this.colorBmp.PixelWidth, this.colorBmp.PixelHeight), 
                         this.colorPixels, 
                         this.colorBmp.PixelWidth * sizeof(int), 
                         0);
                 }
             }
         }


         void StopKinect(KinectSensor sensor)
         {
             if (sensor != null)
             {
                 sensor.Stop();
                 sensor.AudioSource.Stop();
             }
         }

        //Shutdown sensor
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensor.Stop();
        }
    }
}
