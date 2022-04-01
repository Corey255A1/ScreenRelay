//WunderVision 2020
//https://www.wundervisionenvisionthefuture.com/
//Screen Class to hold the Screen Positions and the Bitmap Image Capture from that region


using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace ScreenRelay
{
    public class Screen : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));



        private int _left;
        public int Left
        {
            get { return _left; }
            set { _left = value; Notify(); }
        }

        private int _top;
        public int Top
        {
            get { return _top; }
            set { _top = value; Notify(); }
        }

        private int _right;
        public int Right
        {
            get { return _right; }
            set { _right = value; Notify(); }
        }


        private int _bottom;
        public int Bottom
        {
            get { return _bottom; }
            set { _bottom = value; Notify(); }
        }

        public int Width => _right - _left;

        public int Height => _bottom - _top;


        private double _scaleX = 1.0;
        public double ScaleX
        {
            get { return _scaleX; }
            set { _scaleX = value; Notify(); }
        }

        private double _scaleY = 1.0;
        public double ScaleY
        {
            get { return _scaleY; }
            set { _scaleY = value; Notify(); }
        }

        private BitmapImage _image;

        public BitmapImage Image
        {
            get => _image;
            set { _image = value; Notify(); }
        }

        public Screen(RECT rect)
        {
            _left = rect.left;
            _top = rect.top;
            _right = rect.right;
            _bottom = rect.bottom;
        }

        public Screen(int left, int top, int width, int height)
        {
            _left = left;
            _top = top;
            _right = left + width;
            _bottom = top + height;
        }

        public void SetScale(int width, int height)
        {
            ScaleX = (double)Width / (double)width;
            ScaleY = (double)Height / (double)height;
        }

        public void UpdateImage()
        {
            using (MemoryStream b = ScreenFinder.GetScreen(this))
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = b;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                Image = bi;
            }
        }

    }
}
