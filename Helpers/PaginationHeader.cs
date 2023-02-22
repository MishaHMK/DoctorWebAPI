using DoctorWebApi.Models;

namespace DoctorWebApi.Helpers
{
    public class PaginationHeader<T>
    {
        public PaginationHeader(PagedList<T> pagedList, int currentPage, int itemPerPage, int totalItems)
        {
            PagedList = pagedList;
            CurrentPage = currentPage;
            PageSize = itemPerPage;
            TotalItems = totalItems;
        }

        public PagedList<T> PagedList { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
    }
}
