﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using Path = System.IO.Path;
using System.IO.Compression;
using TextProcessor.Classes;

namespace TextProcessor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool unsavedChanges = false;
        public bool DetectUrls = true;
        private string[] arguments;
        public string currentlyOpenPath;
        string errorPreset = "\nТекстовый процессор пытается продолжить работу.";
        bool canDrop = true;

        private Storyboard animationStoryboard;
        CustomMessageBox messageBox = new CustomMessageBox();
        Image insertImage;
        BlockUIContainer insertImageUIContainer;
        public SaveFileDialog sfdSave = new SaveFileDialog();
        public OpenFileDialog ofdOpen = new OpenFileDialog();
        private OpenFileDialog ofdImage = new OpenFileDialog();

        bool findReplaceChanges = true;
        FindAndReplaceManager findAndReplace = new FindAndReplaceManager(new FlowDocument());

        private List<Classes.Page> pages;
        private int currentPageIndex;

        private List<CustomStyle> customStyles;
        private CustomStyle currentStyle;

        public MainWindow()
        {
            InitializeComponent();
            applySettings();
            
            arguments = getArguments();
            if (arguments.Length > 1)
            {
                if (File.Exists(arguments[1]))
                {
                    try
                    {
                        openDocument(arguments[1], rtbMain);
                        currentlyOpenPath = arguments[1];
                    }
                    catch (Exception ex)
                    {
                        messageBox = new CustomMessageBox();
                        messageBox.SetupMsgBox(ex.Message + errorPreset, "Ошибка!", this.FindResource("iconError"));
                        messageBox.ShowDialog();
                    }
                }
            }

            sfdSave.Filter = "XML документ (*.xml)|*.xml|RTF документ (*.rtf)|*.rtf|Текстовый документ (*.txt)|*.txt|Все файлы (*.*)|*.*";
            sfdSave.Title = "Сохранить документ | Текстовый процессор";
            ofdOpen.Filter = "Доступные типы (*.xml, *.rtf, *.txt)|*.xml;*.rtf;*.txt|XML документ (*.xml)|*.xml|RTF документ (*.rtf)|*.rtf|Текстовый документ (*.txt)|*.txt|Все файлы (*.*)|*.*";
            ofdOpen.Title = "Открыть документ | Текстовый процессор";
            ofdImage.Filter = "Доступные форматы изображения (*.png, *.jpg, *.jpeg, *.bmp)|*.png;*.jpg;*.jpeg;*.bmp";
            ofdImage.Title = "Открыть изображение | Текстовый процессор";

            System.Windows.Media.FontFamily fontFamily = new FontFamily();
            foreach (System.Drawing.FontFamily font in System.Drawing.FontFamily.Families)
            {
                cbFont.Items.Add(new ComboBoxItem() { Content = font.Name, FontFamily = new FontFamily(font.Name) });
                fontFamily = new FontFamily(font.Name);
            }

            cbFontSize.ItemsSource = new List<Double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72, 102, 144, 288 };
            cbLineSpacing.ItemsSource = new List<Double>() { 1.0, 1.15, 1.5, 2.0, 2.5, 3.0, 4.0, 5.0, 8.0, 11.0, 16.0};

            rtbMain.AddHandler(RichTextBox.DragOverEvent, new DragEventHandler(rtbMain_DragOver), true);
            rtbMain.AddHandler(RichTextBox.DropEvent, new DragEventHandler(rtbMain_Drop), true);
            rtbMain.AddHandler(RichTextBox.DragEnterEvent, new DragEventHandler(rtbMain_DragEnter), true);
            rtbMain.AddHandler(RichTextBox.DragLeaveEvent, new DragEventHandler(rtbMain_DragLeave), true);

            pages = new List<Classes.Page>();
            currentPageIndex = 0;

            AddNewPage();
            UpdateRichTextBoxContent();

            customStyles = new List<CustomStyle>();
            customStyles.Add(new CustomStyle
            {
                StyleName = "Стандартный",
                BoldStyle = false,
                ItalicStyle = false,
                UnderlineStyle = false,
                AlignLeft = true,
                AlignCenter = false,
                AlignRight = false,
                AlignJustify = false,
                LineHeightProperty = 16.0,
                FontSize = 14,
                FontFamily = fontFamily
            });
            lstStyles.ItemsSource = customStyles;

            currentStyle = customStyles[0];

            ApplyStyle();
        }

        private void AddNewPage()
        {
            var currentPageContent = new TextRange(rtbMain.Document.ContentStart, rtbMain.Document.ContentEnd).Text;
            pages.Add(new Classes.Page
            {
                Content = currentPageContent,
                Number = pages.Count + 1
            });

            rtbMain.Document.Blocks.Clear();
        }

        private void UpdateRichTextBoxContent()
        {
            if (currentPageIndex < pages.Count)
            {
                var page = pages[currentPageIndex];
                new TextRange(rtbMain.Document.ContentStart, rtbMain.Document.ContentEnd).Text = page.Content;
                txtPageNumber.Text = $"{pages[currentPageIndex].Number}";
            }
        }

        private void rtbMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                AddNewPage();
                UpdateRichTextBoxContent();
            }
        }

        private void ApplyStyle()
        {
            if (currentStyle.BoldStyle)
                rtbMain.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Bold);
            if (currentStyle.ItalicStyle)
                rtbMain.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Italic);
            if (currentStyle.UnderlineStyle)
                rtbMain.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            if (currentStyle.AlignLeft)
                rtbMain.Selection.Start.Paragraph.TextAlignment = TextAlignment.Left;
            if (currentStyle.AlignCenter)
                rtbMain.Selection.Start.Paragraph.TextAlignment = TextAlignment.Center;
            if (currentStyle.AlignRight)
                rtbMain.Selection.Start.Paragraph.TextAlignment = TextAlignment.Right;
            if (currentStyle.AlignJustify)
                rtbMain.Selection.Start.Paragraph.TextAlignment = TextAlignment.Justify;
            rtbMain.Selection.ApplyPropertyValue(Paragraph.LineHeightProperty, Convert.ToString(currentStyle.LineHeightProperty));
            rtbMain.Selection.ApplyPropertyValue(Inline.FontSizeProperty, Convert.ToString(currentStyle.FontSize));
            rtbMain.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, currentStyle.FontFamily);
        }

        private void btnApplyStyle_Click(object sender, RoutedEventArgs e)
        {
            if (lstStyles.SelectedItem is CustomStyle selectedStyle)
            {
                currentStyle = selectedStyle;
                ApplyStyle();
            }
        }

        private void btnSaveCustomStyle_Click(object sender, RoutedEventArgs e)
        {
            var newStyle = new CustomStyle
            {
                StyleName = "Новый стиль" + (customStyles.Count + 1),
                BoldStyle = miBold.IsChecked,
                ItalicStyle = miItalic.IsChecked,
                UnderlineStyle = miUnderline.IsChecked,
                AlignLeft = miAlignLeft.IsChecked,
                AlignCenter = miAlignCenter.IsChecked,
                AlignRight = miAlignRight.IsChecked,
                AlignJustify = miAlignJustify.IsChecked,
                LineHeightProperty = Convert.ToDouble(cbLineSpacing.SelectedItem),
                FontSize = Convert.ToInt16(cbFontSize.SelectedItem),
                FontFamily = ((ComboBoxItem)cbFont.SelectedItem).FontFamily
            };

            customStyles.Add(newStyle);
            lstStyles.ItemsSource = null;
            lstStyles.ItemsSource = customStyles;
        }

        private void btnAddNewStyle_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtNewStyleName.Text))
            {
                var newStyle = new CustomStyle
                {
                    StyleName = txtNewStyleName.Text,
                    BoldStyle = miBold.IsChecked,
                    ItalicStyle = miItalic.IsChecked,
                    UnderlineStyle = miUnderline.IsChecked,
                    AlignLeft = miAlignLeft.IsChecked,
                    AlignCenter = miAlignCenter.IsChecked,
                    AlignRight = miAlignRight.IsChecked,
                    AlignJustify = miAlignJustify.IsChecked,
                    LineHeightProperty = Convert.ToDouble(cbLineSpacing.SelectedItem),
                    FontSize = Convert.ToInt16(cbFontSize.SelectedItem),
                    FontFamily = ((ComboBoxItem)cbFont.SelectedItem).FontFamily
                };

                customStyles.Add(newStyle);
                lstStyles.ItemsSource = null;
                lstStyles.ItemsSource = customStyles;
            }
        }

        private void UpdatePageNumber()
        {
            double pageSize = 11 * 96;

            int pageNumber = (int)Math.Ceiling(rtbMain.ExtentHeight / pageSize);

            txtPageNumber.Text = $"{pageNumber}";
        }

        private void MainWindow_Close(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (unsavedChanges)
            {
                Exit exit = new Exit();
                exit.mWRichTextBox = XamlReader.Parse(XamlWriter.Save(rtbMain)) as RichTextBox;
                exit.mWCurrentlyOpenFile = currentlyOpenPath;
                if (exit.ShowDialog() == true)
                {
                    e.Cancel = true;
                    return;
                }
                Application.Current.Shutdown();
            }
            else
                Application.Current.Shutdown();
        }

        private void applySettings()
        {
            RoutedCommand commandNew = new RoutedCommand();
            commandNew.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(commandNew, anyNew_Click));
            RoutedCommand commandOpen = new RoutedCommand();
            commandOpen.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(commandOpen, anyOpen_Click));
            RoutedCommand commandSave = new RoutedCommand();
            commandSave.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(commandSave, anySave_Click));
            RoutedCommand commandSaveAs = new RoutedCommand();
            commandSaveAs.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control ^ ModifierKeys.Shift));
            CommandBindings.Add(new CommandBinding(commandSaveAs, anySaveAs_Click));
        }

        private string[] getArguments()
        {
            string[] arguments = null;
            arguments = Environment.GetCommandLineArgs();
            if (arguments != null)
                return arguments;
            else
                return null;
        }

        public bool openDocument(string filePath, RichTextBox rtbLoad, Boolean append = false)
        {
            string fileExtension = System.IO.Path.GetExtension(filePath);

            if (!append)
                rtbLoad.SelectAll();
            switch (fileExtension)
            {
                case ".rtf":
                    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                        using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                        {
                            rtbLoad.Selection.Load(gZipStream, DataFormats.Rtf);
                        }
                    break;
                case ".txt":
                    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                        using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                        {
                            rtbLoad.Selection.Load(gZipStream, DataFormats.Text);
                        }
                    break;
                case ".xml":
                    XmlReader xmlReader = XmlReader.Create(filePath);
                    XamlReader xamlReader = new XamlReader();
                    rtbLoad.Document = new FlowDocument();
                    rtbLoad.Document = (FlowDocument)(XamlReader.Load(xmlReader));
                    break;
                default:
                    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                        using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                        {
                            rtbLoad.Selection.Load(gZipStream, DataFormats.Text);
                        }
                    break;
            }
            unsavedChanges = false;
            
            return true;
        }

        public bool saveDocument(string filePath, RichTextBox rtbSave)
        {
            string fileExtension = System.IO.Path.GetExtension(filePath);
            FileStream stream;
            rtbSave.SelectAll();

            switch (fileExtension)
            {
                case ".rtf":
                    using (stream = new FileStream(filePath, FileMode.Create))
                        using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress))
                        {
                            rtbSave.Selection.Save(gZipStream, DataFormats.Rtf);
                        }
                    break;
                case ".txt":
                    using (stream = new FileStream(filePath, FileMode.Create))
                        using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress))
                        {
                            rtbSave.Selection.Save(gZipStream, DataFormats.Text);
                        }
                    break;
                case ".xml":
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(XamlWriter.Save(rtbSave.Document));
                    xdoc.Save(filePath);
                    break;
                default:
                    using (stream = new FileStream(filePath, FileMode.Create))
                        using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress))
                        {
                            rtbSave.Selection.Save(gZipStream, DataFormats.Text);
                        }
                    break;
            }
            unsavedChanges = false;
            currentlyOpenPath = filePath;

            return true;
        }

        private void rtbMain_TextChanged(object sender, TextChangedEventArgs e)
        {
            unsavedChanges = true;
            UpdatePageNumber();
        }

        private void rtbMain_SelectionChanged(object sender, RoutedEventArgs e)
        {
            object temp = rtbMain.Selection.GetPropertyValue(Inline.FontWeightProperty);
            if (temp != null)
                miBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
            temp = rtbMain.Selection.GetPropertyValue(Inline.FontStyleProperty);
            if (temp != null)
                miItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
            temp = rtbMain.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            if (temp != null)
                miUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));
            var ttemp = rtbMain.Selection.Start.Parent;

            checkForAlignment();

            temp = rtbMain.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            cbFont.Text = temp.ToString();
            temp = rtbMain.Selection.GetPropertyValue(Inline.FontSizeProperty);
            cbFontSize.Text = temp.ToString();
        }

        private void rtbMain_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = false;
        }

        private void rtbMain_Drop(object sender, DragEventArgs e)
        {
            if (!canDrop)
            {
                canDrop = true;
                return;
            }
            canDrop = false;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                animate(new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.25))), new PropertyPath(RichTextBox.OpacityProperty), animationStoryboard, canvasDragDrop.Name);
                try
                {
                    if (Path.GetExtension(files[0]) == ".jpg" || Path.GetExtension(files[0]) == ".png")
                    {
                        insertImage = new Image() { Source = new BitmapImage(new Uri(files[0])), Stretch = Stretch.Uniform, Width = rtbMain.ActualWidth - 50 };
                        insertImageUIContainer = new BlockUIContainer(insertImage);
                        rtbMain.Document.Blocks.Add(insertImageUIContainer);
                        return;
                    }
                    openDocument(files[0], rtbMain);
                    currentlyOpenPath = files[0];
                }
                catch (Exception ex)
                {
                    messageBox = new CustomMessageBox();
                    messageBox.SetupMsgBox(ex.Message + errorPreset, "Ошибка!", this.FindResource("iconError"));
                    messageBox.ShowDialog();
                }
            }
            e.Handled = true;
        }

        private void rtbMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                canvasDragDrop.Visibility = Visibility.Visible;
                animate(new DoubleAnimation(0.5, new Duration(TimeSpan.FromSeconds(0.25))), new PropertyPath(RichTextBox.OpacityProperty), animationStoryboard, canvasDragDrop.Name);
            }
        }

        private void rtbMain_DragLeave(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                animate(new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.25))), new PropertyPath(RichTextBox.OpacityProperty), animationStoryboard, canvasDragDrop.Name);
        }

        public void checkForAlignment()
        {
            if (rtbMain.Selection.Start.Paragraph != null)
            {
                miAlignLeft.IsChecked = (rtbMain.Selection.Start.Paragraph.TextAlignment == TextAlignment.Left);
                miAlignCenter.IsChecked = (rtbMain.Selection.Start.Paragraph.TextAlignment == TextAlignment.Center);
                miAlignRight.IsChecked = (rtbMain.Selection.Start.Paragraph.TextAlignment == TextAlignment.Right);
                miAlignJustify.IsChecked = (rtbMain.Selection.Start.Paragraph.TextAlignment == TextAlignment.Justify);
            }
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        private void animate(DoubleAnimation animation, PropertyPath propertyPath, Storyboard storyboard, string objectName)
        {
            storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTargetName(animation, objectName);
            Storyboard.SetTargetProperty(animation, propertyPath);
            storyboard.Begin(this);
        }

        private void animate(ThicknessAnimation animation, PropertyPath propertyPath, Storyboard storyboard, string objectName)
        {
            storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTargetName(animation, objectName);
            Storyboard.SetTargetProperty(animation, propertyPath);
            storyboard.Begin(this);
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private System.Drawing.Color getAverageColor(System.Drawing.Bitmap bm)
        {
            var list = new Dictionary<System.Drawing.Color, int>();
            int addR = 0;
            int addG = 0;
            int addB = 0;
            System.Drawing.Bitmap myImage = new System.Drawing.Bitmap(bm, new System.Drawing.Size(50, 50));
            for (int x = 0; x < myImage.Width; x++)
            {
                for (int y = 0; y < myImage.Height; y++)
                {
                    System.Drawing.Color color = myImage.GetPixel(x, y);
                    if (!list.ContainsKey(color))
                        list.Add(color, 1);
                    else
                        list[color]++;
                }
            }
            System.Drawing.Color keyOfMaxValue = list.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            if (keyOfMaxValue.GetBrightness() < 0.5F)
            {
                Color higherBrightness = ColorFromHSV(keyOfMaxValue.GetHue(), keyOfMaxValue.GetSaturation(), 0.85F);
                keyOfMaxValue = System.Drawing.Color.FromArgb(higherBrightness.R, higherBrightness.G, higherBrightness.B);
            }
            System.Drawing.Color returnColor = System.Drawing.Color.FromArgb((int)keyOfMaxValue.R + addR, (int)keyOfMaxValue.G + addG, (int)keyOfMaxValue.B + addB);
            return returnColor;
        }

        private void anyNew_Click(object sender, RoutedEventArgs e)
        {
            rtbMain.SelectAll();
            rtbMain.Selection.Text = "";
            unsavedChanges = false;
            currentlyOpenPath = "";
        }

        private void anyOpen_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> result = ofdOpen.ShowDialog();
            if (result == true)
            {
                try
                {
                    openDocument(ofdOpen.FileName, rtbMain);
                    currentlyOpenPath = ofdOpen.FileName;
                }
                catch (Exception ex)
                {
                    messageBox = new CustomMessageBox();
                    messageBox.SetupMsgBox(ex.Message + errorPreset, "Ошибка!", this.FindResource("iconError"));
                    messageBox.ShowDialog();
                }
            }
        }

        private void anySave_Click(object sender, RoutedEventArgs e)
        {
            if (currentlyOpenPath == null)
            {
                anySaveAs_Click(sender, e);
            }
            else
            {
                try
                {
                    saveDocument(currentlyOpenPath, rtbMain);
                }
                catch (Exception ex)
                {
                    messageBox = new CustomMessageBox();
                    messageBox.SetupMsgBox(ex.Message + errorPreset, "Ошибка!", this.FindResource("iconError"));
                    messageBox.ShowDialog();
                }
            }
        }

        private void anySaveAs_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> result = sfdSave.ShowDialog();
            if (result == true)
            {
                try
                {
                    saveDocument(sfdSave.FileName, rtbMain);
                }
                catch (Exception ex)
                {
                    messageBox = new CustomMessageBox();
                    messageBox.SetupMsgBox(ex.Message + errorPreset, "Ошибка!", this.FindResource("iconError"));
                    messageBox.ShowDialog();
                }
            }
        }

        private void anyExit_Click(object sender, RoutedEventArgs e)
        {
            if (unsavedChanges)
            {
                Exit exit = new Exit();
                exit.mWRichTextBox = XamlReader.Parse(XamlWriter.Save(rtbMain)) as RichTextBox;
                exit.mWCurrentlyOpenFile = currentlyOpenPath;
                if (exit.ShowDialog() == true)
                    return;
                Application.Current.Shutdown();
            }
            else
                Application.Current.Shutdown();
        }

        private void anyFormat_Click(object sender, RoutedEventArgs e)
        {
            rtbMain.Focus();
            checkForAlignment();
        }

        private void anyFormat_MouseDown(object sender, MouseButtonEventArgs e)
        {
            rtbMain.Focus();
            checkForAlignment();
        }

        private void anyFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbFontSize.SelectedItem != null)
                rtbMain.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cbFontSize.SelectedItem);
            rtbMain.Focus();
        }

        private void anyFont_DropDownClosed(object sender, EventArgs e)
        {
            if (cbFont.SelectedItem != null)
                rtbMain.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, ((ComboBoxItem)cbFont.SelectedItem).FontFamily);
            rtbMain.Focus();
        }

        private void anyFontSize_DropDownClosed(object sender, EventArgs e)
        {
            if (cbFontSize.SelectedItem != null)
                rtbMain.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cbFontSize.SelectedItem);
            rtbMain.Focus();
        }

        private void anyLineSpacing_DropDownClosed(object sender, EventArgs e)
        {
            if (cbLineSpacing.SelectedItem != null)
                rtbMain.Selection.ApplyPropertyValue(Paragraph.LineHeightProperty, cbLineSpacing.SelectedItem);
            rtbMain.Focus();
        }

        private void anyUndo_Click(object sender, RoutedEventArgs e)
        {
            if (rtbMain.CanUndo)
                rtbMain.Undo();
        }

        private void anyRedo_Click(object sender, RoutedEventArgs e)
        {
            if (rtbMain.CanRedo)
                rtbMain.Redo();
        }

        private void anyNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPageIndex < pages.Count - 1)
            {
                currentPageIndex++;
                UpdateRichTextBoxContent();
            }
        }

        private void anyPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                UpdateRichTextBoxContent();
            }
        }

        private void miEdit_Focus(object sender, RoutedEventArgs e)
        {
            miUndo.IsEnabled = false;
            miRedo.IsEnabled = false;
            if (rtbMain.CanUndo)
                miUndo.IsEnabled = true;
            if (rtbMain.CanRedo)
                miRedo.IsEnabled = true;
        }

        private void SidebarOpen(Uri uri)
        {
            frameSidebar.Source = uri;
            float width = 200f;
            EasingMode easing = EasingMode.EaseOut;
            ThicknessAnimation sidebar = new ThicknessAnimation(new Thickness(-width, 0f, 0f, 0f), new Duration(TimeSpan.FromSeconds(0.6f))) { EasingFunction = new QuinticEase() { EasingMode = easing } };
            ThicknessAnimation textBox = new ThicknessAnimation(new Thickness(0f, 0f, width, 0f), new Duration(TimeSpan.FromSeconds(0.6f))) { EasingFunction = new QuinticEase() { EasingMode = easing } };
            DoubleAnimation closeButton = new DoubleAnimation(100f, new Duration(TimeSpan.FromSeconds(0.6f)));
            animate(sidebar, new PropertyPath(Canvas.MarginProperty), animationStoryboard, "canvasSidebar");
            animate(textBox, new PropertyPath(RichTextBox.MarginProperty), animationStoryboard, "rtbMain");
            bCloseSidebar.IsHitTestVisible = true;
            animate(closeButton, new PropertyPath(Button.OpacityProperty), animationStoryboard, "bCloseSidebar");
        }

        private void SidebarClose()
        {
            float width = -200f;
            EasingMode easing = EasingMode.EaseIn;
            ThicknessAnimation sidebar = new ThicknessAnimation(new Thickness(-width, 0f, 0f, 0f), new Duration(TimeSpan.FromSeconds(0.6f))) { EasingFunction = new QuinticEase() { EasingMode = easing } };
            ThicknessAnimation textBox = new ThicknessAnimation(new Thickness(0f, 0f, width, 0f), new Duration(TimeSpan.FromSeconds(0.6f))) { EasingFunction = new QuinticEase() { EasingMode = easing } };
            DoubleAnimation closeButton = new DoubleAnimation(0f, new Duration(TimeSpan.FromSeconds(0.6f)));
            animate(sidebar, new PropertyPath(Canvas.MarginProperty), animationStoryboard, "canvasSidebar");
            animate(textBox, new PropertyPath(RichTextBox.MarginProperty), animationStoryboard, "rtbMain");
            bCloseSidebar.IsHitTestVisible = false;
            animate(closeButton, new PropertyPath(Button.OpacityProperty), animationStoryboard, "bCloseSidebar");
        }

        private void frameSidebar_ContentRendered(object sender, EventArgs e)
        {
            var content = frameSidebar.Content as System.Windows.Controls.Page;
            var grid = content.Content as Grid;

            switch (content.Title)
            {
                case "Поиск и замена":
                    Button bFind = (Button)grid.Children[5];
                    Button bReplace = (Button)grid.Children[6];
                    Button bReplaceNext = (Button)grid.Children[7];
                    bFind.Click += bFindReplace_Click;
                    bReplace.Click += bFindReplace_Click;
                    bReplaceNext.Click += bFindReplace_Click;
                    break;
                default:
                    break;
            }
        }

        private void bCloseSidebar_Click(object sender, RoutedEventArgs e)
        {
            SidebarClose();
        }

        private void miFindReplace_Click(object sender, RoutedEventArgs e)
        {
            SidebarOpen(new Uri("/SidebarPages/FindReplace.xaml", UriKind.Relative));
        }

        private void bFindReplace_Click(object sender, RoutedEventArgs e)
        {
            var content = frameSidebar.Content as System.Windows.Controls.Page;
            string action = "Найти далее";
            var grid = content.Content as Grid;
            TextBox tbFind = (TextBox)grid.Children[2];
            TextBox tbReplace = (TextBox)grid.Children[4];
            var expander = (Expander)grid.Children[8];
            CheckBox cbMatchCase = (CheckBox)((Grid)expander.Content).Children[0];
            CheckBox cbMatchWord = (CheckBox)((Grid)expander.Content).Children[1];
            TextRange textRange = null;
            FindOptions findOptions = FindOptions.None;
            if (cbMatchCase.IsChecked == true)
                findOptions = FindOptions.MatchCase;
            if (cbMatchWord.IsChecked == true)
                findOptions ^= FindOptions.MatchWholeWord;
            if (findReplaceChanges)
            {
                findAndReplace = new FindAndReplaceManager(rtbMain.Document);
                findReplaceChanges = false;
            }
            switch (((Button)sender).Content.ToString())
            {
                case "Найти далее":
                    textRange = findAndReplace.FindNext(tbFind.Text, findOptions);
                    action = "Найти далее";
                    break;
                case "Заменить все":
                    int results = findAndReplace.ReplaceAll(tbFind.Text, tbReplace.Text, findOptions, null);
                    CustomMessageBox customMessageBox = new CustomMessageBox();
                    customMessageBox.SetupMsgBox($"Число выполненных замен: {results}.", "Заменить все", this.FindResource("iconInformation"));
                    customMessageBox.ShowDialog();
                    rtbMain.Focus();
                    return;
                case "Заменить":
                    textRange = findAndReplace.Replace(tbFind.Text, tbReplace.Text, findOptions);
                    action = "Заменить";
                    break;
                default:
                    break;
            }

            if (textRange == null)
            {
                CustomMessageBox customMessageBox = new CustomMessageBox();
                customMessageBox.SetupMsgBox("Нет (больше) результатов.\nВы хотите снова начать с самого верха?", "Сбросить поиск?", this.FindResource("iconInformation"), "Да", "Нет");
                customMessageBox.ShowDialog();
                if (customMessageBox.result.ToString() == "Да")
                {
                    findAndReplace = new FindAndReplaceManager(rtbMain.Document);
                    findReplaceChanges = false;
                    if (action == "Заменить")
                        textRange = findAndReplace.Replace(tbFind.Text, tbReplace.Text, findOptions);
                    else
                        textRange = findAndReplace.FindNext(tbFind.Text, findOptions);
                    if (textRange == null)
                        return;
                }
                else
                    return;
            }
            rtbMain.Selection.Select(textRange.Start, textRange.End);
            rtbMain.Focus();
        }

        private void miForeground_Click(object sender, RoutedEventArgs e)
        {
            ColorPicker colorPicker = new ColorPicker();
            if (colorPicker.ShowDialog() == true)
            {
                double hue = Convert.ToByte(colorPicker.slHue.Value);
                double saturation = Convert.ToByte(colorPicker.slSaturation.Value);
                double value = Convert.ToByte(colorPicker.slValue.Value);
                Color color = new Color();

                color = ColorFromHSV(hue, saturation / 100, value / 100);

                rtbMain.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
            }
        }

        private void miBackground_Click(object sender, RoutedEventArgs e)
        {
            ColorPicker colorPicker = new ColorPicker();
            if (colorPicker.ShowDialog() == true)
            {
                double hue = Convert.ToByte(colorPicker.slHue.Value);
                double saturation = Convert.ToByte(colorPicker.slSaturation.Value);
                double value = Convert.ToByte(colorPicker.slValue.Value);
                Color color = new Color();

                color = ColorFromHSV(hue, saturation / 100, value / 100);

                rtbMain.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(color));
            }
        }

        private void miAnyBar_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null || rtbMain == null)
                return;

            switch (((MenuItem)sender).Name)
            {
                case "miFormattingBar":
                    tbFormatting.Visibility = Visibility.Visible;
                    break;
                case "miParagraphBar":
                    tbParagraph.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void miAnyBar_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender == null || rtbMain == null)
                return;

            switch (((MenuItem)sender).Name)
            {
                case "miFormattingBar":
                    tbFormatting.Visibility = Visibility.Collapsed;
                    break;
                case "miParagraphBar":
                    tbParagraph.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }

        private void miInsertImage_Click(object sender, RoutedEventArgs e)
        {
            if (ofdImage.ShowDialog() == true)
            {
                insertImage = new Image() { Source = new BitmapImage(new Uri(ofdImage.FileName)), Stretch = Stretch.Uniform, Width = rtbMain.ActualWidth - 50 };
                insertImageUIContainer = new BlockUIContainer(insertImage);
                rtbMain.Document.Blocks.Add(insertImageUIContainer);
            }
        }

        private void miInsertHyperLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedText = rtbMain.Selection.Text;

                var start = rtbMain.Selection.Start;
                var end = rtbMain.Selection.End;

                Hyperlink hyperlink = new Hyperlink(start, end);
                hyperlink.NavigateUri = new Uri(selectedText);
                hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
            }
            catch (Exception ex)
            {
                messageBox = new CustomMessageBox();
                messageBox.SetupMsgBox(ex.Message + errorPreset, "Ошибка!", this.FindResource("iconError"));
                messageBox.ShowDialog();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                if (sender is Hyperlink hyperlink)
                {
                    Process process = new Process();
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = hyperlink.NavigateUri.ToString();
                    process.Start();
                }
            }
            catch (Exception ex)
            {
                messageBox = new CustomMessageBox();
                messageBox.SetupMsgBox(ex.Message + errorPreset, "Ошибка!", this.FindResource("iconError"));
                messageBox.ShowDialog();
            }
        }
    }
}
