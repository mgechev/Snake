using System.Windows.Shapes;
using System.Windows;


namespace Snake
{
    public class Obstacle
    {
        //for more flexibility using directly the interface UIElement
        private UIElement obstacle;
        private Point position;
        private double height;
        private double width;

        public Obstacle(UIElement line, int x, int y, Size size)
        {
            this.position.X = x;
            this.position.Y = y;
            this.obstacle = line;
            Height = size.Height;
            Width = size.Width;
        }

        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                if (value > 0)
                {
                    height = value;
                }
            }
        }

        public double Width
        {
            get
            {
                return width;
            }
            set
            {
                if (value > 0)
                {
                    width = value;
                }
            }
        }

        public UIElement ObstacleObject
        {
            get
            {
                return obstacle;
            }
            set
            {
                if (value != null)
                {
                    obstacle = value;
                }
            }
        }

        public Point Position
        {
            get
            {
                return position;
            }
            set
            {
                if (value != null)
                {
                    position = value;
                }
            }
        }
    }
}
