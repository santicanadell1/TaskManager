using DataAccess;
using Domain;
using Service.Models;

namespace Service;

public class MemberPService
{
    private InMemoryDatabase _database;
    public MemberPService(InMemoryDatabase database)
    {
        _database = database;
    }


}