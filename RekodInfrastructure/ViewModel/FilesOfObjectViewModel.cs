using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Interfaces;
using NpgsqlTypes;
using Rekod.Model;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.Services;

namespace Rekod.ViewModel
{
    public class FilesOfObjectViewModel : ViewModelBase
    {
        #region Fields
        private Interfaces.tablesInfo _tableInfo;
        private Interfaces.photoInfo _tablePhotoInfo;
        private int? _idObject;
        private bool _isReadOnly = true;
        private bool _isEnabled = false;

        private ObservableCollection<FilesOfObjectModel> _collectionOfFiles;
        private ObservableCollection<FilesOfObjectModel> _collectionOfSelectedFiles;

        private RelayCommand _loadFilesCommand;             // Загружает полный список файлов
        private RelayCommand _addFilesCommand;              // Добавляет файлы
        private RelayCommand _deleteFilesCommand;           // Удаляет выделенные файлы
        private RelayCommand _openFileCommand;              // Открывает файл
        private RelayCommand _editFilesNoteCommand;         // Открывает редактор комментариев файла и при закрытие запускает сохранение

        private bool _isEditFilesNote = false;              // Идентификатор открытия или закрытия редактора комментариев

        private Func<string[]> _funcOpenDialogToAddFiles;

        #endregion

        #region Properties

        public Interfaces.tablesInfo TableInfo
        {
            get { return _tableInfo; }
            set
            {
                _tableInfo = value;
                if (value != null)
                {
                    _tablePhotoInfo = value.PhotoInfo;
                    _isReadOnly = !Program.app.getWriteTable(_tableInfo.idTable);
                }
                else
                {
                    _tablePhotoInfo = null;
                    _isReadOnly = true;
                }
                _isEnabled = CheckEnabled();
            }
        }
        public int? IdObject
        {
            get { return _idObject; }
            set
            {
                _idObject = value;
                _isEnabled = CheckEnabled();
            }
        }

        public ObservableCollection<FilesOfObjectModel> CollectionOfFiles
        {
            get { return _collectionOfFiles ?? (_collectionOfFiles = new ObservableCollection<FilesOfObjectModel>()); }
            set { _collectionOfFiles = value; OnPropertyChanged(() => this.CollectionOfFiles); }
        }
        public ObservableCollection<FilesOfObjectModel> CollectionOfSelectedFiles
        {
            get { return _collectionOfSelectedFiles ?? (_collectionOfSelectedFiles = new ObservableCollection<FilesOfObjectModel>()); }
            set { _collectionOfSelectedFiles = value; OnPropertyChanged(() => this.CollectionOfSelectedFiles); }
        }

        public bool IsEditFilesNote
        {
            get { return _isEditFilesNote; }
            set { _isEditFilesNote = value; OnPropertyChanged(() => this.IsEditFilesNote); }
        }
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }
        public bool IsEnabled
        {
            get { return _isEnabled; }
        }

        public Func<string[]> FuncOpenDialogToAddFiles
        {
            get { return _funcOpenDialogToAddFiles ?? (_funcOpenDialogToAddFiles = FuncOpenDialogToAddFilesExecution); }
            set { _funcOpenDialogToAddFiles = value; }
        }
        #endregion
        public FilesOfObjectViewModel()
        {
            if (_tableInfo == null)
                return;
            if (_tableInfo.PhotoInfo != null)
                _isEnabled = true;
            else
                return;

        }

        public FilesOfObjectViewModel(Interfaces.tablesInfo table, int idObject)
        {
            TableInfo = table;
            IdObject = idObject;
        }

