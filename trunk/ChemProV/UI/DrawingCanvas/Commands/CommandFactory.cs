/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Windows;
using System.Windows.Controls;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.StickyNote;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.UI.DrawingCanvas.Commands.ProcessUnit;
using ChemProV.UI.DrawingCanvas.Commands.RightClickMenu;
using ChemProV.UI.DrawingCanvas.Commands.StickyNoteCommand;
using ChemProV.UI.DrawingCanvas.Commands.Stream;
using ChemProV.UI.DrawingCanvas.Commands.Stream.PropertiesWindow;

namespace ChemProV.UI.DrawingCanvas.Commands
{
    /// <summary>
    /// This contains all possible commands,for process units movehead and movetail do the same thing.
    /// </summary>
    public enum CanvasCommands
    {
        AddToCanvas,
        MoveHead,
        MoveTail,
        RemoveFromCanvas,
        Resize
    }

    public class CommandFactory
    {
        public static ICommand CreateCommand(CanvasCommands command, object sender, Panel canvas, Point location = new Point(), Point lastLocation = new Point())
        {
            ICommand icommand = NullCommand.GetInstance();

            if (lastLocation.X == 0 && lastLocation.Y == 0)
            {
                lastLocation = new Point(-1, -1);
            }

            if (sender is IStream)
            {
                switch (command)
                {
                    case CanvasCommands.AddToCanvas:
                        {
                            icommand = AddStreamToCanvasCommand.GetInstance();
                            AddStreamToCanvasCommand cmd = icommand as AddStreamToCanvasCommand;
                            cmd.Canvas = canvas;
                            cmd.NewIStream = sender as IStream;
                            cmd.Location = location;
                        }
                        break;
                    case CanvasCommands.MoveHead:
                        {
                            icommand = MoveStreamHeadCommand.GetInstance();
                            MoveStreamHeadCommand cmd = icommand as MoveStreamHeadCommand;
                            cmd.Canvas = canvas;
                            cmd.StreamToMove = sender as IStream;
                            cmd.CurrentMouseLocation = location;
                            cmd.PreviousMouseLocation = lastLocation;
                        }
                        break;
                    case CanvasCommands.MoveTail:
                        {
                            icommand = MoveStreamTailCommand.GetInstance();
                            MoveStreamTailCommand cmd = icommand as MoveStreamTailCommand;
                            cmd.Canvas = canvas;
                            cmd.StreamToMove = sender as IStream;
                            cmd.CurrentMouseLocation = location;
                            cmd.PreviousMouseLocation = lastLocation;
                        }
                        break;
                    case CanvasCommands.RemoveFromCanvas:
                        {
                            icommand = DeleteStreamFromCanvas.GetInstance();
                            DeleteStreamFromCanvas cmd = icommand as DeleteStreamFromCanvas;
                            cmd.Canvas = canvas;
                            cmd.RemovingiStream = sender as IStream;
                            cmd.Location = location;
                        }
                        break;
                    default:
                        {
                            icommand = NullCommand.GetInstance();
                            break;
                        }
                }
            }
            else if (sender is IProcessUnit)
            {
                IProcessUnit pu = sender as IProcessUnit;
                switch (command)
                {
                    case CanvasCommands.AddToCanvas:
                        {
                            icommand = AddProcessUnitToCanvasCommand.GetInstance();
                            AddProcessUnitToCanvasCommand cmd = icommand as AddProcessUnitToCanvasCommand;
                            cmd.Canvas = canvas;
                            cmd.NewProcessUnit = pu;
                            cmd.Location = location;
                            break;
                        }
                    case CanvasCommands.MoveHead:
                        {
                            icommand = MoveProcessUnitCommand.GetInstance();
                            MoveProcessUnitCommand cmd = icommand as MoveProcessUnitCommand;
                            cmd.ProcessUnitToMove = pu;
                            cmd.CurrentMouseLocation = location;
                            cmd.PreviousMouseLocation = lastLocation;
                            cmd.Drawingcanvas = canvas as DrawingCanvas;
                            break;
                        }
                    case CanvasCommands.MoveTail:
                        {
                            icommand = MoveProcessUnitCommand.GetInstance();
                            MoveProcessUnitCommand cmd = icommand as MoveProcessUnitCommand;
                            cmd.ProcessUnitToMove = pu;
                            cmd.CurrentMouseLocation = location;
                            cmd.PreviousMouseLocation = lastLocation;
                            cmd.Drawingcanvas = canvas as DrawingCanvas;
                            break;
                        }
                    case CanvasCommands.RemoveFromCanvas:
                        {
                            icommand = RemoveProcessUnitFromCanvasCommand.GetInstance();
                            RemoveProcessUnitFromCanvasCommand cmd = icommand as RemoveProcessUnitFromCanvasCommand;
                            cmd.Canvas = canvas;
                            cmd.NewProcessUnit = pu;
                            break;
                        }
                    default:
                        icommand = NullCommand.GetInstance();
                        break;
                }
            }
            else if (sender is IPropertiesWindow)
            {
                switch (command)
                {
                    case CanvasCommands.AddToCanvas:
                        {
                            icommand = AddPropertiesWindowToCanvasCommand.GetInstance();
                            AddPropertiesWindowToCanvasCommand cmd = icommand as AddPropertiesWindowToCanvasCommand;
                            cmd.Canvas = canvas;
                            cmd.NewTable = sender as IPropertiesWindow;
                            cmd.Location = location;
                            break;
                        }
                    case CanvasCommands.MoveHead:
                        {
                            icommand = MovePropertiesWindowCommand.GetInstance();
                            MovePropertiesWindowCommand cmd = icommand as MovePropertiesWindowCommand;
                            cmd.TableToMove = sender as IPropertiesWindow;
                            cmd.CurrentMouseLocation = location;
                            cmd.PreviousMouseLocation = lastLocation;
                            cmd.Drawingcanvas = canvas as DrawingCanvas;
                            break;
                        }
                    case CanvasCommands.RemoveFromCanvas:
                        {
                            icommand = RemovePropertiesWindowFromCanvas.GetInstance();
                            RemovePropertiesWindowFromCanvas cmd = icommand as RemovePropertiesWindowFromCanvas;
                            cmd.Canvas = canvas;
                            cmd.RemovingTable = sender as IPropertiesWindow;
                            break;
                        }
                    default:
                        icommand = NullCommand.GetInstance();
                        break;
                }
            }
            else if (sender is StickyNote)
            {
                switch (command)
                {
                    case CanvasCommands.AddToCanvas:
                        {
                            icommand = AddStickyNoteToCanvasCommand.GetInstance();
                            AddStickyNoteToCanvasCommand cmd = icommand as AddStickyNoteToCanvasCommand;
                            cmd.Canvas = canvas;
                            cmd.NewStickyNote = sender as StickyNote;
                            cmd.Location = location;
                            break;
                        }
                    case CanvasCommands.MoveHead:
                        {
                            icommand = MoveStickyNoteCommand.GetInstance();
                            MoveStickyNoteCommand cmd = icommand as MoveStickyNoteCommand;
                            cmd.StickyNoteToMove = sender as StickyNote;
                            cmd.CurrentMouseLocation = location;
                            cmd.PreviousMouseLocation = lastLocation;
                            cmd.Drawingcanvas = canvas as DrawingCanvas;
                            break;
                        }
                    case CanvasCommands.RemoveFromCanvas:
                        {
                            icommand = RemoveStickyNoteFromCanvasCommand.GetInstance();
                            RemoveStickyNoteFromCanvasCommand cmd = icommand as RemoveStickyNoteFromCanvasCommand;
                            cmd.Canvas = canvas;
                            cmd.RemoveStickyNote = sender as StickyNote;
                            break;
                        }
                    case CanvasCommands.Resize:
                        {
                            icommand = ResizeStickNoteCommand.GetInstance();
                            ResizeStickNoteCommand cmd = icommand as ResizeStickNoteCommand;
                            cmd.Canvas = canvas;
                            cmd.Location = location;
                            cmd.ResizingStickyNote = sender as StickyNote;
                            break;
                        }
                    default:
                        icommand = NullCommand.GetInstance();
                        break;
                }
            }
            else if (sender is ContextMenu)
            {
                switch (command)
                {
                    case CanvasCommands.AddToCanvas:
                        {
                            icommand = AddContextMenuToCanvas.GetInstance();
                            AddContextMenuToCanvas cmd = icommand as AddContextMenuToCanvas;
                            cmd.Drawing_Canvas = canvas as DrawingCanvas;
                            cmd.NewContextMenu = sender as ContextMenu;
                            cmd.Location = location;
                            break;
                        }
                    case CanvasCommands.RemoveFromCanvas:
                        {
                            icommand = RemoveContextMenuFromCanvas.GetInstance();
                            RemoveContextMenuFromCanvas cmd = icommand as RemoveContextMenuFromCanvas;
                            cmd.Drawing_Canvas = canvas as DrawingCanvas;
                            cmd.ContextMenuToBeRemove = sender as ContextMenu;
                            cmd.Location = location;
                            break;
                        }
                }
            }
            else
            {
                icommand = NullCommand.GetInstance();
            }
            return icommand;
        }
    }
}