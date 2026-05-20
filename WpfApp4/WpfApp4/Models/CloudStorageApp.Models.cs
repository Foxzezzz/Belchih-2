using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WpfApp4.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }

    public class Folder
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentFolderId { get; set; }
        public int OwnerId { get; set; }

        [ForeignKey("ParentFolderId")]
        public Folder ParentFolder { get; set; }
        public ICollection<Folder> SubFolders { get; set; }
        public ICollection<CloudFile> Files { get; set; }
    }

    public class CloudFile
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadDate { get; set; }
        public int OwnerId { get; set; }
        public int FolderId { get; set; }
        public string FilePath { get; set; } // Путь на сервере

        [ForeignKey("FolderId")]
        public Folder Folder { get; set; }
    }

    public class AccessRight
    {
        [Key]
        public int Id { get; set; }
        public int FileId { get; set; }
        public int UserId { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
    }

    public class ActionLog
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ActionType { get; set; }
        public int TargetId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }
    }

    public class SharedLink
    {
        [Key]
        public int Id { get; set; }
        public int FileId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}