        #region Command: LoadFilesCommand
        public RelayCommand LoadFilesCommand
        {
            get
            {
                return _loadFilesCommand ?? (_loadFilesCommand
                    = new RelayCommand(this.LoadFiles,this.CanLoadFiles));
            }
        }
        void LoadFiles(object param = null)
        {
            if (!CanLoadFiles(param)) return;
            CollectionOfFiles.Clear();
            var listFields = TableInfo.ListField;
            if (listFields == null) return;

            using (var SQLCmd = new SqlWork())
            {
                SQLCmd.sql = string.Format(@"
                                    SELECT ""{2}"", ""{3}"", dataupd, img_preview, file_name, is_photo
                                    FROM {0}.{1}
                                    WHERE id_obj = :id_obj",
                           TableInfo.nameSheme,
                           _tablePhotoInfo.namePhotoTable,
                           _tablePhotoInfo.nameFieldID,
                           _tablePhotoInfo.namePhotoField);
                var Params = new IParams[]
                                       {
                                           new Params
                                               {
                                                   _paramName = "id_obj",
                                                   typeData = NpgsqlDbType.Integer,
                                                   value = IdObject
                                               }
                                       };
                SQLCmd.ExecuteReader(Params);
                while (SQLCmd.CanRead())
                {
                    var fileName = SQLCmd.GetValue<string>("file_name");
                    var imgPreview = GetPreviewFromBD(SQLCmd.GetBytes("img_preview"), fileName);
                    CollectionOfFiles.Add(new FilesOfObjectModel(
                                      idTable: _tablePhotoInfo.idTable,
                                      idObject: (int)IdObject,
                                      idPhoto: SQLCmd.GetValue<int>("id"),
                                      fileName: fileName,
                                      date: SQLCmd.GetValue<DateTime>("dataupd"),
                                      imgPreview: imgPreview,
                                      isPhoto: SQLCmd.GetValue<bool>("is_photo")
                                      ));
                }
            }

        }

        bool CanLoadFiles(object param = null)
        {
            if (!IsEnabled)
                return false;
            return true;
        }

        #endregion

        #region Command: AddFilesCommand
        public RelayCommand AddFilesCommand
        {
            get
            {
                return _addFilesCommand ?? (_addFilesCommand
                    = new RelayCommand(this.AddFiles, this.CanAddFiles));
            }
        }
        void AddFiles(object param = null)
        {
            if (!CanAddFiles(param)) return;

            string[] fileNames = FuncOpenDialogToAddFiles.Invoke();

            for (int i = 0; i < fileNames.Length; i++)
            {
                string fileName = fileNames[i];
                byte[] myData = GetFile(fileName);
                byte[] imgPreview = GetPreviewFromImage(fileName);
                bool isPhoto = (imgPreview != null);

                using (var sqlCmd = new SqlWork())
                {
                    string sql =
                        string.Format(
                            @"
    INSERT INTO {0}.{1} ({2}, {3}, img_preview, file_name, is_photo) 
    VALUES ({4}, :file_blob, :img_preview, :file_name, :is_photo)
    RETURNING *",
                            TableInfo.nameSheme,
                            _tablePhotoInfo.namePhotoTable,
                            _tablePhotoInfo.namePhotoField,
                            _tablePhotoInfo.namePhotoFile,
                            IdObject);

                    sqlCmd.sql = sql;


                    var pParams = new IParams[]
                                      {
                                          new Params
                                              {
                                                  _paramName = "file_blob",
                                                  typeData = NpgsqlTypes.NpgsqlDbType.Bytea,
                                                  value = myData
                                              },
                                          new Params
                                              {
                                                  _paramName = "img_preview",
                                                  typeData = NpgsqlTypes.NpgsqlDbType.Bytea,
                                                  value = imgPreview
                                              },
                                          new Params
                                              {
                                                  _paramName = "file_name",
                                                  typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                  value = Path.GetFileName(fileName)
                                              },
                                          new Params
                                              {
                                                  _paramName = "is_photo",
                                                  typeData = NpgsqlTypes.NpgsqlDbType.Boolean,
                                                  value = isPhoto
                                              }
                                      };
                    sqlCmd.ExecuteReader(pParams);
                    if (sqlCmd.CanRead())
                    {
                        var bmImgPreview = GetPreviewFromBD(imgPreview, Path.GetFullPath(fileName));
                        CollectionOfFiles.Add(new FilesOfObjectModel(
                                          idTable: _tablePhotoInfo.idTable,
                                          idObject: (int)IdObject,
                                          idPhoto: sqlCmd.GetValue<int>("id"),
                                          fileName: Path.GetFileName(fileName),
                                          date: sqlCmd.GetValue<DateTime>("dataupd"),
                                          imgPreview: bmImgPreview,
                                          isPhoto: isPhoto
                                          ));
                    }
                }
            }
        }

