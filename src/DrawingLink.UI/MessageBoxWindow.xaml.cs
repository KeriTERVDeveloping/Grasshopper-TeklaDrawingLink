﻿using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DrawingLink.UI
{
    /// <summary>
    /// Interaction logic for MessageBoxWindow.xaml
    /// </summary>
    public partial class MessageBoxWindow : Window
    {
        public MessageBoxWindow()
        {
            InitializeComponent();
        }

        public void ClearMessages()
        {
            this.textBox.Text = "";
        }

        public void ShowMessages(string title, Dictionary<GH_RuntimeMessageLevel, List<string>> messageDictionary)
        {
            var messages = new List<string>();
            foreach (var item in messageDictionary)
            {
                var level = item.Key;
                foreach (var subMessage in item.Value)
                    messages.Add($"{level}: {subMessage}");
            }
            ShowMessages(title, messages);
        }

        public void ShowMessages(string title, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            ShowMessages(title, new string[] { message });
        }

        public void ShowMessages(string title, IEnumerable<string> messages)
        {
            this.Title = title;
            if (!messages.Any())
                return;

            this.textBox.Text = JoinMessages(messages);

            this.Hide();
            this.Show();
        }

        private string JoinMessages(IEnumerable<string> messages)
        {
            var trimmed = messages.Select(m => m.Trim(new char[]
                {
                    '\n',
                    '\r',
                    '\t',
                    ' '
                }).Replace("\r\n", "\n").Replace("\n", Environment.NewLine));

            return string.Join(Environment.NewLine, trimmed);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);

            this.Left = screen.WorkingArea.Left;
            this.Top = screen.WorkingArea.Bottom - this.Height - 30;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}
