using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using PgAtVM = Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.Controllers;

namespace Rekod.DataAccess.SourcePostgres.Model.PgAttributes
{
    public class PgAttributeFileM : ViewModelBase
    {
        #region Поля
        private readonly PgAtVM.PgAttributeFilesVM _source;
        private readonly int _idFile;
        private DateTime _date;
        private string _fileName;
        private readonly bool _hasPreview;
        private BitmapSource _imgPreview;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Источник фотографий
        /// </summary>
        public PgAtVM.PgAttributeFilesVM Source
        { get { return _source; } }
        /// <summary>
        /// Идентификатор файла
        /// </summary>
        public int IdFile
        { get { return _idFile; } }
        /// <summary>
        /// Дата создания файла
        /// </summary>
        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }
        /// <summary>
        /// Имеется ли у файла миниатюра иконки или катринки (если нет то берется по расширению файла)
        /// </summary>
        public bool HasPreview
        { get { return _hasPreview; } }
        /// <summary>
        /// Миниатюра иконки или картинки
        /// </summary>
        public BitmapSource ImgPreview
        {
            get { return _imgPreview; }
        }
        /// <summary>
        /// Имя файла
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { OnPropertyChanged(ref _fileName, value, () => this.FileName); }
        }
        #endregion // Свойства

        #region Конструктор
        public PgAttributeFileM(PgAtVM.PgAttributeFilesVM source, int idFile, BitmapSource imgPreview)
        {
            _source = source;
            _idFile = idFile;
            _imgPreview = imgPreview;
        }
        #endregion
    }
}
