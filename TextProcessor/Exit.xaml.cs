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
using System.Windows.Shapes;

namespace TextProcessor
{
    /// <summary>
    /// Логика взаимодействия для Exit.xaml
    /// </summary>
    public partial class Exit : Window
    {
        public string mWCurrentlyOpenFile;
        public string mWRawRtf;
        public RichTextBox mWRichTextBox;

        public void getArguments(string currentlyOpenPath, string rawRtf)
        {
            mWCurrentlyOpenFile = currentlyOpenPath;
            mWRawRtf = rawRtf;
        }

        public Exit()
        {
            InitializeComponent();
        }

        private void bExit_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            MainWindow MainWindow = new MainWindow();
            if (mWCurrentlyOpenFile == null)
            {
                Nullable<bool> result = MainWindow.sfdSave.ShowDialog();
                if (result == true)
                {
                    if (MainWindow.saveDocument(MainWindow.sfdSave.FileName, mWRichTextBox))
                    {
                        this.DialogResult = false;
                    }
                }
            }
            else
            {
                try
                {
                    MainWindow.saveDocument(MainWindow.sfdSave.FileName, mWRichTextBox);
                    this.DialogResult = false;
                }
                catch (Exception ex)
                {
                    CustomMessageBox messageBox = new CustomMessageBox();
                    messageBox.SetupMsgBox(ex.Message + "\nТекстовый процессор пытается продолжить работу.", "Ошибка!", this.FindResource("iconError"));
                    messageBox.ShowDialog();
                    throw;
                }
            }
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
