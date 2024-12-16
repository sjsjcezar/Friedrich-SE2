using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;

namespace WorldTime
{
    [RequireComponent(typeof(Light2D))]
    public class WorldTime : MonoBehaviour
    {
        public static WorldTime Instance { get; private set; }

        [Header("Time Settings")]
        public float timeTick = 1f; // Speed of time progression

        [Header("Lighting Settings")]
        public Gradient lightGradient; // Gradient for light color transitions
        [Tooltip("Time to activate lights (in decimal, e.g., 18.42 for 6:25 PM)")]
        public float startLightActivation = 18.42f; // 6:25 PM
        [Tooltip("Time to deactivate lights (in decimal, e.g., 6.75 for 6:45 AM)")]
        public float stopLightDeactivation = 6.75f; // 6:45 AM
        public float nightIntensity = 1f; // Intensity when it’s night
        public float dayIntensity = 0f; // Intensity when it’s day

        public event Action<float, bool> OnTimeChanged;

        private Light2D mainLight;
        private bool previousIsNight;

        private float baseIntensity = 1f;  // Base intensity that can be modified externally
        public bool overrideIntensity { get; private set; } = false;  // Flag to indicate if intensity is being overridden

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            mainLight = GetComponent<Light2D>();
        }

        private void Start()
        {
            // Initialize global time with TimeConfig
            TimeConfig config = FindObjectOfType<TimeConfig>();
            if (config != null)
            {
                GlobalVariables.hours = Mathf.Clamp(config.startingHour, 0, 23);
                GlobalVariables.minutes = Mathf.Clamp(config.startingMinute, 0, 59);
                timeTick = Mathf.Clamp(config.timeTick, 0.1f, 10f);
            }

            // Initialize environment based on starting time
            previousIsNight = IsNightTime();
            UpdateEnvironment();
        }

        private void FixedUpdate()
        {
            UpdateTime();
            bool currentIsNight = IsNightTime();

            // Detect transition
            if (currentIsNight != previousIsNight)
            {
                previousIsNight = currentIsNight;
                OnTimeChanged?.Invoke(GlobalVariables.hours + (GlobalVariables.minutes / 60f), currentIsNight);
            }

            UpdateEnvironment();
        }

        /// <summary>
        /// Updates the global time variables based on the time tick.
        /// </summary>
        private void UpdateTime()
        {
            // Increment seconds based on fixed delta time and tick rate
            GlobalVariables.seconds += Time.fixedDeltaTime * timeTick;

            if (GlobalVariables.seconds >= 60f)
            {
                GlobalVariables.seconds = 0f;
                GlobalVariables.minutes += 1;
            }

            if (GlobalVariables.minutes >= 60)
            {
                GlobalVariables.minutes = 0;
                GlobalVariables.hours += 1;
            }

            if (GlobalVariables.hours >= 24)
            {
                GlobalVariables.hours = 0;
                GlobalVariables.days += 1;
                GlobalVariables.dayOfWeek += 1;

                if (GlobalVariables.dayOfWeek > 7)
                {
                    GlobalVariables.dayOfWeek = 1; // Reset to Mondas
                }

                // Check if month needs to be incremented
                if (GlobalVariables.days > GlobalVariables.monthDays[GlobalVariables.month - 1])
                {
                    GlobalVariables.days = 1;
                    GlobalVariables.month += 1;

                    if (GlobalVariables.month > 12)
                    {
                        GlobalVariables.month = 1;
                        GlobalVariables.year += 1;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the environment (lighting) based on the current time.
        /// </summary>
        private void UpdateEnvironment()
        {
            // Calculate current decimal time
            float currentTime = GlobalVariables.hours + (GlobalVariables.minutes / 60f);

            // Update main light color based on gradient
            if (lightGradient != null)
            {
                mainLight.color = lightGradient.Evaluate(currentTime / 24f);
            }

            // Only update intensity if not being overridden
            if (!overrideIntensity)
            {
                bool isNight = IsNightTime();
                mainLight.intensity = (isNight ? nightIntensity : dayIntensity) * baseIntensity;
            }
        }

        public void SetOverrideIntensity(bool shouldOverride)  // Changed parameter name from 'override'
        {
            overrideIntensity = shouldOverride;
            if (!shouldOverride)
            {
                // When releasing override, update to current time-based intensity
                UpdateEnvironment();
            }
        }

        public void SetBaseIntensity(float intensity)
        {
            baseIntensity = intensity;
            if (!overrideIntensity)
            {
                UpdateEnvironment();
            }
        }

        public void SetLightIntensity(float intensity)
        {
            if (mainLight != null)
            {
                mainLight.intensity = intensity;
            }
        }

        /// <summary>
        /// Determines if it is currently night time.
        /// </summary>
        public bool IsNightTime()
        {
            float currentTime = GlobalVariables.hours + (GlobalVariables.minutes / 60f);
            return currentTime >= startLightActivation || currentTime < stopLightDeactivation;
        }

        /// <summary>
        /// Sets the world to daytime immediately.
        /// </summary>
        public void SetToDaytime()
        {
            GlobalVariables.hours = 12; // Set to noon
            UpdateEnvironment();
            bool isNight = false;
            OnTimeChanged?.Invoke(GlobalVariables.hours + (GlobalVariables.minutes / 60f), isNight);
        }

        /// <summary>
        /// Sets the world to nighttime immediately.
        /// </summary>
        public void SetToNighttime()
        {
            GlobalVariables.hours = 20; // Set to 8 PM
            UpdateEnvironment();
            bool isNight = true;
            OnTimeChanged?.Invoke(GlobalVariables.hours + (GlobalVariables.minutes / 60f), isNight);
        }
    }
}