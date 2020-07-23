using System.Collections.Generic;

namespace Asap2
{
    /// <summary>
    ///     Allows to group <see cref="MEASUREMENT" />s to selection lists, which can be chosen in an MC-system
    ///     for selective recording and display of ECU-internal variables.
    ///     FRAMEs are typically used to bundle variables, which shall be measured and viewed together.
    ///     The FRAME keyword can also be used to describe the packaging of ECU-internal variables in a CAN frame.
    /// </summary>
    [Base]
    public class FRAME : Asap2Base
    {
        /// <summary>
        ///     List of <see cref="MEASUREMENT" />s that are bundled in this frame.
        /// </summary>
        [Element(4, IsArgument = true, IsList = true, Name = "FRAME_MEASUREMENT")]
        public List<string> frame_measurement = new List<string>();

        [Element(5, IsList = true)] public List<IF_DATA> if_data = new List<IF_DATA>();

        public FRAME(Location location, string Name, string LongIdentifier, ulong ScalingUnit, ulong Rate) :
            base(location)
        {
            this.Name = Name;
            this.LongIdentifier = LongIdentifier;
            this.ScalingUnit = ScalingUnit;
            this.Rate = Rate;
        }

        [Element(1, IsArgument = true, Comment = " Name           ")]
        public string Name { get; private set; }

        [Element(2, IsString = true, Comment = " LongIdentifier ")]
        public string LongIdentifier { get; private set; }

        [Element(3, IsArgument = true, Comment = " ScalingUnit    ")]
        public ulong ScalingUnit { get; private set; }

        [Element(4, IsArgument = true, Comment = " Rate           ")]
        public ulong Rate { get; private set; }
    }
}