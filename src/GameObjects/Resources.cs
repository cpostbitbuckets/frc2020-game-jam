using Godot;
using System;

/// <summary>
/// A set of resources a player has
/// </summary>
public class Resources
{
    public int Raw { get; set; }
    public int Power { get; set; }
    public int Science { get; set; }

    public Resources(int raw = 0, int power = 0, int science = 0)
    {
        Raw = raw;
        Power = power;
        Science = science;
    }

    /// <summary>
    /// Overload  the index operator so callers can treat Resources like a dictionary
    /// </summary>
    /// <example>
    /// <code>
    /// if (Resources[ResourceType.Raw] > someValue) 
    /// {
    ///     // do something    
    /// }
    /// </code>
    /// </example>
    /// <value></value>
    public int this[ResourceType i]
    {
        get
        {
            switch (i)
            {
                case ResourceType.Raw:
                    return Raw;
                case ResourceType.Power:
                    return Power;
                case ResourceType.Science:
                    return Science;
            }
            return 0;
        }
        set
        {
            switch (i)
            {
                case ResourceType.Raw:
                    Raw = value;
                    break;
                case ResourceType.Power:
                    Power = value;
                    break;
                case ResourceType.Science:
                    Science = value;
                    break;
            }
        }
    }
}
