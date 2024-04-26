using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Reflection;
using GenshinNotifier;
using Carrot.Common;


namespace Carrot.AutoLock {
    public partial class MainForm : Form {

        private static readonly string NAME = "CarrotLock";
        private static readonly string RE_IP = @"^\d+\.\d+\.\d+\.\d+$";

        private readonly NotifyIcon notifyIcon;
        private readonly ContextMenuStrip contextMenuStrip;

        private string deviceIP = "";

        private readonly ActiveChecker mChecker;


        public MainForm() {
            InitializeComponent();
            mChecker = new ActiveChecker();
            // ��ʼ��NotifyIcon
            notifyIcon = new NotifyIcon {
                Icon = Properties.Resources.carrot_512,
                Text = NAME
            };

            // ��ʼ��ContextMenuStrip
            contextMenuStrip = new ContextMenuStrip();
            // ��Ӳ˵��� - ��ʾ����
            ToolStripMenuItem showMenuItem = new("��ʾ����", null, ShowWindowMenuItem_Click);
            contextMenuStrip.Items.Add(showMenuItem);
            // ��ӷָ���
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            // ��Ӳ˵��� - �˳�Ӧ��
            ToolStripMenuItem exitMenuItem = new("�˳�Ӧ��", null, ExitMenuItem_Click);
            contextMenuStrip.Items.Add(exitMenuItem);

            // ΪNotifyIcon��ContextMenuStrip
            notifyIcon.ContextMenuStrip = contextMenuStrip;

            notifyIcon.Click += NotifyIcon_Click;
        }

        private void MainForm_Load(object sender, EventArgs e) {
            Console.WriteLine("MainForm_Load");
            textIPAddress.Text = ActiveChecker.DEFAULT_IP;
            deviceIP = ActiveChecker.DEFAULT_IP;
            UpdateUI();
        }

        private void MainForm_Resize(object sender, EventArgs e) {
            Console.WriteLine($"MainForm_Resize ${this.WindowState}");
            // �жϴ����Ƿ���С��
            if (this.WindowState == FormWindowState.Minimized) {
                // ���ش���
                this.Hide();
                // ��ʾ״̬��ͼ��
                notifyIcon.Visible = true;
                // ��ʾ״̬����ʾ
                //notifyIcon.ShowBalloonTip(1000, NAME, "��С����״̬��", ToolTipIcon.Info);
            } else {
                ShowWindow();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            Console.WriteLine("MainForm_FormClosing Reason:" + e.CloseReason);
            // You can use the CloseReason property to find out why the event is called.
            // When the user clicks on the close button on the window,
            // e.CloseReason is UserClosing,
            // otherwise it is ApplicationExitCall
            if (e.CloseReason == CloseReason.UserClosing) {
                // ȡ���رն���
                e.Cancel = true;
                // ���ش���
                this.WindowState = FormWindowState.Minimized;
                // ��ʾ״̬��ͼ��
                notifyIcon.Visible = true;
                // ��ʾ״̬����ʾ
                // notifyIcon.ShowBalloonTip(1000, NAME, "��С����״̬��", ToolTipIcon.Info);
            }

        }


        private void NotifyIcon_Click(object sender, EventArgs e) {
            // �����갴ť��״̬
            if (((MouseEventArgs)e).Button == MouseButtons.Left) {
                ShowWindow();
            } else if (((MouseEventArgs)e).Button == MouseButtons.Right) {
                // ��ʾContextMenu
                contextMenuStrip.Show(Cursor.Position);
            }
        }

        private void ShowWindowMenuItem_Click(object sender, EventArgs e) {
            ShowWindow();
        }

        private void ExitMenuItem_Click(object sender, EventArgs e) {
            Console.WriteLine("ExitMenuItem_Click");
            // �˳�Ӧ��
            mChecker.Stop();
            Application.Exit();
        }

        private void BtnExit_Click(object sender, EventArgs e) {
            Console.WriteLine("BtnExit_Click");
            mChecker.Stop();
            mChecker.callback = null;
            Application.Exit();

        }

        private void BtnStart_Click(object sender, EventArgs e) {
            Console.WriteLine("BtnStart_Click");
            if (mChecker.IsRunning()) {
                mChecker.Stop();
                mChecker.callback = null;
            } else {
                if (!Regex.IsMatch(deviceIP, RE_IP) || !deviceIP.StartsWith("192.168.")) {
                    MessageBox.Show("IP��ַ��ʽ����ȷ");
                    return;
                }
                mChecker.callback = OnStatusChanged;
                mChecker.Start();
            }
        }

        public void OnStatusChanged(string result) {
            Logger.Info("OnStatusChanged");
            if (InvokeRequired) {
                Invoke(new MethodInvoker(UpdateUI));
            } else {
                UpdateUI();
            }
        }

        private void UpdateUI() {
            var running = mChecker.IsRunning();
            textIPAddress.Enabled = !running;
            btnStart.Text = running ? "STOP" : "START";
            var textLines = new List<string> {
                running ? "Status:    Running" : "Status:    Stopped",
            };
            if (running) {
                textLines.Add(mChecker.IsDeviceOnline() ? "Device:    Online" : "Device:    Offline");
                textLines.Add($"InActive:    {(int)mChecker.GetInactiveSeconds()}s");
            }
            InfoText.Text = String.Join(Environment.NewLine, textLines);

        }

        private void ShowWindow() {
            // ��ʾ����
            notifyIcon.Visible = false;
            this.Show();
            this.Activate();
            UpdateUI();
        }

        private void CbAutoStart_CheckedChanged(object sender, EventArgs e) {
            var cb = sender as CheckBox;
            Console.WriteLine("CbAutoStart_CheckedChanged " + cb!.Checked);
            ShortcutHelper.EnableAutoStart(cb!.Checked);
        }

        private void TextIPAddress_TextChanged(object sender, EventArgs e) {
            var textBox = sender as TextBox;
            deviceIP = textBox!.Text;
            Console.WriteLine("TextIPAddress_TextChanged " + deviceIP);
        }

    }
}
