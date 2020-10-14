using System;

namespace uzSurfaceMapper.Core.Attrs.CodeAnalysis
{
    /// <summary>
    ///     The code with this attribute must be reviewed
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.All)]
    public class MustBeReviewed : Attribute
    {
    }

    /// <summary>
    ///     The code with this attribute is bugged
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.All)]
    public class Bugged : Attribute
    {
    }

    /// <summary>
    ///     The code with this attribute is in (work in progress)
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.All)]
    public class WIP : Attribute
    {
    }

    /// <summary>
    ///     The code with this attribute is only for debug purpouses
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.All)]
    public class OnlyDebug : Attribute
    {
    }

    /// <summary>
    ///     The code with this attribute is being tested or part of a test
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.All)]
    public class Testing : Attribute
    {
    }

    /// <summary>
    ///     The code with this attribute needs to be changed
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.All)]
    public class ChangeCode : Attribute
    {
    }

    /// <summary>
    ///     The code with this attribute isn't recommended to be used
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.All)]
    public class NotRecommended : Attribute
    {
    }
}