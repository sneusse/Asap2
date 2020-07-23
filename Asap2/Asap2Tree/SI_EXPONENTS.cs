namespace Asap2
{
    /// <summary>
    ///     Specifies the exponents of the seven SI base units to express this derived SI unit.
    /// </summary>
    [Base(IsSimple = true)]
    public class SI_EXPONENTS : Asap2Base
    {
        [Element(5, IsArgument = true)] public long AmountOfSubstance;

        [Element(3, IsArgument = true)] public long ElectricCurrent;

        [Element(0, IsArgument = true)] public long Length;

        [Element(6, IsArgument = true)] public long LuminousIntensity;

        [Element(1, IsArgument = true)] public long Mass;

        [Element(4, IsArgument = true)] public long Temperature;

        [Element(2, IsArgument = true)] public long Time;

        public SI_EXPONENTS(Location location, long Length, long Mass, long Time, long ElectricCurrent,
            long Temperature, long AmountOfSubstance, long LuminousIntensity) : base(location)
        {
            this.Length = Length;
            this.Mass = Mass;
            this.Time = Time;
            this.ElectricCurrent = ElectricCurrent;
            this.Temperature = Temperature;
            this.LuminousIntensity = LuminousIntensity;
        }
    }
}