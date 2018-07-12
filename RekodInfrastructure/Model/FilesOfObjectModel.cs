using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Drawing;
using Rekod.ViewModel;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.Controllers;

namespace Rekod.Model
{
    public class FilesOfObjectModel : ViewModelBase
    {
        public enum TypeFileEnum
        {
            Preview,
            Icon
        }

        private readonly int _idTable;
        private readonly int _idObject;
        private readonly int _idPhoto;
        private readonly DateTime _date;
        private readonly string _fileNameMain;
        private string _fileName;
        private BitmapImage _imgPreview;
        private readonly bool _isPhoto;

        public FilesOfObjectModel(int idTable, int idObject, int idPhoto, string fileName, DateTime date, BitmapImage imgPreview, bool isPhoto)
        {
            _idTable = idTable;
            _idObject = idObject;
            _idPhoto = idPhoto;
            _fileName = fileName;
            _fileNameMain = fileName;
            _date = date;
            _imgPreview = imgPreview;
            _isPhoto = isPhoto;
        }

        public int IdTable { get { return _idTable; } }
        public int IdObject { get { return _idObject; } }
        public int IdPhoto { get { return _idPhoto; } }
        public DateTime Date { get { return _date; } }

        public bool IsPhoto { get { return _isPhoto; } }

        public BitmapImage ImgPreview
        {
            get { return _imgPreview; }
            set { _imgPreview = value; }
        }
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; OnPropertyChanged(() => this.FileName); }
        }

        public bool IsEdit { get { return !string.Equals(_fileName, _fileNameMain); } }

        public void ClearEdit()
        {
            FileName = _fileNameMain;
        }
    }
}
