//using System;

//namespace Arcade.Games.Tetris
//{
//    public sealed class TetrisBotGame
//    {
//        public const int Rows = 20;
//        public const int Cols = 10;
//        private const int ShapeSize = 4;

//        private readonly int[,] _grid = new int[Rows, Cols];

//        public int[,] Grid => _grid;
//        public bool IsGameOver { get; private set; }
//        public bool IsPaused { get; set; }
//        public int Score { get; private set; }
//        public int LinesClearedTotal { get; private set; }
//        public int Level { get; private set; } = 1;
//        public bool AutoPlay { get; set; }

//        private TetrominoType _currentType;
//        private TetrominoType _nextType;
//        private int _currentRow;
//        private int _currentCol;
//        private int _currentRotation;
//        private int _pieceId;

//        private readonly Random _rng = new();

//        private BotPlan? _activePlan;

//        private sealed class BotPlan
//        {
//            public int PieceId { get; set; }
//            public int TargetColumn { get; set; }
//            public int TargetRotation { get; set; }
//        }

//        private static readonly int[,,] Shapes = InitShapes();

//        public TetrisBotGame()
//        {
//            Restart();
//        }

//        public void Restart()
//        {
//            Array.Clear(_grid, 0, _grid.Length);
//            IsGameOver = false;
//            IsPaused = false;
//            Score = 0;
//            LinesClearedTotal = 0;
//            Level = 1;
//            _pieceId = 0;
//            _activePlan = null;

//            _nextType = RandomTetromino();
//            SpawnNewPiece();
//        }

//        private TetrominoType RandomTetromino()
//            => (TetrominoType)_rng.Next(0, 7);

//        private void SpawnNewPiece()
//        {
//            _currentType = _nextType;
//            _nextType = RandomTetromino();

//            _currentRow = 0;
//            _currentCol = Cols / 2 - 2;
//            _currentRotation = 0;
//            _pieceId++;
//            _activePlan = null;

//            if (!CanPlace(_currentType, _currentRow, _currentCol, _currentRotation))
//                IsGameOver = true;
//        }

//        public int[,] GetNextShapeMatrix()
//        {
//            var shape = new int[ShapeSize, ShapeSize];
//            var src = GetShape(_nextType, 0);
//            for (int r = 0; r < ShapeSize; r++)
//            {
//                for (int c = 0; c < ShapeSize; c++)
//                {
//                    shape[r, c] = src[r, c];
//                }
//            }
//            return shape;
//        }

//        public void Tick()
//        {
//            if (IsGameOver || IsPaused) return;

//            if (AutoPlay)
//            {
//                EnsureBotPlan();
//                ApplyBotStep();
//            }

//            if (CanPlace(_currentType, _currentRow + 1, _currentCol, _currentRotation))
//            {
//                _currentRow++;
//            }
//            else
//            {
//                LockCurrentPiece();
//                int cleared = ClearFullLines();
//                UpdateScore(cleared);
//                SpawnNewPiece();
//            }
//        }

//        public void MoveLeft()
//        {
//            if (IsGameOver || IsPaused) return;
//            if (CanPlace(_currentType, _currentRow, _currentCol - 1, _currentRotation))
//            {
//                _currentCol--;
//                _activePlan = null;
//            }
//        }

//        public void MoveRight()
//        {
//            if (IsGameOver || IsPaused) return;
//            if (CanPlace(_currentType, _currentRow, _currentCol + 1, _currentRotation))
//            {
//                _currentCol++;
//                _activePlan = null;
//            }
//        }

//        public void SoftDrop()
//        {
//            if (IsGameOver || IsPaused) return;

//            if (CanPlace(_currentType, _currentRow + 1, _currentCol, _currentRotation))
//            {
//                _currentRow++;
//            }
//            else
//            {
//                LockCurrentPiece();
//                int cleared = ClearFullLines();
//                UpdateScore(cleared);
//                SpawnNewPiece();
//            }
//        }

//        public void Rotate()
//        {
//            if (IsGameOver || IsPaused) return;

//            int nextRot = (_currentRotation + 1) % 4;
//            if (CanPlace(_currentType, _currentRow, _currentCol, nextRot))
//            {
//                _currentRotation = nextRot;
//                _activePlan = null;
//            }
//        }

//        private void UpdateScore(int cleared)
//        {
//            if (cleared <= 0) return;

