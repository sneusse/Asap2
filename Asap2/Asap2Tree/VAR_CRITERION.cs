﻿using System.Collections.Generic;

namespace Asap2
{
    /// <summary>
    ///     Description of a variant criterion. The description consists of a list of named variants and
    ///     a selector variable (reference to a <see cref="MEASUREMENT" /> or <see cref="CHARACTERISTIC" />) defining the
    ///     currently active variant by its value.
    /// </summary>
    [Base]
    public class VAR_CRITERION : Asap2Base
    {
        [Element(3, IsArgument = true, Comment = " Ident          ", IsList = true)]
        public List<string> Idents = new List<string>();

        [Element(4)] public VAR_MEASUREMENT var_measurement;

        [Element(5)] public VAR_SELECTION_CHARACTERISTIC var_selection_characteristic;

        public VAR_CRITERION(Location location, string Name, string LongIdentifier) : base(location)
        {
            this.Name = Name;
            this.LongIdentifier = LongIdentifier;
        }

        [Element(1, IsArgument = true, Comment = " Name           ")]
        public string Name { get; private set; }

        [Element(2, IsString = true, Comment = " LongIdentifier ")]
        public string LongIdentifier { get; private set; }
    }

    /// <summary>
    ///     Reference to a tunable parameter, which selects the active variant by its value.
    ///     The corresponding <see cref="CHARACTERISTIC" /> must refer to a <see cref="COMPU_TAB" />, whose strings
    ///     must correspond with the variant names defined in <see cref="VAR_CRITERION" />.
    /// </summary>
    [Base(IsSimple = true)]
    public class VAR_SELECTION_CHARACTERISTIC : Asap2Base
    {
        public VAR_SELECTION_CHARACTERISTIC(Location location, string Name) : base(location)
        {
            this.Name = Name;
        }

        [Element(0, IsArgument = true)] public string Name { get; private set; }
    }

    /// <summary>
    ///     Reference to an ECU-internal variable, which selects the active variant by its value.
    ///     The corresponding <see cref="MEASUREMENT" /> must refer to a <see cref="COMPU_TAB" />,
    ///     whose strings must correspond with the variant names defined in <see cref="VAR_CRITERION" />.
    /// </summary>
    [Base(IsSimple = true)]
    public class VAR_MEASUREMENT : Asap2Base
    {
        public VAR_MEASUREMENT(Location location, string Name) : base(location)
        {
            this.Name = Name;
        }

        [Element(0, IsArgument = true)] public string Name { get; private set; }
    }
}