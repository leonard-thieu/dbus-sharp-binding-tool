namespace Toofz.DBusSharp.CodeDom
{
    /// <summary>
    /// Defines DBusSharp container types.
    /// </summary>
    internal enum Container
    {
        /// <summary>
        /// Basic types plus VARIANT. VARIANT is not treated as a container because 
        /// DBusSharp‎ represents them as System.Object.
        /// </summary>
        None,
        /// <summary>
        /// STRUCT
        /// </summary>
        Struct,
        /// <summary>
        /// ARRAY
        /// </summary>
        Array,
        /// <summary>
        /// ARRAY of DICT_ENTRY
        /// </summary>
        DictEntryArray
    }
}
