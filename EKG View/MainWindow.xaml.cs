using System;
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

using InteractiveDataDisplay.WPF;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Timers;

namespace EKG_View
{
    public partial class MainWindow : Window
    {
        private const int maxDataPoints = 10000;
        private float t = 0;
        public Timer GraphDataTimer;
        WaveInEvent waveIn = new WaveInEvent();
        private Tuple<LinkedList<float>, LinkedList<float>> data = 
            new Tuple<LinkedList<float>, LinkedList<float>>(new LinkedList<float>(), new LinkedList<float>());
        private bool recording = false;

        public MainWindow()
        {
            InitializeComponent();

            GraphDataTimer = new Timer(50);
            GraphDataTimer.Start();
            GraphDataTimer.Elapsed += GraphDataTimer_Elapsed;
            waveIn.DataAvailable += OnDataAvailable;

            chart.LegendVisibility = Visibility.Hidden;

        }

        private void OnDataAvailable(object sender, WaveInEventArgs args)
        {
            lock(data)
            for(int i = 0; i < args.BytesRecorded; i += 2)
            {
                short sample = (short)((args.Buffer[i + 1] << 8) |
                                args.Buffer[i + 0]);

                float sample32 = sample / 32786f;
                data.Item1.AddLast(t); t += 1.0f / (float)waveIn.WaveFormat.AverageBytesPerSecond;
                data.Item2.AddLast(sample);
            }

            while(data.Item1.Count > maxDataPoints)
            {
                data.Item1.RemoveFirst();
                data.Item2.RemoveFirst();
            }
        }
        private void GraphDataTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                lock (data)
                linegraph.Plot(data.Item1, data.Item2);
            });
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
                waveIn.StartRecording();
                recording = true;
            }
        }
        private void btn_clr_Click(object sender, RoutedEventArgs e)
        {
            lock(data)
            while(data.Item1.Count != 0)
            {
                data.Item1.RemoveFirst();
                data.Item2.RemoveFirst();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GraphDataTimer.Stop();
            waveIn.StopRecording();
        }
    }
}
