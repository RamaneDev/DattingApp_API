namespace DattingApp.API.Helpers
{
    public class PaginationHeader
    {
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }

        public PaginationHeader(int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            this.CurrentPage = currentPage;
            this.TotalItems = totalItems;
            this.ItemsPerPage = itemsPerPage;
            this.TotalPages = totalPages;
        }
    }
}