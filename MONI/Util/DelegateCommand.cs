using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace MONI.Util {
    /// <summary>
    ///     This class allows delegating the commanding logic to methods passed as parameters,
    ///     and enables a View to bind commands to objects that are not part of the element tree.
    /// </summary>
    public class DelegateCommand : ICommand {
        private readonly Func<bool> canExecuteMethod;
        private readonly Action executeMethod;
        private List<WeakReference> canExecuteChangedHandlers;

        /// <summary>
        /// ctor for command that can always be executed. executes given action
        /// </summary>
        /// <param name="executeMethod">action to execute</param>
        public DelegateCommand(Action executeMethod)
            : this(executeMethod, null) {
        }

        /// <summary>
        /// ctor for command that has canexecute func and automatic requery can be disabled and an accesscontrol token to be tested BEFORE canexecute is queried
        /// </summary>
        /// <param name="executeMethod">action to execute</param>
        /// <param name="canExecuteMethod">func to query for canexecute</param>
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod) {
            if (executeMethod == null) {
                throw new ArgumentNullException("executeMethod");
            }

            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

        #region ICommand Members

        /// <summary>
        ///     ICommand.CanExecuteChanged implementation
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                CommandManagerHelper.AddWeakReferenceHandler(ref this.canExecuteChangedHandlers, value, 2);
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
                CommandManagerHelper.RemoveWeakReferenceHandler(this.canExecuteChangedHandlers, value);
            }
        }

        bool ICommand.CanExecute(object parameter) {
            return this.CanExecute();
        }

        void ICommand.Execute(object parameter) {
            this.Execute();
        }

        #endregion

        /// <summary>
        ///     Method to determine if the command can be executed
        /// <remarks>if there is a accesscontrol token that is valid check for it</remarks>
        /// </summary>
        public bool CanExecute() {
            if (this.canExecuteMethod != null) {
                return this.canExecuteMethod();
            }
            return true;
        }

        /// <summary>
        ///     Execution of the command
        /// </summary>
        public void Execute() {
            if (this.executeMethod != null) {
                this.executeMethod();
            }
        }

        /// <summary>
        ///     Raises the CanExecuteChaged event
        /// </summary>
        public void RaiseCanExecuteChanged() {
            this.OnCanExecuteChanged();
        }

        /// <summary>
        ///     Protected virtual method to raise CanExecuteChanged event
        /// </summary>
        protected virtual void OnCanExecuteChanged() {
            CommandManagerHelper.CallWeakReferenceHandlers(this.canExecuteChangedHandlers);
        }
    }

    /// <summary>
    ///     This class contains methods for the CommandManager that help avoid memory leaks by
    ///     using weak references.
    /// </summary>
    internal class CommandManagerHelper {
        internal static void CallWeakReferenceHandlers(List<WeakReference> handlers) {
            if (handlers != null) {
                // Take a snapshot of the handlers before we call out to them since the handlers
                // could cause the array to me modified while we are reading it.

                var callees = new EventHandler[handlers.Count];
                int count = 0;

                for (int i = handlers.Count - 1; i >= 0; i--) {
                    WeakReference reference = handlers[i];
                    var handler = reference.Target as EventHandler;
                    if (handler == null) {
                        // Clean up old handlers that have been collected
                        handlers.RemoveAt(i);
                    }
                    else {
                        callees[count] = handler;
                        count++;
                    }
                }

                // Call the handlers that we snapshotted
                for (int i = 0; i < count; i++) {
                    EventHandler handler = callees[i];
                    handler(null, EventArgs.Empty);
                }
            }
        }

        internal static void AddHandlersToRequerySuggested(List<WeakReference> handlers) {
            if (handlers != null) {
                foreach (WeakReference handlerRef in handlers) {
                    var handler = handlerRef.Target as EventHandler;
                    if (handler != null) {
                        CommandManager.RequerySuggested += handler;
                    }
                }
            }
        }

        internal static void RemoveHandlersFromRequerySuggested(List<WeakReference> handlers) {
            if (handlers != null) {
                foreach (WeakReference handlerRef in handlers) {
                    var handler = handlerRef.Target as EventHandler;
                    if (handler != null) {
                        CommandManager.RequerySuggested -= handler;
                    }
                }
            }
        }

        internal static void AddWeakReferenceHandler(ref List<WeakReference> handlers, EventHandler handler, int defaultListSize) {
            if (handlers == null) {
                handlers = (defaultListSize > 0 ? new List<WeakReference>(defaultListSize) : new List<WeakReference>());
            }

            handlers.Add(new WeakReference(handler));
        }

        internal static void RemoveWeakReferenceHandler(List<WeakReference> handlers, EventHandler handler) {
            if (handlers != null) {
                for (int i = handlers.Count - 1; i >= 0; i--) {
                    WeakReference reference = handlers[i];
                    var existingHandler = reference.Target as EventHandler;
                    if ((existingHandler == null) || (existingHandler == handler)) {
                        // Clean up old handlers that have been collected
                        // in addition to the handler that is to be removed.
                        handlers.RemoveAt(i);
                    }
                }
            }
        }
    }
}