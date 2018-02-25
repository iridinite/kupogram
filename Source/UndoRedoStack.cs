/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System.Collections.Generic;

namespace Kupogram {

    /// <summary>
    /// Represents a stack that records board transactions, to enable undo/redo functionality.
    /// </summary>
    internal sealed class UndoRedoStack {

        private readonly Stack<CellState[]> undo = new Stack<CellState[]>();
        private readonly Stack<CellState[]> redo = new Stack<CellState[]>();

        /// <summary>
        /// Returns true if the undo stack is not empty.
        /// </summary>
        public bool IsUndoAvailable => undo.Count > 0;

        /// <summary>
        /// Returns true if the redo stack is not empty.
        /// </summary>
        public bool IsRedoAvailable => redo.Count > 0;

        /// <summary>
        /// Records the state of the given board and pushes it on the undo stack.
        /// </summary>
        public void Push(Board board) {
            undo.Push((CellState[])board.State.Clone());
            redo.Clear();
        }

        /// <summary>
        /// Pushes the current state on the redo stack, then pops and applies a state from the undo stack.
        /// </summary>
        public void Undo(Board board) {
            redo.Push((CellState[])board.State.Clone());
            board.State = undo.Pop();
        }

        /// <summary>
        /// Pushes the current state on the undo stack, then pops and applies a state from the redo stack.
        /// </summary>
        /// <param name="board"></param>
        public void Redo(Board board) {
            undo.Push((CellState[])board.State.Clone());
            board.State = redo.Pop();
        }

        /// <summary>
        /// Clears the contents of both stacks.
        /// </summary>
        public void Clear() {
            undo.Clear();
            redo.Clear();
        }

    }

}
