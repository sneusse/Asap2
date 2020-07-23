using System;
using System.Collections.Generic;
using System.Linq;

namespace Asap2
{
    public class Asap2File : IValidator
    {
        private readonly List<Asap2Base> elementsInternal = new List<Asap2Base>();

        /// <summary>
        ///     Default no-arg constructor.
        /// </summary>
        public Asap2File() : this("")
        {
        }

        /// <summary>
        ///     Constructor with filename parameter.
        /// </summary>
        /// <param name="baseFilename"></param>
        public Asap2File(string baseFilename)
        {
            /* Default version if ASAP2_VERSION element is missing. */
            asap2_version = new ASAP2_VERSION(new Location(baseFilename), 1, 51);
        }

        /// <summary>
        ///     Filename to use for base validation.
        /// </summary>
        public string baseFilename { get; set; }

        public ASAP2_VERSION asap2_version { private set; get; }

        public A2ML_VERSION a2ml_version { private set; get; }

        public List<Asap2Base> elements
        {
            get { return elementsInternal; }
        }

        public void Validate(IErrorReporter errorReporter)
        {
            var projects = elements.FindAll(x => x.GetType() == typeof(PROJECT));

            if (projects == null || projects.Count == 0)
            {
                errorReporter.reportError(baseFilename + " : No PROJECT found, must be one");
                throw new ValidationErrorException(baseFilename + " : No PROJECT found, must be one");
            }

            if (projects.Count > 1)
            {
                projects[projects.Count - 1]
                    .reportErrorOrWarning("Second PROJECT found, shall only be one", false, errorReporter);
            }

            var asap2_versions = elements.FindAll(x => x.GetType() == typeof(ASAP2_VERSION));
            if (asap2_versions != null && asap2_versions.Count > 0)
            {
                if (asap2_versions.Count > 1)
                    asap2_versions[asap2_versions.Count - 1]
                        .reportErrorOrWarning("Second ASAP2_VERSION found, shall only be one", false, errorReporter);
                if (asap2_versions[0].OrderID >= projects[0].OrderID)
                    asap2_versions[0].reportErrorOrWarning("ASAP2_VERSION shall be placed before PROJECT", false,
                        errorReporter);
            }
            else
            {
                asap2_version.reportErrorOrWarning(
                    "Mandatory element ASAP2_VERSION is not found, version of the file is set to 1.5.1", false,
                    errorReporter);
            }

            if (asap2_version.VersionNo != 1)
                asap2_version.reportErrorOrWarning(
                    "ASAP2_VERSION.VersionNo is not 1. This parser is primarly designed for version 1.", false,
                    errorReporter);
            else if (asap2_version.UpgradeNo < 60)
                asap2_version.reportErrorOrWarning(
                    "ASAP2_VERSION is less than 1.6.0. This parser is primarly designed for version 1.6.0 and newer.",
                    false, errorReporter);

            var a2ml_versions = elements.FindAll(x => x.GetType() == typeof(A2ML_VERSION));
            if (a2ml_versions != null && a2ml_versions.Count > 0)
            {
                if (a2ml_versions.Count > 1)
                    a2ml_versions[a2ml_versions.Count - 1]
                        .reportErrorOrWarning("Second A2ML_VERSION found, shall only be one", false, errorReporter);
                if (a2ml_versions[0].OrderID >= projects[0].OrderID)
                    asap2_versions[0].reportErrorOrWarning("A2ML_VERSION shall be placed before PROJECT", false,
                        errorReporter);
            }

            var project = projects[0] as PROJECT;
            project.Validate(errorReporter);
        }

        public void AddAsap2_version(ASAP2_VERSION asap2_version)
        {
            var version = elements.FirstOrDefault(x => x.GetType() == typeof(ASAP2_VERSION)) as ASAP2_VERSION;
            if (version != null) elements.Remove(version);
            elementsInternal.Add(asap2_version);
            this.asap2_version = asap2_version;
        }

        public void AddA2ml_version(A2ML_VERSION a2ml_version)
        {
            var version = elements.FirstOrDefault(x => x.GetType() == typeof(A2ML_VERSION)) as A2ML_VERSION;
            if (version != null) elements.Remove(version);
            elementsInternal.Add(a2ml_version);
            this.a2ml_version = a2ml_version;
        }
    }

    [Base(IsSimple = true)]
    public class ASAP2_VERSION : Asap2Base
    {
        [Element(1, IsArgument = true)] public uint UpgradeNo;

        [Element(0, IsArgument = true)] public uint VersionNo;

        public ASAP2_VERSION(Location location, uint VersionNo, uint UpgradeNo) : base(location)
        {
            this.VersionNo = VersionNo;
            this.UpgradeNo = UpgradeNo;
        }
    }

    [Base(IsSimple = true)]
    public class A2ML_VERSION : Asap2Base
    {
        [Element(1, IsArgument = true)] public uint UpgradeNo;

        [Element(0, IsArgument = true)] public uint VersionNo;

        public A2ML_VERSION(Location location, uint VersionNo, uint UpgradeNo) : base(location)
        {
            this.VersionNo = VersionNo;
            this.UpgradeNo = UpgradeNo;
        }
    }

    [Base]
    public class PROJECT : Asap2Base, IValidator
    {
        [Element(2)] public HEADER header;

        [Element(1, IsString = true)] public string LongIdentifier;

        /// <summary>
        ///     Dictionary with the project modules. The key is the name of the module.
        /// </summary>
        [Element(3, IsDictionary = true)] public Dictionary<string, MODULE> modules;

        [Element(0, IsArgument = true)] public string name;

        public PROJECT(Location location) : base(location)
        {
            modules = new Dictionary<string, MODULE>();
        }

        public void Validate(IErrorReporter errorReporter)
        {
            if (modules.Count == 0)
            {
                reportErrorOrWarning("No MODULE found, must be atleast one", true, errorReporter);
                throw new ValidationErrorException("No MODULE found, must be atleast one");
            }

            foreach (var mod in modules.Values) mod.Validate(errorReporter);
        }
    }

    [Base]
    public class HEADER : Asap2Base
    {
        [Element(0, IsString = true, ForceNewLine = true)]
        public string longIdentifier;

        [Element(2, IsArgument = true, Name = "PROJECT_NO")]
        public string project_no;

        [Element(1, IsString = true, Name = "VERSION")]
        public string version;

        public HEADER(Location location) : base(location)
        {
        }
    }

    [Base(IsSimple = true)]
    public class ALIGNMENT : Asap2Base
    {
        public enum ALIGNMENT_type
        {
            ALIGNMENT_BYTE,
            ALIGNMENT_WORD,
            ALIGNMENT_LONG,
            ALIGNMENT_INT64,
            ALIGNMENT_FLOAT32_IEEE,
            ALIGNMENT_FLOAT64_IEEE
        }

        public ALIGNMENT_type type;

        [Element(1, IsArgument = true)] public uint value;

        public ALIGNMENT(Location location, ALIGNMENT_type type, uint value) : base(location)
        {
            this.type = type;
            this.value = value;
            name = Enum.GetName(type.GetType(), type);
        }