//            int points = cleared switch
//            {
//                1 => 40,
//                2 => 100,
//                3 => 300,
//                4 => 1200,
//                _ => cleared * 100
//            };

//            Score += points * Level;
//            LinesClearedTotal += cleared;
//            Level = 1 + LinesClearedTotal / 10;
//        }

//        private void LockCurrentPiece()
//        {
//            var shape = GetShape(_currentType, _currentRotation);
//            int color = GetIndex(_currentType) + 1;

//            for (int r = 0; r < ShapeSize; r++)
//            {
//                for (int c = 0; c < ShapeSize; c++)
//                {
//                    if (shape[r, c] == 0) continue;

//                    int br = _currentRow + r;
//                    int bc = _currentCol + c;

//                    if (br >= 0 && br < Rows && bc >= 0 && bc < Cols)
//                    {
//                        _grid[br, bc] = color;
//                    }
//                }
//            }
//        }

//        private int ClearFullLines()
//        {
//            int cleared = 0;

//            for (int r = Rows - 1; r >= 0; r--)
//            {
//                bool full = true;
//                for (int c = 0; c < Cols; c++)
//                {
//                    if (_grid[r, c] == 0)
//                    {
//                        full = false;
//                        break;
//                    }
//                }

//                if (!full) continue;

//                cleared++;
//                for (int rr = r; rr > 0; rr--)
//                {
//                    for (int cc = 0; cc < Cols; cc++)
//                    {
//                        _grid[rr, cc] = _grid[rr - 1, cc];
//                    }
//                }

//                for (int cc = 0; cc < Cols; cc++)
//                {
//                    _grid[0, cc] = 0;
//                }

//                r++;
//            }

//            return cleared;
//        }

//        private bool CanPlace(TetrominoType type, int row, int col, int rotation)
//        {
//            var shape = GetShape(type, rotation);

//            for (int r = 0; r < ShapeSize; r++)
//            {
//                for (int c = 0; c < ShapeSize; c++)
//                {
//                    if (shape[r, c] == 0) continue;

//                    int br = row + r;
//                    int bc = col + c;

//                    if (br < 0 || br >= Rows || bc < 0 || bc >= Cols)
//                        return false;

//                    if (_grid[br, bc] != 0)
//                        return false;
//                }
//            }

//            return true;
//        }

//        private int[,] GetShape(TetrominoType type, int rotation)
//        {
//            var shape = new int[ShapeSize, ShapeSize];
//            int idx = GetIndex(type);

//            for (int i = 0; i < ShapeSize * ShapeSize; i++)
//            {
//                int v = Shapes[idx, rotation, i];
//                int r = i / ShapeSize;
//                int c = i % ShapeSize;
//                shape[r, c] = v;
//            }

//            return shape;
//        }

//        private void EnsureBotPlan()
//        {
//            if (!AutoPlay) return;
//            if (_activePlan != null && _activePlan.PieceId == _pieceId) return;

//            _activePlan = FindBestPlan();
//        }

//        private void ApplyBotStep()
//        {
//            if (_activePlan == null || _activePlan.PieceId != _pieceId)
//                return;

//            if (_currentRotation != _activePlan.TargetRotation)
//            {
//                int nextRot = (_currentRotation + 1) % 4;
//                if (CanPlace(_currentType, _currentRow, _currentCol, nextRot))
//                {
//                    _currentRotation = nextRot;
//                }
//                else
//                {
//                    _activePlan = null;
//                }
//                return;
//            }

//            if (_currentCol < _activePlan.TargetColumn)
//            {
//                if (CanPlace(_currentType, _currentRow, _currentCol + 1, _currentRotation))
//                    _currentCol++;
//                else
//                    _activePlan = null;

//                return;
//            }

//            if (_currentCol > _activePlan.TargetColumn)
//            {
//                if (CanPlace(_currentType, _currentRow, _currentCol - 1, _currentRotation))
//                    _currentCol--;
//                else
//                    _activePlan = null;

//                return;
//            }
//        }

//        private BotPlan? FindBestPlan()
//        {
//            double bestScore = double.MinValue;
//            BotPlan? best = null;

//            var gridCopy = new int[Rows, Cols];

//            for (int rotation = 0; rotation < 4; rotation++)
//            {
//                var shape = GetShape(_currentType, rotation);

