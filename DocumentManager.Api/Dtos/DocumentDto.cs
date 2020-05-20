namespace DocumentManager.Api.Dtos
{
    public class DocumentDto
    {
        /// <summary>
        /// Document identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Document name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Location where document file is stored
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Size of the document file
        /// </summary>
        public long FileSize { get; set; }
    }
}
