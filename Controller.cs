using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading;

namespace Snake
{
    public enum SpeedLevel
    {
        VERY_SLOW = 80,
        SLOW = 70,
        MEDIUM = 60,
        FAST = 50,
        VERY_FAST = 40
    }

    public enum Orientation
    {
        HORIZONTAL,
        VERTICAL
    }

    public enum GameLevel
    {
        ROCKIE = 3,
        EASIEST = 6,
        EASY = 8,
        MEDIUM = 10,
        HARD = 12,
        HARDEST = 14
    }

    public class Controller
    {

        private delegate void CreateFood(ref Ellipse food, ref Food foodObject);
        private delegate void RemoveFood(Food foodObject);

        private GameLevel level;

        /* Field properties
         * * * * * * * * * */
        private Canvas playField;
        private Size playGroundSize;
        private PlayGround field;
        private Brush fieldColor;

        /* Snake properties
         * * * * * * * * * */
        private Snake snake;
        private int speed;
        private Thread snakeMovementThread;

        /* Game properties
         * * * * * * * * */
        private int score;
        private bool gameEnd;
        private bool paused;
        private int moveInterval;
        private bool mouseEnabled;
        private bool fullScreen;
        private Rectangle fade;

        /* Obstacles
         * * * * * * */
        private Brush obstacleColor;
        private List<Obstacle> obstacles;


        /* Food properties
         * * * * * * * * */
        private Dictionary<Food, int> foodCollection;
        private Size foodSize;
        private Thread foodThread;

        private bool keyPressed;
        private Key lastKey;

        /* Kind of builder pattern
         * * * * * * * * * * * * * * * * */

        public Controller()
        {
            //getting all preferences
            List<string> options = (List<string>)Options.LoadOptions();
            BrushConverter cnv = new BrushConverter();
            //converting the strings to Brushes
            Brush bodyColor = cnv.ConvertFromString(options[0]) as Brush;
            Brush headColor = cnv.ConvertFromString(options[1]) as Brush;
            
            //getting field height from the options
            int fieldHeight = Convert.ToInt16(options[3]);

            //getting playground size
            this.playGroundSize = new Size(Convert.ToInt16(options[2]), fieldHeight);

            //getting the food size
            this.foodSize = new Size(fieldHeight / 20, fieldHeight / 20);

            //getting field color from the options
            this.fieldColor = cnv.ConvertFromString(options[5]) as Brush;

            this.obstacleColor = cnv.ConvertFromString(options[6]) as Brush;
            this.mouseEnabled = Convert.ToBoolean(options[7]);
            this.fullScreen = Convert.ToBoolean(options[8]);

            this.keyPressed = false;


            //field properties
            this.Field = new PlayGround();
            this.Field.Width = playGroundSize.Width;
            this.Field.Height = playGroundSize.Height;
            if (Field != null && fullScreen) 
            {
                Field.WindowStyle = WindowStyle.None;
                Field.WindowState = WindowState.Maximized;
            }

            //setting game score
            this.score = 0;
            this.speed = GetSpeed();

            this.playGroundSize = new Size(playGroundSize.Width - 10, playGroundSize.Height - 10);
            //this.foodSize = foodSize;

            this.MoveInterval = 5;
            this.level = (GameLevel)Convert.ToInt16(options[4]);

            //creating Instance of the snake
            this.Snake = Snake.Instance(
                bodyColor,
                headColor,
                fieldHeight / 60,
                (int)((field.Height / 25) / MoveInterval));

            this.snakeMovementThread = null;
            this.gameEnd = false;

            this.playField = new Canvas();
            this.playField.Height = this.playGroundSize.Height;
            this.playField.Width = this.playGroundSize.Width;
            this.playField.Background = this.fieldColor;

            Canvas.SetTop(snake.Body, 0);
            Canvas.SetLeft(snake.Body, 0);

            playField.Children.Add(snake.Body);
            playField.Children.Add(snake.Head);

            if (mouseEnabled)
                playField.MouseDown += new MouseButtonEventHandler(PlayField_MouseDown);

            field.KeyDown += new KeyEventHandler(Field_KeyDown);
            field.KeyUp += new KeyEventHandler(Field_KeyUp);
            field.fieldGrid.Children.Add(playField);

            //creating rectangle for fading when the user press pause key
            fade = new Rectangle();
            fade.Width = playField.Width;
            fade.Height = playField.Height;
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = Color.FromRgb(0, 0, 0);
            brush.Opacity = 0.5;
            fade.Stroke = brush;
            fade.Fill = brush;
            Canvas.SetLeft(fade, 0);
            Canvas.SetTop(fade, 0);
            Canvas.SetZIndex(fade, 2147483647);

            obstacles = new List<Obstacle>();
            foodCollection = new Dictionary<Food, int>();

        }

