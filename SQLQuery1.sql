CREATE TABLE Files (
    Id INT PRIMARY KEY IDENTITY,
    FileName NVARCHAR(255),
    FilePath NVARCHAR(1000),
    UploadDate DATETIME
);


select * from Files