        bool CanAddFiles(object param = null)
        {
            if (!IsEnabled)
                return false;
            return !IsReadOnly;
        }

        #endregion

        #region Command: DeleteFilesCommand
        public RelayCommand DeleteFilesCommand
        {
            get
            {
                return _deleteFilesCommand ?? (_deleteFilesCommand
                    = new RelayCommand(this.DeleteFiles, this.CanDeleteFiles));
            }
        }
        void DeleteFiles(object param = null)
        {
            if (!CanDeleteFiles(param)) return;
            string sql = "";
            for (int i = 0; i < CollectionOfSelectedFiles.Count; i++)
            {
                sql += string.Format(@"
                                    DELETE
                                    FROM {0}.{1}
                                    WHERE ""{2}"" = {3}\n",
                                TableInfo.nameSheme,
                                _tablePhotoInfo.namePhotoTable,
                                _tablePhotoInfo.nameFieldID,
                                CollectionOfSelectedFiles[i].IdPhoto);
            }

            using (var sqlCmd = new SqlWork())
            {
                sqlCmd.sql = sql;
                sqlCmd.ExecuteNonQuery();
            }
        }

        bool CanDeleteFiles(object param = null)
        {
            if (!IsEnabled) return false;
            if (IsReadOnly) return false;
            if (!(param is FilesOfObjectModel)) return false;
            return true;
        }
        #endregion

        #region Command: OpenFileCommand <WinAttribFileModel>
        public RelayCommand OpenFileCommand
        {
            get
            {
                return _openFileCommand ?? (_openFileCommand
                    = new RelayCommand(this.OpenFile, this.CanOpenFile));
            }
        }
        void OpenFile(object param = null)
        {
            var attr = param as FilesOfObjectModel;
            if (attr == null) return;

        }

        bool CanOpenFile(object param = null)
        {
            if (!IsEnabled) return false;
            if (!(param is FilesOfObjectModel)) return false;
            return true;
        }
        #endregion

        #region Command: EditFilesNoteCommand
        public RelayCommand EditFilesNoteCommand
        {
            get
            {
                return _editFilesNoteCommand ?? (_editFilesNoteCommand
                    = new RelayCommand(this.EditFilesNote, this.CanEditFilesNote));
            }
        }

        void EditFilesNote(object param = null)
        {
        }

        bool CanEditFilesNote(object param = null)
        {
            if (!IsEnabled) return false;
            if (IsReadOnly) return false;
            if (!(param is FilesOfObjectModel)) return false;
            return true;
        }

        #endregion

        #region Services

        private bool CheckEnabled()
        {
            return (_tablePhotoInfo != null && IdObject != null);
        }

        /// <summary> Предпросмотр загруженых фотографий из БД
        /// </summary>
        /// <param name="rawImageBytes">Массив файтов</param>
        /// <param name="extension">Расщирение файла если preview отсутствует</param>
        /// <returns></returns>
        private BitmapImage GetPreviewFromBD(byte[] rawImageBytes, string extension)
        {
            if (rawImageBytes != null)
                return FilePreviewController.GetBitmapImage(rawImageBytes);

            var bitmapIcon = FilePreviewController.GetIcon(extension);
            return FilePreviewController.GetBitmapImage(bitmapIcon);
        }

        /// <summary> Возвращает preview картинки
        /// </summary>
        /// <param name="fileName">Полный путь к файлу</param>
        /// <returns></returns>
        private byte[] GetPreviewFromImage(string fileName)
        {
            Bitmap preview = FilePreviewController.GetPreviewPhoto(fileName);
            if (preview != null)
                return preview.ToByteArray();
            return null;
        }

        private byte[] GetFile(string fileName)
        {
            byte[] myData = null;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                myData = new byte[fs.Length];
                fs.Read(myData, 0, (int)fs.Length);
                fs.Close();
            }
            return myData;
        }


        private string[] FuncOpenDialogToAddFilesExecution()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();

            if (dlg.ShowDialog() != true)
                return new string[0];

            return dlg.FileNames;
        }
        #endregion
    }
}
