using System.Windows.Shapes;
using System.Windows;

namespace Snake
{
    public class Food
    {
        private Ellipse foodObject;
        private Point position;

        public Food(Ellipse food, Point position)
        {
            this.foodObject = food;
            this.Position = position;
        }

        public Ellipse FoodObject
        {
            get
            {
                return foodObject;
            }
            set
            {
                if (value != null)
                {
                    foodObject = value;
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
