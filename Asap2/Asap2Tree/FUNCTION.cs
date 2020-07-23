﻿using System.Collections.Generic;

namespace Asap2
{
    [Base]
    public class FUNCTION : Asap2Base
    {
        [Element(3, IsList = true)] public List<ANNOTATION> annotation = new List<ANNOTATION>();

        [Element(4)] public DEF_CHARACTERISTIC def_characteristic;

        /// <summary>
        ///     String that describes the version of this function.
        /// </summary>
        [Element(5, IsString = true, Name = "FUNCTION_VERSION")]
        public string function_version;

        [Element(6, IsList = true)] public List<IF_DATA> if_data = new List<IF_DATA>();

        [Element(8)] public IN_MEASUREMENT in_measurement;

        [Element(9)] public LOC_MEASUREMENT loc_measurement;

        [Element(10)] public OUT_MEASUREMENT out_measurement;

        [Element(7)] public REF_CHARACTERISTIC ref_characteristic;

        [Element(11)] public SUB_FUNCTION sub_function;

        public FUNCTION(Location location, string Name, string LongIdentifier) : base(location)
        {
            this.Name = Name;
            this.LongIdentifier = LongIdentifier;
        }

        [Element(1, IsArgument = true, Comment = " Name           ")]
        public string Name { get; private set; }

        [Element(2, IsString = true, Comment = " LongIdentifier ")]
        public string LongIdentifier { get; private set; }
    }
}