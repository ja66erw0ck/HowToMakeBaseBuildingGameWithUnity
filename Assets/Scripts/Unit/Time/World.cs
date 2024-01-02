using System;
using System.Globalization;
using System.Text;
using UnityEngine;
using WorldTimeUnit = Unit.Time.World;

namespace Unit.Time
{
    public struct World : IFormattable
    {
        private const int DAYS_PER_QUARTER = 15;
        private const int STARTING_YEAR = 2999;
        private const float SECONDS_PER_MINUTE = 60;
        private const float SECONDS_PER_HOUR = SECONDS_PER_MINUTE * 60;
        private const float SECONDS_PER_DAY = SECONDS_PER_HOUR * 24;
        private const float SECONDS_PER_QUARTER = SECONDS_PER_DAY * DAYS_PER_QUARTER;
        private const float SECONDS_PER_YEAR = SECONDS_PER_DAY * 4;

        // Gets the seconds since Epoch date (Midnight Q1 Day 1 2999).
        public float Seconds { get; set; }
        // Gets or sets the second component.
        public int Second {
            get => (int) (Seconds % 60);
            set {
                Seconds -= Second;
                Seconds += value;
            }
        }
        // Gets the minutes since Epoch date (Midnight Q1 Day 1 2999).
        public int Minutes => (int) Seconds / 60;
        // Gets or sets the minute component.
        public int Minute {
            get => Minutes % 60;
            set {
                Seconds -= Minute * SECONDS_PER_MINUTE;
                Seconds += value * SECONDS_PER_MINUTE;
            }
        }
        // Gets the hours since Epoch date (Midnight Q1 Day 1 2999).
        public int Hours => Minutes / 60;
        // Gets or sets the hour component.
        public int Hour {
            get => Hours % 24;
            set {
                Seconds -= Hour * SECONDS_PER_HOUR;
                Seconds += value * SECONDS_PER_HOUR;
            }
        }
        // Gets the days since Epoch date (Midnight Q1 Day 1 2999).
        public int Days => Hours / 24;
        // Gets or sets the day component.
        public int Day {
            get => (Days % DAYS_PER_QUARTER) + 1;
            set {
                Seconds -= Day * SECONDS_PER_DAY;
                Seconds += value * SECONDS_PER_DAY;
            }
        }
        // Gets the quarters since Epoch date (Midnight Q1 Day 1 2999).
        public int Quarters => Days / DAYS_PER_QUARTER;
        // Gets or sets the quarter component.
        public int Quarter {
            get => (Quarters % 4) + 1;
            set {
                Seconds -= Quarter * SECONDS_PER_QUARTER;
                Seconds += value * SECONDS_PER_QUARTER;
            }
        }
        // Gets the years since Epoch date (Midnight Q1 Day 1 2999).
        public int Years => Quarters / 4;
        // Gets or sets the year component.
        public int Year {
            get => Years + STARTING_YEAR;
            set {
                Seconds -= Years * SECONDS_PER_YEAR;
                Seconds += (value - STARTING_YEAR) * SECONDS_PER_YEAR;
            }
        }

        public World(int seconds)
        {
            Seconds = seconds;
        }

        public World(int hour, int minute, int second, int day, int quarter, int year)
        {
            if (hour < 0 || hour > 23) {
                Debug.LogError("! Hour component should be an int from 0-23. Defaulting to 0.");
            }

            if (minute < 0 || minute > 59) {
                Debug.LogError("! Minute component should be an int from 0-59. Defaulting to 0.");
            }

            if (second < 0 || second > 59) {
                Debug.LogError("! Second component should be an int from 0-59. Defaulting to 0.");
            }

            if (day < 1 || day > DAYS_PER_QUARTER) {
                Debug.LogError($"Day component should be an int from 1-{DAYS_PER_QUARTER}. Defaulting to 1.");
            }

            if (quarter < 1 || quarter > 4) {
                Debug.LogError("! Quarter component should be an int from 1-4. Defauling to 1.");
                quarter = 1;
            }

            if (year < 0) {
                Debug.LogError($"Year component should be an int greater than or equal to {STARTING_YEAR}. Defaulting to {STARTING_YEAR}");
                year = STARTING_YEAR;
            }

            year -= STARTING_YEAR;

            var timeComponent = (int) (second + (minute * SECONDS_PER_MINUTE) + (hour * SECONDS_PER_HOUR));
            var dateComponent = (int) (((day - 1) * SECONDS_PER_DAY) + ((quarter - 1) * SECONDS_PER_QUARTER) + (year * SECONDS_PER_YEAR));
            Seconds = timeComponent + dateComponent;
        }

        public World(float seconds)
        {
            Seconds = seconds;
        }

        // operator
        public static WorldTimeUnit operator +(WorldTimeUnit time1, WorldTimeUnit time2)
        {
            var worldTime = new WorldTimeUnit(time1.Seconds + time2.Seconds);
            return worldTime;
        }

        public static WorldTimeUnit operator -(WorldTimeUnit time1, WorldTimeUnit time2)
        {
            var worldTime = new WorldTimeUnit(time1.Seconds - time2.Seconds);
            return worldTime;
        }

