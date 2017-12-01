using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace StellarCartography.Model
{
    class StellarCartographyModel
    {
        public enum StellarCartographyStates
        {
            GameStartSplashScreen = 0,
            Drawing = 1,
            EvaluationSplashScreen = 2,
            Evaluation = 3,
            GameRoundSplashScreen = 4,
            GameOverSplashScreen = 5
        }

        private static readonly Random Random = new Random();
        private static readonly Int32 GAME_ROUNDS = 2;
        
        private Int32 roundNumber;

        private IList<PictureRect> stars;
        private IList<PictureRect> planets;
        private IList<PictureRect> backgrounds;

        private IList<Int32> lastStarIndices;
        private IList<Int32> lastPlanetIndices;
        private IList<Int32> lastBackgroundIndices;

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


        public event EventHandler UpdateGameField;
        public event EventHandler UpdateStatusText;

        public StellarCartographyModel()
        {
            this.CurrentState = StellarCartographyStates.GameStartSplashScreen;

            this.LoadResources();

            this.lastStarIndices = new List<Int32>();
            this.lastPlanetIndices = new List<Int32>();
            this.lastBackgroundIndices = new List<Int32>();
        }

        private void LoadResources()
        {
            this.stars = new List<PictureRect>();
            this.planets = new List<PictureRect>();
            this.backgrounds = new List<PictureRect>();

            ResourceSet resourceSet = Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            foreach (DictionaryEntry entry in resourceSet)
            {
                if (entry.Key.ToString().Contains("planet"))
                {
                    this.planets.Add(new PictureRect(0, 0, entry.Value as Bitmap));
                }
                else if (entry.Key.ToString().Contains("background"))
                {
                    this.backgrounds.Add(new PictureRect(0, 0, entry.Value as Bitmap));
                }
                else if (entry.Key.ToString().Contains("sun"))
                {
                    double ratio = StellarCartographyModel.Random.NextDouble() * (1 - 0.8) + 0.8;

                    this.stars.Add(new PictureRect(0, 0, new Bitmap(entry.Value as Bitmap, new Size((int)(200 * ratio), (int)(200 * ratio)))));
                }
            }

            this.SplashImage = new PictureRect(0, 0, Properties.Resources.splash);
            this.CRTEffectImage = new PictureRect(0, 0, Properties.Resources.crt_effect);
        }

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

        private void GenerateField()
        {
            this.GenerateStar();
        }

        private void GenerateStar()
        {
            int x = StellarCartographyModel.Random.Next(500);
            int y = StellarCartographyModel.Random.Next(500);

            int index = GetUnusedIndex(this.stars.Count, lastStarIndices, 2);

            PictureRect selectedStar = this.stars[index];
            selectedStar.X = 710 + x - 1920 / 2;
            selectedStar.Y = 290 + y - 1080 / 2;

            this.Star = selectedStar;
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
    }
}

