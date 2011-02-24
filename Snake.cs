using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Snake
{
    public class Snake
    {
        private static Snake instance;
        private Polyline body;
        private Ellipse head;
        private int size;

        private Snake(Brush bodyColor, Brush headColor, int thickness, int size)
        {
            Size = size;
            SnakeInitialize(bodyColor, headColor, thickness, Size);
        }

        public static Snake Instance(Brush bodyColor, Brush headColor, int thickness, int size)
        {
            instance = new Snake(bodyColor, headColor, thickness, size);
            return instance;
        }

        public Polyline Body
        {
            get
            {
                return body;
            }

        }

        public Ellipse Head
        {
            get
            {
                return head;
            }

        }

        public int Size
        {
            get
            {
                return size;
            }
            set
            {
                if (value > 0)
                {
                    size = value;
                }
            }
        }


        public void Move(Point newPosition)
        {
            body.Points.Remove(body.Points.First());
            body.Points.Add(newPosition);
        }

        public void IncreaseSize(int length)
        {
            size += length;
            for (int i = 0; i < length; i++)
            {
                body.Points.Add(new Point(body.Points.Last().X, body.Points.Last().Y));
            }
        }

        public void SnakeInitialize(Brush bodyColor, Brush headColor, int thickness, int size)
        {
            head = new Ellipse();
            head.Stroke = headColor;
            head.Fill = headColor;
            head.Width = thickness + thickness / 3;
            head.Height = thickness + thickness / 3;

            body = new Polyline();
            body.Stroke = bodyColor;
            body.StrokeThickness = thickness;
            PointCollection snakeBody = new PointCollection();
            for (int i = 0; i < size; i++)
            {
                Point snakePoint = new Point(10, 3 * i);
                snakeBody.Add(snakePoint);
            }
            body.Points = snakeBody;
        }

        public bool IsSelfCrossing()
        {
            if (size < 10)
                return false;
            Point headPoint = body.Points.ElementAt(size - 1);
            Point neckPoint = body.Points.ElementAt(size - 2);
            for (int i = 0; i < size - 3; i++)
            {
                if (LineIntersects(headPoint, neckPoint, 
                    body.Points.ElementAt(i), body.Points.ElementAt(i + 2)))
                {
                    return true;
                }
            }
            return false;
        }


        private bool LineIntersects(Point p1, Point p2, Point q1, Point q2)
        {
            return (Clockwise(p1, p2, q1) * Clockwise(p1, p2, q2) <= 0) &&
                (Clockwise(q1, q2, p1) * Clockwise(q1, q2, p2) <= 0);
        }


        private int Clockwise(Point p0, Point p1, Point p2)
        {
            const double epsilon = 1e-13;

            double dx1 = p1.X - p0.X;
            double dy1 = p1.Y - p0.Y;
            double dx2 = p2.X - p0.X;
            double dy2 = p2.Y - p0.Y;
            double d = dx1 * dy2 - dy1 * dx2;
            if (d > epsilon) return 1;
            if (d < -epsilon) return -1;
            if ((dx1 * dx2 < -epsilon) || (dy1 * dy2 < -epsilon)) return -1;
            if ((dx1 * dx1 + dy1 * dy1) < (dx2 * dx2 + dy2 * dy2) + epsilon) return 1;
            return 0;
        }

    }
}
