using UnityEngine;
using TimeManager = Manager.Time;
using KeyboardManager = Manager.Keyboard;
using WorldTimeModel = Unit.Time.World;
using MappedInputKeyboardType = Type.Keyboard.MappedInput;

namespace Manager
{
    public class Time
    {
        private static TimeManager _instance;
        public static TimeManager Instance => _instance ?? new TimeManager(); 
        

        // An array of possible time multipliers.
        private readonly float[] _possibleTimeScales = new float[] { 0.1f, 0.5f, 1f, 2f, 4f, 8f };

        // Gets the game time.
        // TODO: Implement saving and loading game time, so time is persistent across loads.
        public float GameTime { get; private set; } 
        
        private const float REAL_TIME_TO_WORLD_TIME_FACTOR = 90;
        
        public WorldTimeModel WorldTime {
            get => new WorldTimeModel(GameTime * REAL_TIME_TO_WORLD_TIME_FACTOR);
            set => GameTime = value.Seconds / REAL_TIME_TO_WORLD_TIME_FACTOR;
        }

        // Multiplier of Time.deltaTime.
        private float _timeScale;
        
        // Current position in that array.
        // Public so TimeScaleUpdater can easily get a position appropriate to an image.
        private int _timeScalePosition;

        //
        public int OpenedModalCount { get; set; }
        public bool IsModal {
            get => OpenedModalCount != 0;
            set {
                if (value) {
                    OpenedModalCount++;
                } else {
                    if (OpenedModalCount > 0) {
                        OpenedModalCount--;
                    }
                }
            }
        }

        public bool IsPaused { get; set; }

        private Time()
        {
            _instance = this;
            
            _timeScale = 1f;
            _timeScalePosition = 2;
            
            WorldTime = new WorldTimeModel().SetHour(8);

            KeyboardManager.Instance.RegisterInputAction("SetSpeed1", MappedInputKeyboardType.KeyUp, () => SetTimeScalePosition(2));
            KeyboardManager.Instance.RegisterInputAction("SetSpeed2", MappedInputKeyboardType.KeyUp, () => SetTimeScalePosition(3));
            KeyboardManager.Instance.RegisterInputAction("SetSpeed3", MappedInputKeyboardType.KeyUp, () => SetTimeScalePosition(4));
            KeyboardManager.Instance.RegisterInputAction("DecreaseSpeed", MappedInputKeyboardType.KeyUp, DecreaseTimeScale);
            KeyboardManager.Instance.RegisterInputAction("IncreaseSpeed", MappedInputKeyboardType.KeyUp, IncreaseTimeScale);
        }

        public void Destroy()
        {
            _instance = null;
        }
        
        // Systems that update every frame.
        public event System.Action<float> EveryFrame;

        // Systems that update every frame not in Modal.
        public event System.Action<float> EveryFrameNotModal;

        // Systems that update every frame while unpaused.
        public event System.Action<float> EveryFrameUnpaused;

        // Update the total time and invoke the required events.
        public void Update(float time)
        {
            var deltaTime = time * _timeScale;

            // Systems that update every frame not in Modal.
            if (!IsModal) {
                InvokeEvent(EveryFrameNotModal, time);
            }

            // Systems that update every frame while unpaused.
            if (!IsPaused && !IsModal) {
                GameTime += deltaTime;
                InvokeEvent(EveryFrameUnpaused, deltaTime);
            }
            
            // Systems that update every frame.
            InvokeEvent(EveryFrame, time);
        }

        // Sets the speed of the game. Greater time scale position equals greater speed.
        private void SetTimeScalePosition(int newTimeScalePosition)
        {
            if (newTimeScalePosition >= _possibleTimeScales.Length || newTimeScalePosition < 0 || newTimeScalePosition == _timeScalePosition) {
                return;
            }

            _timeScalePosition = newTimeScalePosition;
            _timeScale = _possibleTimeScales[newTimeScalePosition];
            Debug.Log($"* Game speed set to {_timeScale} X");
        }

        // Increases the game speed by increasing the time scale by 1.
        private void IncreaseTimeScale()
        {
            SetTimeScalePosition(_timeScalePosition + 1);
        }

        // Decreases the game speed by decreasing the time scale by 1.
        private void DecreaseTimeScale()
        {
            SetTimeScalePosition(_timeScalePosition - 1);
        }

        // Invokes the given event action.
        private static void InvokeEvent(System.Action<float> action, float time)
        {
            action?.Invoke(time);
        }
    }
}