        public static bool operator <(WorldTimeUnit time1, WorldTimeUnit time2)
        {
            return time1.Seconds < time2.Seconds;
        }

        public static bool operator <=(WorldTimeUnit time1, WorldTimeUnit time2)
        {
            return time1.Seconds <= time2.Seconds;
        }

        public static bool operator >(WorldTimeUnit time1, WorldTimeUnit time2)
        {
            return time1.Seconds > time2.Seconds;
        }

        public static bool operator >=(WorldTimeUnit time1, WorldTimeUnit time2)
        {
            return time1.Seconds >= time2.Seconds;
        }

        public static bool operator ==(WorldTimeUnit time1, WorldTimeUnit time2)
        {
            return Math.Abs(time1.Seconds - time2.Seconds) < float.Epsilon;
        }

        public static bool operator !=(WorldTimeUnit time1, WorldTimeUnit time2)
        {
            return Math.Abs(time1.Seconds - time2.Seconds) > float.Epsilon;
        }

        public override bool Equals(object timeObject)
        {
            if (timeObject.Equals(this)) {
                return true;
            }

            return timeObject is WorldTimeUnit time && time == this;
        }

        public override int GetHashCode()
        {
            return Seconds.GetHashCode();
        }

        public override string ToString()
        {
            // Note: overloading is used, rather than defaults so that this plays nicely with Lua, which can't see default parameter values properly.
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0:y}/{0:q}/{0:d} {0:h}:{0:mm}:{0:tt}");
            return stringBuilder.ToString();
        }

        // Adds seconds to the WorldTimeUnit object.
        public WorldTimeUnit AddSeconds(int seconds)
        {
            AddSeconds((float) seconds);
            return this;
        }

        // Adds seconds to the WorldTimeUnit object.
        public WorldTimeUnit AddSeconds(float seconds)
        {
            Seconds += seconds;
            return this;
        }

        // Adds minutes to the WorldTimeUnit object.
        public WorldTimeUnit AddMinutes(int minutes)
        {
            Seconds += minutes * SECONDS_PER_MINUTE;
            return this;
        }

        // Adds hours to the WorldTimeUnit object.
        public WorldTimeUnit AddHours(int hours)
        {
            Seconds += hours * SECONDS_PER_HOUR;
            return this;
        }

        // Adds days to the WorldTimeUnit object.
        public WorldTimeUnit AddDays(int days)
        {
            Seconds += days * SECONDS_PER_DAY;
            return this;
        }

        // Adds quarters to the WorldTimeUnit object.
        public WorldTimeUnit AddQuarters(int quarters)
        {
            Seconds += quarters * SECONDS_PER_QUARTER;
            return this;
        }

        // Adds years to the WorldTimeUnit object.
        public WorldTimeUnit AddYears(int years)
        {
            Seconds += years * SECONDS_PER_YEAR;
            return this;
        }

        // Sets the second component.
        public WorldTimeUnit SetSecond(int secondComponent)
        {
            Second = secondComponent;
            return this;
        }

        // Sets the minute component.
        public WorldTimeUnit SetMinute(int minuteComponent)
        {
            Minute = minuteComponent;
            return this;
        }

        // Sets the hour component.
        public WorldTimeUnit SetHour(int hourComponent)
        {
            Hour = hourComponent;
            return this;
        }

        // Sets the day component.
        public WorldTimeUnit SetDay(int dayComponent)
        {
            Day = dayComponent;
            return this;
        }

        // Sets the quarter component.
        public WorldTimeUnit SetQuarter(int quarterComponent)
        {
            Quarter = quarterComponent;
            return this;
        }

        // Sets the year component.
        public WorldTimeUnit SetYear(int yearComponent)
        {
            Year = yearComponent;
            return this;
        }
       
        public string ToString(string format, IFormatProvider provider = null)
        {
            provider ??= CultureInfo.CurrentCulture;
            var dateTimeFormatInfo = (DateTimeFormatInfo) provider.GetFormat(typeof(DateTimeFormatInfo));

            switch (format) {
                case "HH":
                    return Hour.ToString("00");
                case "H":
                    return Hour.ToString();
                case "hh":
                    var longhour = Hour;
                    if (longhour >= 12) {
                        longhour -= 12;
                    }

                    if (longhour == 0) {
                        longhour = 12;
                    }

                    return longhour.ToString("00");
                case "h":
                    var shorthour = Hour;
                    
                    if (shorthour >= 12) {
                        shorthour -= 12;
                    }

                    if (shorthour == 0) {
                        shorthour = 12;
                    }

                    return shorthour.ToString();
                case "mm":
                    return Minute.ToString("00");
                case "m":
                    return Minute.ToString();
                case "ss":
                    return Second.ToString("00");
                case "s":
                    return Second.ToString();
                case "tt":
                    return Hour >= 12 ? dateTimeFormatInfo.PMDesignator : dateTimeFormatInfo.AMDesignator;
                case "q":
                    return Quarter.ToString();
                case "d":
                    return Day.ToString();
                case "dd":
                    return Day.ToString("00");
                case "y":
                    return Year.ToString();
                case "G":
                    return ToString();
                default:
                    return string.Empty;
            }
        }
    }
}