        [Element(0, IsName = true)] public string name { get; private set; }
    }

    [Base]
    public class AXIS_DESCR : Asap2Base
    {
        /// <summary>
        ///     Specifies the properties of an axis that belongs to a tunable curve, map or cuboid.
        /// </summary>
        public enum Attribute
        {
            /// <summary>
            ///     Axis shared by various tables and rescaled, i.e. normalized, by a curve (CURVE_AXIS_REF).
            /// </summary>
            CURVE_AXIS,

            /// <summary>
            ///     Axis shared by various tables.
            /// </summary>
            COM_AXIS,

            /// <summary>
            ///     Axis specific to one table with calculated axis points. Axis points are not stored in ECU memory
            /// </summary>
            FIX_AXIS,

            /// <summary>
            ///     Axis shared by various tables and rescaled, i.e. normalized, by another axis (AXIS_PTS_REF).
            /// </summary>
            RES_AXIS,

            /// <summary>
            ///     Axis specific to one table.
            /// </summary>
            STD_AXIS
        }

        [Element(7, IsList = true)] public List<ANNOTATION> annotation = new List<ANNOTATION>();

        [Element(1, IsArgument = true, Comment = " Type           ")]
        public Attribute attribute;

        /// <summary>
        ///     Reference to <see cref="AXIS_PTS" /> in case the axis values are stored in a different memory location than the
        ///     values of
        ///     the <see cref="CHARACTERISTIC" /> the axis description belongs to.
        /// </summary>
        [Element(8, IsArgument = true, Name = "AXIS_PTS_REF")]
        public string axis_pts_ref;

        [Element(9)] public BYTE_ORDER byte_order;

        [Element(3, IsArgument = true, Comment = " Conversion     ")]
        public string Conversion;

        /// <summary>
        ///     Reference to the curve's <see cref="CHARACTERISTIC" /> that is used to normalize or scale the axis.
        /// </summary>
        [Element(10, IsArgument = true, Name = "CURVE_AXIS_REF")]
        public string curve_axis_ref;

        [Element(11)] public DEPOSIT deposit;

        [Element(12)] public EXTENDED_LIMITS extended_limits;

        [Element(13)] public FIX_AXIS_PAR fix_axis_par;

        [Element(14)] public FIX_AXIS_PAR_DIST fix_axis_par_dist;

        [Element(15)] public FIX_AXIS_PAR_LIST fix_axis_par_list;

        [Element(16, IsString = true, Name = "FORMAT")]
        public string format;

        [Element(2, IsArgument = true, Comment = " InputQuantity  ")]
        public string InputQuantity;

        [Element(5, IsArgument = true, Comment = " LowerLimit     ")]
        public decimal LowerLimit;

        /// <summary>
        ///     Specifies the maximum permissible gradient for this axis.
        /// </summary>
        [Element(17, IsArgument = true, Name = "MAX_GRAD")]
        public decimal? max_grad;

        [Element(4, IsArgument = true, Comment = " MaxAxisPoints  ")]
        public ulong MaxAxisPoints;

        [Element(18)] public MONOTONY monotony;

        /// <summary>
        ///     Specifies the physical unit. Overrules the unit specified in the referenced <see cref="COMPU_METHOD" />.
        /// </summary>
        [Element(19, IsString = true, Name = "PHYS_UNIT")]
        public string phys_unit;

        [Element(20, Comment = "Write-access is not allowed for this AXIS_DESCR")]
        public READ_ONLY read_only;

        /// <summary>
        ///     Specifies an increment value that is added or subtracted when using the up/down keys while calibrating.
        /// </summary>
        [Element(21, IsArgument = true, Name = "STEP_SIZE")]
        public decimal? step_size;

        [Element(6, IsArgument = true, Comment = " UpperLimit     ")]
        public decimal UpperLimit;

        public AXIS_DESCR(Location location, Attribute attribute, string InputQuantity, string Conversion,
            ulong MaxAxisPoints, decimal LowerLimit, decimal UpperLimit) : base(location)
        {
            this.attribute = attribute;
            this.InputQuantity = InputQuantity;
            this.Conversion = Conversion;
            this.MaxAxisPoints = MaxAxisPoints;
            this.LowerLimit = LowerLimit;
            this.UpperLimit = UpperLimit;
        }
    }

    /// <summary>
    ///     Specifies position, datatype, index increment and addressing method of the X, Y, Z, Z4 or Z5 axis points in memory.
    /// </summary>
    [Base(IsSimple = true)]
    public class AXIS_PTS_XYZ45 : Asap2Base
    {
        [Element(4, IsArgument = true, Comment = " addrType  ")]
        public AddrType addrType;

        [Element(2, IsArgument = true, Comment = " dataType  ")]
        public DataType dataType;

        [Element(3, IsArgument = true, Comment = " indexIncr ")]
        public IndexOrder indexIncr;

        [Element(1, IsArgument = true, Comment = " Position  ")]
        public ulong Position;

        public AXIS_PTS_XYZ45(Location location, string Name, ulong Position, DataType dataType, IndexOrder indexIncr,
            AddrType addrType) : base(location)
        {
            this.Name = Name;
            this.Position = Position;
            this.dataType = dataType;
            this.indexIncr = indexIncr;
            this.addrType = addrType;
        }

        [Element(0, IsName = true)] public string Name { get; private set; }
    }

    /// <summary>
    ///     Specifies the rescale mapping between stored axis points and used points for curve and maps.
    /// </summary>
    [Base(IsSimple = true)]
    public class AXIS_RESCALE_XYZ45 : Asap2Base
    {
        [Element(5, IsArgument = true, Comment = " addrType            ")]
        public AddrType addrType;

        [Element(2, IsArgument = true, Comment = " dataType            ")]
        public DataType dataType;

        [Element(4, IsArgument = true, Comment = " indexIncr           ")]
        public IndexOrder indexIncr;

        [Element(3, IsArgument = true, Comment = " MaxNoOfRescalePairs ")]
        public ulong MaxNoOfRescalePairs;

        [Element(1, IsArgument = true, Comment = " Position            ")]
        public ulong Position;

        public AXIS_RESCALE_XYZ45(Location location, string Name, ulong Position, DataType dataType,
            ulong MaxNoOfRescalePairs, IndexOrder indexIncr, AddrType addrType) : base(location)
        {
            this.Name = Name;
            this.Position = Position;
            this.dataType = dataType;
            this.MaxNoOfRescalePairs = MaxNoOfRescalePairs;
            this.indexIncr = indexIncr;
            this.addrType = addrType;
        }

        [Element(0, IsName = true)] public string Name { get; private set; }
    }

    /// <summary>
    ///     Specifies position and datatype of the distance (i.e. slope) value within the record layout.
    ///     The distance value is used to calculate the axis points for the described FIX_AXIS.
    /// </summary>
    [Base(IsSimple = true)]
    public class DIST_OP_XYZ45 : Asap2Base
    {
        [Element(2, IsArgument = true, Comment = " dataType            ")]
        public DataType dataType;

        [Element(1, IsArgument = true, Comment = " Position            ")]
        public ulong Position;

