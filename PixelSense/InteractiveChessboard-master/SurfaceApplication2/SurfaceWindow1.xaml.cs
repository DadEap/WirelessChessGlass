using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using SurfaceApplication2.Models;

using InTheHand.Net.Bluetooth;
using InTheHand.Net.Ports;
using InTheHand.Net.Sockets;


namespace SurfaceApplication2
{
    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class SurfaceWindow1 : SurfaceWindow
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// 
        public delegate void ChosenHandler(int row, int column);
        public event ChosenHandler Chosen;
        private ChessBoard _chessBoard;
        private MoveValidator _validator;
        private WindowBoard _windowBoard;
        //private colorPiece _playerColor;
        private ChessEngine _engine;
        private Thread _thread;
        private bool _isCpuGame;

        private bool isMoving;

        private BluetoothManager _bluetoothManager;

        //Log contenant la majorité des événements. Met le jour le TextBlock à gauche de l'application
        private LogHandler _log;

        public SurfaceWindow1()
        {
            InitializeComponent();
            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
            InitializeDefinitions();
        }
        
        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        bool _windowBoard_Chosen(int fromRow, int fromColumn, int toRow, int toColumn)
        {
            byte fromSquare = (byte)(fromRow * 16 + fromColumn);
            byte toSquare = (byte)(toRow * 16 + toColumn);

            if (CancelMove(fromSquare) == true)
                return false;

            if (_validator.IsMoveIn(fromSquare, toSquare) == false)
                return false;

            _chessBoard = _chessBoard.Move(fromSquare, toSquare);
            
            _validator = new MoveValidator(_chessBoard);
            _validator.Validate();
            if (_chessBoard.IsBoardValid() == false)
            {
                _chessBoard = _chessBoard.LastBoard;
                _validator = new MoveValidator(_chessBoard);
                _validator.Validate();
                return false;
            }
            Evaluator.Evaluate(_chessBoard, _validator);

            UpdateWindow();

            if (_chessBoard.IsDraw() == true)
            {
                MessageBox.Show("Draw");
                _windowBoard.Freeze();
            }
            else if (_chessBoard.IsBlancMated == true || _chessBoard.IsNoirMated == true)
            {
                MessageBox.Show("Checkmate");
                _windowBoard.Freeze();
            }

            return true;
        }

        private bool CancelMove(byte fromSquare)
        {
            if (_chessBoard.Pieces[fromSquare].type == typePiece.Rien
                || _chessBoard.Pieces[fromSquare].color != _chessBoard.NowPlays)
                return true;

            return false;
        }

        private void UpdateWindow()
        {
            SetChessboardGrid();
            if (_isCpuGame == false && _chessBoard.LastBoard == null)
                undoButton1.IsEnabled = false;
            else if (_isCpuGame == true && (_chessBoard.LastBoard == null ||
                _chessBoard.LastBoard.LastBoard == null))
                undoButton1.IsEnabled = false;
            else
                undoButton1.IsEnabled = true;
        }

