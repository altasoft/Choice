using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests
{
    [Choice]
    public sealed partial record TwoValueTypeChoice
    {
        /// <summary>
        /// <para>Specifies the authorisation, in a coded form.</para>
        /// </summary>
        public partial Authorisation1Code? Code { get; set; }

        /// <summary>
        /// <para>Specifies the authorisation, in a free text form.</para>
        /// </summary>
        public partial int? Integer { get; set; }

    }
}