        public DIST_OP_XYZ45(Location location, string Name, ulong Position, DataType dataType) : base(location)
        {
            this.Name = Name;
            this.Position = Position;
            this.dataType = dataType;
        }

        [Element(0, IsName = true)] public string Name { get; private set; }
    }

    /// <summary>
    ///     Specifies the number of axis points. This number is fixed and not stored in memory.
    /// </summary>
    [Base(IsSimple = true)]
    public class FIX_NO_AXIS_PTS_XYZ45 : Asap2Base
    {
        [Element(1, IsArgument = true, Comment = " NumberOfAxisPoints ")]
        public ulong NumberOfAxisPoints;

        public FIX_NO_AXIS_PTS_XYZ45(Location location, string Name, ulong NumberOfAxisPoints) : base(location)
        {
            this.Name = Name;
            this.NumberOfAxisPoints = NumberOfAxisPoints;
        }

        [Element(0, IsName = true)] public string Name { get; private set; }
    }

    /// <summary>
    ///     Specifies position and data type of an identification number for the stored object.
    /// </summary>
    [Base(IsSimple = true)]
    public class IDENTIFICATION : Asap2Base
    {
        [Element(2, IsArgument = true, Comment = " DataType ")]
        public DataType dataType;

        [Element(1, IsArgument = true, Comment = " Position ")]
        public ulong Position;

        public IDENTIFICATION(Location location, ulong Position, DataType dataType) : base(location)
        {
            this.Position = Position;
            this.dataType = dataType;
        }
    }

    /// <summary>
    ///     Specifies position and datatype of the number of axis points within the record layout.
    /// </summary>
    [Base(IsSimple = true)]
    public class NO_AXIS_PTS_XYZ45 : Asap2Base
    {
        [Element(2, IsArgument = true, Comment = " dataType ")]
        public DataType dataType;

        [Element(1, IsArgument = true, Comment = " Position ")]
        public ulong Position;

        public NO_AXIS_PTS_XYZ45(Location location, string Name, ulong Position, DataType dataType) : base(location)
        {
            this.Name = Name;
            this.Position = Position;
            this.dataType = dataType;
        }

        [Element(0, IsName = true)] public string Name { get; private set; }
    }

    /// <summary>
    ///     Specifies position and datatype of the number of rescaling values within the record layout.
    /// </summary>
    [Base(IsSimple = true)]
    public class NO_RESCALE_XYZ45 : Asap2Base
    {
        [Element(2, IsArgument = true, Comment = " dataType ")]
        public DataType dataType;

        [Element(1, IsArgument = true, Comment = " Position ")]
        public ulong Position;

        public NO_RESCALE_XYZ45(Location location, string Name, ulong Position, DataType dataType) : base(location)
        {
            this.Name = Name;
            this.Position = Position;
            this.dataType = dataType;
        }

        [Element(0, IsName = true)] public string Name { get; private set; }
    }

    /// <summary>
    ///     Specifies position and datatype of the offset value within the record layout.
    ///     The offset value is used to calculate the axis points for the described FIX_AXIS.
    /// </summary>
    [Base(IsSimple = true)]
    public class OFFSET_XYZ45 : Asap2Base
    {
        [Element(2, IsArgument = true, Comment = " dataType ")]
        public DataType dataType;

        [Element(1, IsArgument = true, Comment = " Position ")]
        public ulong Position;

        public OFFSET_XYZ45(Location location, string Name, ulong Position, DataType dataType) : base(location)
        {
            this.Name = Name;
            this.Position = Position;
            this.dataType = dataType;
        }

        [Element(0, IsName = true)] public string Name { get; private set; }
    }

    /// <summary>
    ///     Specifies a position in this record layout that shall be ignored (i.e. not interpreted).
    /// </summary>
    [Base(IsSimple = true)]
    public class RESERVED_DISTAB_MEMORY : Asap2Base
    {
        [Element(2, IsArgument = true, Comment = " DataSize ")]
        public DataSize dataSize;

        [Element(1, IsArgument = true, Comment = " Position ")]
        public ulong Position;

        public RESERVED_DISTAB_MEMORY(Location location, ulong Position, DataSize dataSize) : base(location)
        {
            this.Position = Position;
            this.dataSize = dataSize;
        }
    }

    /// <summary>
    ///     Specifies position and datatype to store the result of interpolation for the X, Y, Z, Z4 or Z5 axis and the look-up
    ///     table's output W.
    /// </summary>
    [Base(IsSimple = true)]
    public class RIP_ADDR_WXYZ45 : Asap2Base
    {
        [Element(2, IsArgument = true, Comment = " DataType ")]
        public DataType dataType;

        [Element(1, IsArgument = true, Comment = " Position ")]
        public ulong Position;

        public RIP_ADDR_WXYZ45(Location location, string Name, ulong Position, DataType dataType) : base(location)
        {
            this.Name = Name;
            this.Position = Position;
            this.dataType = dataType;
        }

        [Element(0, IsName = true)] public string Name { get; private set; }
    }

    /// <summary>
    ///     Specifies position and datatype of the power-of-two exponent of the distance (i.e. slope) value within the record
    ///     layout.
    ///     The distance value is used to calculate the axis points for the described FIX_AXIS.
    /// </summary>
    [Base(IsSimple = true)]
    public class SHIFT_OP_XYZ45 : Asap2Base
    {
        [Element(2, IsArgument = true, Comment = " DataType ")]
        public DataType dataType;

        [Element(1, IsArgument = true, Comment = " Position ")]
        public ulong Position;

        public SHIFT_OP_XYZ45(Location location, string Name, ulong Position, DataType dataType) : base(location)
        {
            this.Name = Name;
            this.Position = Position;
            this.dataType = dataType;
        }

        [Element(0, IsName = true)] public string Name { get; private set; }
    }

    /// <summary>
    ///     Specifies position and datatype of the address of the axis' input value within the record layout.
    /// </summary>
    [Base(IsSimple = true)]
    public class SRC_ADDR_XYZ45 : Asap2Base
    {
        [Element(2, IsArgument = true, Comment = " DataType ")]
        public DataType dataType;

        [Element(1, IsArgument = true, Comment = " Position ")]
        public ulong Position;

        public SRC_ADDR_XYZ45(Location location, string Name, ulong Position, DataType dataType) : base(location)
        {
            this.Name = Name;
            this.Position = Position;
            this.dataType = dataType;
        }

        [Element(0, IsName = true)] public string Name { get; private set; }
    }

    [Base(IsSimple = true)]
    public class STATIC_RECORD_LAYOUT : Asap2Base
    {
        public STATIC_RECORD_LAYOUT(Location location) : base(location)
        {
        }
    }

    [Base(IsSimple = true)]
    public class DEPOSIT : Asap2Base
    {
        public enum DEPOSIT_type
        {
            ABSOLUTE,
            DIFFERENCE
        }

        [Element(0, IsArgument = true)] public DEPOSIT_type value;

        public DEPOSIT(Location location, DEPOSIT_type value) : base(location)
        {
            this.value = value;
        }
    }

