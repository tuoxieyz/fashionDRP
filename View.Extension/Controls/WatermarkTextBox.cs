using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace View.Extension
{
    /// <summary>
    /// 水印文本框
    /// </summary>
    public class WatermarkTextBox : TextBox
    {        
        private string _watermarkText;

        private Brush _watermarkColor = Brushes.Gray;

        private Brush _foreground;

        /// <summary>
        /// 水印文字
        /// </summary>
        public string WatermarkText
        {
            private get
            {
                return _watermarkText;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("WatermarkText", "水印文字不能为空");
                _watermarkText = value;
            }
        }

        public Brush WatermarkColor
        {
            get { return _watermarkColor; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("WatermarkColor", "水印颜色不能为空");
                _watermarkColor = value;
            }
        }

        public WatermarkTextBox(string watermarkText)
        {
            if (string.IsNullOrEmpty(watermarkText))
                throw new ArgumentNullException("WatermarkText", "水印文字不能为空");

            _watermarkText = watermarkText;
            _foreground = this.Foreground;
            this.LostFocus += new System.Windows.RoutedEventHandler(WatermarkTextBox_LostFocus);
            this.GotFocus += new System.Windows.RoutedEventHandler(WatermarkTextBox_GotFocus);
            this.TextChanged += new TextChangedEventHandler(WatermarkTextBox_TextChanged);
            this.Loaded += delegate { this.Foreground = _watermarkColor; this.Text = WatermarkText; };
        }

        void WatermarkTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.Text == WatermarkText)
                this.Clear();
        }

        void WatermarkTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.Text.Trim()))
            {
                this.Text = WatermarkText;
                this.Foreground = _watermarkColor;
            }
        }

        void WatermarkTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.Text == WatermarkText)
                e.Handled = true;//路由事件可阻止后续处理器处理
            else
                this.Foreground = _foreground;
        }
    }
}
