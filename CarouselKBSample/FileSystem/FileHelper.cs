using PCLStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarouselKBSample.FileSystem
{
    public class FileHelper
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="previousPath"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public static async Task MoveFileAsync(string previousPath, string newPath)
        {
            IFile file = await PCLStorage.FileSystem.Current.GetFileFromPathAsync(previousPath);
            await file.MoveAsync(newPath, NameCollisionOption.ReplaceExisting);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<string> CreateFormDirectory(string identifier, int userId)
        {
            IFolder userFolder = await PCLStorage.FileSystem.Current.LocalStorage.CreateFolderAsync(userId.ToString(),
                CreationCollisionOption.OpenIfExists);
            IFolder formFolder = await userFolder.CreateFolderAsync(identifier, CreationCollisionOption.OpenIfExists);
            return formFolder.Path;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<bool> DeleteFile(string path)
        {
            bool result = true;
            try
            {
                IFile file = await PCLStorage.FileSystem.Current.GetFileFromPathAsync(path);
                await file.DeleteAsync();
            }
            catch
            {
                result = false;
            }
            return result;
        }
    }
}