//                for (int col = -2; col < Cols + 2; col++)
//                {
//                    Array.Copy(_grid, gridCopy, _grid.Length);

//                    int row = 0;
//                    while (CanPlaceOnGrid(shape, row + 1, col, gridCopy))
//                    {
//                        row++;
//                    }

//                    if (!CanPlaceOnGrid(shape, row, col, gridCopy))
//                        continue;

//                    PlaceOnGrid(shape, row, col, gridCopy);
//                    int cleared = ClearFullLinesVirtual(gridCopy);
//                    double score = EvaluateGrid(gridCopy, cleared);

//                    if (score > bestScore)
//                    {
//                        bestScore = score;
//                        best = new BotPlan
//                        {
//                            PieceId = _pieceId,
//                            TargetColumn = col,
//                            TargetRotation = rotation
//                        };
//                    }
//                }
//            }

//            return best;
//        }

//        private static bool CanPlaceOnGrid(int[,] shape, int row, int col, int[,] grid)
//        {
//            int rows = grid.GetLength(0);
//            int cols = grid.GetLength(1);

//            for (int r = 0; r < ShapeSize; r++)
//            {
//                for (int c = 0; c < ShapeSize; c++)
//                {
//                    if (shape[r, c] == 0) continue;

//                    int br = row + r;
//                    int bc = col + c;

//                    if (br < 0 || br >= rows || bc < 0 || bc >= cols)
//                        return false;

//                    if (grid[br, bc] != 0)
//                        return false;
//                }
//            }

//            return true;
//        }

//        private static void PlaceOnGrid(int[,] shape, int row, int col, int[,] grid)
//        {
//            int rows = grid.GetLength(0);
//            int cols = grid.GetLength(1);

//            for (int r = 0; r < ShapeSize; r++)
//            {
//                for (int c = 0; c < ShapeSize; c++)
//                {
//                    if (shape[r, c] == 0) continue;

//                    int br = row + r;
//                    int bc = col + c;

//                    if (br >= 0 && br < rows && bc >= 0 && bc < cols)
//                    {
//                        grid[br, bc] = 8;
//                    }
//                }
//            }
//        }

//        private static int ClearFullLinesVirtual(int[,] grid)
//        {
//            int rows = grid.GetLength(0);
//            int cols = grid.GetLength(1);
//            int cleared = 0;

//            for (int r = rows - 1; r >= 0; r--)
//            {
//                bool full = true;
//                for (int c = 0; c < cols; c++)
//                {
//                    if (grid[r, c] == 0)
//                    {
//                        full = false;
//                        break;
//                    }
//                }

//                if (!full) continue;

//                cleared++;
//                for (int rr = r; rr > 0; rr--)
//                {
//                    for (int cc = 0; cc < cols; cc++)
//                    {
//                        grid[rr, cc] = grid[rr - 1, cc];
//                    }
//                }

//                for (int cc = 0; cc < cols; cc++)
//                {
//                    grid[0, cc] = 0;
//                }

//                r++;
//            }

//            return cleared;
//        }

//        private static double EvaluateGrid(int[,] grid, int linesCleared)
//        {
//            int rows = grid.GetLength(0);
//            int cols = grid.GetLength(1);

//            int[] heights = new int[cols];
//            int holes = 0;

//            for (int c = 0; c < cols; c++)
//            {
//                bool blockSeen = false;
//                for (int r = 0; r < rows; r++)
//                {
//                    if (grid[r, c] != 0)
//                    {
//                        if (!blockSeen)
//                        {
//                            blockSeen = true;
//                            heights[c] = rows - r;
//                        }
//                    }
//                    else if (blockSeen)
//                    {
//                        holes++;
//                    }
//                }
//            }

//            int maxHeight = 0;
//            int bumpiness = 0;

//            for (int c = 0; c < cols; c++)
//            {
//                if (heights[c] > maxHeight) maxHeight = heights[c];
//                if (c < cols - 1)
//                {
//                    bumpiness += Math.Abs(heights[c] - heights[c + 1]);
//                }
//            }

//            double score =
//                linesCleared * 8.0
//                - holes * 5.0
//                - maxHeight * 1.0
//                - bumpiness * 1.0;

//            return score;
//        }

//        private static int[,,] InitShapes()
//        {
//            var shapes = new int[7, 4, ShapeSize * ShapeSize];

