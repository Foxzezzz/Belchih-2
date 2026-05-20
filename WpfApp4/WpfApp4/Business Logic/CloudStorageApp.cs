using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WpfApp4.Data;
using WpfApp4.Models;
using Microsoft.EntityFrameworkCore;

namespace WpfApp4.Services
{
    public class CloudService
    {
        private readonly AppDbContext _context;
        private const string StoragePath = @"C:\CloudStorageFiles"; 

        public CloudService(AppDbContext context)
        {
            _context = context;
            if (!Directory.Exists(StoragePath)) Directory.CreateDirectory(StoragePath);
        }

        public async Task<bool> UploadFileAsync(int userId, int folderId, string fileName, byte[] fileData, string contentType)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var folder = await _context.Folders.FindAsync(folderId);
                if (folder == null) return false;

                var uniqueName = $"{Guid.NewGuid()}_{fileName}";
                var fullPath = Path.Combine(StoragePath, uniqueName);
                await File.WriteAllBytesAsync(fullPath, fileData);

                var newFile = new CloudFile
                {
                    Name = fileName,
                    Size = fileData.Length,
                    ContentType = contentType,
                    UploadDate = DateTime.Now,
                    OwnerId = userId,
                    FolderId = folderId,
                    FilePath = uniqueName
                };

                _context.Files.Add(newFile);

                _context.ActionLogs.Add(new ActionLog
                {
                    UserId = userId,
                    ActionType = "Upload",
                    TargetId = newFile.Id,
                    Timestamp = DateTime.Now,
                    Details = $"Uploaded {fileName}"
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteFileAsync(int userId, int fileId)
        {
            var file = await _context.Files.FindAsync(fileId);
            if (file == null) return false;

            if (file.OwnerId != userId)
            {
                var right = await _context.AccessRights
                    .FirstOrDefaultAsync(ar => ar.FileId == fileId && ar.UserId == userId && ar.CanWrite);
                if (right == null) return false;
            }

            var physicalPath = Path.Combine(StoragePath, file.FilePath);
            if (File.Exists(physicalPath)) File.Delete(physicalPath);

            _context.Files.Remove(file);

            _context.ActionLogs.Add(new ActionLog
            {
                UserId = userId,
                ActionType = "Delete",
                TargetId = fileId,
                Timestamp = DateTime.Now,
                Details = $"Deleted {file.Name}"
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<long> GetUsedSpaceAsync(int userId)
        {
            return await _context.Files
                .Where(f => f.OwnerId == userId)
                .SumAsync(f => f.Size);
        }

        public async Task<bool> CheckAccessAsync(int userId, int fileId, bool needWrite)
        {
            var file = await _context.Files.FindAsync(fileId);
            if (file == null) return false;
            if (file.OwnerId == userId) return true; 

            var access = await _context.AccessRights
                .FirstOrDefaultAsync(ar => ar.FileId == fileId && ar.UserId == userId);

            if (access == null) return false;
            return needWrite ? access.CanWrite : access.CanRead;
        }

        public async Task<string> CreateSharedLinkAsync(int fileId)
        {
            var token = Guid.NewGuid().ToString("N");
            var link = new SharedLink
            {
                FileId = fileId,
                Token = token,
                ExpiryDate = DateTime.Now.AddDays(7)
            };

            _context.SharedLinks.Add(link);
            await _context.SaveChangesAsync();

            return $"https://mycloud.com/share/{token}";
        }

        public async Task<List<Folder>> GetFoldersAsync(int? parentId)
        {
            return await _context.Folders
                .Where(f => f.ParentFolderId == parentId)
                .ToListAsync();
        }

        public async Task<List<CloudFile>> GetFilesAsync(int folderId)
        {
            return await _context.Files
                .Where(f => f.FolderId == folderId)
                .ToListAsync();
        }

        public async Task<List<ActionLog>> GetLogsAsync()
        {
            return await _context.ActionLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(50)
                .ToListAsync();
        }
    }
}