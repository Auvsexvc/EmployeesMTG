namespace MTGWebApi.Data
{
    public sealed class PaginetedList<T> : List<T>
    {
        public int Pageindex { get; }
        public int TotalPages { get; }

        public bool HasPreviousPage
        {
            get { return Pageindex > 1; }
        }

        public bool HasNextPage
        {
            get { return Pageindex < TotalPages; }
        }

        private PaginetedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            Pageindex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            AddRange(items);
        }

        public static PaginetedList<T> Create(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return new PaginetedList<T>(items, count, pageIndex, pageSize);
        }
    }
}