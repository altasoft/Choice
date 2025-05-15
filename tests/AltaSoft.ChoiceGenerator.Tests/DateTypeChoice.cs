using System;
using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests
{
    [Choice]
    public sealed partial record DateTypeChoice
    {
        /// <summary>
        /// <para>Specifies the authorisation, in a coded form.</para>
        /// </summary>
        public partial DateOnly? OnlyDate { get; set; }

        /// <summary>
        /// <para>Specifies the authorisation, in a free text form.</para>
        /// </summary>
        public partial DateTime? DateTimeChoice { get; set; }
    }
}
