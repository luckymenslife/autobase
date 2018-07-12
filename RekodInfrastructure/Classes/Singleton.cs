using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rekod.Classes
{
    /// generic Singleton<T> (потокобезопасный с использованием generic-класса и с отложенной инициализацией)
    /// <typeparam name="T">Singleton class</typeparam>
    public class Singleton<T> where T : class
    {
        /// Защищённый конструктор необходим для того, чтобы предотвратить создание экземпляра класса Singleton. 
        /// Он будет вызван из закрытого конструктора наследственного класса.
        protected Singleton() { }

        /// Фабрика используется для отложенной инициализации экземпляра класса
        private sealed class SingletonCreator<S> where S : class
        {
            //Используется Reflection для создания экземпляра класса без публичного конструктора
            private static readonly S method = (S)typeof(S).GetConstructor(
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new Type[0],
                        new ParameterModifier[0]).Invoke(null);

            public static S CreatorInstance
            {
                get { return method; }
            }
        }

        public static T Method
        {
            get { return SingletonCreator<T>.CreatorInstance; }
        }
    }
}
