﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Carrot.UI.Controls.Font;
using Newtonsoft.Json;
using NotifierWidget.Properties;
using Carrot.UI.Controls.Utils;
using System.Collections.Generic;
using System.Linq;
using Carrot.UI.Controls.Picker;
using PropertyChanged;

namespace NotifierWidget {

    public class WidgetStyle : INotifyPropertyChanged {
        public static WidgetStyle User { get; private set; }
        public static WidgetStyle ResDefault { get; private set; }

        public static WidgetStyle FromResource(ResourceDictionary res) {
            var rTextNormalColor = (Color)res["TextNormalColor"];
            var rTextHighlightColor = (Color)res["TextHighlightColor"];
            var rTextErrorColor = (Color)res["TextErrorColor"];
            var rBackgroundColor = (Color)res["BackgroundColor"];
            var rTextFontSize = (double)res["TextFontSize"];
            var rFontFamily = (FontFamily)res["TextFontFamily"];
            var rFontWeight = (FontWeight)res["TextFontWeight"];
            var rFontStyle = (FontStyle)res["TextFontStyle"];
            return new WidgetStyle(rTextNormalColor, rTextHighlightColor, rTextErrorColor, rBackgroundColor, rTextFontSize, rFontFamily, rFontWeight, rFontStyle);
        }

        public static Color ERROR_COLOR = UIHelper.ParseColor("#00000000");

        private static ColorComboBoxItem CreateColorPair(string s, Color c) {
            return ColorComboBoxItem.Create(s, c);
        }

        public WidgetStyle() {
        }

        public WidgetStyle(Color textNormalColor,
            Color textHighlightColor,
            Color textErrorColor,
            Color backgroundColor,
            double textFontSize,
            FontFamily fontFamily,
            FontWeight fontWeight,
            FontStyle fontStyle) {
            TextNormalColor = textNormalColor;
            TextHighlightColor = textHighlightColor;
            TextErrorColor = textErrorColor;
            BackgroundColor = backgroundColor;
            TextFontSize = textFontSize;
            TextFontFamily = fontFamily;
            TextFontWeight = fontWeight;
            TextFontStyle = fontStyle;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region normal properties

        public Color TextNormalColor { get; set; }
        public Color TextHighlightColor { get; set; }
        public Color TextErrorColor { get; set; }
        public Color BackgroundColor { get; set; }
        public bool BackgroundTransparent { get; set; }
        public double TextFontSize { get; set; } = double.NaN;
        public FontFamily TextFontFamily { get; set; }
        public FontWeight? TextFontWeight { get; set; }
        public FontStyle? TextFontStyle { get; set; }

        #endregion

        #region calculated properties
        [JsonIgnore]
        [DoNotNotify]
        public List<ColorComboBoxItem> AppendBgColors { get; private set; }
        [JsonIgnore]
        [DoNotNotify]
        public List<ColorComboBoxItem> AppendTextNColors { get; private set; }
        [JsonIgnore]
        [DoNotNotify]
        public List<ColorComboBoxItem> AppendTextHColors { get; private set; }
        [JsonIgnore]
        [DoNotNotify]
        public List<double> FontSizeRange => Enumerable.Range(12, 9).Select(it => Convert.ToDouble(it)).ToList();


        [JsonIgnore]
        public Color BackgroundColorOpposite => BackgroundColor.IsDark() ? Colors.White : Colors.Black;

        [JsonIgnore]
        public FontExtraInfo TextFontExtraInfo => FontUtilities.GetLocalizedFontFamily(TextFontFamily);

        [JsonIgnore]
        public bool TextFontBold {
            get => TextFontWeight == FontWeights.Bold;
            set => TextFontWeight = value ? FontWeights.Bold : FontWeights.Normal;
        }
        [JsonIgnore]
        public bool TextFontItalic {
            get => TextFontStyle == FontStyles.Italic;
            set => TextFontStyle = value ? FontStyles.Italic : FontStyles.Normal;
        }

        [JsonIgnore]
        public double HeaderFontSize => MiscUtils.Clamp(TextFontSize + 3, ResDefault.TextFontSize, 24);

        [JsonIgnore]
        public double FooterFontSize => MiscUtils.Clamp(TextFontSize - 3, 12, ResDefault.TextFontSize);

        #endregion

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }

        private static bool IsValidFontSize(double fontSize) {
            return !double.IsNaN(fontSize) && fontSize >= 8 && fontSize <= 24;
        }

