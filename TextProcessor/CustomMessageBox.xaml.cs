﻿using System;
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
using System.Windows.Shapes;

namespace TextProcessor
{
    /// <summary>
    /// Логика взаимодействия для CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public object result = "Закрыть";

        public CustomMessageBox()
        {
            InitializeComponent();
        }

        public void SetupMsgBox(string content)
        {
            this.Title = "";
            frameIcon.Visibility = Visibility.Hidden;
            textTitle.Visibility = Visibility.Hidden;
            bSecond.Visibility = Visibility.Hidden;
            bThird.Visibility = Visibility.Hidden;
            scrviewerDescription.Margin = new Thickness(10, 10, 10, 45);
            textDescription.Text = content;
        }
        public void SetupMsgBox(string content, string title)
        {
            this.Title = title + " - Текстовый процессор";
            frameIcon.Visibility = Visibility.Hidden;
            bSecond.Visibility = Visibility.Hidden;
            bThird.Visibility = Visibility.Hidden;
            textTitle.Margin = new Thickness(10, 10, 10, 0);
            textDescription.Text = content;
            textTitle.Text = title;
        }
        public void SetupMsgBox(string content, string title, object icon)
        {
            this.Title = title + " - Текстовый процессор";
            bSecond.Visibility = Visibility.Hidden;
            bThird.Visibility = Visibility.Hidden;
            textDescription.Text = content;
            textTitle.Text = title;
            frameIcon.Content = icon;
        }
        public void SetupMsgBox(string content, string title, string firstButtonText, string secondButtonText)
        {
            this.Title = title + " - Текстовый процессор";
            bThird.Visibility = Visibility.Hidden;
            frameIcon.Visibility = Visibility.Hidden;
            textTitle.Margin = new Thickness(10, 10, 10, 0);
            textDescription.Text = content;
            textTitle.Text = title;
            bFirst.Content = firstButtonText;
            bSecond.Content = secondButtonText;
        }
        public void SetupMsgBox(string content, string title, string firstButtonText, string secondButtonText, string thirdButtonText)
        {
            this.Title = title + " - Текстовый процессор";
            frameIcon.Visibility = Visibility.Hidden;
            textTitle.Margin = new Thickness(10, 10, 10, 0);
            textDescription.Text = content;
            textTitle.Text = title;
            bFirst.Content = firstButtonText;
            bSecond.Content = secondButtonText;
            bThird.Content = thirdButtonText;
        }
        public void SetupMsgBox(string content, string title, object icon, string firstButtonText, string secondButtonText)
        {
            this.Title = title + " - Текстовый процессор";
            bThird.Visibility = Visibility.Hidden;
            frameIcon.Content = icon;
            textDescription.Text = content;
            textTitle.Text = title;
            bFirst.Content = firstButtonText;
            bSecond.Content = secondButtonText;
        }
        public void SetupMsgBox(string content, string title, object icon, string firstButtonText, string secondButtonText, string thirdButtonText)
        {
            this.Title = title + " - Текстовый процессор";
            frameIcon.Content = icon;
            textDescription.Text = content;
            textTitle.Text = title;
            bFirst.Content = firstButtonText;
            bSecond.Content = secondButtonText;
            bThird.Content = thirdButtonText;
        }

        private void bFirst_Click(object sender, RoutedEventArgs e)
        {
            result = bFirst.Content;
            this.Close();
        }

        private void bSecond_Click(object sender, RoutedEventArgs e)
        {
            result = bSecond.Content;
            this.Close();
        }

        private void bThird_Click(object sender, RoutedEventArgs e)
        {
            result = bThird.Content;
            this.Close();
        }
    }
}
