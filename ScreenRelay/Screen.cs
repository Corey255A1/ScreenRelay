//WunderVision 2020
//https://www.wundervisionenvisionthefuture.com/
//Screen Class to hold the Screen Positions and the Bitmap Image Capture from that region


using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;
using System.IO;

namespace ScreenRelay
{
    public class Screen : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify([CallerMemberName]string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));



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
