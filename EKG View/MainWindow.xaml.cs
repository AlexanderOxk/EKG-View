using System;
using System.Collections.Generic;
using System.Windows;

using InteractiveDataDisplay.WPF;
using NAudio.Wave;
using System.Timers;

namespace EKG_View
{
    public partial class MainWindow : Window
    {
        private int maxDataPoints = 1024;
        private float t = 0;
        public Timer GraphDataTimer;
        WaveInEvent waveIn = new WaveInEvent();
        private LinkedList<float> data = new LinkedList<float>();
        private bool recording = false;
        private bool invert = false;

        public MainWindow()
        {
            InitializeComponent();

            GraphDataTimer = new Timer(50);
            GraphDataTimer.Start();
            waveIn.WaveFormat = new WaveFormat(8192, 16, 1);                                          
            GraphDataTimer.Elapsed += GraphDataTimer_Elapsed;
            waveIn.DataAvailable += OnDataAvailable;

            chart.LegendVisibility = Visibility.Hidden;

        }

        private void OnDataAvailable(object sender, WaveInEventArgs args)
        {
            lock (data)
            {
                for (int i = 0; i < args.BytesRecorded; i += 2)
                {
                    short sample = (short)((args.Buffer[i + 1] << 8) |
                                    args.Buffer[i + 0]);

                    float sample32 = sample / 32786f;

                    if (invert)
                    {
                        sample32 = -sample32;
                    }

                    data.AddLast(sample);
                }

                while (data.Count > maxDataPoints)
                {
                    data.RemoveFirst();
                }
            }
        }
        private void GraphDataTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            
            lock (data)
            {
                Dispatcher.Invoke(() =>
                {
                    linegraph.PlotY(data);
                });
            }
        }
        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            waveIn.StopRecording();
            recording = false;
        }
        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            if (!recording)
            {
                try
                {
                    waveIn.StartRecording();
                    linegraph.IsAutoFitEnabled = true;
                    recording = true;
                } 
                catch
                {
                    MessageBox.Show("No sensor detected. Make sure EKG is connected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void btn_clr_Click(object sender, RoutedEventArgs e)
        {
            lock (data)
            {
                while (data.Count != 0)
                {
                    data.RemoveFirst();
                }
            }

            t = 0;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GraphDataTimer.Stop();
            waveIn.StopRecording();

            System.Threading.Thread.Sleep(100);
        }
        private void cb_Invert_Checked(object sender, RoutedEventArgs e)
        {
            invert = (bool)cb_Invert.IsChecked;
        }
        private void slider_samples_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            maxDataPoints = (int)slider_samples.Value;
        }

        private void btn_input_device_Click(object sender, RoutedEventArgs e)
        {
            //waveIn.DeviceNumber = 1;
        }
    }
}