    /// <summary>
    ///     Specifies which kind of monotony for the sample values is allowed for a <see cref="AXIS_DESCR" /> or
    ///     <see cref="AXIS_PTS" />.
    /// </summary>
    [Base(IsSimple = true)]
    public class MONOTONY : Asap2Base
    {
        public enum MONOTONY_type
        {
            /// <summary>
            ///     Monotonously decreasing.
            /// </summary>
            MON_DECREASE,

            /// <summary>
            ///     Monotonously increasing.
            /// </summary>
            MON_INCREASE,

            /// <summary>
            ///     Strict monotonously decreasing.
            /// </summary>
            STRICT_DECREASE,

            /// <summary>
            ///     Strict monotonously increasing.
            /// </summary>
            STRICT_INCREASE,

            /// <summary>
            ///     Monotonously in- or decreasing.
            /// </summary>
            MONOTONOUS,

            /// <summary>
            ///     Strict monotonously in- or decreasing.
            /// </summary>
            STRICT_MON,

            /// <summary>
            ///     No monotony required.
            /// </summary>
            NOT_MON
        }

        [Element(0, IsArgument = true)] public MONOTONY_type value;

        public MONOTONY(Location location, MONOTONY_type value) : base(location)
        {
            this.value = value;
        }
    }


    /// <summary>
    ///     Specifies the value of the first sample point, the power-of-two exponent of the increment value and total number of
    ///     sample points for computing the sample point values of an equidistant axis of type FIX_AXIS.
    /// </summary>
    [Base(IsSimple = true)]
    public class FIX_AXIS_PAR : Asap2Base
    {
        /// <summary>
        ///     Number of axis points.
        /// </summary>
        [Element(0, IsArgument = true, Comment = " Number of axis points ")]
        public ulong NumberAPo;

        [Element(0, IsArgument = true, Comment = " Offset                ")]
        public long Offset;

        [Element(0, IsArgument = true, Comment = " Shift                 ")]
        public long Shift;

        public FIX_AXIS_PAR(Location location, long Offset, long Shift, ulong NumberAPo) : base(location)
        {
            this.Offset = Offset;
            this.Shift = Shift;
            this.NumberAPo = NumberAPo;
        }
    }

    /// <summary>
    ///     Specifies the value of the first sample point, the increment value and the total number of sample points
    ///     for computing the sample point values of an equidistant axis of type FIX_AXIS.
    /// </summary>
    [Base(IsSimple = true)]
    public class FIX_AXIS_PAR_DIST : Asap2Base
    {
        [Element(0, IsArgument = true, Comment = " Distance              ")]
        public long Distance;

        /// <summary>
        ///     Number of axis points.
        /// </summary>
        [Element(0, IsArgument = true, Comment = " Number of axis points ")]
        public ulong NumberAPo;

        [Element(0, IsArgument = true, Comment = " Offset                ")]
        public long Offset;

        public FIX_AXIS_PAR_DIST(Location location, long Offset, long Distance, ulong NumberAPo) : base(location)
        {
            this.Offset = Offset;
            this.Distance = Distance;
            this.NumberAPo = NumberAPo;
        }
    }

    /// <summary>
    ///     Explicitly specifies the sample point values of the axis of type FIX_AXIS.
    /// </summary>
    [Base]
    public class FIX_AXIS_PAR_LIST : Asap2Base
    {
        [Element(0, IsArgument = true, IsList = true, Comment = " Sample point values ")]
        public List<decimal> AxisPts_Values = new List<decimal>();

        public FIX_AXIS_PAR_LIST(Location location) : base(location)
        {
        }
    }

    /// <summary>
    ///     Specifies position, datatype, orientation and addressing method to store table data in the
    ///     <see cref="RECORD_LAYOUT" />.
    /// </summary>
    [Base(IsSimple = true)]
    public class FNC_VALUES : Asap2Base
    {
        public enum IndexMode
        {
            ALTERNATE_CURVES,
            ALTERNATE_WITH_X,
            ALTERNATE_WITH_Y,
            COLUMN_DIR,
            ROW_DIR
        }

        [Element(3, IsArgument = true)] public AddrType addrType;

        [Element(1, IsArgument = true)] public DataType dataType;

        [Element(2, IsArgument = true)] public IndexMode indexMode;

        [Element(0, IsArgument = true)] public ulong Position;

        public FNC_VALUES(Location location, ulong Position, DataType dataType, IndexMode indexMode,
            AddrType addrType) : base(location)
        {
            this.Position = Position;
            this.dataType = dataType;
            this.indexMode = indexMode;
            this.addrType = addrType;
        }
    }

    [Base(IsSimple = true)]
    public class BYTE_ORDER : Asap2Base
    {
        public enum BYTE_ORDER_type
        {
            LITTLE_ENDIAN,
            BIG_ENDIAN,
            MSB_FIRST,
            MSB_LAST
        }

        [Element(0, IsArgument = true)] public BYTE_ORDER_type value;

        public BYTE_ORDER(Location location, BYTE_ORDER_type value) : base(location)
        {
            this.value = value;
        }
    }

    [Base]
    public class MOD_COMMON : Asap2Base
    {
        [Element(1, IsDictionary = true)] public Dictionary<string, ALIGNMENT> alignments;

        [Element(2)] public BYTE_ORDER byte_order;

        [Element(3, IsArgument = true, Name = "DATA_SIZE")]
        public ulong? data_size;

        [Element(4)] public DEPOSIT deposit;

        [Element(5, IsArgument = true, Name = "S_REC_LAYOUT")]
        public string s_rec_layout;

        public MOD_COMMON(Location location, string Comment) : base(location)
        {
            alignments = new Dictionary<string, ALIGNMENT>();
            this.Comment = Comment;
        }

        [Element(0, IsString = true)] public string Comment { get; private set; }
    }

    /// <summary>
    ///     Describes how structures of tunable parameters (<see cref="CHARACTERISTIC" />) and axes (<see cref="AXIS_PTS" />)
    ///     are stored in memory.
    ///     It describes byte alignments, order and position of calibration objects in memory, their rescaling, memory offset
    ///     and further properties.
    /// </summary>
    [Base]
    public class RECORD_LAYOUT : Asap2Base
    {
        [Element(1, IsDictionary = true)] public Dictionary<string, ALIGNMENT> alignments;

        [Element(2, IsDictionary = true)]
        public Dictionary<string, AXIS_PTS_XYZ45> axis_pts_xyz45 = new Dictionary<string, AXIS_PTS_XYZ45>();

        [Element(3, IsDictionary = true)]
        public Dictionary<string, AXIS_RESCALE_XYZ45> axis_rescale_xyz45 = new Dictionary<string, AXIS_RESCALE_XYZ45>();

        [Element(4, IsDictionary = true)]
        public Dictionary<string, DIST_OP_XYZ45> dist_op_xyz45 = new Dictionary<string, DIST_OP_XYZ45>();

        [Element(5, IsDictionary = true)]
        public Dictionary<string, FIX_NO_AXIS_PTS_XYZ45> fix_no_axis_pts_xyz45 =
            new Dictionary<string, FIX_NO_AXIS_PTS_XYZ45>();

