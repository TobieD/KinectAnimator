using System;
using System.Collections.Generic;

namespace KinectTool.Redux
{
    public static class ReduxManager
    {
        private static readonly Stack<ICommand> UndoStack = new Stack<ICommand>();
        private static readonly Stack<ICommand> RedoStack = new Stack<ICommand>();

        /// <summary>
        /// When a new command is put in the Undo stack the old Redo commands get cleared
        /// </summary>
        /// <param name="command"></param>
        public static void InsertInUndoRedo(ICommand command)
        {
            UndoStack.Push(command);
            RedoStack.Clear();
        }

        public static void Undo()
        {
            if (UndoStack.Count == 0)
                return;

            var c = UndoStack.Pop();
            RedoStack.Push(c);
            c.UnExecute();

            Console.WriteLine("Undo {0}", c.ToString());
        }
        public static void Redo()
        {
            if (RedoStack.Count == 0)
                return;

            var c = RedoStack.Pop();
            UndoStack.Push(c);
            c.Execute();

            Console.WriteLine("Redo {0}", c.ToString());
        }
    }
}

