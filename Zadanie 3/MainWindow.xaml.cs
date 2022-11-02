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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace Zadanie_3
{
    public partial class MainWindow : Window
    {
        private byte r;
        private byte g;
        private byte b;
        private byte c;
        private byte m;
        private byte y;
        private byte k;
        private int angleX;
        private int angleY;
        private Color color;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #region Commands

        public ICommand ChangeXAngleComand { get; set; }
        public ICommand ChangeYAngleComand { get; set; }
        #endregion
        public MainWindow()
        {
            InitializeComponent();
        }
        public Color Color
        {
            get { return color; }
            set
            {
                color = value;
                OnPropertyChanged();
                OnPropertyChanged("SelectedBrush");
                R = color.R;
                G = color.G;
                B = color.B;
                RefreshCmyk();
            }
        }
        public SolidColorBrush SelectedBrush
        {
            get
            {
                return new SolidColorBrush(color);
            }
        }
        public byte R
        {
            get { return r; }
            set 
            {
                r = value;
                OnPropertyChanged();
                color.R = r;
                color.A = 0xff;
                OnPropertyChanged("Color");
                OnPropertyChanged("SelectedBrush");
                RefreshCmyk();
            }
        }
        public byte G
        {
            get { return g; }
            set
            {
                g = value;
                OnPropertyChanged();
                color.G = g;
                color.A = 0xff;
                OnPropertyChanged("Color");
                OnPropertyChanged("SelectedBrush");
                RefreshCmyk();
            }
        }
        public byte B
        {
            get { return b; }
            set
            {
                b = value;
                OnPropertyChanged();
                color.B = b;
                color.A = 0xff;
                OnPropertyChanged("Color");
                OnPropertyChanged("SelectedBrush");
                RefreshCmyk();
            }
        }
        public byte C
        {
            get { return c; }
            set
            {
                c = value;
                OnPropertyChanged();
            }
        }
        public byte M
        {
            get { return m; }
            set
            {
                m = value;
                OnPropertyChanged();
            }
        }
        public byte Y
        {
            get { return y; }
            set
            {
                y = value;
                OnPropertyChanged();
            }
        }
        public byte K
        {
            get { return k; }
            set
            {
                k = value;
                OnPropertyChanged();
            }
        }
        public int AngleX
        {
            get { return angleX; }
            set 
            { 
                angleX = value;
                OnPropertyChanged();
            }
        }
        public int AngleY
        {
            get { return angleY; }
            set 
            { 
                angleY = value;
                OnPropertyChanged();
            }
        }
        public float Divideby0(float v)
        {
            if(v<0 || float.IsNaN(v))
                return 0;
            return v;
        }
        public void RefreshCmyk()
        {
            float newR = (float) R / 255;
            float newG = (float) G / 255;
            float newB = (float) B / 255;
            c = (byte) Divideby0((1 - newR - K) / (1 - k));
            m = (byte)Divideby0((1 - newG - K) / (1 - k));
            y = (byte)Divideby0((1 - newB - K) / (1 - k));
            k = (byte)Divideby0(1-Math.Max(Math.Max(newR,newG),newB));
            OnPropertyChanged("C");
            OnPropertyChanged("M");
            OnPropertyChanged("Y");
            OnPropertyChanged("K");
        }
        public void RefreshRGB()
        {
            r = (byte)(255 * (1 - K) * (1 - C));
            g = (byte)(255 * (1 - K) * (1 - M));
            b = (byte)(255 * (1 - K) * (1 - Y));
            OnPropertyChanged("R");
            OnPropertyChanged("G");
            OnPropertyChanged("B");
            color=Color.FromRgb(r,g,b);
            OnPropertyChanged("Color");
            OnPropertyChanged("SelectedBrush");
        }
       //public view3d()
       //{
      //      ChangeXAngleComand = new RelayCommand(ChangeX);
      //
       //}
       private void ChangeX(object obj)
       {
            if (int.TryParse(obj.ToString(), out int angle))
                AngleX = (AngleX + angle) % 360;
       }
        private void ChangeY(object obj)
        {
            if (int.TryParse(obj.ToString(), out int angle))
                AngleY = (AngleY + angle) % 360;
        }
    }
}