        [Element(6)] public FNC_VALUES fnc_values;

        [Element(7)] public IDENTIFICATION identification;

        [Element(8)]
        public Dictionary<string, NO_AXIS_PTS_XYZ45> no_axis_pts_xyz45 = new Dictionary<string, NO_AXIS_PTS_XYZ45>();

        [Element(9)]
        public Dictionary<string, NO_RESCALE_XYZ45> no_rescale_xyz45 = new Dictionary<string, NO_RESCALE_XYZ45>();

        [Element(10)] public Dictionary<string, OFFSET_XYZ45> offset_xyz45 = new Dictionary<string, OFFSET_XYZ45>();

        [Element(11)] public RESERVED_DISTAB_MEMORY reserved_distab_memory;

        [Element(12)]
        public Dictionary<string, RIP_ADDR_WXYZ45> rip_addr_wxyz45 = new Dictionary<string, RIP_ADDR_WXYZ45>();

        [Element(13)]
        public Dictionary<string, SHIFT_OP_XYZ45> shift_op_xyz45 = new Dictionary<string, SHIFT_OP_XYZ45>();

        [Element(14)]
        public Dictionary<string, SRC_ADDR_XYZ45> src_addr_xyz45 = new Dictionary<string, SRC_ADDR_XYZ45>();

        /// <summary>
        ///     Specifies that a tunable axis with a dynamic number of axis points does not compact or expand in memory when
        ///     removing or inserting axis points.
        /// </summary>
        [Element(15)] public STATIC_RECORD_LAYOUT static_record_layout;

        public RECORD_LAYOUT(Location location, string Name) : base(location)
        {
            this.Name = Name;
        }

        [Element(0, IsArgument = true)] public string Name { get; private set; }
    }

    [Base]
    public class IF_DATA : Asap2Base
    {
        [Element(0, IsArgument = true)] public string data;

        public IF_DATA(Location location, string data) : base(location)
        {
            this.data = data;
            char[] delimiterChars = {' ', '\t'};
            var words = data.Split(delimiterChars);
            name = words[0];
        }

        public string name { get; private set; }
    }

    [Base]
    public class A2ML : Asap2Base
    {
        [Element(0, IsArgument = true)] public string data;

        public A2ML(Location location, string data) : base(location)
        {
            this.data = data;
        }
    }

    [Base(IsSimple = true)]
    public class EXTENDED_LIMITS : Asap2Base
    {
        [Element(1, IsArgument = true, Comment = " LowerLimit     ")]
        public decimal LowerLimit;

        [Element(2, IsArgument = true, Comment = " UpperLimit     ")]
        public decimal UpperLimit;

        public EXTENDED_LIMITS(Location location, decimal LowerLimit, decimal UpperLimit) : base(location)
        {
            this.LowerLimit = LowerLimit;
            this.UpperLimit = UpperLimit;
        }
    }

    /// <summary>
    ///     Lists the FUNCTIONs in which this object is listed. Obsolete keyword. Please use <see cref="FUNCTION" /> instead.
    /// </summary>
    [Base(IsObsolete = "Obsolete keyword. Please use FUNCTION instead.")]
    public class FUNCTION_LIST : Asap2Base
    {
        [Element(0, IsArgument = true, IsList = true, Comment = " List of functions. ")]
        public List<string> functions = new List<string>();

        public FUNCTION_LIST(Location location) : base(location)
        {
        }
    }

    [Base]
    public class VIRTUAL : Asap2Base
    {
        [Element(0, IsArgument = true, IsList = true, Comment = " MeasuringChannels ")]
        public List<string> MeasuringChannel = new List<string>();

        public VIRTUAL(Location location) : base(location)
        {
        }
    }


    [Base(IsSimple = true)]
    public class SYMBOL_LINK : Asap2Base
    {
        [Element(1, IsArgument = true, Comment = " Offset     ")]
        public ulong Offset;

        [Element(0, IsArgument = true, Comment = " SymbolName ")]
        public string SymbolName;

        public SYMBOL_LINK(Location location, string SymbolName, ulong Offset) : base(location)
        {
            this.SymbolName = SymbolName;
            this.Offset = Offset;
        }
    }

    [Base(IsSimple = true)]
    public class MAX_REFRESH : Asap2Base
    {
        [Element(1, IsArgument = true, Comment = " Rate        ")]
        public ulong Rate;

        [Element(0, IsArgument = true, Comment = " ScalingUnit ")]
        public ulong ScalingUnit;

        public MAX_REFRESH(Location location, ulong ScalingUnit, ulong Rate) : base(location)
        {
            this.ScalingUnit = ScalingUnit;
            this.Rate = Rate;
        }
    }

    [Base(IsSimple = true)]
    public class ECU_ADDRESS_EXTENSION : Asap2Base
    {
        [Element(0, IsArgument = true, CodeAsHex = true)]
        public ulong value;

        public ECU_ADDRESS_EXTENSION(Location location, ulong value) : base(location)
        {
            this.value = value;
        }
    }

    [Base(IsSimple = true)]
    public class ECU_ADDRESS : Asap2Base
    {
        [Element(0, IsArgument = true, CodeAsHex = true)]
        public ulong value;

        public ECU_ADDRESS(Location location, ulong value) : base(location)
        {
            this.value = value;
        }
    }

    [Base(IsSimple = true)]
    public class ADDR_EPK : Asap2Base
    {
        [Element(0, IsArgument = true, CodeAsHex = true)]
        public ulong Address;

        public ADDR_EPK(Location location, ulong Address) : base(location)
        {
            this.Address = Address;
        }
    }

    [Base]
    public class ANNOTATION : Asap2Base
    {
        [Element(0)] public ANNOTATION_LABEL annotation_label;

        [Element(1)] public ANNOTATION_ORIGIN annotation_origin;

        [Element(2)] public ANNOTATION_TEXT annotation_text;

        public ANNOTATION(Location location) : base(location)
        {
        }
    }

    [Base(IsSimple = true)]
    public class ANNOTATION_LABEL : Asap2Base
    {
        [Element(0, IsString = true)] public string value;

        public ANNOTATION_LABEL(Location location, string value) : base(location)
        {
            this.value = value;
        }
    }

    [Base(IsSimple = true)]
    public class ANNOTATION_ORIGIN : Asap2Base
    {
        [Element(0, IsString = true)] public string value;

        public ANNOTATION_ORIGIN(Location location, string value) : base(location)
        {
            this.value = value;
        }
    }

    [Base]
    public class ANNOTATION_TEXT : Asap2Base
    {
        [Element(0, IsString = true, IsList = true)]
        public List<string> data = new List<string>();

        public ANNOTATION_TEXT(Location location) : base(location)
        {
        }
    }

    [Base(IsSimple = true, IsObsolete = "Obsolete keyword. Please use MATRIX_DIM instead.")]
    public class ARRAY_SIZE : Asap2Base
    {
        [Element(0, IsArgument = true)] public ulong value;

        public ARRAY_SIZE(Location location, ulong value) : base(location)
        {
            this.value = value;
        }
    }

