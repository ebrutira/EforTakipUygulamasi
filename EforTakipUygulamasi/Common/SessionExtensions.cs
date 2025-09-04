namespace EforTakipUygulamasi.Common
{
    public static class SessionExtensions
    {
        public static bool IsAdmin(this ISession session)
        {
            var userRole = session.GetInt32("UserRole");
            return userRole == (int)UserRole.Admin;
        }

        public static bool IsDeveloper(this ISession session)
        {
            var userRole = session.GetInt32("UserRole");
            return userRole == (int)UserRole.Developer;
        }

        public static bool IsViewer(this ISession session)
        {
            var userRole = session.GetInt32("UserRole");
            return userRole == (int)UserRole.Viewer;
        }

        public static bool CanEdit(this ISession session)
        {
            var userRole = session.GetInt32("UserRole");
            return userRole == (int)UserRole.Admin || userRole == (int)UserRole.Developer;
        }

        public static bool CanDelete(this ISession session)
        {
            var userRole = session.GetInt32("UserRole");
            return userRole == (int)UserRole.Admin;
        }
    }
}