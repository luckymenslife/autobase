using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace cti
{
    static class ThreadProgress
    {
        static readonly SynchronizationContext uiContext
            = SynchronizationContext.Current;                   // Ссылки на основной поток
        static readonly List<string> listProcess = new List<string>();   // Коллекция операций
        //static Thread ThreadForm;                               // Поток с открытым окном
        static Thread threadFormClose;
        static ThreadForm threadForm { get; set; }

        /// <summary> Функция открытия операции загрузки
        /// </summary>
        /// <param name="key">Ключ операции (по этому ключу открытвается и закрывается операция загрузки)</param>
        public static void ShowWait(string key = "")
        {
            uiContext.Send(d =>
            {
                Debug.WriteLine("Открытие процесса: " + key);
                if (!listProcess.Contains(key))
                    listProcess.Add(key);
                VisableForm(listProcess.Count);

            }, null);
        }

        public static string Open(string prefix = "")
        {
            string key = prefix + Guid.NewGuid();
            ShowWait(key);
            return key;
        }

        /// <summary> Функция закрытия операции загрузки
        /// </summary>
        /// <param name="key">Ключ операции (по этому ключу открытвается и закрывается операция загрузки)</param>
        public static void Close(string key = "")
        {
            //if (_timerIsRunning)
            //{
            //    _timer.Stop();
            //    _timerIsRunning = false;
            //}
            uiContext.Send(d =>
            {
                listProcess.Remove(key);
                VisableForm(listProcess.Count);
                Debug.WriteLine("Закрытие процесса: " + key);
            }, null);
        }
        static public void SetText(string txt)
        {
            uiContext.Send(d =>
            {
                if (listProcess.Count == 0 || threadForm == null) return;   //Если есть операции или поток уже закрыт, то отмена
                threadForm.SetText(txt);
            }, null);
            
            //_currentText = txt;
            //if (!_timerIsRunning)
            //{
            //    _timerIsRunning = true;
            //    _timer.Start();
            //}
        }

        /// <summary> Функция выполняемая в отдельном потоке с открытием окна загрузки
        /// </summary>
        /// <param name="func"> Функция выполняемая в отдельном потоке</param>
        static public void FuncExecution(Action func)
        {
            string guid = Guid.NewGuid().ToString();
            ShowWait(guid);
            func.Invoke();
            Close(guid);
        }
        /// <summary> Процедура определяющий отобразить или скрыть окно загрузки
        /// </summary>
        /// <param name="countOperation">Колличество открытых операций</param>
        static void VisableForm(int countOperation)
        {
            if (countOperation == 0)
            {
                if (threadForm == null) return;
                if (threadFormClose != null)
                {
                    Thread tempThreadForm = threadFormClose;
                    threadFormClose = null;
                    tempThreadForm.Abort();
                }
                threadFormClose = new Thread(ThreadCloseForm);
                threadFormClose.Start();
            }
            else
            {
                if (threadForm != null) return;
                threadForm = new ThreadForm();
            }
        }
        /// <summary> Поток закрытия окна
        /// </summary>
        static void ThreadCloseForm()
        {
            Thread.Sleep(500);     //Задержка перед закрытием окна
            uiContext.Send(d =>
            {
                if (listProcess.Count != 0 || threadForm == null) return;   //Если есть операции или поток уже закрыт, то отмена
                ThreadForm tempThreadForm = threadForm;
                threadForm = null;
                tempThreadForm.CloseForm();
                threadFormClose = null;
            }, null);
        }




        //static ThreadProgress()
        //{
        //    _timer = new System.Windows.Forms.Timer();
        //    _timer.Interval = 150;
        //    _timer.Tick += _timer_Tick;
        //}
        //static void _timer_Tick(object sender, EventArgs e)
        //{
        //    if (_currentText != null)
        //    {
        //        uiContext.Send(d =>
        //        {
        //            if (listProcess.Count == 0 || threadForm == null) return;   //Если есть операции или поток уже закрыт, то отмена
        //            threadForm.SetText(_currentText);
        //        }, null);
        //        _currentText = null;
        //    }
        //}
        //private static String _currentText = null;
        //private static bool _timerIsRunning = false;
        //private static System.Windows.Forms.Timer _timer; 
    }

    class ThreadForm
    {
        public Thread thread { get; set; }

        public ProgressForm frm { get; set; }

        public ThreadForm()
        {
            thread = new Thread(ThreadShowForm);
            thread.Start();
        }

        /// <summary> Поток открытия окна
        /// </summary>
        void ThreadShowForm()
        {
            //Создаем новую форму
            frm = new ProgressForm();
            frm.Activate();
            frm.ShowDialog();
        }

        public void CloseForm()
        {
            frm.InThread(() => frm.Close());
        }

        public void SetText(string txt)
        {
            if (frm == null)
                Thread.Sleep(500);     //Задержка перед закрытием окна
            frm.InThread(() => frm.procLable.Text = txt);
        }
    }
}