using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Reflection;
using System.Windows;
using System.CodeDom.Compiler;
using System.Windows.Controls;

namespace Rekod.Behaviors
{
    /// <summary>
    /// Defines the command behavior binding
    /// </summary>
    public class CommandBehaviorBinding : IDisposable
    {
        #region Поля
        private Behaviors.CommandEventParameter _commandEventParameter;
        private Action<object> _action;
        #endregion Поля

        #region Свойства
        /// <summary>
        /// Get the owner of the CommandBinding ex: a Button
        /// This property can only be set from the BindEvent Method
        /// </summary>
        public DependencyObject Owner { get; private set; }
        /// <summary>
        /// The command to execute when the specified event is raised
        /// </summary>
        public ICommand Command { get; set; }
        /// <summary>
        /// Gets or sets a CommandParameter
        /// </summary>
        public object CommandParameter { get; set; }
        /// <summary>
        /// Gets or sets an ExtraParameter
        /// </summary>
        public object ExtraParameter { get; set; }
        /// <summary>
        /// The event name to hook up to
        /// This property can only be set from the BindEvent Method
        /// </summary>
        public string EventName { get; private set; }
        /// <summary>
        /// The event info of the event
        /// </summary>
        public EventInfo Event { get; private set; }
        /// <summary>
        /// Gets the EventHandler for the binding with the event
        /// </summary>
        public Delegate EventHandler { get; private set; }
        /// <summary>
        /// Gets or sets action associated with event (handler)
        /// </summary>
        public Action<object> Action
        {
            get { return _action; }
            set
            {
                _action = value;
            }
        }
        #endregion Свойства

        #region Методы
        //Creates an EventHandler on runtime and registers that handler to the Event specified
        public void BindEvent(DependencyObject owner, string eventName)
        {
            EventName = eventName;
            Owner = owner;

            if (EventName.Contains('.'))
            {
                UIElement uiOwner = owner as UIElement;
                if (uiOwner != null)
                {
                    String[] splitType = EventName.Split(new[] { '.' });
                    String typeName = "";
                    String fieldName = splitType[splitType.Length - 1];
                    for (int i = 0; i < splitType.Length - 1; i++)
                    {
                        typeName += (i > 0 ? "." : "") + splitType[i];
                    }
                    Type typeInfo = null; 
                    List<System.Reflection.Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                    foreach (var assembly in assemblies)
                    {
                        Type t = assembly.GetType(typeName, false);
                        if (t != null)
                        {
                            typeInfo = t; 
                        }
                    }
                    if (typeInfo == null)
                    {
                        throw new NotImplementedException(String.Format("Type {0} not found", typeName)); 
                    }
                    
                    FieldInfo someInfo = typeInfo.GetField(fieldName);                    
                    RoutedEvent rEvent = (RoutedEvent)someInfo.GetValue(null);
                    EventHandler = EventHandlerGenerator.CreateDelegate(
                    rEvent.HandlerType, typeof(CommandBehaviorBinding).GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance), this);
                    uiOwner.AddHandler(rEvent, EventHandler);
                }
            }
            else
            {   
                Event = Owner.GetType().GetEvent(EventName, BindingFlags.Public | BindingFlags.Instance);
                if (Event == null)
                    throw new InvalidOperationException(String.Format("Could not resolve event name {0}", EventName));

                //Create an event handler for the event that will call the ExecuteCommand method
                EventHandler = EventHandlerGenerator.CreateDelegate(
                    Event.EventHandlerType, typeof(CommandBehaviorBinding).GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance), this);
                //Register the handler to the Event

                Event.AddEventHandler(Owner, EventHandler);
            }
        }
        //public static Type GetTypeFromFullName(string fullClassName)
        //{
        //    AssemblyPartCollection parts = System.Deployment.Current.Parts;
        //    foreach (var part in parts)
        //    {
        //        Uri resUri = new Uri(part.Source, UriKind.Relative);
        //        Stream resStream = Application.GetResourceStream(resUri).Stream;
        //        Assembly resAssembly = part.Load(resStream);
        //        Type tryType = resAssembly.GetType(fullClassName, false);
        //        if (tryType != null)
        //            return tryType;
        //    }
        //    return null;
        //}


        /// <summary>
        /// Executes the command
        /// </summary>
        public void Execute(Object sender = null, Object eventargs = null)
        {
            CommandEventParameter commEventPar = new CommandEventParameter() 
            {
                 CommandParameter = this.CommandParameter, 
                 EventArgs = eventargs, 
                 EventSender = sender, 
                 ExtraParameter = ExtraParameter
            };
            if (Action != null)
            {
                Action(commEventPar); 
            }
            else if (Command != null)
            {
                Command.Execute(commEventPar);
            }
        }
        #endregion Методы

        #region IDisposable
        bool disposed = false;
        /// <summary>
        /// Unregisters the EventHandler from the Event
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                Event.RemoveEventHandler(Owner, EventHandler);
                disposed = true;
            }
        }
        #endregion
    }
}