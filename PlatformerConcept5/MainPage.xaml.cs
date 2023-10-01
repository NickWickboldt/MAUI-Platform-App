namespace PlatformerConcept5;

public partial class MainPage : ContentPage
{


    private bool gridReady;
    private Player user;
    private Platform[] platformList = new Platform[4];
    public int level = 1;
    public int score = 0;
    public Label scoreLabel;
    public Label levelLabel;
    public double difficulty = 1000;

    public MainPage()
    {
        InitializeComponent();
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if (gridReady) //user cannot tap until all grid assets are loaded
        {
            gridReady = user.Jump(gameGrid, platformList, gridReady, this);
            if (!gridReady)
            {
                Start_Button.Text = "Game Over!";
            }
        }
    }

    private void Start_Button_Clicked(object sender, EventArgs e)
    {
        Fill_Grid(true);

        gridReady = true;
        Start_Button.IsEnabled = false;
    }

    private void Reset_Button_Clicked(object sender, EventArgs e)
    {
        gameGrid.Clear();
        Start_Button.IsEnabled = true;
        gridReady = true;
        score = 0;
        level = 1;
    }

    async public void Fill_Grid(bool start) //fill grid or reset grid based on start
    {
        if (start) //filss grid with rows, columns, user, platforms, score label, and level label
        {
            Image clouds = new Image { Source = "clouds.png", ZIndex = 0 };
            Image clouds2 = new Image { Source = "clouds.png", ZIndex = 0 };
            int rows = 5;
            int cols = 5;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    StackLayout unit = new StackLayout() { ZIndex = 0 };
                    unit.BackgroundColor = Colors.LightSkyBlue;
                    gameGrid.Add(unit, j, i);

                    if (i < 4 && j == 0)
                    {
                        BoxView newRect = new BoxView()
                        {
                            HeightRequest = 20,
                            Color = Colors.Green,
                            VerticalOptions = LayoutOptions.End,
                            CornerRadius = 10,
                            ZIndex = 1
                        };
                        Platform newPlat = new Platform(newRect, i, j);
                        gameGrid.Add(newRect, newPlat.col, newPlat.row);
                        platformList[i] = newPlat;

                    }

                    await Task.Delay(100); //game start animation
                }
            }
            user = Create_User();
            gameGrid.Add(user.image, user.col, user.row);
            scoreLabel = new Label()
            {
                Text = "Score: " + score,
                FontSize = 30,
                ZIndex = 1,
                TextColor = Colors.White,
                Margin = new Thickness(10),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
            };
            levelLabel = new Label()
            {
                Text = "Level: " + level,
                FontSize = 30,
                ZIndex = 1,
                TextColor = Colors.White,
                Margin = new Thickness(10),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
            };
            await Task.Delay(500);
            gameGrid.Add(scoreLabel, 0, 4);
            await Task.Delay(500);
            gameGrid.Add(levelLabel, 4, 4);
            await Task.Delay(500);
            gameGrid.Add(clouds, 1, 1);
            await Task.Delay(500);
            gameGrid.Add(clouds2, 4, 0);
        }
        else //resets platforms on grid reset
        {

            user.row = 4;
            gameGrid.SetRow(user.image, user.row);
            foreach (Platform plat in platformList)
            {
                plat.col = 0;
                gameGrid.SetColumn(plat.rect, plat.col);
            }
        }

        foreach (Platform plat in platformList) //starts oscillation
        {
            plat.isMovingRight = true;
            plat.Oscillate(gameGrid, this);
        }
    }

    private Player Create_User() //creates player character
    {
        Image userIcon = new Image { Source = "dotnet_bot.png", ZIndex = 1 };
        user = new Player(userIcon, 4, 2);
        return user;
    }


}

public class Player
{
    public Image image;
    public int row;
    public int col;

    public Player(Image i, int r, int c)
    {
        this.image = i;
        this.row = r;
        this.col = c;
    }

    public bool Jump(Grid g, Platform[] list, bool con, MainPage page)
    {
        if (this.row == 0) // reach top of grid, resets grid
        {
            page.difficulty = page.difficulty * 0.9;
            page.level++;
            page.levelLabel.Text = "Level: " + page.level;
            page.Fill_Grid(false);
        }
        else
        {
            g.SetRow(this.image, this.row -= 1);
            Platform plat = list[this.row];
            if (this.row == plat.row && this.col == plat.col) //successful collision with platform
            {
                page.score++;
                page.scoreLabel.Text = "Score: " + page.score;
                plat.StopMovement();
            }
            else
            {
                con = false; //gameover, if miss the platform, user can no longer move

            }
        }
        return con;
    }
}

public class Platform
{
    public BoxView rect;
    public int row;
    public int col;
    public bool isMovingLeft;
    public bool isMovingRight;

    public Platform(BoxView rectangle, int r, int c)
    {
        this.rect = rectangle;
        this.row = r;
        this.col = c;
        this.isMovingLeft = false;
        this.isMovingRight = true;
    }

    async public void Oscillate(Grid g, MainPage page) //starts movement of platforms
    {
        int boundaryLeft = 0;
        int boundaryRight = 4;
        var rand = new Random();
        await Task.Delay(rand.Next(1000)); //random delays for the platform movement
        MoveRight(g, boundaryLeft, boundaryRight, page); //recursive movement
    }

    async public void MoveRight(Grid g, int limitLeft, int limitRight, MainPage page) //moves platforms right
    {
        while (this.col < limitRight && isMovingRight) //while no collision
        {
            this.col++;
            g.SetColumn(this.rect, this.col);
            await Task.Delay((int)page.difficulty);
        }
        if (isMovingRight) //if there has not been a collision
        {
            this.isMovingLeft = true;
            this.isMovingRight = false;
            MoveLeft(g, limitLeft, limitRight, page);
        }

    }

    async public void MoveLeft(Grid g, int limitLeft, int limitRight, MainPage page) //moves platforms left
    {
        while (this.col > limitLeft && isMovingLeft) //while no collision
        {
            this.col--;
            g.SetColumn(this.rect, this.col);
            await Task.Delay((int)page.difficulty);
        }
        if (isMovingLeft) //if there has not been a collision
        {
            this.isMovingRight = true;
            this.isMovingLeft = false;
            MoveRight(g, limitLeft, limitRight, page);
        }

    }

    public void StopMovement() //provides condition for stopping recursive calls of MoveLeft and MoveRight
    {
        this.isMovingLeft = false;
        this.isMovingRight = false;

    }
}