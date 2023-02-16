using DoctorWebApi.Models;

namespace DoctorWebApi.Helpers
{
    public class PaginationHeader
    {
        public PaginationHeader(PagedList<User> pagedList, int currentPage, int itemPerPage, int totalItems)
        {
            PagedUsers = pagedList;
            CurrentPage = currentPage;
            PageSize = itemPerPage;
            TotalItems = totalItems;
        }

        public PagedList<User> PagedUsers { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
    }
}
