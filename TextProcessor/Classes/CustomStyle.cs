using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace TextProcessor.Classes
{
    public class CustomStyle
    {
        public string StyleName { get; set; }
        public bool BoldStyle { get; set; }
        public bool ItalicStyle { get; set; }
        public bool UnderlineStyle { get; set; }
        public bool AlignLeft { get; set; }
        public bool AlignCenter { get; set; }
        public bool AlignRight { get; set; }
        public bool AlignJustify { get; set; }
        public double LineHeightProperty { get; set; }
        public int FontSize { get; set; }
        public System.Windows.Media.FontFamily FontFamily { get; set; }
    }
}
