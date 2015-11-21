using System;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk
{
    public sealed class MyStrategy : IStrategy
    {
        static TileType[][] TilesXY;
        static int TickStop = -1;
        static int X = -1;
        static int Y = -1;
        static int[] PathX;
        static int[] PathY;
        static int n = 0;
        static double correctX = 0.5;
        static double correctY = 0.5;

        public double GetDistance(double X, double Y, double X1, double Y1)
        {
            return Math.Sqrt((X1 - X) * (X1 - X) + (Y1 - Y) * (Y1 - Y));
        }
        public double GetDistance(Unit U, double X, double Y)
        {
            return GetDistance(U.X, U.Y, X, Y);
        }
        public double GetDistance(double X, double Y, Unit U)
        {
            return GetDistance(U.X, U.Y, X, Y);
        }

        public double NormalizeAngle(double angle)
        {
            while (angle > Math.PI) angle -= 2.0 * Math.PI;
            while (angle < -Math.PI) angle += 2.0 * Math.PI;
            return angle;
        }

        public double GetAngleBetween(double x, double y, double x1, double y1, double angle)
        {
            double absoluteAngleTo = Math.Atan2(y1 - y, x1 - x);
            double relativeAngleTo = absoluteAngleTo - angle;
            return NormalizeAngle(relativeAngleTo);
        }

        public void GoTo(Unit self, Unit target, out double power, out double turn)
        {
            GoTo(self, target.X, target.Y, target.SpeedX, target.SpeedY, out power, out turn);
        }

        public void GoTo(Unit self, double tX, double tY, double tSpeedX, double tSpeedY, out double power, out double turn)
        {
            double Rx = self.X - tX;
            double Ry = self.Y - tY;
            double Vx = self.SpeedX - tSpeedX;
            double Vy = self.SpeedY - tSpeedY;

            double t = (Vx * Vx + Vy * Vy) / (Vx * Vx + Vy * Vy + Rx * Rx + Ry * Ry);
            double Ax = -Rx * (1 - t) - Vx * t;
            double Ay = -Ry * (1 - t) - Vy * t;

            power = Math.Cos(GetAngleBetween(0, 0, Ax, Ay, self.Angle))
                * Math.Sqrt(Ax * Ax + Ay * Ay);

            double Frw = GetAngleBetween(0, 0, Ax, Ay, self.Angle);
            double Bck = GetAngleBetween(0, 0, -Ax, -Ay, self.Angle);

            if (Math.Abs(Frw) < Math.Abs(Bck) + Math.PI / 18.0D)
                turn = Frw;
            else
                turn = -Bck;
        }

        public bool isConnected(int X, int Y, Direction dir)
        {
            if (X < 0 || X >= TilesXY.Length) return false;
            if (Y < 0 || Y >= TilesXY[X].Length) return false;

            if (dir == Direction.Left && X == 0 ||
                dir == Direction.Right && X == TilesXY.Length - 1 ||
                dir == Direction.Up && Y == 0 ||
                dir == Direction.Down && Y == TilesXY[X].Length - 1) return false;

            if (TilesXY[X][Y] == TileType.Crossroads) return true;
            if (TilesXY[X][Y] == TileType.BottomHeadedT && dir != Direction.Up) return true;
            if (TilesXY[X][Y] == TileType.TopHeadedT && dir != Direction.Down) return true;
            if (TilesXY[X][Y] == TileType.LeftHeadedT && dir != Direction.Right) return true;
            if (TilesXY[X][Y] == TileType.RightHeadedT && dir != Direction.Left) return true;
            if (TilesXY[X][Y] == TileType.Horizontal && dir != Direction.Up && dir != Direction.Down) return true;
            if (TilesXY[X][Y] == TileType.Vertical && dir != Direction.Left && dir != Direction.Right) return true;
            if (TilesXY[X][Y] == TileType.LeftTopCorner && dir != Direction.Up && dir != Direction.Left) return true;
            if (TilesXY[X][Y] == TileType.LeftBottomCorner && dir != Direction.Down && dir != Direction.Left) return true;
            if (TilesXY[X][Y] == TileType.RightTopCorner && dir != Direction.Up && dir != Direction.Right) return true;
            if (TilesXY[X][Y] == TileType.RightBottomCorner && dir != Direction.Down && dir != Direction.Right) return true;

            return false;
        }

        public int ManhDistance(int X, int Y, int X1, int Y1)
        {
            return Math.Abs(X - X1) + Math.Abs(Y - Y1);
        }

        public void FindTargets(int X, int Y, int tagX, int tagY, out int[] TargetsX, out int[] TargetsY)
        {
            // Массив для поиска пути
            int step = -1;
            int[][] Tiles = new int[TilesXY.Length][];
            for (int i = TilesXY.Length - 1; i >= 0; --i)
            {
                Tiles[i] = new int[TilesXY[i].Length];
                for (int j = TilesXY[i].Length - 1; j >= 0; --j)
                    Tiles[i][j] = step;
            }

            // Прямое распространение волны поиска
            step++;
            Tiles[X][Y] = step;
            int minX = X;
            int minY = Y;
            bool next = true, target = false;
            while (next && !target)
            {
                next = false;
                for (int x = Tiles.Length - 1; x >= 0; --x)
                    for (int y = Tiles[x].Length - 1; y >= 0; --y)
                    {
                        if (Tiles[x][y] == step && isConnected(x, y, Direction.Up))
                            if (Tiles[x][y - 1] == -1)
                            {
                                if (ManhDistance(x, y - 1, tagX, tagY) < ManhDistance(minX, minY, tagX, tagY))
                                {
                                    minX = x;
                                    minY = y - 1;
                                }
                                Tiles[x][y - 1] = step + 1;
                                if (x == tagX && y - 1 == tagY) target = true;
                                next = true;
                            }
                        if (Tiles[x][y] == step && isConnected(x, y, Direction.Down))
                            if (Tiles[x][y + 1] == -1)
                            {
                                if (ManhDistance(x, y + 1, tagX, tagY) < ManhDistance(minX, minY, tagX, tagY))
                                {
                                    minX = x;
                                    minY = y + 1;
                                }
                                Tiles[x][y + 1] = step + 1;
                                if (x == tagX && y + 1 == tagY) target = true;
                                next = true;
                            }
                        if (Tiles[x][y] == step && isConnected(x, y, Direction.Left))
                            if (Tiles[x - 1][y] == -1)
                            {
                                if (ManhDistance(x - 1, y, tagX, tagY) < ManhDistance(minX, minY, tagX, tagY))
                                {
                                    minX = x - 1;
                                    minY = y;
                                }
                                Tiles[x - 1][y] = step + 1;
                                if (x - 1 == tagX && y == tagY) target = true;
                                next = true;
                            }
                        if (Tiles[x][y] == step && isConnected(x, y, Direction.Right))
                            if (Tiles[x + 1][y] == -1)
                            {
                                if (ManhDistance(x + 1, y, tagX, tagY) < ManhDistance(minX, minY, tagX, tagY))
                                {
                                    minX = x + 1;
                                    minY = y;
                                }
                                Tiles[x + 1][y] = step + 1;
                                if (x + 1 == tagX && y == tagY) target = true;
                                next = true;
                            }
                    }
                step++;
            }

            // Обратное распространение волны поиска
            TargetsX = new int[step];
            TargetsY = new int[step];
            step--;
            while (step >= 0)
            {
                TargetsX[step] = minX;
                TargetsY[step] = minY;

                if (isConnected(minX, minY, Direction.Up) && Tiles[minX][minY - 1] == step) minY--;
                else
                    if (isConnected(minX, minY, Direction.Down) && Tiles[minX][minY + 1] == step) minY++;
                    else
                        if (isConnected(minX, minY, Direction.Left) && Tiles[minX - 1][minY] == step) minX--;
                        else
                            if (isConnected(minX, minY, Direction.Right) && Tiles[minX + 1][minY] == step) minX++;
                step--;
            }
        }

        public void Move(Car self, World world, Game game, Move move)
        {
            if (world.Tick == 0)
            {
                // Инициализация
                TilesXY = new TileType[world.TilesXY.Length][];
                for (int t = world.TilesXY.Length - 1; t >= 0; --t)
                {
                    TilesXY[t] = new TileType[world.TilesXY[t].Length];
                    Array.Copy(world.TilesXY[t], TilesXY[t], world.TilesXY[t].Length);
                }
            }

            // Предстартовое повышение мощности двигателя на холостых оборотах
            if (world.Tick <= game.InitialFreezeDurationTicks)
            {
                move.EnginePower = 1.0D;
                return;
            }

            // Координаты машины в таблице тайлов
            int x = (int)(self.X / game.TrackTileSize);
            int y = (int)(self.Y / game.TrackTileSize);

            // Перерасчет пути
            if (X != x || Y != y)
            {
                X = x;
                Y = y;

                int[][] LocalPathX = new int[3][];
                int[][] LocalPathY = new int[3][];

                // Путь между машиной и ближайшей путевой точкой
                FindTargets(
                    X, Y,
                    world.Waypoints[self.NextWaypointIndex][0],
                    world.Waypoints[self.NextWaypointIndex][1],
                    out LocalPathX[0], out LocalPathY[0]);

                // Путь между очередной путевой точкой и следующей путевой точкой
                for (int i = 0; i < LocalPathX.Length - 1; ++i)
                    FindTargets(
                        world.Waypoints[(self.NextWaypointIndex + i) % world.Waypoints.Length][0],
                        world.Waypoints[(self.NextWaypointIndex + i) % world.Waypoints.Length][1],
                        world.Waypoints[(self.NextWaypointIndex + i + 1) % world.Waypoints.Length][0],
                        world.Waypoints[(self.NextWaypointIndex + i + 1) % world.Waypoints.Length][1],
                        out LocalPathX[i + 1], out LocalPathY[i + 1]);

                // Путь от машины через промежуточные путевые точки к следующей путевой точке
                int Len = 0;
                for (int i = 0; i <= LocalPathX.Length - 1; ++i)
                    Len += LocalPathX[i].Length;

                PathX = new int[Len];
                PathY = new int[Len];

                Len = 0;
                for (int i = 0; i <= LocalPathX.Length - 1; ++i)
                {
                    for (int j = LocalPathX[i].Length - 1; j >= 0; --j)
                    {
                        PathX[Len + j] = LocalPathX[i][j];
                        PathY[Len + j] = LocalPathY[i][j];
                    }
                    Len += LocalPathX[i].Length;
                }

                // Срезание углов на поворотах
                correctX = 0.5;
                correctY = 0.5;
                // Определение длины прямолинейного участка пути
                n = 0;
                if (PathX[n] == X)
                {
                    int dY = PathY[n] - Y;
                    // Отрезок по вертикали
                    while (PathX[n] == X && n < PathX.Length - 1)
                        if (PathX[n + 1] == X && dY == PathY[n + 1] - PathY[n]) n++; else break;
                    // Срезание углов на поворотах
                    if (n < PathX.Length - 1)
                    {
                        if (PathX[n + 1] < PathX[n]) correctX = 0.2;
                        else correctX = 0.8;
                    }
                }
                else if (PathY[n] == Y)
                {
                    int dX = PathX[n] - X;
                    // Отрезок по горизонтали
                    while (PathY[n] == Y && n < PathY.Length - 1)
                        if (PathY[n + 1] == Y && dX == PathX[n + 1] - PathX[n]) n++; else break;
                    // Срезание углов на поворотах
                    if (n < PathX.Length - 1)
                    {
                        if (PathY[n + 1] < PathY[n]) correctY = 0.2;
                        else correctY = 0.8;
                    }
                }
            }

            // Прицельная точка - ближайший поворот
            double nextX = (PathX[n] + correctX) * game.TrackTileSize;
            double nextY = (PathY[n] + correctY) * game.TrackTileSize;

            // Расчет положения руля и педали газа - цель nextX, nextY
            double power, turn;
            GoTo(self, nextX, nextY, 0, 0, out power, out turn);
            move.EnginePower = power;
            move.WheelTurn = turn;

            // Подбор попутных бонусов
            bool bonus = false;
            foreach (Bonus b in world.Bonuses)
            {
                bool OnPath = false;
                for (int i = 0; i < n; i++)
                    if ((int)(b.X / game.TrackTileSize) == PathX[i] &&
                        (int)(b.Y / game.TrackTileSize) == PathY[i])
                        OnPath = true;

                double angle = GetAngleBetween(self.X, self.Y, b.X, b.Y, self.Angle);
                angle += GetAngleBetween(0, 0, self.SpeedX, self.SpeedY, self.Angle);

                if (Math.Abs(angle) < Math.PI / 12.0 &&
                    self.GetDistanceTo(b) < (n - 1) * game.TrackTileSize &&
                    OnPath)
                {
                    if (GetDistance(self, nextX, nextY) > GetDistance(self, b.X, b.Y))
                    {
                        nextX = b.X;
                        nextY = b.Y;
                        GoTo(self, nextX, nextY, 0, 0, out power, out turn);
                        move.EnginePower = power;
                        move.WheelTurn = turn;
                        bonus = true;
                    }
                }
            }

            // Торможение при приближении к повороту
            if (!bonus)
                if (GetDistance(self.SpeedX, self.SpeedY, 0, 0) * 33 > self.GetDistanceTo(nextX, nextY))
                {
                    if (Math.Abs(GetAngleBetween(0, 0, self.SpeedX, self.SpeedY, self.Angle)) < Math.PI / 2 &&
                        move.EnginePower > 0)
                        move.IsBrake = true;
                    if (Math.Abs(GetAngleBetween(0, 0, self.SpeedX, self.SpeedY, self.Angle)) > Math.PI / 2 &&
                        move.EnginePower < 0)
                        move.IsBrake = true;
                    move.EnginePower = -move.EnginePower;
                }

            // Взаимодействие с соперниками
            foreach (Car c in world.Cars)
                if (!c.IsTeammate)
                {
                    if (Math.Abs(GetAngleBetween(self.X, self.Y, c.X, c.Y, self.Angle)) < Math.PI / 90.0 &&
                        self.GetDistanceTo(c) < 2400)
                        move.IsThrowProjectile = true;
                    if (Math.Abs(GetAngleBetween(self.X, self.Y, c.X, c.Y, Math.PI + self.Angle)) < Math.PI / 90.0 &&
                        self.GetDistanceTo(c) < 2400)
                        if (Math.Abs(self.AngularSpeed) > 0.005)
                            move.IsSpillOil = true;
                }

            // Если скорость меньше 1, то засекаем 60 тиков.
            // Если скорость не изменилась за это время, то 60 тиков выруливать из тупика
            // Засекаем 60 тиков перед включением режима выруливания
            if (GetDistance(self.SpeedX, self.SpeedY, 0, 0) < 1 && TickStop == -1)
                TickStop = world.Tick;
            // Отмена режима выруливания, если скорость поднялась или вышло время работы режима
            if (GetDistance(self.SpeedX, self.SpeedY, 0, 0) > 1 && (world.Tick - TickStop <= 60) ||
                (TickStop != -1 && world.Tick - TickStop > 160) ||
                GetDistance(self.SpeedX, self.SpeedY, 0, 0) > 10)
                TickStop = -1;
            // Режим выруливания 100 тиков
            if (TickStop != -1 && world.Tick - TickStop > 60 && world.Tick - TickStop <= 160)
            {
                GoTo(self, 2 * self.X - nextX, 2 * self.Y - nextY, 0, 0, out power, out turn);
                move.EnginePower = power;
                move.WheelTurn = turn;
            }
            if (TickStop != -1 && world.Tick - TickStop > 140 && world.Tick - TickStop <= 160)
            {
                GoTo(self, nextX, nextY, 0, 0, out power, out turn);
                move.EnginePower = power;
            }

            // Ускоряться
            if (GetDistance(self.SpeedX, self.SpeedY, 0, 0) > 1 && GetDistance(self.SpeedX, self.SpeedY, 0, 0) < 20)
                if (n > 5 && Math.Abs(GetAngleBetween(0, 0, self.SpeedX, self.SpeedY, self.Angle)) < Math.PI / 8.0D)
                    move.IsUseNitro = true;
        }
    }
}