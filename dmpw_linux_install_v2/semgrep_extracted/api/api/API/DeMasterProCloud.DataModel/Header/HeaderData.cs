namespace DeMasterProCloud.DataModel.Header
{
    /// <summary>
    /// Data model of header.
    /// This model is used to set a list of header in web page.
    /// </summary>
    public class HeaderData
    {
        /// <summary>
        /// A page name
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// An identifier of header
        /// </summary>
        public int HeaderId { get; set; }

        /// <summary>
        /// Header name
        /// </summary>
        public string HeaderName { get; set; }

        /// <summary>
        /// A variable name of header
        /// </summary>
        public string HeaderVariable { get; set; }

        /// <summary>
        /// A flag that distinguish whether the variable is Category data
        /// </summary>
        public bool IsCategory { get; set; }

        /// <summary>
        /// An order of header
        /// (This is not used now)
        /// </summary>
        //[JsonIgnore]
        public int HeaderOrder { get; set; }

        /// <summary>
        /// This variable is for visibility on page.
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }
}