    [Base]
    public class BIT_OPERATION : Asap2Base
    {
        [Element(2)] public LEFT_SHIFT left_shift;

        [Element(0)] public RIGHT_SHIFT right_shift;

        [Element(3)] public SIGN_EXTEND sign_extend;

        public BIT_OPERATION(Location location) : base(location)
        {
        }
    }

    [Base(IsSimple = true)]
    public class RIGHT_SHIFT : Asap2Base
    {
        [Element(0, IsArgument = true)] public ulong value;

        public RIGHT_SHIFT(Location location, ulong value) : base(location)
        {
            this.value = value;
        }
    }

    [Base(IsSimple = true)]
    public class LEFT_SHIFT : Asap2Base
    {
        [Element(0, IsArgument = true)] public ulong value;

        public LEFT_SHIFT(Location location, ulong value) : base(location)
        {
            this.value = value;
        }
    }

    [Base(IsSimple = true)]
    public class SIGN_EXTEND : Asap2Base
    {
        public SIGN_EXTEND(Location location) : base(location)
        {
        }
    }

    [Base(IsSimple = true)]
    public class CALIBRATION_ACCESS : Asap2Base
    {
        public enum CALIBRATION_ACCESS_type
        {
            CALIBRATION,
            NO_CALIBRATION,
            NOT_IN_MCD_SYSTEM,
            OFFLINE_CALIBRATION
        }

        [Element(0, IsArgument = true)] public CALIBRATION_ACCESS_type value;

        public CALIBRATION_ACCESS(Location location, CALIBRATION_ACCESS_type value) : base(location)
        {
            this.value = value;
        }
    }

    [Base(IsSimple = true)]
    public class COEFFS : Asap2Base
    {
        [Element(0, IsArgument = true, Comment = " Coefficients for the rational function (RAT_FUNC) ")]
        public decimal a;

        [Element(1, IsArgument = true)] public decimal b;

        [Element(2, IsArgument = true)] public decimal c;

        [Element(3, IsArgument = true)] public decimal d;

        [Element(4, IsArgument = true)] public decimal e;

        [Element(5, IsArgument = true)] public decimal f;

