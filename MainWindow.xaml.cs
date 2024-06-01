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

using System.Reflection;
using NAudio.Wave;

namespace MicLevelViewer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public event EventHandler<string> ImageSelect;
        private WaveInEvent waveIn;
        private MovingAverage movingAverage;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = Assembly.GetExecutingAssembly().GetName().Name;
            movingAverage = new MovingAverage(3);
        }

        // マイクの一覧を取得し、ComboBox に追加する
        public void PopulateMicrophoneComboBox()
        {
            List<string> microphoneList = new List<string>();

            for (var i = 0; i < WaveIn.DeviceCount; i++)
            {
                MicrophoneComboBox.Items.Add(WaveIn.GetCapabilities(i).ProductName);
            }

            if (microphoneList.Count > 0)
            {
                // マイクが存在する場合は、ComboBox に追加する
                foreach (string microphone in microphoneList)
                {
                    MicrophoneComboBox.Items.Add(microphone);
                }
            }
            // 最初のアイテムを選択状態にする
            MicrophoneComboBox.SelectedIndex = 0;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
                waveIn.DataAvailable -= OnDataAvailable;
                waveIn.Dispose();
            }

            waveIn = new WaveInEvent { DeviceNumber = MicrophoneComboBox.SelectedIndex };
            waveIn.DataAvailable += OnDataAvailable;
            // waveIn.BufferMilliseconds = 100; // OnDataAvailableが呼ばれる周期 100ms
            waveIn.StartRecording();
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            var max = 0f;
            for (var i = 0; i < e.BytesRecorded; i += 2)
            {
                var sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i + 0]);
                var sample32 = sample / 32768f;
                if (sample32 < 0) sample32 = -sample32;
                if (sample32 > max) max = sample32;
            }
            movingAverage.AddValue(max);
            double ave = movingAverage.GetAverage();

            Dispatcher.Invoke(() =>
            {
                // Console.WriteLine($"{ave:F3}"); // 出力: 0.123
                // プログレスバー更新
                textBlock_Vol.Text = (100 * ave).ToString("0.0");
                progressBar.Value = 100 * ave;

                // アイコン更新
                string iconName = "level_min.ico";
                if (ave > 0.80)
                {
                    iconName = "level_max.ico";
                }
                else if (ave >= 0.4)
                {
                    iconName = "level_8.ico";
                }
                else if (ave >= 0.1)
                {
                    iconName = "level_5.ico";
                }
                else if (ave >= 0.03)
                {
                    iconName = "level_3.ico";
                }
                else if (ave >  0.01)
                {
                    iconName = "level_1.ico";
                }
                OnImageSelect(iconName);
            });
        }
        protected virtual void OnImageSelect(string iconName)
        {
            ImageSelect?.Invoke(this, iconName);
        }
    }

    public class MovingAverage
    {
        private readonly Queue<double> _values;
        private readonly int _windowSize;
        private double _sum;

        public MovingAverage(int windowSize)
        {
            _windowSize = windowSize;
            _values = new Queue<double>(windowSize);
            _sum = 0;
        }

        public void AddValue(double val)
        {
            if (_values.Count == _windowSize)
            {
                _sum -= _values.Dequeue();
            }

            _values.Enqueue(val);
            _sum += val;
        }

        public double GetAverage()
        {
            if (_values.Count == 0)
            {
                return 0;
            }

            return _sum / _values.Count;
        }
    }
}