//            void Set(TetrominoType type, int rot, params string[] rows)
//            {
//                int t = GetIndex(type);
//                for (int r = 0; r < ShapeSize; r++)
//                {
//                    for (int c = 0; c < ShapeSize; c++)
//                    {
//                        char ch = rows[r][c];
//                        int v = ch == 'X' ? t + 1 : 0;
//                        shapes[t, rot, r * ShapeSize + c] = v;
//                    }
//                }
//            }

//            Set(TetrominoType.I, 0,
//                "....",
//                "XXXX",
//                "....",
//                "....");
//            Set(TetrominoType.I, 1,
//                "..X.",
//                "..X.",
//                "..X.",
//                "..X.");
//            Set(TetrominoType.I, 2,
//                "....",
//                "....",
//                "XXXX",
//                "....");
//            Set(TetrominoType.I, 3,
//                ".X..",
//                ".X..",
//                ".X..",
//                ".X..");

//            Set(TetrominoType.O, 0,
//                ".XX.",
//                ".XX.",
//                "....",
//                "....");
//            Set(TetrominoType.O, 1,
//                ".XX.",
//                ".XX.",
//                "....",
//                "....");
//            Set(TetrominoType.O, 2,
//                ".XX.",
//                ".XX.",
//                "....",
//                "....");
//            Set(TetrominoType.O, 3,
//                ".XX.",
//                ".XX.",
//                "....",
//                "....");

//            Set(TetrominoType.T, 0,
//                ".X..",
//                "XXX.",
//                "....",
//                "....");
//            Set(TetrominoType.T, 1,
//                ".X..",
//                ".XX.",
//                ".X..",
//                "....");
//            Set(TetrominoType.T, 2,
//                "....",
//                "XXX.",
//                ".X..",
//                "....");
//            Set(TetrominoType.T, 3,
//                ".X..",
//                "XX..",
//                ".X..",
//                "....");

//            Set(TetrominoType.S, 0,
//                ".XX.",
//                "XX..",
//                "....",
//                "....");
//            Set(TetrominoType.S, 1,
//                ".X..",
//                ".XX.",
//                "..X.",
//                "....");
//            Set(TetrominoType.S, 2,
//                ".XX.",
//                "XX..",
//                "....",
//                "....");
//            Set(TetrominoType.S, 3,
//                ".X..",
//                ".XX.",
//                "..X.",
//                "....");

//            Set(TetrominoType.Z, 0,
//                "XX..",
//                ".XX.",
//                "....",
//                "....");
//            Set(TetrominoType.Z, 1,
//                "..X.",
//                ".XX.",
//                ".X..",
//                "....");
//            Set(TetrominoType.Z, 2,
//                "XX..",
//                ".XX.",
//                "....",
//                "....");
//            Set(TetrominoType.Z, 3,
//                "..X.",
//                ".XX.",
//                ".X..",
//                "....");

//            Set(TetrominoType.J, 0,
//                "X...",
//                "XXX.",
//                "....",
//                "....");
//            Set(TetrominoType.J, 1,
//                ".XX.",
//                ".X..",
//                ".X..",
//                "....");
//            Set(TetrominoType.J, 2,
//                "....",
//                "XXX.",
//                "..X.",
//                "....");
//            Set(TetrominoType.J, 3,
//                ".X..",
//                ".X..",
//                "XX..",
//                "....");

//            Set(TetrominoType.L, 0,
//                "..X.",
//                "XXX.",
//                "....",
//                "....");
//            Set(TetrominoType.L, 1,
//                ".X..",
//                ".X..",
//                ".XX.",
//                "....");
//            Set(TetrominoType.L, 2,
//                "....",
//                "XXX.",
//                "X...",
//                "....");
//            Set(TetrominoType.L, 3,
//                "XX..",
//                ".X..",
//                ".X..",
//                "....");

//            return shapes;
//        }

//        private static int GetIndex(TetrominoType type)
//        {
//            return type switch
//            {
//                TetrominoType.I => 0,
//                TetrominoType.O => 1,
//                TetrominoType.T => 2,
//                TetrominoType.S => 3,
//                TetrominoType.Z => 4,
//                TetrominoType.J => 5,
//                TetrominoType.L => 6,
//                _ => 0
//            };
//        }
//    }
//}
