namespace ContentTemplates.app {

    // Namespaces.
    using Newtonsoft.Json;
    using System.Collections.Generic;


    /// <summary>
    /// Represents a list of selected doc types. Supports working with the DoctypePicker
    /// content property type.
    /// </summary>
    public class DoctypePickerList : List<DoctypePickerItem> {

        #region Methods

        /// <summary>
        /// Serializes a list of document types.
        /// </summary>
        /// <param name="source">
        /// The items to serialize.
        /// </param>
        /// <returns>The serialized items.</returns>
        public static string Serialize(DoctypePickerList source) {
            return JsonConvert.SerializeObject(source);
        }


        /// <summary>
        /// Deserializes a list of document types.
        /// </summary>
        /// <param name="source">
        /// The string to deserialize.
        /// </param>
        /// <returns>
        /// The list of items.
        /// </returns>
        public static DoctypePickerList Deserialize(string source) {
            return JsonConvert.DeserializeObject<DoctypePickerList>(source);
        }

        #endregion

    }

}