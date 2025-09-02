using System;

namespace LaserControllerApp.Models
{
    /// <summary>
    /// Represents the operational state of the laser.
    /// </summary>
    public enum LaserState
    {
        /// <summary>
        /// Laser is idle, not performing any operation.
        /// </summary>
        Idle,

        /// <summary>
        /// Laser is preparing to arm.
        /// </summary>
        Arming,

        /// <summary>
        /// Laser is charging its capacitors.
        /// </summary>
        Charging,

        /// <summary>
        /// Laser is armed and ready to fire.
        /// </summary>
        Armed,

        /// <summary>
        /// Laser is actively firing or running.
        /// </summary>
        Running,

        /// <summary>
        /// Laser has completed its operation.
        /// </summary>
        Finished,

        /// <summary>
        /// Laser is transitioning to a paused state.
        /// </summary>
        Pausing,

        /// <summary>
        /// Laser is paused.
        /// </summary>
        Paused,

        /// <summary>
        /// Laser is disarming.
        /// </summary>
        Disarming,

        /// <summary>
        /// Laser is discharging its capacitors.
        /// </summary>
        Discharging,

        /// <summary>
        /// An error occurred during firing.
        /// </summary>
        FireError
    }

    /// <summary>
    /// Defines the trigger source for the laser.
    /// </summary>
    public enum TriggerMode
    {
        Internal,
        External
    }

    /// <summary>
    /// Defines the firing mode of the laser.
    /// </summary>
    public enum FireMode
    {
        Continuous,
        Burst
    }

    /// <summary>
    /// Defines the shutter operation mode.
    /// </summary>
    public enum ShutterMode
    {
        Auto,
        Manual
    }

    /// <summary>
    /// Defines the shutter state.
    /// </summary>
    public enum ShutterState
    {
        Closed,
        Open
    }

    /// <summary>
    /// Defines the password protection mode.
    /// </summary>
    public enum PasswordMode
    {
        None,
        User,
        Factory
    }

    /// <summary>
    /// Defines the soft start mode for the laser.
    /// </summary>
    public enum SoftStartMode
    {
        Off,
        On
    }

    /// <summary>
    /// Defines the condition of an interlock.
    /// </summary>
    public enum InterlockCondition
    {
        Good,
        Fault,
        Recovered,
        Disabled
    }
}