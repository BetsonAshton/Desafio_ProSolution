using System.Collections.Generic;

public interface IFileRepository
{
    void AddFile(string fileName, string filePath);
    List<string> GetAllFileNames();
}
