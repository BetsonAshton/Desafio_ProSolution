using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;

public class FileRepository : IFileRepository
{
    private readonly string _connectionString;

    public FileRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void AddFile(string fileName, string filePath)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var sql = "INSERT INTO Files (FileName, FilePath, UploadDate) VALUES (@FileName, @FilePath, @UploadDate)";
            connection.Execute(sql, new { FileName = fileName, FilePath = filePath, UploadDate = DateTime.Now });
        }
    }

    public List<string> GetAllFileNames()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var sql = "SELECT FileName FROM Files";
            return connection.Query<string>(sql).AsList();
        }
    }
}
