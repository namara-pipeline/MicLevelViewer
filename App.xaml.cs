using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Drawing;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace MicLevelViewer
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private System.Windows.Forms.ContextMenuStrip _menu;
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private MainWindow _win = null;  // 多重起動抑止用

        // 常駐開始処理
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var icon = GetResourceStream(new Uri("images/level_max.ico", UriKind.Relative)).Stream;
            _menu = CreateMenu();

            // 通知領域にアイコン
            _notifyIcon = new System.Windows.Forms.NotifyIcon {
                Visible = true,
                Icon = new System.Drawing.Icon(icon),
                Text = Assembly.GetExecutingAssembly().GetName().Name,
                ContextMenuStrip = _menu
            };
            // アイコン左クリックでMainWindowを表示
            _notifyIcon.MouseClick += (sender, evt) => {
                if (evt.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    ShowMainWindow();
                }
            };
            ShowMainWindow();

            // 別タスクで監視処理を実行
            Task.Run(() => {


            });
        }

        // 常駐終了処理
        protected override void OnExit(ExitEventArgs e)
        {
            _menu.Dispose();
            _notifyIcon.Dispose();

            base.OnExit(e);
        }

        // MainWindowを表示
        private void ShowMainWindow()
        {
            if (_win == null)
            {
                _win = new MainWindow();
                _win.ImageSelect += MainWindow_ImageSelect;
                // マイクの一覧を取得し、MainWindow の ComboBox に追加する
                _win.PopulateMicrophoneComboBox();

                // 画面中央に表示
                _win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                // _win.Show();

                // 閉じるボタンの処理
                _win.Closing += (sender, e) => {
                    // 閉じないで非表示にする
                    _win.Hide();
                    e.Cancel = true;
                };
            }
            else
            {
                // 非表示から表示に変更
                _win.Show();
            }
        }

        private void MainWindow_ImageSelect(object sender, string iconName)
        {
            // iconName に基づいてアイコンを設定する
            string uriString = "images/" + iconName;
            // システムトレイのアイコン変更
            var iconSystemTray = GetResourceStream(new Uri(uriString, UriKind.Relative)).Stream;
            _notifyIcon.Icon = new System.Drawing.Icon(iconSystemTray);
            // タスクバーのアイコン変更
            var iconTaskBar = GetResourceStream(new Uri(uriString, UriKind.Relative)).Stream;
            _win.Icon = BitmapFrame.Create(iconTaskBar);
        }

        // コンテキストメニュー
        private ContextMenuStrip CreateMenu()
        {
            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add("Setting", null, (sender, e) => { ShowMainWindow(); });
            menu.Items.Add("Exit", null, (sender, e) => { Shutdown(); });
            return menu;
        }
    }
}
