using System.Collections.Generic;

namespace Asap2
{
    /// <summary>
    ///     Combination of variants that are not allowed.
    /// </summary>
    [Base]
    public class VAR_FORBIDDEN_COMB : Asap2Base
    {
        [Element(0, IsArgument = true, IsList = true)]
        public List<Combo> combinations = new List<Combo>();

        public VAR_FORBIDDEN_COMB(Location location) : base(location)
        {
        }

        /// <summary>
        ///     Combination class.
        /// </summary>
        public class Combo
        {
            public Combo(string criterionName, string criterionValue)
            {
                CriterionName = criterionName;
                CriterionValue = criterionValue;
            }

            public string CriterionName { get; private set; }

            public string CriterionValue { get; private set; }

            public override string ToString()
            {
                return CriterionName + " " + CriterionValue;
            }
        }
    }
}