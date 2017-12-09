using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace StellarCartography.Model
{
    class StellarCartographyModel
    {
        #region StellarCartographyStates enum

        public enum StellarCartographyStates
        {
            GameStartSplashScreen = 0,
            Drawing = 1,
            EvaluationSplashScreen = 2,
            Evaluation = 3,
            GameRoundSplashScreen = 4,
            GameOverSplashScreen = 5
        }

        #endregion

        #region Static fields

        public static readonly Random Random = new Random();
        
        #endregion

        #region Fields


        public Int32 RESOLUTION_X;
        public Int32 RESOLUTION_Y;
        private Int32 GAME_ROUNDS;
        private Int32 GAME_FIELD_ROWS;
        private Int32 GAME_FIELD_COLUMNS;

        private Int32 roundNumber;

        private Double xRatio;
        private Double yRatio;

        private IList<PictureRect> stars;
        private IList<PictureRect> planets;
        private IList<PictureRect> backgrounds;
        private IList<PictureRect> asteroids;
        private IList<PictureRect> ships;

        private IList<Int32> lastStarIndices;
        private IList<Int32> lastPlanetIndices;
        private IList<Int32> lastBackgroundIndices;
        private IList<Int32> lastStarShipIndices;

        #endregion

        #region Properties

        public StellarCartographyStates CurrentState { get; private set; }
        public String StatusMessage { get; private set; }

        public PictureRect SplashImage { get; private set; }
        public PictureRect CRTEffectImage { get; private set; }
        public PictureRect Background
        {
            get
            {
                return this.backgrounds[this.GetUnusedIndex(this.backgrounds.Count, this.lastBackgroundIndices, 4)];
            }
        }

        public PictureRect Star { get; private set; }
        public IList<PictureRect> Planets { get; private set; }
        public IList<PictureRect> Ships { get; private set; }
        public PictureRect Stargate { get; private set; }

        #endregion

        #region Events

        public event EventHandler UpdateGameField;
        public event EventHandler UpdateStatusText;

        #endregion

        #region Constructor

        public StellarCartographyModel(int width, int height, int rounds, int rows, int columns)
        {
            this.RESOLUTION_X = width;
            this.RESOLUTION_Y = height;

            this.GAME_ROUNDS = rounds;

            this.GAME_FIELD_ROWS = rows;
            this.GAME_FIELD_COLUMNS = columns;

            this.xRatio = (Double)RESOLUTION_X / this.GAME_FIELD_COLUMNS;
            this.yRatio = (Double)RESOLUTION_Y / this.GAME_FIELD_ROWS;

            this.CurrentState = StellarCartographyStates.GameStartSplashScreen;

            this.LoadResources();

            this.lastStarIndices = new List<Int32>();
            this.lastPlanetIndices = new List<Int32>();
            this.lastBackgroundIndices = new List<Int32>();
            this.lastStarShipIndices = new List<Int32>();

            this.Planets = new List<PictureRect>();
            this.Ships = new List<PictureRect>();
        }

        #endregion

        #region Public methods

        public void StartNewGame()
        {
            this.CurrentState = StellarCartographyStates.Drawing;
            this.roundNumber = 0;

            this.GenerateField();

            this.UpdateGameField(this, new EventArgs());
        }

        public void StepGame()
        {
            switch (this.CurrentState)
            {
                case StellarCartographyStates.GameStartSplashScreen:

                    this.CurrentState = StellarCartographyStates.Drawing;
                    this.StatusMessage = String.Empty;

                    break;
                case StellarCartographyStates.Drawing:

                    this.CurrentState = StellarCartographyStates.EvaluationSplashScreen;
                    this.StatusMessage = "Time is up!" + System.Environment.NewLine + String.Format("Commence evaluation phase #{0}", this.roundNumber + 1);

                    this.UpdateStatusText(this, new EventArgs());

                    break;
                case StellarCartographyStates.EvaluationSplashScreen:

                    this.CurrentState = StellarCartographyStates.Evaluation;
                    this.StatusMessage = "Evaluate scores!";

                    this.UpdateStatusText(this, new EventArgs());

                    break;
                case StellarCartographyStates.Evaluation:

                    this.roundNumber++;

                    if (this.roundNumber < GAME_ROUNDS)
                    {
                        this.GenerateField();

                        this.CurrentState = StellarCartographyStates.GameRoundSplashScreen;
                        this.StatusMessage = String.Format("Get ready for round #{0}", this.roundNumber + 1);
                    }
                    else
                    {
                        this.CurrentState = StellarCartographyStates.GameOverSplashScreen;
                        this.StatusMessage = "Game over!" + System.Environment.NewLine + "Thank you for playing!";
                    }

                    this.UpdateStatusText(this, new EventArgs());

                    break;
                case StellarCartographyStates.GameRoundSplashScreen:

                    this.CurrentState = StellarCartographyStates.Drawing;
                    this.StatusMessage = String.Empty;

                    break;
                case StellarCartographyStates.GameOverSplashScreen:

                    this.CurrentState = StellarCartographyStates.GameStartSplashScreen;
                    this.StatusMessage = "Get ready!";
                    
                    this.UpdateStatusText(this, new EventArgs());

                    break;
            }

            this.UpdateGameField(this, new EventArgs());
        }

        #endregion

        #region Private methods

        private void LoadResources()
        {
            this.stars = new List<PictureRect>();
            this.planets = new List<PictureRect>();
            this.backgrounds = new List<PictureRect>();
            this.asteroids = new List<PictureRect>();
            this.ships = new List<PictureRect>();

            Double baseScale = (Double)100 / 1920;

            ResourceSet resourceSet = Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            foreach (DictionaryEntry entry in resourceSet)
            {
                if (entry.Key.ToString().Contains("planet"))
                {
                    this.planets.Add(new PictureRect(0, 0, new Bitmap(entry.Value as Bitmap, this.ResizeKeepAspect((entry.Value as Bitmap).Size, (int)(RESOLUTION_X * baseScale), (int)(RESOLUTION_X * baseScale)))));
                }
                else if (entry.Key.ToString().Contains("background"))
                {
                    this.backgrounds.Add(new PictureRect(0, 0, entry.Value as Bitmap));
                }
                else if (entry.Key.ToString().Contains("sun"))
                {
                    double ratio = StellarCartographyModel.Random.NextDouble() * (1 - 0.8) + 0.8;

                    this.stars.Add(new PictureRect(0, 0, new Bitmap(entry.Value as Bitmap, this.ResizeKeepAspect((entry.Value as Bitmap).Size, (int)(RESOLUTION_X * baseScale * 2 * ratio), (int)(RESOLUTION_X * baseScale * 2 * ratio)))));
                }
                else if (entry.Key.ToString().Contains("asteroid"))
                {
                    this.asteroids.Add(new PictureRect(0, 0, new Bitmap(entry.Value as Bitmap, this.ResizeKeepAspect((entry.Value as Bitmap).Size, (int)(RESOLUTION_X * baseScale), (int)(RESOLUTION_X * baseScale)))));
                }
                else if (entry.Key.ToString().Contains("stargate_ring"))
                {
                    this.Stargate = new PictureRect(0, 0, new Bitmap(entry.Value as Bitmap, this.ResizeKeepAspect((entry.Value as Bitmap).Size, (int)(RESOLUTION_X * baseScale), (int)(RESOLUTION_X * baseScale))));
                }
                else if (entry.Key.ToString().Contains("star_ship"))
                {
                    this.ships.Add(new PictureRect(0, 0, new Bitmap(entry.Value as Bitmap, this.ResizeKeepAspect((entry.Value as Bitmap).Size, (int)(RESOLUTION_X * baseScale), (int)(RESOLUTION_X * baseScale)))));
                }
            }

            this.SplashImage = new PictureRect(0, 0, Properties.Resources.splash);
            this.CRTEffectImage = new PictureRect(0, 0, Properties.Resources.crt_effect);
        }

        private void GenerateField()
        {
            this.GenerateStar();
            this.GeneratePlanets();
            this.GenerateAsteroids();
            this.GenerateStarGate();
            this.GenerateStarShips();
        }

        private void GenerateStar()
        {
            int randomLocation = StellarCartographyModel.Random.Next(3);

            int x = this.GAME_FIELD_COLUMNS / 2;
            int y = this.GAME_FIELD_ROWS / 2 - 1 + randomLocation;

            int index = this.GetUnusedIndex(this.stars.Count, this.lastStarIndices, 2);

            PictureRect selectedStar = this.stars[index];
            selectedStar.X = x * this.xRatio;
            selectedStar.Y = y * this.yRatio;

            this.Star = selectedStar;
        }

        private void GeneratePlanets()
        {
            this.Planets.Clear();

            int x, y;

            for (int i = 0; i < 4; i++)
            {
                x = StellarCartographyModel.Random.Next(this.GAME_FIELD_COLUMNS / 2 - 1);
                y = StellarCartographyModel.Random.Next(this.GAME_FIELD_ROWS / 2 - 1);

                int index = this.GetUnusedIndex(this.planets.Count, this.lastPlanetIndices, 10);

                PictureRect selectedPlanet = this.planets[index];
                selectedPlanet.X = i == 1 || i == 3 ? x + this.GAME_FIELD_COLUMNS / 2 + 1 : x;
                selectedPlanet.Y = i == 2 || i == 3 ? y + this.GAME_FIELD_ROWS / 2 + 1 : y;
                
                selectedPlanet.X *= this.xRatio;
                selectedPlanet.Y *= this.yRatio;

                this.Planets.Add(selectedPlanet);
            }
        }

        private void GenerateAsteroids()
        {
            int sideBlocked = StellarCartographyModel.Random.Next(4); // 0: NORTH 1: EAST 2: SOUTH 3: WEST

            int x = 0, y = 0;

            for (int j = 0; j < 4; j++)
            {
                int numberOfRoids = j % 2 == 0 ? this.GAME_FIELD_ROWS / 2 : this.GAME_FIELD_COLUMNS / 2;
                int removeIndex = j == sideBlocked ? -1 : StellarCartographyModel.Random.Next(numberOfRoids);

                for (int i = 0; i < numberOfRoids; i++)
                {
                    switch (j)
                    {
                        case 0:
                            x = this.GAME_FIELD_COLUMNS / 2;
                            y = 0;
                            break;
                        case 1:
                            x = this.GAME_FIELD_COLUMNS / 2 + 1;
                            y = this.GAME_FIELD_ROWS / 2;
                            break;
                        case 2:
                            x = this.GAME_FIELD_COLUMNS / 2;
                            y = this.GAME_FIELD_ROWS / 2 + 1;
                            break;
                        case 3:
                            x = 0;
                            y = this.GAME_FIELD_ROWS / 2;
                            break;
                    }

                    x = j % 2 == 0 ? x : x + i;
                    y = j % 2 == 0 ? y + i : y;

                    PictureRect newRoid = asteroids[1].Clone();
                    newRoid.X = x * this.xRatio;
                    newRoid.Y = y * this.yRatio;

                    if (j % 2 == 0)
                        newRoid.Rotate();

                    if ((newRoid.X != this.Star.X || newRoid.Y != this.Star.Y) && i != removeIndex)
                        this.Planets.Add(newRoid);
                }
            }
        }

        private void GenerateStarGate()
        {
            int x, y;

            int side = StellarCartographyModel.Random.Next(4); // 0: NORTH 1: EAST 2: SOUTH 3: WEST

            do
            {
                x = side % 2 == 0 ? StellarCartographyModel.Random.Next(this.GAME_FIELD_COLUMNS) : StellarCartographyModel.Random.Next(2);
                y = side % 2 == 0 ? StellarCartographyModel.Random.Next(2) : StellarCartographyModel.Random.Next(this.GAME_FIELD_ROWS);

                this.Stargate.X = x * this.xRatio;
                this.Stargate.Y = y * this.yRatio;

            } while (this.Planets.Any(p => p.X == this.Stargate.X && p.Y == this.Stargate.Y));
        }

        private void GenerateStarShips()
        {
            this.Ships.Clear();

            int numberOfShips = StellarCartographyModel.Random.Next(2, 4);
            int x, y;

            for (int i = 0; i < numberOfShips; i++)
            {
                int index = this.GetUnusedIndex(this.ships.Count, this.lastStarShipIndices, numberOfShips);

                PictureRect ship = this.ships[index];

                do
                {
                    x = StellarCartographyModel.Random.Next(this.GAME_FIELD_COLUMNS);
                    y = StellarCartographyModel.Random.Next(this.GAME_FIELD_ROWS);

                    ship.X = x * this.xRatio;
                    ship.Y = y * this.yRatio;

                } while (this.Planets.Any(p => p.X == ship.X && p.Y == ship.Y) ||
                         this.Ships.Any(s => s.X == ship.X && s.Y == ship.Y) || 
                         (this.Star.X == ship.X && this.Star.Y == ship.Y) || 
                         (this.Stargate.X == ship.X || this.Stargate.Y == ship.Y));

                ship.RandomRotate();
                this.Ships.Add(ship);
            }
        }

        private int GetUnusedIndex(int numberOfElements, IList<Int32> indexCollection, int numberToKeep)
        {
            int index;
            do
            {
                index = StellarCartographyModel.Random.Next(numberOfElements);
            }
            while (indexCollection.Contains(index));

            indexCollection.Add(index);

            if (indexCollection.Count > numberToKeep)
                indexCollection.RemoveAt(0);

            return index;
        }

        private Size ResizeKeepAspect(Size src, int maxWidth, int maxHeight)
        {
            decimal rnd = Math.Min(maxWidth / (decimal)src.Width, maxHeight / (decimal)src.Height);
            return new Size((int)Math.Round(src.Width * rnd), (int)Math.Round(src.Height * rnd));
        }

        #endregion
    }

}
