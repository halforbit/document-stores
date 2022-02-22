using System;

namespace Halforbit.DocumentStores.Tests
{
    public static class TestValues
    {
        public const string CosmosDbConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
    }

    public record Person_String_Guid(
        Guid PersonId,
        string FirstName,
        string LastName,
        DateTime DateOfBirth);

    public record Person_Guid_Guid(
        Guid PersonId,
        Guid AccountId,
        string FirstName,
        string LastName,
        DateTime DateOfBirth);

    public record Person_Int_Int(
        int DepartmentId,
        int PersonId,
        string FirstName,
        string LastName,
        DateTime DateOfBirth);
}
