﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GenshinNotifier.Net;

namespace GenshinNotifier {
    public partial class CookieDialog : Form {
        private string OldCookie;

        private static string COOKIE_GUIDE =
            @"使用说明：
1. 打开 http://bbs.mihoyo.com/ys/ 并进行登录操作；
2. 新建标签页，打开 http://user.mihoyo.com/ 并进行登录操作；
3. 按下键盘上的F12或右键检查,打开开发者工具,点击控制台；
4. 控制台输入 document.cookie 复制出现的字符串；
5. 将复制好的Cookie字符串粘贴到输入框，点击保存";

        public event EventHandler Handlers;

        public CookieDialog() {
            InitializeComponent();
            OldCookie = DataController.Default.Cookie;
            this.CookieTextBox.Text = OldCookie;
            this.CookieLabel.Text = COOKIE_GUIDE;
        }

        private void ShowToolTip(string message, int duration = 3000) {
            new ToolTip().Show(message, CookieTextBox, 240, 120, duration);
        }

        private void CookieDialog_Shown(object sender, EventArgs e) {
            Logger.Debug("CookieDialog_Shown");
        }

        private void NoButton_Click(object sender, EventArgs e) {
            Close();
        }

        private bool valiteCookieFields(string value) {
            var cookieDict = Utility.ParseCookieString(value);
            var validKeys = new string[] { "cookie_token", "account_id", "login_ticket"};
            return validKeys.All(it => cookieDict.ContainsKey(it));
        }

        private async void YesButton_Click(object sender, EventArgs e) {
            var tempCookie = CookieTextBox.Text?.Trim().Replace("\"", "").Replace("'", "");
            Logger.Info($"YesButton_Click");
            if (String.IsNullOrEmpty(tempCookie)) {
                ShowToolTip("Cookie不能为空");
                return;
            }
            if (!valiteCookieFields(tempCookie)) {
                ShowToolTip("Cookie无效：缺少必须字段");
                return;
            }
            if (tempCookie == OldCookie) {
                Close();
                return;
            }
            var user = await DataController.ValidateCookie(tempCookie);
            Logger.Info($"CookieValidateButton_Click {user?.GameUid}");
            if (user?.GameUid != null) {
                Handlers?.Invoke(this, new SimpleEventArgs(tempCookie));
                Close();
            } else {
                ShowToolTip("Cookie验证失败，请检查");
            }
        }

        private void CookieLabel_LinkClicked(object sender, LinkClickedEventArgs e) {
            Logger.Debug($"CookieLabel_LinkClicked {e.LinkText}");
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void CookieDialog_FormClosing(object sender, FormClosingEventArgs e) {
            foreach (EventHandler d in Handlers.GetInvocationList()) {
                Handlers -= d;
            }
        }
    }
}