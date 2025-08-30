
using MikoMe.Data;

namespace MikoMe.Services;

public static class DatabaseService
{
    private static DatabaseContext? _ctx;
    public static DatabaseContext Context => _ctx ??= new DatabaseContext();
}