        private void SetChessboardGrid()
        {
            int i, j;

            _windowBoard.SetBoard(_chessBoard);
            ChessBoardGrid.Children.Clear();
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    ChessBoardGrid.Children.Add(_windowBoard.Pieces[i, j].DisplayControl);
                }
            }
        }

        private void undoButton1_TouchDown(object sender, TouchEventArgs e)
        {
            if (_isCpuGame == false)
                _chessBoard = _chessBoard.LastBoard;
            else
                _chessBoard = _chessBoard.LastBoard.LastBoard;
            _validator = new MoveValidator(_chessBoard);
            _validator.Validate();
            UpdateWindow();
        }

        private void newGameButton1_TouchDown(object sender, TouchEventArgs e)
        {
            //Window w = new SurfaceWindow2();
            //w.Show();
            _chessBoard = new ChessBoard();
            _windowBoard = new WindowBoard();
            _validator = new MoveValidator(_chessBoard);
            _validator.Validate();
            _windowBoard.Chosen += new WindowBoard.ChosenHandler(_windowBoard_Chosen_Cpu);
            _isCpuGame = true;
            UpdateWindow();
            _engine = new ChessEngine(4);

            _log = new LogHandler();

            grid1.Visibility = Visibility.Visible;
            isMoving = false;
            //_bluetoothManager = new BluetoothManager();
            //_bluetoothManager.DoEverything();
        }
        

        private void CpuMove()
        { 
            Action action = new Action(delegate()
            {
                bool finished = false;

                Action refreshAction = new Action(delegate()
                {
                    SearchProgressBar.Value++;
                });

                _engine.ProgressChanged += delegate()
                {
                    Dispatcher.BeginInvoke(refreshAction);
                };

                _chessBoard = _engine.CpuMove(_chessBoard, _validator);

                _validator = new MoveValidator(_chessBoard, true);
                _validator.Validate();

                //Mise à jour du label affichant le coup joué
                byte fromSquare = _chessBoard.CpuMovedFromSquare;
                byte toSquare = _chessBoard.CpuMovedToSquare;

                EventLabel.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        EventLabel.Content = getMoveStr(fromSquare, toSquare);
                        _log.AddString("[Noir] " + _chessBoard.Pieces[toSquare].type+"_"+getMoveStr(_chessBoard.MovedFromSquare, _chessBoard.MovedToSquare));
                       
                        EventTextBlock.Text = _log.getFullLog();
                    }
                 ));


                if (_chessBoard.IsDraw() == true)
                {
                    MessageBox.Show("Draw");
                    finished = true;
                }
                else if (_chessBoard.IsBlancMated == true || _chessBoard.IsNoirMated == true)
                {
                    MessageBox.Show("Checkmate");
                    finished = true;
                }

                Action act = new Action(delegate()
                {
                    UpdateWindow();
                    if (finished == true)
                        _windowBoard.Freeze();
                });
                Dispatcher.BeginInvoke(act);



                _engine.RemoveHandler();

            });

            SearchProgressBar.Maximum = _validator.ValidMoves.Count;
            SearchProgressBar.Visibility = System.Windows.Visibility.Visible;
            SearchProgressBar.Value = 0;
            _windowBoard.Freeze();
            _thread = new Thread(new ThreadStart(action));
            _thread.Start();
        }

        bool _windowBoard_Chosen_Cpu(int fromRow, int fromColumn, int toRow, int toColumn)
        {
            byte fromSquare = (byte)(fromRow * 16 + fromColumn);
            byte toSquare = (byte)(toRow * 16 + toColumn);

            _chessBoard.MovedFromSquare = fromSquare;
            _chessBoard.MovedToSquare = toSquare;
            _log.AddString("[Blanc] " + _chessBoard.Pieces[toSquare].type+"_"+getMoveStr(_chessBoard.MovedFromSquare, _chessBoard.MovedToSquare));
            EventTextBlock.Text = _log.getFullLog();

            if (CancelMove(fromSquare) == true)
                return false;

            if (_validator.IsMoveIn(fromSquare, toSquare) == false)
                return false;

            _chessBoard = _chessBoard.Move(fromSquare, toSquare);
            _validator = new MoveValidator(_chessBoard);
            _validator.Validate();
            if (_chessBoard.IsBoardValid() == false)
            {
                _chessBoard = _chessBoard.LastBoard;
                _validator = new MoveValidator(_chessBoard);
                _validator.Validate();
                return false;
            }
            Evaluator.Evaluate(_chessBoard, _validator);

            UpdateWindow();

            if (_chessBoard.IsDraw() == true)
            {
                MessageBox.Show("Draw");
                _windowBoard.Freeze();
                return true;
            }
            else if (_chessBoard.IsBlancMated == true || _chessBoard.IsNoirMated == true)
            {
                MessageBox.Show("Checkmate");
                _windowBoard.Freeze();
                return true;
            }

            CpuMove();

            return true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_thread != null)
                _thread.Abort();
        }

        private void exitButton1_TouchDown(object sender, TouchEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //Traduit un byte correspond à un move en langage des "échecs" Ex : 19 -> D3
        public string getMoveFromByte(byte move)
        {
            Dictionary<int, string> columnsDictionary = new Dictionary<int, string>() {
                {0, "A"},
                {1, "B"},
                {2, "C"},
                {3, "D"},
                {4, "E"},
                {5, "F"},
                {6, "G"},
                {7, "H"}
            };
            string str = "";
            int row = move / 16 +1;
            int column = move % 16;

            columnsDictionary.TryGetValue(column, out str);

            return str = str + row;
        }

        //Traduit un move entier (from/to) en langage "échec"
        public string getMoveStr(byte from, byte to)
        {
            string fromStr = getMoveFromByte(from);
            string toStr = getMoveFromByte(to);

            return fromStr + toStr;
        }

        //Initialization des tags avec leurs valeurs
        private void InitializeDefinitions()
        {
            for (byte k = 0; k <= 15; k++)
            {
                TagVisualizationDefinition tagDef =
                    new TagVisualizationDefinition();
                // The tag value that this definition will respond to.
                tagDef.Value = k;
                // The .xaml file for the UI
                tagDef.Source =
                    new Uri("CameraVisualization.xaml", UriKind.Relative);
                // The maximum number for this tag value.
                tagDef.MaxCount = 1;
                // The visualization stays for 1 seconds.
                tagDef.LostTagTimeout = 1000.0;
                // Orientation offset (default).
                //tagDef.OrientationOffsetFromTag = 0.0;
                // Physical offset (horizontal inches, vertical inches).
                //tagDef.PhysicalCenterOffsetFromTag = new Vector(2.0, 2.0);
                // Tag removal behavior (default).
                tagDef.TagRemovedBehavior = TagRemovedBehavior.Wait;
                // Orient UI to tag? (default).
                tagDef.UsesTagOrientation = true;
                // Add the definition to the collection.
                TagVisualizerA1.Definitions.Add(tagDef);
                TagVisualizerA2.Definitions.Add(tagDef);
                TagVisualizerA3.Definitions.Add(tagDef);
                TagVisualizerA4.Definitions.Add(tagDef);
                TagVisualizerA5.Definitions.Add(tagDef);
                TagVisualizerA6.Definitions.Add(tagDef);
                TagVisualizerA7.Definitions.Add(tagDef);
                TagVisualizerA8.Definitions.Add(tagDef);

                TagVisualizerB1.Definitions.Add(tagDef);
                TagVisualizerB2.Definitions.Add(tagDef);
                TagVisualizerB3.Definitions.Add(tagDef);
                TagVisualizerB4.Definitions.Add(tagDef);
                TagVisualizerB5.Definitions.Add(tagDef);
                TagVisualizerB6.Definitions.Add(tagDef);
                TagVisualizerB7.Definitions.Add(tagDef);
                TagVisualizerB8.Definitions.Add(tagDef);

                TagVisualizerC1.Definitions.Add(tagDef);
                TagVisualizerC2.Definitions.Add(tagDef);
                TagVisualizerC3.Definitions.Add(tagDef);
                TagVisualizerC4.Definitions.Add(tagDef);
                TagVisualizerC5.Definitions.Add(tagDef);
                TagVisualizerC6.Definitions.Add(tagDef);
                TagVisualizerC7.Definitions.Add(tagDef);
                TagVisualizerC8.Definitions.Add(tagDef);

                TagVisualizerD1.Definitions.Add(tagDef);
                TagVisualizerD2.Definitions.Add(tagDef);
                TagVisualizerD3.Definitions.Add(tagDef);
                TagVisualizerD4.Definitions.Add(tagDef);
                TagVisualizerD5.Definitions.Add(tagDef);
                TagVisualizerD6.Definitions.Add(tagDef);
                TagVisualizerD7.Definitions.Add(tagDef);
                TagVisualizerD8.Definitions.Add(tagDef);

                TagVisualizerE1.Definitions.Add(tagDef);
                TagVisualizerE2.Definitions.Add(tagDef);
                TagVisualizerE3.Definitions.Add(tagDef);
                TagVisualizerE4.Definitions.Add(tagDef);
                TagVisualizerE5.Definitions.Add(tagDef);
                TagVisualizerE6.Definitions.Add(tagDef);
                TagVisualizerE7.Definitions.Add(tagDef);
                TagVisualizerE8.Definitions.Add(tagDef);

                TagVisualizerF1.Definitions.Add(tagDef);
                TagVisualizerF2.Definitions.Add(tagDef);
                TagVisualizerF3.Definitions.Add(tagDef);
                TagVisualizerF4.Definitions.Add(tagDef);
                TagVisualizerF5.Definitions.Add(tagDef);
                TagVisualizerF6.Definitions.Add(tagDef);
                TagVisualizerF7.Definitions.Add(tagDef);
                TagVisualizerF8.Definitions.Add(tagDef);

                TagVisualizerG1.Definitions.Add(tagDef);
                TagVisualizerG2.Definitions.Add(tagDef);
                TagVisualizerG3.Definitions.Add(tagDef);
                TagVisualizerG4.Definitions.Add(tagDef);
                TagVisualizerG5.Definitions.Add(tagDef);
                TagVisualizerG6.Definitions.Add(tagDef);
                TagVisualizerG7.Definitions.Add(tagDef);
                TagVisualizerG8.Definitions.Add(tagDef);

                TagVisualizerH1.Definitions.Add(tagDef);
                TagVisualizerH2.Definitions.Add(tagDef);
                TagVisualizerH3.Definitions.Add(tagDef);
                TagVisualizerH4.Definitions.Add(tagDef);
                TagVisualizerH5.Definitions.Add(tagDef);
                TagVisualizerH6.Definitions.Add(tagDef);
                TagVisualizerH7.Definitions.Add(tagDef);
                TagVisualizerH8.Definitions.Add(tagDef);
            }
        }

        //Evenement quand un tag est identifié
        //Prend en compte les 16 pièces
        private void OnVisualizationAddedA1(object sender, TagVisualizerEventArgs e)
        {
            CameraVisualization camera = (CameraVisualization)e.TagVisualization;
            TagVisualizer tag = (TagVisualizer)sender;

            string name = tag.Name;
            if (isMoving)
            {
                int column = Grid.GetColumn(tag);
                int row = Grid.GetRow(tag);
                _windowBoard.WindowBoard_Chosen(row, column);
                isMoving = false;
            }
            else
            {
                switch (camera.VisualizedTag.Value)
                {
                    case 1:
                        testValuePiece(tag, typePiece.Pion);
                        break;
                    case 2:
                        testValuePiece(tag, typePiece.Pion);
                        break;
                    case 3:
                        testValuePiece(tag, typePiece.Pion);
                        break;
                    case 4:
                        testValuePiece(tag, typePiece.Pion);
                        break;
                    case 5:
                        testValuePiece(tag, typePiece.Pion);
                        break;
                    case 6:
                        testValuePiece(tag, typePiece.Pion);
                        break;
                    case 7:
                        testValuePiece(tag, typePiece.Pion);
                        break;
                    case 8:
                        testValuePiece(tag, typePiece.Fou);
                        break;
                    case 9:
                        testValuePiece(tag, typePiece.Fou);
                        break;
                    case 10:
                        testValuePiece(tag, typePiece.Cavalier);
                        break;
                    case 11:
                        testValuePiece(tag, typePiece.Cavalier);
                        break;
                    case 12:
                        testValuePiece(tag, typePiece.Tour);
                        break;
                    case 13:
                        testValuePiece(tag, typePiece.Tour);
                        break;
                    case 14:
                        testValuePiece(tag, typePiece.Dame);
                        break;
                    case 15:
                        testValuePiece(tag, typePiece.Roi);
                        break;
                }
            }

            EventTextBlock.Text = _log.getFullLog();
        }

        //Association pièce Virtuelle / pièce Réelle
        private bool testValuePiece(TagVisualizer tag, typePiece typePiece)
        {
            int column = Grid.GetColumn(tag);
            int row = Grid.GetRow(tag);
            /*
            WindowPiece[,] pieces = _windowBoard.Pieces;

            WindowPiece piece = pieces[row, column];
            */
            Piece[] p = _chessBoard.Pieces;

            Piece piece = p[row * 16 + column];

            if (piece.type == typePiece)
            {
                byte position = (byte)(row*16 + column);
                _log.AddString(piece.type+"_"+getMoveFromByte(position)+" déposée.");
            }
            else
            {
                _log.AddString(piece.type+"_Mauvaise position.");
            }
            EventTextBlock.Text = _log.getFullLog();
            return false;
        }

        private void OnVisualizationRemoved(object sender, TagVisualizerEventArgs e)
        {
            TagVisualizer tag = (TagVisualizer)sender;
            int column = Grid.GetColumn(tag);
            int row = Grid.GetRow(tag);
            WindowPiece[,] pieces = _windowBoard.Pieces;
            WindowPiece piece = pieces[row, column];
            Piece[] p = _chessBoard.Pieces;
            //Piece[] lastPieces = _chessBoard.LastBoard.Pieces;
            
            Piece currentPiece = p[row * 16 + column];
            //Piece oldPiece = lastPieces[row * 16 + column];
            if (currentPiece.type != typePiece.Rien && currentPiece.color == colorPiece.Blanc)
            {
                _windowBoard.WindowBoard_Chosen(row, column);
                isMoving = true;
            }
            else
            {
                //_log.AddString(oldPiece.type+"_supprimé.");
                EventTextBlock.Text = _log.getFullLog();
            }
            
            
        }
    }
}