using StellarCartography.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace StellarCartography.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        private static readonly Int32 ROUND_TIME = 5;

        private StellarCartographyModel model;
        private Timer timer;
        private Int32 time;

        public IEnumerable<PictureRect> pictures;

        public DelegateCommand QuitCommand { get; private set; }
        public DelegateCommand StepCommand { get; private set; }
        public DelegateCommand TestCommand { get; private set; }

        public event EventHandler Quit;

        private Boolean itemsControlVisible;
        public Boolean ItemsControlVisible
        {
            get
            {
                return this.itemsControlVisible;
            }
            set
            {
                this.itemsControlVisible = value;
                OnPropertyChanged();
            }
        }

        private Boolean statusVisible;
        public Boolean StatusVisible
        {
            get
            {
                return this.statusVisible;
            }
            set
            {
                this.statusVisible = value;
                OnPropertyChanged();
            }
        }

        private String timeLabelContent;
        public String TimeLabelContent
        {
            get
            {
                return this.timeLabelContent;
            }
            set
            {
                this.timeLabelContent = value;
                OnPropertyChanged();
            }
        }

        private String statusLabelContent;
        public String StatusLabelContent
        {
            get
            {
                return this.statusLabelContent;
            }
            set
            {
                this.statusLabelContent = value;
                OnPropertyChanged();
            }
        }

        public PictureRect SplashImage
        {
            get
            {
                return this.model.SplashImage;
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public IEnumerable<PictureRect> Pictures
        {
            get
            {
                return this.pictures;
            }
            set
            {
                this.pictures = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel(StellarCartographyModel model)
        {
            this.model = model;
            this.model.UpdateGameField += OnUpdateGameField;
            this.model.UpdateStatusText += OnUpdateStatusText;

            this.ItemsControlVisible = false;
            this.StatusVisible = true;
            this.StatusLabelContent = "Get ready!";

            this.QuitCommand = new DelegateCommand(param => OnQuitCommand());
            this.StepCommand = new DelegateCommand(param => OnStepCommand());
            this.TestCommand = new DelegateCommand(param => OnTestCommand());
        }

        private void OnQuitCommand()
        {
            this.Quit(this, new EventArgs());
        }

        private void OnStepCommand()
        {
            switch (this.model.CurrentState)
            {
                case StellarCartographyModel.StellarCartographyStates.GameStartSplashScreen:
                    this.model.StartNewGame();
                    break;
                case StellarCartographyModel.StellarCartographyStates.Drawing:
                    break;
                case StellarCartographyModel.StellarCartographyStates.EvaluationSplashScreen:
                case StellarCartographyModel.StellarCartographyStates.Evaluation:
                case StellarCartographyModel.StellarCartographyStates.GameRoundSplashScreen:
                case StellarCartographyModel.StellarCartographyStates.GameOverSplashScreen:
                    this.model.StepGame();
                    break;
            }
        }

        private void OnUpdateGameField(object sender, EventArgs e)
        {
            switch (this.model.CurrentState)
            {
                case StellarCartographyModel.StellarCartographyStates.Drawing:

                    this.Pictures = new List<PictureRect>()
                    {
                        model.Background,
                        model.Star,
                        model.CRTEffectImage
                    };

                    this.time = ROUND_TIME;
                    this.timer = new Timer(1000);
                    this.timer.Elapsed += OnTimerElapsed;
                    this.timer.Start();

                    this.ItemsControlVisible = true;
                    this.StatusVisible = false;

                    break;
                case StellarCartographyModel.StellarCartographyStates.Evaluation:

                    this.ItemsControlVisible = true;

                    break;
                case StellarCartographyModel.StellarCartographyStates.GameRoundSplashScreen:
                case StellarCartographyModel.StellarCartographyStates.GameStartSplashScreen:
                case StellarCartographyModel.StellarCartographyStates.GameOverSplashScreen:

                    this.ItemsControlVisible = false;

                    break;
                case StellarCartographyModel.StellarCartographyStates.EvaluationSplashScreen:

                    this.ItemsControlVisible = false;
                    this.StatusVisible = true;

                    break;
            }

        }

        private void OnUpdateStatusText(object sender, EventArgs e)
        {
            if (this.StatusVisible)
            {
                this.StatusVisible = false;

                Task.Factory.StartNew(() =>
                {
                    System.Threading.Thread.Sleep(1000);
                    this.StatusLabelContent = this.model.StatusMessage;
                    System.Windows.Application.Current.Dispatcher.Invoke(() => { this.StatusVisible = true; });
                });
            }
            else
                this.StatusLabelContent = this.model.StatusMessage;
        }

        private void OnTestCommand()
        {
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.TimeLabelContent = this.time--.ToString();

            if (this.time == -1)
            {
                this.timer.Stop();

                this.TimeLabelContent = String.Empty;

                this.model.StepGame();
            }
        }
    }
}