        //starting the game
        public void Start()
        {
            GenerateObstacles((int)level);
            foodThread = new Thread(new ThreadStart(GenerateFood));
            foodThread.Start();

            field.Closed += new EventHandler(Field_Closed);

            this.Field.Show();
        }

        //Getting the speed, it depends on the screen size
        private int GetSpeed()
        {
            switch ((int)field.Height)
            {
                case 480: return (int)SpeedLevel.VERY_SLOW;
                case 600: return (int)SpeedLevel.SLOW;
                case 768: return (int)SpeedLevel.MEDIUM;
                case 1024: return (int)SpeedLevel.FAST;
                default: return (int)SpeedLevel.MEDIUM;
            }
        }

        private void Field_Closed(object sender, EventArgs e)
        {
            if (snakeMovementThread != null)
                snakeMovementThread.Abort();
            if (foodThread != null)
                foodThread.Abort();
        }

        //start snake set and get properties
        public Snake Snake
        {
            get
            {
                return snake;
            }
            set
            {
                if (value != null)
                {
                    snake = value;
                }
            }
        }//end of the snake's properties

        //start field set and get properties
        public PlayGround Field
        {
            get
            {
                return field;
            }
            set
            {
                if (value != null)
                {
                    field = value;
                }
            }
        }//end field set and get properties

        //start moveInterval's set and get properties
        public int MoveInterval
        {
            get
            {
                return moveInterval;
            }
            set
            {
                if (value > 0)
                {
                    moveInterval = value;
                }
            }
        }//end moveInterval's set and get properties






        /* * * * * * * * * * * * *
         *                       *
         *        Food           *
         *                       *
         * * * * * * * * * * * * */



        //Food generatin thread
        private void GenerateFood()
        {
            AddFoodThread();
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = ((int)speed) * 130;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
            timer.Enabled = true;
        }//end of the food generation

        //Generating food's color
        private Brush GenerateColor()
        {
            Random color = new Random();
            switch (color.Next(1, 8))
            {
                case 1: return Brushes.Blue;
                case 2: return Brushes.BurlyWood;
                case 3: return Brushes.Coral;
                case 4: return Brushes.DarkBlue;
                case 5: return Brushes.DarkCyan;
                case 6: return Brushes.DarkKhaki;
                case 7: return Brushes.Gold;
                default: return Brushes.DarkTurquoise;
            }
        }//end of the color generation


        //Adding food in randome places
        private void FoodCreate(ref Ellipse food, ref Food foodObject)
        {
            double x, y;
            Size size = foodSize;
            Random randomNumber = new Random();
            food = new Ellipse();
            food.Fill = GenerateColor();
            food.Width = size.Width;
            food.Height = size.Height;
            //generating random coordinates while there is a free place for the food
            do
            {
                x = randomNumber.Next(0, (int)(playGroundSize.Width - size.Width) + 1);
                y = randomNumber.Next(0, (int)(playGroundSize.Height - size.Height) + 1);
            } while (!IsFree(x, y, size, 0));

            playField.Children.Add(food);
            Canvas.SetTop(food, y);
            Canvas.SetLeft(food, x);
            //creating food object
            foodObject = new Food(food, new Point(x, y));
            foodCollection.Add(foodObject, 0);
        }//end of the method