        private static WidgetStyle LoadUserStyle() {
            try {
                var styleJson = Settings.Default.WidgetStyle;
                if (string.IsNullOrWhiteSpace(styleJson)) {
                    return new WidgetStyle();
                }
                return JsonConvert.DeserializeObject<WidgetStyle>(styleJson);
            } catch (Exception ex) {
                Debug.WriteLine("LoadUserStyle failed");
                Debug.WriteLine(ex.StackTrace);
                Settings.Default.WidgetStyle = null;
                Settings.Default.Save();
                return new WidgetStyle();
            }
        }

        public static bool SaveUserStyle() {
            Debug.WriteLine("SaveUserStyle");
            try {
                var styleJson = JsonConvert.SerializeObject(WidgetStyle.User);
                if (string.IsNullOrWhiteSpace(styleJson)) {
                    return false;
                }
                Settings.Default.WidgetStyle = styleJson;
                Settings.Default.Save();
                return true;
            } catch (Exception ex) {
                Debug.WriteLine("SaveUserStyle failed");
                Debug.WriteLine(ex.StackTrace);
                return false;
            }
        }

        public static void ResetUserStyle() {
            Debug.WriteLine("ResetUserStyle");
            var res = Application.Current.Resources;
            var rs = WidgetStyle.FromResource(res);
            var us = WidgetStyle.User;
            us.TextNormalColor = rs.TextNormalColor;
            us.TextHighlightColor = rs.TextHighlightColor;
            us.TextErrorColor = rs.TextErrorColor;
            us.BackgroundColor = rs.BackgroundColor;
            us.BackgroundTransparent = rs.BackgroundColor.A == 0;
            us.TextFontSize = rs.TextFontSize;
            us.TextFontFamily = rs.TextFontFamily;
            us.TextFontWeight = rs.TextFontWeight;
            us.TextFontStyle = rs.TextFontStyle;
            Settings.Default.WidgetStyle = null;
            Settings.Default.Save();
        }

        // must call on app start
        public static void Initialize() {
            // read user setting values
            var settingStyle = LoadUserStyle();
            Debug.WriteLine("WidgetStyle settings=" + settingStyle);

            // read xaml resource values
            var res = Application.Current.Resources;
            //UI.PrintResources(res);
            var resourceStyle = WidgetStyle.FromResource(res);


            var userStyle = new WidgetStyle();
            userStyle.TextNormalColor = settingStyle.TextNormalColor == ERROR_COLOR ? resourceStyle.TextNormalColor : settingStyle.TextNormalColor;
            userStyle.TextHighlightColor = settingStyle.TextHighlightColor == ERROR_COLOR ? resourceStyle.TextHighlightColor : settingStyle.TextHighlightColor;
            userStyle.TextErrorColor = settingStyle.TextErrorColor == ERROR_COLOR ? resourceStyle.TextErrorColor : settingStyle.TextErrorColor;
            userStyle.BackgroundColor = settingStyle.BackgroundColor == ERROR_COLOR ? resourceStyle.BackgroundColor : settingStyle.BackgroundColor;
            userStyle.BackgroundTransparent = settingStyle.BackgroundTransparent;
            userStyle.TextFontSize = IsValidFontSize(settingStyle.TextFontSize) ? settingStyle.TextFontSize : resourceStyle.TextFontSize;
            userStyle.TextFontFamily = settingStyle.TextFontFamily ?? resourceStyle.TextFontFamily;
            userStyle.TextFontWeight = settingStyle.TextFontWeight ?? resourceStyle.TextFontWeight;
            userStyle.TextFontStyle = settingStyle.TextFontStyle ?? resourceStyle.TextFontStyle;

            userStyle.AppendBgColors = new List<ColorComboBoxItem>() {
            CreateColorPair("当前", userStyle.BackgroundColor),
            CreateColorPair("默认", resourceStyle.BackgroundColor)
            };

            userStyle.AppendTextNColors = new List<ColorComboBoxItem>() {
            CreateColorPair("当前", userStyle.TextNormalColor),
            CreateColorPair("默认", resourceStyle.TextNormalColor)
        };

            userStyle.AppendTextHColors = new List<ColorComboBoxItem>() {
            CreateColorPair("当前", userStyle.TextHighlightColor),
            CreateColorPair("默认", resourceStyle.TextHighlightColor)
        };

            WidgetStyle.User = userStyle;
            WidgetStyle.ResDefault = resourceStyle;
            Debug.WriteLine("WidgetStyle res=" + ResDefault);
            Debug.WriteLine("WidgetStyle user=" + User);

            //Settings.Default.WidgetStyle = User.ToString();
            //Settings.Default.Save();

        }
    }
}