        public COEFFS(Location location, decimal a, decimal b, decimal c, decimal d, decimal e, decimal f) :
            base(location)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.e = e;
            this.f = f;
        }
    }

    [Base(IsSimple = true)]
    public class COEFFS_LINEAR : Asap2Base
    {
        [Element(0, IsArgument = true, Comment = " Coefficients for the linear function (LINEAR). ")]
        public decimal a;

        [Element(1, IsArgument = true)] public decimal b;

        public COEFFS_LINEAR(Location location, decimal a, decimal b) : base(location)
        {
            this.a = a;
            this.b = b;
        }
    }

    [Base]
    public class FORMULA : Asap2Base
    {
        /// <summary>
        ///     Specifies a conversion formula to calculate the physical value from the ECU-internal value.
        ///     Expression of the formula complies with ANSI C notation.
        ///     Shall be used only, if linear or rational functions are not sufficient.
        /// </summary>
        [Element(1, IsString = true)] public string formula;

        /// <summary>
        ///     Specifies a conversion formula to calculate the ECU-internal value from the physical value.
        ///     Is the inversion of the referenced FORMULA.
        ///     Expression of the formula complies to ANSI C notation.
        /// </summary>
        [Element(2, IsString = true, Name = "FORMULA_INV")]
        public string formula_inv;

        public FORMULA(Location location, string formula) : base(location)
        {
            this.formula = formula;
        }

        public void Validate(IErrorReporter errorReporter, MODULE module)
        {
        }
    }

    [Base]
    public class COMPU_TAB : Asap2Base
    {
        [Element(5, IsList = true, Comment = " ValuePairs       ")]
        public List<COMPU_TAB_DATA> data;

        [Element(6, IsString = true, Name = "DEFAULT_VALUE")]
        public string default_value;

        [Element(7, IsArgument = true, Name = "DEFAULT_VALUE_NUMERIC")]
        public decimal default_value_numeric;

        public COMPU_TAB(Location location, string Name, string LongIdentifier, ConversionType conversionType,
            uint NumberValuePairs) : base(location)
        {
            this.Name = Name;
            this.LongIdentifier = LongIdentifier;
            this.conversionType = conversionType;
            parsedNumberValuePairs = NumberValuePairs;
            data = new List<COMPU_TAB_DATA>();
        }

        /// <summary>
        ///     Set from parsed data. Only used to validate the parsed list of <see cref="COMPU_TAB_DATA" />.
        /// </summary>
        public uint parsedNumberValuePairs { get; private set; }

        [Element(1, IsArgument = true, Comment = " Name             ")]
        public string Name { get; private set; }

        [Element(2, IsString = true, Comment = " LongIdentifier   ")]
        public string LongIdentifier { get; private set; }

        [Element(3, IsArgument = true, Comment = " ConversionType   ")]
        public ConversionType conversionType { get; private set; }

        [Element(4, IsArgument = true, Comment = " NumberValuePairs ")]
        public uint NumberValuePairs
        {
            get { return (uint) data.Count; }
        }
    }

    [Base(IsSimple = true)]
    public class COMPU_TAB_DATA : Asap2Base
    {
        [Element(1, IsArgument = true)] public decimal InVal;

        [Element(0, IsName = true)] public string name = "";

        [Element(2, IsString = true)] public decimal OutVal;

        public COMPU_TAB_DATA(Location location, decimal InVal, decimal OutVal) : base(location)
        {
            this.InVal = InVal;
            this.OutVal = OutVal;
        }
    }

    [Base]
    public class COMPU_VTAB : Asap2Base
    {
        [Element(5, IsList = true, Comment = " ValuePairs       ")]
        public List<COMPU_VTAB_DATA> data;

        [Element(6, IsString = true, Name = "DEFAULT_VALUE")]
        public string default_value;

        public COMPU_VTAB(Location location, string Name, string LongIdentifier, ConversionType conversionType,
            uint NumberValuePairs) : base(location)
        {
            this.Name = Name;
            this.LongIdentifier = LongIdentifier;
            this.conversionType = conversionType;
            parsedNumberValuePairs = NumberValuePairs;
            data = new List<COMPU_VTAB_DATA>();
        }

        /// <summary>
        ///     Set from parsed data. Only used to validate the parsed list of <see cref="COMPU_VTAB_DATA" />.
        /// </summary>
        public uint parsedNumberValuePairs { get; private set; }

        [Element(1, IsArgument = true, Comment = " Name             ")]
        public string Name { get; private set; }

        [Element(2, IsString = true, Comment = " LongIdentifier   ")]
        public string LongIdentifier { get; private set; }

        [Element(3, IsArgument = true, Comment = " ConversionType   ")]
        public ConversionType conversionType { get; private set; }

        [Element(4, IsArgument = true, Comment = " NumberValuePairs ")]
        public uint NumberValuePairs
        {
            get { return (uint) data.Count; }
        }
    }

    [Base(IsSimple = true)]
    public class COMPU_VTAB_DATA : Asap2Base
    {
        [Element(1, IsArgument = true)] public decimal InVal;

        [Element(0, IsName = true)] public string name = "";

        [Element(2, IsString = true)] public string OutVal;

        public COMPU_VTAB_DATA(Location location, decimal InVal, string OutVal) : base(location)
        {
            this.InVal = InVal;
            this.OutVal = OutVal;
        }
    }

    [Base]
    public class COMPU_VTAB_RANGE : Asap2Base
    {
        [Element(4, IsList = true)] public List<COMPU_VTAB_RANGE_DATA> data;

        [Element(5, IsString = true, Name = "DEFAULT_VALUE")]
        public string default_value;

        public COMPU_VTAB_RANGE(Location location, string Name, string LongIdentifier, uint NumberValueTriples) :
            base(location)
        {
            this.Name = Name;
            this.LongIdentifier = LongIdentifier;
            parsedNumberValueTriples = NumberValueTriples;
            data = new List<COMPU_VTAB_RANGE_DATA>();
        }

        /// <summary>
        ///     Set from parsed data. Only used to validate the parsed list of <see cref="COMPU_VTAB_RANGE_DATA" />.
        /// </summary>
        public uint parsedNumberValueTriples { get; private set; }

        [Element(1, IsArgument = true, Comment = " Name               ")]
        public string Name { get; private set; }

        [Element(2, IsString = true, Comment = " LongIdentifier     ")]
        public string LongIdentifier { get; private set; }

        [Element(3, IsArgument = true, Comment = " NumberValueTriples ")]
        public uint NumberValueTriples
        {
            get { return (uint) data.Count; }
        }
    }

    [Base(IsSimple = true)]
    public class COMPU_VTAB_RANGE_DATA : Asap2Base
    {
        [Element(2, IsArgument = true)] public decimal InValMax;

        [Element(1, IsArgument = true)] public decimal InValMin;

        [Element(0, IsName = true)] public string name = "";

        [Element(3, IsString = true)] public string value;

        public COMPU_VTAB_RANGE_DATA(Location location, decimal InValMin, decimal InValMax, string value) :
            base(location)
        {
            this.InValMin = InValMin;
            this.InValMax = InValMax;
            this.value = value;
        }
    }

    [Base(IsSimple = true)]
    public class MATRIX_DIM : Asap2Base
    {
        [Element(0, IsArgument = true)] public uint xDim;

        [Element(1, IsArgument = true)] public uint yDim;

        [Element(2, IsArgument = true)] public uint zDim;

        public MATRIX_DIM(Location location, uint xDim, uint yDim, uint zDim) : base(location)
        {
            this.xDim = xDim;
            this.yDim = yDim;
            this.zDim = zDim;
        }
    }

    [Base]
    public class MEMORY_SEGMENT : Asap2Base
    {
        public enum Attribute
        {
            INTERN,
            EXTERN
        }

        public enum MemoryType
        {
            EEPROM,
            EPROM,
            FLASH,
            RAM,
            ROM,
            REGISTER
        }

        public enum PrgType
        {
            CALIBRATION_VARIABLES,
            CODE,
            DATA,
            EXCLUDED_FROM_FLASH,
            OFFLINE_DATA,
            RESERVED,
            SERAM,
            VARIABLES
        }

        [Element(5, IsArgument = true, Comment = " Address    ", CodeAsHex = true)]
        public ulong address;

        [Element(4, IsArgument = true, Comment = " Attribute  ")]
        public Attribute attribute;

        [Element(12, IsList = true)] public List<IF_DATA> if_data = new List<IF_DATA>();

        [Element(1, IsString = true)] public string longIdentifier;

        [Element(3, IsArgument = true, Comment = " MemoryType ")]
        public MemoryType memoryType;

        [Element(7, IsArgument = true, Comment = " offset     ")]
        public long offset0;

        [Element(8, IsArgument = true)] public long offset1;

        [Element(9, IsArgument = true)] public long offset2;

        [Element(10, IsArgument = true)] public long offset3;

        [Element(11, IsArgument = true)] public long offset4;

        [Element(2, IsArgument = true, Comment = " PrgTypes   ")]
        public PrgType prgType;

        [Element(6, IsArgument = true, Comment = " Size       ", CodeAsHex = true)]
        public ulong size;

        public MEMORY_SEGMENT(Location location, string Name, string longIdentifier, PrgType prgType,
            MemoryType memoryType, Attribute attribute, ulong address, ulong size,
            long offset0, long offset1, long offset2, long offset3, long offset4) : base(location)
        {
            this.Name = Name;
            this.longIdentifier = longIdentifier;
            this.prgType = prgType;
            this.memoryType = memoryType;
            this.attribute = attribute;
            this.address = address;
            this.size = size;
            this.offset0 = offset0;
            this.offset1 = offset1;
            this.offset2 = offset2;
            this.offset3 = offset3;
            this.offset4 = offset4;
        }

        [Element(0, IsArgument = true)] public string Name { get; private set; }
    }

    /// <summary>
    ///     Description of the memory layout of an ECU. Obsolete keyword. Please use <see cref="MEMORY_SEGMENT" /> instead.
    /// </summary>
    [Base(IsObsolete = "Obsolete keyword. Please use MEMORY_SEGMENT instead.")]
    public class MEMORY_LAYOUT : Asap2Base
    {
        public enum PrgType
        {
            PRG_CODE,
            PRG_DATA,
            PRG_RESERVED
        }

        [Element(1, IsArgument = true, Comment = " Address              ", CodeAsHex = true)]
        public ulong Address;

        [Element(8, IsList = true)] public List<IF_DATA> if_data = new List<IF_DATA>();

        [Element(3, IsArgument = true, Comment = " offset               ")]
        public long offset0;

        [Element(4, IsArgument = true)] public long offset1;

        [Element(5, IsArgument = true)] public long offset2;

        [Element(6, IsArgument = true)] public long offset3;

        [Element(7, IsArgument = true)] public long offset4;

        [Element(0, IsArgument = true, Comment = " Program segment type ")]
        public PrgType prgType;

        [Element(2, IsArgument = true, Comment = " Size                 ", CodeAsHex = true)]
        public ulong Size;

        public MEMORY_LAYOUT(Location location, PrgType prgType, ulong Address, ulong Size,
            long offset0, long offset1, long offset2, long offset3, long offset4) : base(location)
        {
            this.prgType = prgType;
            this.Address = Address;
            this.Size = Size;
            this.offset0 = offset0;
            this.offset1 = offset1;
            this.offset2 = offset2;
            this.offset3 = offset3;
            this.offset4 = offset4;
        }
    }

    [Base]
    public class CALIBRATION_METHOD : Asap2Base
    {
        [Element(2)] public CALIBRATION_HANDLE calibration_handle;

        [Element(0, IsString = true, Comment = " Method  ")]
        public string Method;

        [Element(1, IsArgument = true, Comment = " Version ")]
        public ulong Version;

        public CALIBRATION_METHOD(Location location, string Method, ulong Version) : base(location)
        {
            this.Method = Method;
            this.Version = Version;
        }
    }

    [Base]
    public class CALIBRATION_HANDLE : Asap2Base
    {
        [Element(0, IsArgument = true, ForceNewLine = true, IsList = true, CodeAsHex = true, Comment = " Handles ")]
        public List<long> Handles = new List<long>();

        [Element(1, IsString = true, Name = "CALIBRATION_HANDLE_TEXT")]
        public string text;

        public CALIBRATION_HANDLE(Location location) : base(location)
        {
        }
    }

    [Base(IsSimple = true)]
    public class SYSTEM_CONSTANT : Asap2Base
    {
        public SYSTEM_CONSTANT(Location location, string name, string value) : base(location)
        {
            this.name = name;
            this.value = value;
        }

        [Element(1, IsString = true)] public string name { get; private set; }

        [Element(1, IsString = true)] public string value { get; private set; }
    }

    [Base]
    public class MOD_PAR : Asap2Base
    {
        [Element(2, IsList = true)] public List<ADDR_EPK> addr_epk = new List<ADDR_EPK>();

        [Element(3, IsList = true)] public List<CALIBRATION_METHOD> calibration_method = new List<CALIBRATION_METHOD>();

        [Element(1, IsString = true)] public string comment;

        [Element(4, IsString = true, Name = "CPU_TYPE")]
        public string cpu_type;

        [Element(5, IsString = true, Name = "CUSTOMER")]
        public string customer;

        [Element(6, IsString = true, Name = "CUSTOMER_NO")]
        public string customer_no;

        [Element(7, IsString = true, Name = "ECU")]
        public string ecu;

        [Element(8, IsArgument = true, Name = "ECU_CALIBRATION_OFFSET")]
        public long? ecu_calibration_offset;

        [Element(9, IsString = true, Name = "EPK")]
        public string epk;

        [Element(10, IsList = true)] public List<MEMORY_LAYOUT> memory_layout = new List<MEMORY_LAYOUT>();

        [Element(11, IsDictionary = true)]
        public Dictionary<string, MEMORY_SEGMENT> memory_segment = new Dictionary<string, MEMORY_SEGMENT>();

        [Element(12, IsArgument = true, Name = "NO_OF_INTERFACES")]
        public ulong? no_of_interfaces;

        [Element(13, IsString = true, Name = "PHONE_NO")]
        public string phone_no;

        [Element(14, IsString = true, Name = "SUPPLIER")]
        public string supplier;

        [Element(15, IsDictionary = true)]
        public Dictionary<string, SYSTEM_CONSTANT> system_constants = new Dictionary<string, SYSTEM_CONSTANT>();

        [Element(16, IsString = true, Name = "USER")]
        public string user;

        [Element(17, IsString = true, Name = "VERSION")]
        public string version;

        public MOD_PAR(Location location, string comment) : base(location)
        {
            this.comment = comment;
        }
    }

    [Base(IsSimple = true)]
    public class DISCRETE : Asap2Base
    {
        public DISCRETE(Location location) : base(location)
        {
        }
    }

    [Base(IsSimple = true)]
    public class READ_ONLY : Asap2Base
    {
        public READ_ONLY(Location location) : base(location)
        {
        }
    }

    [Base(IsSimple = true)]
    public class READ_WRITE : Asap2Base
    {
        public READ_WRITE(Location location) : base(location)
        {
        }
    }

    [Base(IsSimple = true)]
    public class GUARD_RAILS : Asap2Base
    {
        public GUARD_RAILS(Location location) : base(location)
        {
        }
    }

    [Base]
    public class GROUP : Asap2Base
    {
        [Element(3, IsList = true)] public List<ANNOTATION> annotation = new List<ANNOTATION>();

        [Element(4)] public FUNCTION_LIST function_list;

        [Element(5, IsList = true)] public List<IF_DATA> if_data = new List<IF_DATA>();

        [Element(6)] public REF_CHARACTERISTIC ref_characteristic;

        [Element(7)] public REF_MEASUREMENT ref_measurement;

        [Element(8)] public ROOT root;

        [Element(9)] public SUB_GROUP sub_group;

        public GROUP(Location location, string Name, string GroupLongIdentifier) : base(location)
        {
            this.Name = Name;
            this.GroupLongIdentifier = GroupLongIdentifier;
        }

        [Element(1, IsArgument = true, Comment = " GroupName           ")]
        public string Name { get; private set; }

        [Element(2, IsString = true, Comment = " GroupLongIdentifier ")]
        public string GroupLongIdentifier { get; private set; }
    }

    [Base]
    public class REF_MEASUREMENT : Asap2Base
    {
        [Element(0, IsArgument = true, IsList = true, Comment = " Measurement references ")]
        public List<string> reference = new List<string>();

        public REF_MEASUREMENT(Location location) : base(location)
        {
        }
    }

    [Base]
    public class SUB_GROUP : Asap2Base
    {
        [Element(0, IsArgument = true, IsList = true, Comment = " Sub groups ")]
        public List<string> groups = new List<string>();

        public SUB_GROUP(Location location) : base(location)
        {
        }
    }

    [Base(IsSimple = true)]
    public class ROOT : Asap2Base
    {
        public ROOT(Location location) : base(location)
        {
        }
    }

    /// <summary>
    ///     Lists the maps which comprise a cuboid.
    /// </summary>
    [Base]
    public class MAP_LIST : Asap2Base
    {
        [Element(1, IsArgument = true, IsList = true)]
        public List<string> MapList = new List<string>();

        public MAP_LIST(Location location) : base(location)
        {
        }
    }

    /// <summary>
    ///     Specifies a formula to calculate the initialization value of this virtual characteristic based upon referenced
    ///     <see cref="CHARACTERISTIC" />s.
    ///     The value of the virtual characteristic is not stored in ECU memory. It is typically used to calculate
    ///     <see cref="DEPENDENT_CHARACTERISTIC" />s.
    /// </summary>
    [Base]
    public class VIRTUAL_CHARACTERISTIC : Asap2Base
    {
        [Element(1, IsArgument = true, IsList = true)]
        public List<string> Characteristic = new List<string>();

        [Element(0, IsString = true)] public string Formula;

        public VIRTUAL_CHARACTERISTIC(Location location, string Formula) : base(location)
        {
            this.Formula = Formula;
        }
    }

    /// <summary>
    ///     The value of the <see cref="CHARACTERISTIC" />, which references this DEPENDENT_CHARACTERISTIC, is calculated
    ///     instead of read from ECU memory.
    ///     DEPENDENT_CHARACTERISTIC specifies a formula and references to other parameters (in memory or virtual) for the
    ///     purpose to calculate the value.
    ///     The value changes automatically, once one of the referenced parameters has changed its value.
    /// </summary>
    [Base]
    public class DEPENDENT_CHARACTERISTIC : Asap2Base
    {
        [Element(1, IsArgument = true, IsList = true)]
        public List<string> Characteristic = new List<string>();

        [Element(0, IsString = true)] public string Formula;

        public DEPENDENT_CHARACTERISTIC(Location location, string Formula) : base(location)
        {
            this.Formula = Formula;
        }
    }
}