        //Food's lifetime, at the end (when this food is not needed anymore)
        //calling FoodRemove method
        private void FoodLifeTime(Food foodObject)
        {
            Random randomNumber = new Random();
            //generating randome food life time in seconds
            int lifeTime = randomNumber.Next(((int)speed) / 6, ((int)speed) / 6 + ((int)speed) / 6);
            //while the food time havent ended
            for (int i = 0; i < lifeTime; i++)
            {
                Thread.Sleep(1000);
                if (foodCollection.ContainsKey(foodObject))
                {
                    foodCollection[foodObject]++;
                }
            }
            field.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                FoodRemove(foodObject);
            }));
        }//end of hiding definition


        //removing food
        private void FoodRemove(Food foodObject)
        {
            playField.Children.Remove(foodObject.FoodObject);
            foodCollection.Remove(foodObject);
        }//end of the definition


        //On timer elapsed  - generating a single food object
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            AddFoodThread();
        }

        private void AddFoodThread()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(FoodThread));
        }

        private void FoodThread(Object o)
        {
            Ellipse food = null;
            Food foodObject = null;
            CreateFood createFood = FoodCreate;
            RemoveFood removeFood = FoodLifeTime;
            field.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                if (createFood != null)
                {
                    createFood(ref food, ref foodObject);
                }
            }));
            //if this thread have been generated a food
            //starting a countdown                
            if (removeFood != null)
            {   
                //starting food countdown, when the food life ends it's going to be hide
                removeFood(foodObject);
                //recycling current thread
            }
        }

        //checking if the snake's head is hitting any food object
        private bool CheckForFood(double x, double y)
        {
            //check all food objects in the list
            foreach (var foodObj in foodCollection.ToList())
            {
                if (!(foodObj.Key.Position.X > x ||
                    foodObj.Key.Position.X + foodObj.Key.FoodObject.Width < x) &&
                    !(foodObj.Key.Position.Y > y ||
                    foodObj.Key.Position.Y + foodObj.Key.FoodObject.Height < y))
                {
                    if (playField.Children.Contains(foodObj.Key.FoodObject))
                    {
                        playField.Children.Remove(foodObj.Key.FoodObject);  //hiding the food
                        score += 100 / (foodObj.Value + 1); //increasing score
                        Snake.IncreaseSize((int)((field.Width/35)/MoveInterval));  
                        //increasing snake's length
                        foodCollection.Remove(foodObj.Key); //removing the food object from the list
                    }
                    return true;
                }
            }
            return false;
        }




        /* * * * * * * * * * * * *
         *                       *
         *      Obstacles        *
         *                       *
         * * * * * * * * * * * * */


        //Every obstacle is with different size, here I'm generating random size for
        //every obstacle
        private int GenerateObstacleSize(Orientation orientation)
        {
            Random obstacleDimention = new Random();

            if (orientation == Orientation.HORIZONTAL)
                return obstacleDimention.Next((int)playGroundSize.Width / 6, 
                    (int)playGroundSize.Width / 4);

            else
                return obstacleDimention.Next((int)playGroundSize.Height / 6,
                    (int)playGroundSize.Height / 4);

        }//end of the definition

        //Generate obstacle's position
        private bool FindObstaclePlace(out Point newPosition, Size obstacleSize)
        {
            Random coordinates = new Random();
            int x = 0, y = 0;
            x = coordinates.Next(0, (int)(playGroundSize.Width - obstacleSize.Width + 1));
            y = coordinates.Next(0, (int)(playGroundSize.Height - obstacleSize.Height + 1));
            newPosition = new Point(x, y);
            if (IsFree(x, y, obstacleSize, snake.Body.StrokeThickness * 2) && !InSafeZone(x, y))
                return true;

            return false;
        }//end of the definition

        //There is a safe zone for the snake. This zone is used for start position
        private bool InSafeZone(double x, double y)
        {
            if (x < playField.Width / 10 &&
                y < playField.Height / 10)
            {
                return true;
            }
            return false;
        }

        //Generating obstacles
        private void GenerateObstacles(int count)
        {
            Random random = new Random();
            Point newPosition;
            Size obstacleSize;
            int obstacleLength;

            //0 for vertical, false for horizontal
            for (int i = 0; i < count; i++)
            {
                int orientationTemp = random.Next(0,2);
                if (orientationTemp == 0)
                {
                    obstacleLength = GenerateObstacleSize(Orientation.VERTICAL);
                    obstacleSize = new Size(snake.Body.StrokeThickness, obstacleLength);
                }
                else
                {
                    obstacleLength = GenerateObstacleSize(Orientation.HORIZONTAL);
                    obstacleSize = new Size(obstacleLength, snake.Body.StrokeThickness);
                }
                //generating new obstacle position
                while (!FindObstaclePlace(out newPosition, obstacleSize)) { }
                //setting obstacle's properties

                UIElement shape = CreateObstacle(obstacleSize, newPosition);

                Canvas.SetLeft(shape, newPosition.X);
                Canvas.SetTop(shape, newPosition.Y);
                playField.Children.Add(shape);

                Obstacle obstacle = new Obstacle(shape, (int)newPosition.X, 
                    (int)newPosition.Y, obstacleSize);
                obstacles.Add(obstacle);
            }
        }//end of the definition

        //Creating an obstacle
        private UIElement CreateObstacle(Size size, Point position)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Stroke = obstacleColor;
            rectangle.Fill = obstacleColor;
            rectangle.Width = size.Width;
            rectangle.Height = size.Height;
            return rectangle;
        }


        //checking if in the (x,y) position there is no any obstacle
        //using displacement because there is an case in which there is not
        //enough for transiting
        private bool IsFree(double x, double y, Size size, double displacement)
        {
            if (obstacles.Count == 0)
            {
                return true;
            }
            foreach (var obstacle in obstacles)
            {
                if (!(obstacle.Position.X > x + size.Width + displacement ||
                    obstacle.Position.X + obstacle.Width + displacement < x) &&
                    !(obstacle.Position.Y > y + size.Height + displacement ||
                    obstacle.Position.Y + obstacle.Height + displacement < y))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsFree(double x, double y, double displacement)
        {
            if (obstacles.Count == 0)
            {
                return true;
            }
            foreach (var obstacle in obstacles)
            {
                if (!(obstacle.Position.X > x + displacement ||
                    obstacle.Position.X + obstacle.Width + displacement < x) &&
                    !(obstacle.Position.Y > y + displacement ||
                    obstacle.Position.Y + obstacle.Height + displacement < y))
                {
                    return false;
                }
            }
            return true;
        }

        //this could be used in feature if game rulles are going to be changed
        //here in defined score count snake's speed is increasing
        //private void UpSpeed()
        //{
        //    if (score >= 50 && score < 200)
        //    {
        //        foodAliveTime = ((int)SpeedLevel.SLOW) / 10;
        //        speed = (int)SpeedLevel.SLOW;
        //    }
        //    else if (score >= 200 && score < 1500)
        //    {
        //        foodAliveTime = ((int)SpeedLevel.MEDIUM) / 10;
        //        speed = (int)SpeedLevel.MEDIUM;
        //    }
        //    else if (score >= 1500 && score < 4500)
        //    {
        //        foodAliveTime = ((int)SpeedLevel.FAST) / 10;
        //        speed = (int)SpeedLevel.FAST;
        //    }
        //    else
        //    {
        //        foodAliveTime = ((int)SpeedLevel.VERY_FAST) / 10;
        //        speed = (int)SpeedLevel.VERY_FAST;
        //    }
        //}





        /* * * * * * * * * * * * * * *
         *                           *
         *  Movement and game rules  *
         *                           *
         * * * * * * * * * * * * * * */
        
        //on mouse click getting mouse direction
        private void PlayField_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double x = snake.Body.Points.Last().X;
            double y = snake.Body.Points.Last().Y;

            double mouseCoordinatesX = e.GetPosition(playField).X;
            double mouseCoordinatesY = e.GetPosition(playField).Y;


            double a = mouseCoordinatesY - y;
            double b = x - mouseCoordinatesX;
            double c = -x * mouseCoordinatesY + mouseCoordinatesX * y;

            if (snakeMovementThread != null && snakeMovementThread.IsAlive)
                snakeMovementThread.Abort();

            Move(a, b, c, mouseCoordinatesX, mouseCoordinatesY,
                snake.Body.Points.Last().X, snake.Body.Points.Last().Y);
        }


        private void PlayField_MouseMove(object sender, MouseEventArgs e)
        {
            double x = snake.Body.Points.Last().X;
            double y = snake.Body.Points.Last().Y;

            double mouseCoordinatesX = e.GetPosition(playField).X;
            double mouseCoordinatesY = e.GetPosition(playField).Y;


            double a = mouseCoordinatesY - y;
            double b = x - mouseCoordinatesX;
            double c = -x * mouseCoordinatesY + mouseCoordinatesX * y;

            if (snakeMovementThread != null && snakeMovementThread.IsAlive)
                snakeMovementThread.Abort();

            Move(a, b, c, mouseCoordinatesX, mouseCoordinatesY,
                snake.Body.Points.Last().X, snake.Body.Points.Last().Y);
        }

        private void Field_KeyUp(object sender, KeyEventArgs e)
        {
            keyPressed = false;
        }

        private bool OppositeKey(Key key1, Key key2)
        {
            if ((key1 == Key.Down && key2 == Key.Up) ||
                (key1 == Key.Up && key2 == Key.Down) ||
                (key1 == Key.Left && key2 == Key.Right) ||
                (key1 == Key.Right && key2 == Key.Left))
                return true;

            return false;
        }

        private void MoveWithKeyboard(Key key)
        {
            if (!OppositeKey(key, lastKey))
            {
                lastKey = key;
                if (keyPressed)
                    return;

                keyPressed = true;


                double x = snake.Body.Points.Last().X;
                double y = snake.Body.Points.Last().Y;

                double mouseCoordinatesX = x;
                double mouseCoordinatesY = y;

                switch (key)
                {
                    case Key.Up: mouseCoordinatesY -= 3; 
                        break;
                    case Key.Down: mouseCoordinatesY += 3; 
                        break;
                    case Key.Right: mouseCoordinatesX += 3; 
                        break;
                    case Key.Left: mouseCoordinatesX -= 3; 
                        break;
                }


                double a = mouseCoordinatesY - y;
                double b = x - mouseCoordinatesX;
                double c = -x * mouseCoordinatesY + mouseCoordinatesX * y;

                if (snakeMovementThread != null && snakeMovementThread.IsAlive)
                    snakeMovementThread.Abort();

                Move(a, b, c, mouseCoordinatesX, mouseCoordinatesY,
                    snake.Body.Points.Last().X, snake.Body.Points.Last().Y);
            }
        }

        private void Field_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.Key)
            {
                case Key.Up: MoveWithKeyboard(e.Key);
                    break;
                case Key.Down: MoveWithKeyboard(e.Key);
                    break;
                case Key.Left: MoveWithKeyboard(e.Key);
                    break;
                case Key.Right: MoveWithKeyboard(e.Key);
                    break;
                case Key.Escape: EndGame();
                    break;
                case Key.Pause: PauseGame();
                    break;
            }

        }

        private void PauseGame()
        {
            if (!paused)
                playField.Children.Add(fade);

            else
                playField.Children.Remove(fade);

            paused = !paused;
        }

        private void ComputeNexCoordinates(double a, double b, double c, double mouseCoordinatesX,
            double mouseCoordinatesY, double lastHeadPositionX, double lastHeadPositionY, out double u, out double v)
        {
            double x = lastHeadPositionX;
            double y = lastHeadPositionY;

            //length of the current vector which is collinear with ax+by+c=0
            double currentLength = moveInterval / Math.Sqrt(a * a + b * b);


            //calculating new coordinates
            u = -b * currentLength + lastHeadPositionX;
            v = a * currentLength + lastHeadPositionY;

        }

        private void Move(double a, double b, double c, double mouseCoordinatesX,
            double mouseCoordinatesY, double lastHeadPositionX, double lastHeadPositionY)
        {
            snakeMovementThread = new Thread(new ThreadStart(() =>
            {
                while (!gameEnd)
                {
                    if (!paused)
                    {
                        double u, v;

                        ComputeNexCoordinates(a, b, c, mouseCoordinatesX,
                        mouseCoordinatesY, lastHeadPositionX, lastHeadPositionY, out u, out v);
                        
                        lastHeadPositionX = u;
                        lastHeadPositionY = v;

                        // SetLeft is done in the UI thread
                        field.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action<Point>(MoveSnake), new Point(u,v));
                        Thread.Sleep(speed);
                    }
                }
            }));
            snakeMovementThread.Start();
        }

        private void MoveSnake(Point position)
        {
            double x1 = position.X - ((snake.Head.Width + snake.Head.Width) / 3) * Math.Cos(45 * Math.PI / 180);
            double y1 = position.Y - ((snake.Head.Height + snake.Head.Height) / 3) * Math.Sin(45 * Math.PI / 180);
            CheckForFood(position.X, position.Y);

            if (GameEnded(position.X, position.Y))
                EndGame();

            Canvas.SetLeft(snake.Head, x1);
            Canvas.SetTop(snake.Head, y1);
            snake.Move(new Point(position.X, position.Y));
        }

        //finishing the game
        public void EndGame()
        {
            gameEnd = true;
            MessageBox.Show("Game Over.\n You earn " + score.ToString() + " points!");
            int maxUsernameLength = 15;
            Inputbox inp = new Inputbox();
            string username = "";

            if (inp.ShowDialog() == true)
                username = inp.username.Text;

            if (username.Length < 1)
                username = "Anonymous";

            if (username.Length > maxUsernameLength)
                username = username.Substring(0, maxUsernameLength);

            Results.AddResult((int)level, username, score);
            field.Close();
        }//end of the definition


        //check if the game have been ended
        private bool GameEnded(double newX, double newY)
        {
            if (newX >= playField.Width || newY >= playField.Height
                || newX <= 0 || newY <= 0)     
                return true;

            if (snake.IsSelfCrossing())
                return true;

            if (!IsFree(newX, newY, snake.Body.StrokeThickness / 2))
                return true;

            return false;
        }//end of the definition
    }
}
