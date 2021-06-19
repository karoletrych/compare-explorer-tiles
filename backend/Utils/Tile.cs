using System;

namespace backend
{
    public class Tile
    {

        public int X { get; set; }
        public int Y { get; set; }
        static readonly double DIV = Math.Pow(2, 14);

        double Deg2Rad(double t)
        {
            return t * (Math.PI / 180);
        }

        public override bool Equals(object obj)
        {
            return obj is Tile tile &&
                   X == tile.X &&
                   Y == tile.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public Tile(Point point)
        {
            X = (int)(((point.lng + 180) / 360) * DIV);
            Y = (int)(((1 - Math.Log(Math.Tan(Deg2Rad(point.lat)) + 1 / Math.Cos(Deg2Rad(point.lat))) / Math.PI) / 2) * DIV);
        }
    }
}
