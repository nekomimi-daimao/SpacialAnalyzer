using System.Collections.Specialized;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using Nekomimi.Daimao;
using UnityEngine;

namespace SpacialAnalyzer.Scripts.Utils
{
    public static class EasyHttpRpcFileUtil
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            PersistentDataPath = Application.persistentDataPath;
            TemporaryCachePath = Application.temporaryCachePath;

            EasyHttpRpcHolder.EasyHttpRPC.RegisterRPC(nameof(ListFile), ListFile);
        }

        private static string PersistentDataPath;
        private static string TemporaryCachePath;

        // t, use TemporaryCachePath.
        private const string ArgPathType = "pathtype";
        private const string ArgDir = "dir";

        private static UniTask<string> ListFile(NameValueCollection arg)
        {
            var pathType = arg[ArgPathType] ?? string.Empty;
            var dir = arg[ArgDir] ?? string.Empty;

            var parentPath = pathType.StartsWith("t") ? TemporaryCachePath : PersistentDataPath;
            var path = Path.Combine(parentPath, dir);
            var dirInfo = new DirectoryInfo(path);

            if (!dirInfo.Exists)
            {
                return UniTask.FromResult("no such directory");
            }

            var builder = new StringBuilder();
            builder.AppendLine(path);
            builder.AppendLine();
            var tmpLength = builder.Length;

            foreach (var enumerateDirectory in dirInfo.EnumerateDirectories())
            {
                builder.AppendLine(enumerateDirectory.Name).Append("/");
            }

            foreach (var enumerateFile in dirInfo.EnumerateFiles())
            {
                builder.AppendLine(enumerateFile.Name);
            }

            if (builder.Length == tmpLength)
            {
                builder.AppendLine("no file");
            }

            return UniTask.FromResult(builder.ToString());
        }
    }
}
